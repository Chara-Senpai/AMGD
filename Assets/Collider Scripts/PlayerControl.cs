using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AABBCollider))]
public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpSpeed = 10f;
    [SerializeField] private float _gravity = -25f;

    private AABBCollider _col;
    private Vector2 _velocity;
    private bool _grounded;
    private readonly List<AABBCollider> _hits = new();

    private void Awake() => _col = GetComponent<AABBCollider>();

    private void Update()
    {
        float dt = Time.deltaTime;

        // Horizontal input
        _velocity.x = Input.GetAxisRaw("Horizontal") * _moveSpeed;

        // Jump only if standing on something -> no double jump
        if (Input.GetButtonDown("Jump") && _grounded)
            _velocity.y = _jumpSpeed;

        _velocity.y += _gravity * dt;
        _grounded = false; // re-earned below if we land on something

        MoveX(_velocity.x * dt);
        MoveY(_velocity.y * dt);

        CheckFallZone();
    }

    private void MoveX(float dx)
    {
        transform.position += new Vector3(dx, 0f, 0f);
        CollisionManager.Instance.GetOverlaps(_col, _hits);

        foreach (var other in _hits)
        {
            if (!IsSolid(other)) continue;

            // Moving right: stop at the left edge of the other. Moving left: stop at its right edge.
            float shift = dx > 0 ? other.min.x - _col.max.x
                                 : other.max.x - _col.min.x;
            transform.position += new Vector3(shift, 0f, 0f);
        }
    }

    private void MoveY(float dy)
    {
        transform.position += new Vector3(0f, dy, 0f);
        CollisionManager.Instance.GetOverlaps(_col, _hits);

        foreach (var other in _hits)
        {
            if (!IsSolid(other)) continue;

            if (dy <= 0f)
            {
                // Falling: land on top
                transform.position += new Vector3(0f, other.max.y - _col.min.y, 0f);
                _grounded = true;
            }
            else
            {
                // Rising: bonk head on the underside
                transform.position += new Vector3(0f, other.min.y - _col.max.y, 0f);
            }
            _velocity.y = 0f;
        }
    }

    private void CheckFallZone()
    {
        CollisionManager.Instance.GetOverlaps(_col, _hits);
        foreach (var other in _hits)
        {
            if (other.Type == ColliderType.FallZone)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
        }
    }

    private static bool IsSolid(AABBCollider c) =>
        c.Type == ColliderType.Ground || c.Type == ColliderType.Obstacle;
}