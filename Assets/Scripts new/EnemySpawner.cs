using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Enemy enemyPrefab;
    public Transform spawnQuad, spawnCubic, target;

    [Header("Optional manual control points")]
    public Transform quadControl;
    public Transform cubicControl1, cubicControl2;

    [Header("Auto control point offsets")]
    public float quadArcOffset = 6f;
    public float cubicSOffset = 4f;

    [Header("Spawning")]
    public int totalEnemies = 30;
    public float spawnInterval = 1.5f;
    public float enemySpeed = 2.5f;
    public bool showPathPreview = true;

    public bool Finished => spawned >= totalEnemies;
    public int RemainingToSpawn => totalEnemies - spawned;

    private int spawned;
    private float timer = 1f;
    private bool nextIsQuad = true;

    public Vector3[] QuadPoints()
    {
        Vector3 a = spawnQuad.position, c = target.position;
        Vector3 b = quadControl ? quadControl.position
                  : (a + c) * 0.5f + Perp(c - a) * quadArcOffset;
        return new[] { a, b, c };
    }

    public Vector3[] CubicPoints()
    {
        Vector3 a = spawnCubic.position, d = target.position;
        Vector3 dir = d - a;
        Vector3 b = cubicControl1 ? cubicControl1.position : a + dir / 3f + Perp(dir) * cubicSOffset;
        Vector3 c = cubicControl2 ? cubicControl2.position : a + dir * 2f / 3f - Perp(dir) * cubicSOffset;
        return new[] { a, b, c, d };
    }

    static Vector3 Perp(Vector3 v) { return new Vector3(-v.y, v.x, 0f).normalized; }

    void Start()
    {
        if (!showPathPreview) return;
        MakePathLine("QuadPath", QuadPoints(), new Color(1f, 0.8f, 0.2f, 0.5f));
        MakePathLine("CubicPath", CubicPoints(), new Color(0.3f, 0.9f, 1f, 0.5f));
    }

    void Update()
    {
        if (GameManager.I.State != GameManager.GameState.Playing || Finished) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = spawnInterval;

        Vector3[] pts = nextIsQuad ? QuadPoints() : CubicPoints();
        nextIsQuad = !nextIsQuad;

        Enemy c = Instantiate(enemyPrefab);
        c.Init(pts, enemySpeed);
        spawned++;
    }

    void MakePathLine(string n, Vector3[] pts, Color col)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.widthMultiplier = 0.08f;
        lr.sortingOrder = 1;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = col;
        int steps = 40;
        lr.positionCount = steps + 1;
        for (int i = 0; i <= steps; i++) lr.SetPosition(i, Bezier.Evaluate(pts, i / (float)steps));
    }

    void OnDrawGizmos()
    {
        if (!spawnQuad || !spawnCubic || !target) return;
        DrawGizmoPath(QuadPoints(), Color.yellow);
        DrawGizmoPath(CubicPoints(), Color.cyan);
    }

    void DrawGizmoPath(Vector3[] p, Color col)
    {
        Gizmos.color = col;
        Vector3 prev = p[0];
        for (int i = 1; i <= 40; i++)
        {
            Vector3 cur = Bezier.Evaluate(p, i / 40f);
            Gizmos.DrawLine(prev, cur);
            prev = cur;
        }
        foreach (var v in p) Gizmos.DrawWireSphere(v, 0.2f);
    }
}