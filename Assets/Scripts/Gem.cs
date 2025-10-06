using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Gem : MonoBehaviour
{
    public Vector2Int GridPos { get; private set; }
    public GemType Type { get; private set; }
    public bool IsMoving { get; private set; }

    SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        // 保证能被点到
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    public void Init(GemType t, Vector2Int gridPos)
    {
        Type = t;
        GridPos = gridPos;
        _sr.sprite = t.sprite;
        _sr.color  = t.tint;
        name = $"Gem[{gridPos.x},{gridPos.y}]/{t.id}";
    }

    public void SetGridPos(Vector2Int p) => GridPos = p;

    public void SetHighlight(bool on)
    {
        _sr.color = on ? Type.tint * 1.2f : Type.tint;
    }

    public void MoveTo(Vector3 worldPos, float speed)
    {
        if (gameObject.activeInHierarchy) StartCoroutine(MoveCR(worldPos, speed));
    }

    System.Collections.IEnumerator MoveCR(Vector3 worldPos, float speed)
    {
        IsMoving = true;
        while ((transform.position - worldPos).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.MoveTowards(transform.position, worldPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = worldPos;
        IsMoving = false;
    }
}
