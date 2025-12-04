using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float acceleration = 10f;
    public float deceleration = 0.1f;

    Rigidbody2D rb;

    Vector2 moveInputs;
    Vector2 lookDirection;
    Vector2 velocity;
    Vector2 smoothDampVelocity; // ← necesario para SmoothDamp
    
    Vector2 lastDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        moveInputs = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        if(moveInputs.sqrMagnitude != 0)
            lastDirection = Vector2.Lerp(lookDirection, moveInputs.normalized, Time.deltaTime * 15f);
        
        lookDirection =lastDirection;
    }

    private void FixedUpdate()
    {
        if (moveInputs.sqrMagnitude > 0.01f)
        {
            // Acelera
            velocity += moveInputs.normalized * (acceleration * Time.fixedDeltaTime);
            velocity = Vector2.ClampMagnitude(velocity, moveSpeed);
        }
        else
        {
            // Frenado suave
            velocity = Vector2.SmoothDamp(
                velocity, 
                Vector2.zero, 
                ref smoothDampVelocity, 
                deceleration
            );
        }

        rb.linearVelocity = velocity;

        rb.transform.up = Vector3.Lerp(
            rb.transform.up, 
            lookDirection, 
            Time.fixedDeltaTime * 10f
        );
    }
}