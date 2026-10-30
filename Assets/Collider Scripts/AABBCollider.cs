using UnityEngine;

public enum ColliderType { Player, Ground, Obstacle, FallZone }

public class AABBCollider : MonoBehaviour
{
    [SerializeField] private float _length = 1f;
    [SerializeField] private float _width = 1f;
    [SerializeField] private ColliderType _type;

    public ColliderType Type => _type;

    // Computed fresh every time so they're never out of date
    public Vector2 max => new Vector2(transform.position.x + _width * .5f,
                                      transform.position.y + _length * .5f);
    public Vector2 min => new Vector2(transform.position.x - _width * .5f,
                                      transform.position.y - _length * .5f);

    private void Start()
    {
        if (CollisionManager.Instance != null)
            CollisionManager.Instance.Register(this);
    }

    private void OnDestroy()
    {
        if (CollisionManager.Instance != null)
            CollisionManager.Instance.Unregister(this);
    }

    private void Update()
    {
        Debug.DrawLine(new Vector3(min.x, max.y), new Vector3(max.x, max.y), Color.blueViolet);
        Debug.DrawLine(new Vector3(max.x, max.y), new Vector3(max.x, min.y), Color.blueViolet);
        Debug.DrawLine(new Vector3(max.x, min.y), new Vector3(min.x, min.y), Color.blueViolet);
        Debug.DrawLine(new Vector3(min.x, min.y), new Vector3(min.x, max.y), Color.blueViolet);
    }

    public void OnCollision(AABBCollider other)
    {
        // Debug.Log($"Hit {other.name}"); // spams every frame once things touch, so I commented it out
    }
}