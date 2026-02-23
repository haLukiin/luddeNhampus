using UnityEngine;

public class playerScript : MonoBehaviour
{
    [Header("Auto Drive")]
    public float acceleration = 30f;
    public float maxSpeed = 15f;

    [Header("Air Control")]
    public float airTorque = 10f;

    [Header("Wheels")]
    public Transform frontWheel;
    public Transform backWheel;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Auto move right
        if (rb.linearVelocity.x < maxSpeed)
        {
            rb.AddForce(Vector2.right * acceleration);
        }
    }

    void Update()
    {
        HandleAirControl();
        RotateWheels();
    }

    void HandleAirControl()
    {
        if (!IsGrounded())
        {
            if (Input.GetKey(KeyCode.A))
                rb.AddTorque(airTorque);

            if (Input.GetKey(KeyCode.D))
                rb.AddTorque(-airTorque);
        }
    }

    void RotateWheels()
    {
        float spinSpeed = rb.linearVelocity.x * 50f * Time.deltaTime;

        if (frontWheel != null)
            frontWheel.Rotate(0, 0, -spinSpeed);

        if (backWheel != null)
            backWheel.Rotate(0, 0, -spinSpeed);
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f);
        return hit.collider != null;
    }
}
