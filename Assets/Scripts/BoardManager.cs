using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Match-3 Board Manager
/// 匹配消除核心棋盘管理
/// </summary>
public class BoardManager : MonoBehaviour
{
    [Header("Board")]
    public int width = 8;                 // Board width / 棋盘宽
    public int height = 8;                // Board height / 棋盘高
    public float tileSize = 1f;           // Cell size / 单元格尺寸
    public float fallSpeed = 12f;         // Move speed for swaps and falls / 交换与下落速度
    public int minMatch = 3;              // Minimum run length to clear / 最小消除长度

    [Header("Prefabs & Types")]
    public Gem gemPrefab;                 // Gem prefab with Gem script / 带 Gem 脚本的宝石预制
    public List<GemType> gemTypes;        // Scriptable gem types / 宝石类型资源列表

    [Header("Input")]
    public LayerMask gemLayer;            // Layer for raycast picking / 射线拾取图层

    [Header("Scoring & Timer")]
    public int targetScore = 800;         // Target score to win / 胜利目标分
    public float timeLimitSeconds = 60f;  // Countdown time / 倒计时秒数
    public float cascadeBonusStep = 0.5f; // Chain multiplier step / 连锁加成步进

    [Header("UI (TMP)")]
    public TMP_Text scoreText;            // HUD: score text / 分数文本
    public TMP_Text timeText;             // HUD: time text / 时间文本
    public TMP_Text targetText;           // HUD: target text / 目标文本
    public GameObject resultPanel;        // Result popup panel / 结果面板
    public TMP_Text resultText;           // Result message text (NOT the button label) / 结果信息文本（不要绑到按钮里的字）
    public Button restartButton;          // Restart button / 重新开始按钮

    // Runtime state / 运行期状态
    private Gem[,] _grid;
    private Camera _cam;
    private Gem _selected;

    private int _score;
    private float _timeLeft;
    private bool _gameOver;

    // ================== Lifecycle ==================
    void Start()
    {
        _cam = Camera.main;
        StartLevel();
    }

    /// <summary>
    /// Initialize or restart the level.
    /// 初始化或重开一局
    /// </summary>
    public void StartLevel()
    {
        _gameOver = false;
        _score = 0;
        _timeLeft = timeLimitSeconds;

        if (resultPanel) resultPanel.SetActive(false);

        // Clear existing board objects / 清理旧棋盘对象
        if (_grid != null)
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (_grid[x, y] != null) Destroy(_grid[x, y].gameObject);
        }

        BuildBoard();
        UpdateHUD();

