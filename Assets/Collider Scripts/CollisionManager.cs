using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    public static CollisionManager Instance => instance;
    private static CollisionManager instance;

    private List<AABBCollider> colliders = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    public void Register(AABBCollider collider)
    {
        if (colliders.Contains(collider)) return;
        colliders.Add(collider);
    }

    public void Unregister(AABBCollider collider)
    {
        if (!colliders.Contains(collider)) return;
        colliders.Remove(collider);
    }

    public void GetOverlaps(AABBCollider source, List<AABBCollider> results)
    {
        results.Clear();
        foreach (var other in colliders)
        {
            if (other == source) continue;
            if (source.min.x < other.max.x && other.min.x < source.max.x &&
                source.min.y < other.max.y && other.min.y < source.max.y)
            {
                results.Add(other);
            }
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < colliders.Count; i++)
        {
            var colliderA = colliders[i];
            for (int j = 0; j < colliders.Count; j++)
            {
                if (i == j) continue;
                var colliderB = colliders[j];
                if (Overlaps(colliderA, colliderB))
                {
                    colliderA.OnCollision(colliderB);
                    colliderB.OnCollision(colliderA);
                }
            }
        }
    }

    bool Overlaps(AABBCollider a, AABBCollider b)
    {
        return a.min.x <= b.max.x && b.min.x <= a.max.x &&
               a.min.y <= b.max.y && b.min.y <= a.max.y;
    }
}