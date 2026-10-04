using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float hitRadius = 0.4f;
    public float lifetime = 3f;

    private Vector3 dir;
    private float speed;

    public void Setup(Vector3 d, float s)
    {
        dir = d.normalized;
        speed = s;
        transform.up = dir;
    }

    void Update()
    {
        Vector2 a = transform.position;
        Vector2 b = a + (Vector2)(dir * speed * Time.deltaTime);
        transform.position = b;

        foreach (var c in Enemy.All)
        {
            if (c.IsDead) continue;
            if (DistToSegment(c.transform.position, a, b) <= hitRadius)
            {
                c.Kill();
                Destroy(gameObject);
                return;
            }
        }

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Destroy(gameObject);
    }

    static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float len2 = ab.sqrMagnitude;
        float t = len2 > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / len2) : 0f;
        return Vector2.Distance(p, a + ab * t);
    }
}