using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Gem : MonoBehaviour
{
    // === 状态 ===
    public Vector2Int GridPos { get; private set; }
    public GemType    Type    { get; private set; }
    public bool       IsMoving { get; private set; }

    // === 可调参数 ===
    [Header("Visual Tweaks")]
    [SerializeField] private float highlightMul = 1.2f;  // 高亮增益
    [SerializeField] private float popScale     = 1.2f;  // Pop 放大倍数
    [SerializeField] private float popTime      = 0.08f; // Pop 时长
    [SerializeField] private float fadeTime     = 0.10f; // 淡出时长
    [SerializeField] private float moveEpsilon  = 0.0001f; // 移动阈值（只保留这一份）

    [Header("Refs")]
    [SerializeField] private SpriteRenderer sr; // Inspector 可拖

    // === 缓存组件 ===
    private BoxCollider2D _col;

    void Awake()
    {
        // 手动没拖就自动找；别覆盖手动设置
        sr   ??= GetComponent<SpriteRenderer>();
        _col ??= GetComponent<BoxCollider2D>();

        _col.isTrigger = true;

        if (sr.sprite != null)
            RefreshColliderToSprite();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        sr   ??= GetComponent<SpriteRenderer>();
        _col ??= GetComponent<BoxCollider2D>();
        if (_col != null) _col.isTrigger = true;
        if (sr != null && sr.sprite != null)
            RefreshColliderToSprite();
    }
#endif

    // === 初始化与类型切换 ===
    public void Init(GemType t, Vector2Int gridPos)
    {
        SetType(t);
        GridPos = gridPos;
        name    = $"Gem[{gridPos.x},{gridPos.y}]/{t.id}";
        RefreshColliderToSprite();
    }

    public void SetType(GemType t)
    {
        Type = t;

        sr ??= GetComponent<SpriteRenderer>();

        if (t != null)
        {
            if (t.sprite != null) sr.sprite = t.sprite;
            sr.color = t.tint;
        }
        else
        {
            Debug.LogWarning("[Gem] SetType 收到 null，保持当前外观。", this);
        }
    }

    public void SetGridPos(Vector2Int p) => GridPos = p;

    public void SetHighlight(bool on)
    {
        if (sr == null) return;

        Color baseCol = (Type != null) ? Type.tint : sr.color;
        float mul = on ? highlightMul : 1f;
        sr.color = new Color(
            Mathf.Clamp01(baseCol.r * mul),
            Mathf.Clamp01(baseCol.g * mul),
            Mathf.Clamp01(baseCol.b * mul),
            baseCol.a
        );
    }

    // === 移动 ===
    public void MoveTo(Vector3 worldPos, float speed)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(MoveCR(worldPos, speed));
    }

    System.Collections.IEnumerator MoveCR(Vector3 worldPos, float speed)
    {
        IsMoving = true;
        while ((transform.position - worldPos).sqrMagnitude > moveEpsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, worldPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = worldPos;
        IsMoving = false;
    }

    // === 动画占位 ===
    public System.Collections.IEnumerator PopCR()
    {
        Vector3 origin = Vector3.one;
        Vector3 target = Vector3.one * popScale;

        float t = 0f;
        while (t < popTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / popTime);
            transform.localScale = Vector3.Lerp(origin, target, k);
            yield return null;
        }
        transform.localScale = target;
    }

    public System.Collections.IEnumerator FadeOutCR()
    {
        if (sr == null) yield break;

        float t = 0f;
        Color c0 = sr.color;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeTime);
            sr.color = new Color(c0.r, c0.g, c0.b, 1f - k);
            yield return null;
        }
        sr.color = new Color(c0.r, c0.g, c0.b, 0f);
    }

    // === 碰撞盒自适应当前 sprite ===
    public void RefreshColliderToSprite()
    {
        if (sr == null || _col == null || sr.sprite == null) return;

        Bounds b = sr.sprite.bounds; // 本地坐标
        _col.size   = b.size;
        _col.offset = b.center;
    }
}
