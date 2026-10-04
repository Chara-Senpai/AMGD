using UnityEngine;

public static class Bezier
{
    public static Vector3 Quadratic(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);
    }

    public static Vector3 Cubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        return Vector3.Lerp(Quadratic(a, b, c, t), Quadratic(b, c, d, t), t);
    }

    public static Vector3 Evaluate(Vector3[] p, float t)
    {
        return p.Length == 3 ? Quadratic(p[0], p[1], p[2], t)
                             : Cubic(p[0], p[1], p[2], p[3], t);
    }

    public static float Length(Vector3[] p, int steps = 60)
    {
        float len = 0f;
        Vector3 prev = p[0];
        for (int i = 1; i <= steps; i++)
        {
            Vector3 cur = Evaluate(p, i / (float)steps);
            len += Vector3.Distance(prev, cur);
            prev = cur;
        }
        return len;
    }
}

public static class Easing
{
    public static float OutCubic(float t) { t = Mathf.Clamp01(t); return 1f - Mathf.Pow(1f - t, 3f); }
    public static float InQuad(float t)   { t = Mathf.Clamp01(t); return t * t; }
}