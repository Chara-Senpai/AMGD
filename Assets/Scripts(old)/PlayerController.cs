using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed;
    public float jumpHeight;
    public Transform groundCheckRayPoint;
    public LayerMask groundLayer;

    float horizontalSpeed, verticalSpeed;
    bool isGrounded;
    Vector2 movement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
        CheckJumping();
        ApplyMovement();
        
    }

    void CheckGround()
    {
        RaycastHit2D _hit = Physics2D.Raycast(groundCheckRayPoint.position, -Vector2.up, 0.2f, groundLayer);
        if(_hit.collider)
        {
            verticalSpeed = 0;
            isGrounded = true;
        }
        else
            verticalSpeed = Mathf.Lerp(verticalSpeed, -10, 1f * Time.deltaTime);
    }

    void ApplyMovement()
    {
        horizontalSpeed = Input.GetAxis("Horizontal");
        movement = new Vector2(horizontalSpeed, verticalSpeed);
        transform.Translate(movement * movementSpeed * Time.deltaTime); 
    }

    void CheckJumping()
    {
        if(isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            isGrounded = false;
            verticalSpeed = jumpHeight;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(groundCheckRayPoint.position, -Vector2.up * 0.2f);
    }
}
