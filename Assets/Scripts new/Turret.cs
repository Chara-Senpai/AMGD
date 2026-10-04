using UnityEngine;

public class Turret : MonoBehaviour
{
    public enum Type { Flame, Sniper, Shotgun }
    public Type type = Type.Sniper;

    [Header("Detection (rotate)")]
    public float detectRadius = 8f;
    public float rotateSpeed = 240f;

    [Header("Firing cone (shoot)")]
    public float fireRange = 6f;
    public float coneAngle = 45f;
    public float fireRate = 1f;

    [Header("Bullets (Sniper / Shotgun)")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 12f;
    public int pellets = 5;
    public float pelletSpread = 10f;
    public float streamInterval = 0.05f;
    public float streamJitter = 4f;

    private LineRenderer radiusLine, coneLine;
    private float timer;

    static readonly Color radiusIdle = new Color(1f, 1f, 1f, 0.35f);
    static readonly Color radiusAlert = new Color(1f, 0.9f, 0.2f, 0.9f);
    static readonly Color coneIdle = new Color(1f, 0.4f, 0.3f, 0.5f);
    static readonly Color coneHot = new Color(1f, 0.1f, 0.1f, 1f);

    void Awake()
    {
        radiusLine = MakeLine("RadiusLine");
        coneLine = MakeLine("ConeLine");
        timer = fireRate;
    }

    void Update()
    {
        Enemy target = FindClosest(detectRadius);

        if (target)
        {
            Vector2 dir = target.transform.position - transform.position;
            float desired = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            float z = Mathf.MoveTowardsAngle(transform.eulerAngles.z, desired, rotateSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, z);
        }

        timer += Time.deltaTime;
        Enemy inCone = FindInCone();
        float interval = type == Type.Flame ? streamInterval : fireRate;
        if (inCone && timer >= interval && GameManager.I.State == GameManager.GameState.Playing)
        {
            timer = 0f;
            Fire(inCone);
        }

        DrawLines(target != null, inCone != null);
    }

    Enemy FindClosest(float radius)
    {
        Enemy best = null;
        float bestD = float.MaxValue;
        foreach (var c in Enemy.All)
        {
            if (c.IsDead) continue;
            float d = Vector2.Distance(c.transform.position, transform.position);
            if (d <= radius && d < bestD) { best = c; bestD = d; }
        }
        return best;
    }

    bool InCone(Enemy c)
    {
        Vector2 to = c.transform.position - transform.position;
        return to.magnitude <= fireRange && Vector2.Angle(transform.up, to) <= coneAngle * 0.5f;
    }

    Enemy FindInCone()
    {
        Enemy best = null;
        float bestD = float.MaxValue;
        foreach (var c in Enemy.All)
        {
            if (c.IsDead || !InCone(c)) continue;
            float d = Vector2.Distance(c.transform.position, transform.position);
            if (d < bestD) { best = c; bestD = d; }
        }
        return best;
    }

    void Fire(Enemy target)
    {
        switch (type)
        {
            case Type.Flame:
                float jitter = Random.Range(-streamJitter, streamJitter);
                float flameRad = (transform.eulerAngles.z + jitter) * Mathf.Deg2Rad;
                SpawnBullet(new Vector3(-Mathf.Sin(flameRad), Mathf.Cos(flameRad), 0f));
                break;

            case Type.Sniper:
                SpawnBullet((target.transform.position - transform.position).normalized);
                break;

            case Type.Shotgun:
                for (int i = 0; i < pellets; i++)
                {
                    float offset = (i - (pellets - 1) * 0.5f) * pelletSpread;
                    float rad = (transform.eulerAngles.z + offset) * Mathf.Deg2Rad;
                    SpawnBullet(new Vector3(-Mathf.Sin(rad), Mathf.Cos(rad), 0f));
                }
                break;
        }
    }

    void SpawnBullet(Vector3 dir)
    {
        SpawnBullet(dir, bulletSpeed);
    }

    void SpawnBullet(Vector3 dir, float speed)
    {
        GameObject b = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Bullet bullet = b.GetComponent<Bullet>();
        if (!bullet) bullet = b.AddComponent<Bullet>();
        bullet.Setup(dir, speed);
    }

    LineRenderer MakeLine(string n)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.widthMultiplier = 0.06f;
        lr.sortingOrder = 10;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        return lr;
    }

    void DrawLines(bool alert, bool hot)
    {
        int seg = 48;
        radiusLine.positionCount = seg + 1;
        for (int i = 0; i <= seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            radiusLine.SetPosition(i, transform.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * detectRadius);
        }
        radiusLine.startColor = radiusLine.endColor = alert ? radiusAlert : radiusIdle;

        int pts = 20;
        coneLine.positionCount = pts + 3;
        coneLine.SetPosition(0, transform.position);
        float start = transform.eulerAngles.z - coneAngle * 0.5f;
        for (int i = 0; i <= pts; i++)
        {
            float rad = (start + i * (coneAngle / pts)) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(-Mathf.Sin(rad), Mathf.Cos(rad), 0f);
            coneLine.SetPosition(i + 1, transform.position + dir * fireRange);
        }
        coneLine.SetPosition(pts + 2, transform.position);
        coneLine.startColor = coneLine.endColor = hot ? coneHot : coneIdle;
    }
}