        if (restartButton)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => StartLevel());
        }

        StopAllCoroutines();
        StartCoroutine(TimerCR());
    }

    /// <summary>
    /// Countdown coroutine.
    /// 倒计时协程
    /// </summary>
    IEnumerator TimerCR()
    {
        while (!_gameOver && _timeLeft > 0f)
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft < 0f) _timeLeft = 0f;
            UpdateHUD();
            yield return null;
        }

        if (_gameOver) yield break;

        // Wait until animations settle then evaluate result
        // 等待动画结束再判断胜负
        while (IsBusy()) yield return null;

        EndGame(_score >= targetScore);
    }

    /// <summary>
    /// Show end result.
    /// 展示结算结果
    /// </summary>
    void EndGame(bool win)
    {
        if (_gameOver) return;
        _gameOver = true;

        if (resultPanel) resultPanel.SetActive(true);

        if (resultText)
        {
            resultText.text = win ? "Win!" : "Lose...";
            resultText.color = win ? Color.green : Color.red;
        }
    }

    /// <summary>
    /// Check win during cascades.
    /// 连锁中途检查胜利
    /// </summary>
    void CheckWin()
    {
        if (_gameOver) return;
        if (_score >= targetScore) EndGame(true);
    }

    // ================== Board Build ==================
    /// <summary>
    /// Build initial board with no starting matches.
    /// 生成初始棋盘且避免开局即成型消除
    /// </summary>
    void BuildBoard()
    {
        if (gemTypes == null || gemTypes.Count == 0)
        {
            Debug.LogError("gemTypes is empty. Please assign at least one GemType in BoardManager.gemTypes.");
            return;
        }
        if (width <= 0 || height <= 0)
        {
            Debug.LogError("Invalid board size. width and height must be > 0.");
            return;
        }

        _grid = new Gem[width, height];
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            PlaceNewGem(x, y);
            int guard = 0;
            while (HasMatchAt(x, y) && guard++ < 100)
            {
                Destroy(_grid[x, y].gameObject);
                PlaceNewGem(x, y);
            }
        }
    }

    /// <summary>
    /// Spawn a new gem at grid position.
    /// 在网格坐标生成新宝石
    /// </summary>
    void PlaceNewGem(int x, int y)
    {
        var world = GridToWorld(x, y);
        var g = Instantiate(gemPrefab, world, Quaternion.identity, transform);
        var t = gemTypes[Random.Range(0, gemTypes.Count)];
        g.Init(t, new Vector2Int(x, y));
        _grid[x, y] = g;
        g.gameObject.layer = LayerMask.NameToLayer("Gem");
    }

    /// <summary>
    /// Grid to world position.
    /// 网格坐标转为世界坐标
    /// </summary>
    Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x * tileSize, y * tileSize, 0f)
             - new Vector3((width - 1) * tileSize / 2f, (height - 1) * tileSize / 2f, 0f);
    }

    bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

    // ================== Loop & Input ==================
    void Update()
    {
        if (_gameOver) return;
        if (IsBusy()) return;
        HandleInput();
    }

    /// <summary>
    /// Check if any gem is moving.
    /// 是否有宝石在移动
    /// </summary>
    bool IsBusy()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            if (_grid[x, y] && _grid[x, y].IsMoving) return true;
        return false;
    }

    /// <summary>
    /// Mouse input for selecting and swapping.
    /// 鼠标输入：选择与交换
    /// </summary>
    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.GetRayIntersection(_cam.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, gemLayer);
            if (hit.collider)
            {
                var g = hit.collider.GetComponent<Gem>();
                SelectGem(g);
            }
        }
    }

    /// <summary>
    /// Selection logic.
    /// 选择逻辑
    /// </summary>
    void SelectGem(Gem g)
    {
        if (_selected == null)
        {
            _selected = g;
            _selected.SetHighlight(true);
            return;
        }

        if (g == _selected)
        {
            _selected.SetHighlight(false);
            _selected = null;
            return;
        }

        if (IsAdjacent(_selected, g))
        {
            StartCoroutine(SwapAndResolve(_selected, g));
            _selected.SetHighlight(false);
            _selected = null;
        }
        else
        {
            _selected.SetHighlight(false);
            _selected = g;
            _selected.SetHighlight(true);
        }
    }

    /// <summary>
    /// Manhattan adjacency check.
    /// 曼哈顿邻接判断
    /// </summary>
    bool IsAdjacent(Gem a, Gem b)
    {
        var d = a.GridPos - b.GridPos;
        return Mathf.Abs(d.x) + Mathf.Abs(d.y) == 1;
    }

    /// <summary>
    /// Swap two gems and resolve matches.
    /// 交换两枚宝石并处理连消
    /// </summary>
    IEnumerator SwapAndResolve(Gem a, Gem b)
    {
        SwapInGrid(a, b);
        yield return AnimateSwap(a, b);

        var matches = FindAllMatches();
        if (matches.Count == 0)
        {
            // Invalid swap: swap back / 无效交换：换回
            SwapInGrid(a, b);
            yield return AnimateSwap(a, b);
            yield break;
        }

        // Valid: resolve cascades / 有效：开始连锁解析
        yield return ResolveBoard(matches, 1);
        CheckWin();
    }

    /// <summary>
    /// Swap references and grid positions.
    /// 交换引用与网格坐标
    /// </summary>
    void SwapInGrid(Gem a, Gem b)
    {
        var pa = a.GridPos;
        var pb = b.GridPos;
        _grid[pa.x, pa.y] = b;
        _grid[pb.x, pb.y] = a;
        a.SetGridPos(pb);
        b.SetGridPos(pa);
    }

    /// <summary>
    /// Animate swapping motion.
    /// 播放交换动画
    /// </summary>
    IEnumerator AnimateSwap(Gem a, Gem b)
    {
        var wa = GridToWorld(a.GridPos.x, a.GridPos.y);
        var wb = GridToWorld(b.GridPos.x, b.GridPos.y);
        a.MoveTo(wa, fallSpeed);
        b.MoveTo(wb, fallSpeed);
        while (a.IsMoving || b.IsMoving) yield return null;
    }

    // ================== Match / Resolve ==================
    /// <summary>
    /// Find all horizontal and vertical match groups.
    /// 扫描全盘，找出所有横纵成组的匹配
    /// </summary>
    List<List<Vector2Int>> FindAllMatches()
    {
        var result = new List<List<Vector2Int>>();

        // Horizontal runs / 横向
        for (int y = 0; y < height; y++)
        {
            int run = 1;
            for (int x = 1; x <= width; x++)
            {
                bool same = false;
                if (x < width) same = _grid[x, y].Type == _grid[x - 1, y].Type;

                if (same) run++;
                else
                {
                    if (run >= minMatch)
                    {
                        var group = new List<Vector2Int>();
                        for (int k = 0; k < run; k++) group.Add(new Vector2Int(x - 1 - k, y));
                        result.Add(group);
                    }
                    run = 1;
                }
            }
        }

        // Vertical runs / 纵向
        for (int x = 0; x < width; x++)
        {
            int run = 1;
            for (int y = 1; y <= height; y++)
            {
                bool same = false;
                if (y < height) same = _grid[x, y].Type == _grid[x, y - 1].Type;

                if (same) run++;
                else
                {
                    if (run >= minMatch)
                    {
                        var group = new List<Vector2Int>();
                        for (int k = 0; k < run; k++) group.Add(new Vector2Int(x, y - 1 - k));
                        result.Add(group);
                    }
                    run = 1;
                }
            }
        }

        // Merge and de-duplicate indices / 合并并去重
        var seen = new HashSet<Vector2Int>();
        var merged = new List<List<Vector2Int>>();
        foreach (var g in result)
        {
            var m = new List<Vector2Int>();
            foreach (var p in g)
                if (seen.Add(p)) m.Add(p);
            if (m.Count > 0) merged.Add(m);
        }
        return merged;
    }

    /// <summary>
    /// Check if a cell forms a match.
    /// 检查某格是否成组匹配
    /// </summary>
    bool HasMatchAt(int x, int y)
    {
        if (_grid == null || !InBounds(x, y) || _grid[x, y] == null) return false;

        GemType t = _grid[x, y].Type;

        // Horizontal count / 横向统计
        int h = 1;
        for (int i = x - 1; i >= 0 && _grid[i, y] != null && _grid[i, y].Type == t; i--) h++;
        for (int i = x + 1; i < width && _grid[i, y] != null && _grid[i, y].Type == t; i++) h++;
        if (h >= minMatch) return true;

        // Vertical count / 纵向统计
        int v = 1;
        for (int j = y - 1; j >= 0 && _grid[x, j] != null && _grid[x, j].Type == t; j--) v++;
        for (int j = y + 1; j < height && _grid[x, j] != null && _grid[x, j].Type == t; j++) v++;
        return v >= minMatch;
    }

    /// <summary>
    /// Chain multiplier by index: 1→1.0, 2→1.0+step, 3→1.0+2*step...
    /// 连锁倍数：1连=1.0，2连=1.0+step，3连=1.0+2*step…
    /// </summary>
    float CascadeMultiplier(int chainIndex)
    {
        return 1f + (chainIndex - 1) * Mathf.Max(0f, cascadeBonusStep);
    }

    /// <summary>
    /// Resolve clear, fall, refill, and subsequent chains.
    /// 处理消除、下落、补齐与后续连锁
    /// </summary>
    IEnumerator ResolveBoard(List<List<Vector2Int>> matches, int chainIndex)
    {
        // 1) Score and clear / 计分并清除
        int baseScore = 0;

        foreach (var group in matches)
        foreach (var p in group)
        {
            if (!InBounds(p.x, p.y)) continue;
            var gem = _grid[p.x, p.y];
            if (gem == null) continue;
            baseScore += gem.Type.score;
            Destroy(gem.gameObject);
            _grid[p.x, p.y] = null;
        }

        if (baseScore > 0)
        {
            _score += Mathf.RoundToInt(baseScore * CascadeMultiplier(chainIndex));
            UpdateHUD();

            // Early win guard to avoid later UI overwrites
            // 提前判赢，防止后续流程覆盖文案
            if (_score >= targetScore)
            {
                EndGame(true);
                yield break;
            }
        }

        yield return new WaitForSeconds(0.05f);

        // 2) Collapse columns / 掉落
        for (int x = 0; x < width; x++)
        {
            int writeY = 0;
            for (int y = 0; y < height; y++)
            {
                if (_grid[x, y] != null)
                {
                    if (y != writeY)
                    {
                        var g = _grid[x, y];
                        _grid[x, writeY] = g;
                        _grid[x, y] = null;
                        g.SetGridPos(new Vector2Int(x, writeY));
                        g.MoveTo(GridToWorld(x, writeY), fallSpeed);
                    }
                    writeY++;
                }
            }

            // 3) Refill from top / 顶部补齐
            for (int y = writeY; y < height; y++)
            {
                var spawn = GridToWorld(x, height - 1) + Vector3.up * tileSize * 2f;
                var g = Instantiate(gemPrefab, spawn, Quaternion.identity, transform);
                var t = gemTypes[Random.Range(0, gemTypes.Count)];
                g.Init(t, new Vector2Int(x, y));
                _grid[x, y] = g;
                g.gameObject.layer = LayerMask.NameToLayer("Gem");
                g.MoveTo(GridToWorld(x, y), fallSpeed);
            }
        }

        // Wait until all motions stop / 等待动画结束
        bool moving;
        do
        {
            moving = false;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (_grid[x, y] && _grid[x, y].IsMoving) { moving = true; break; }
            yield return null;
        } while (moving);

        // 4) Next chain / 连锁
        var next = FindAllMatches();
        if (next.Count > 0)
            yield return ResolveBoard(next, chainIndex + 1);
    }

    // ================== HUD ==================
    /// <summary>
    /// Update on-screen texts.
    /// 更新 HUD 文本
    /// </summary>
    void UpdateHUD()
    {
        if (scoreText) scoreText.text = $"Score: {_score}";
        if (targetText) targetText.text = $"Target: {targetScore}";
        if (timeText)
        {
            int s = Mathf.CeilToInt(_timeLeft);
            int mm = s / 60, ss = s % 60;
            timeText.text = $"Time: {mm:00}:{ss:00}";
        }
    }
}
