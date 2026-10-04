using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> All = new List<Enemy>();

    public bool IsDead { get; private set; }

    private Vector3[] pts;
    private float speed, length, t;

    void OnEnable()  { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public void Init(Vector3[] path, float moveSpeed)
    {
        pts = path;
        speed = moveSpeed;
        length = Mathf.Max(Bezier.Length(pts), 0.01f);
        transform.position = pts[0];
    }

    void Update()
    {
        if (IsDead || pts == null) return;
        if (GameManager.I.State != GameManager.GameState.Playing) return;

        t += speed * Time.deltaTime / length;

        Vector3 prev = transform.position;
        Vector3 next = Bezier.Evaluate(pts, Mathf.Clamp01(t));
        transform.position = next;

        Vector3 d = next - prev;
        if (d.sqrMagnitude > 1e-6f) transform.up = d.normalized;

        if (t >= 1f)
        {
            IsDead = true;
            GameManager.I.DamagePlayer(1);
            Destroy(gameObject);
        }
    }

    public void Kill()
    {
        if (IsDead) return;
        IsDead = true;
        GameManager.I.OnEnemyKilled(transform.position);
        Destroy(gameObject);
    }
}