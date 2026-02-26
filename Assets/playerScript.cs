using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour
{
    
    public float acceleration = 30f;
    public float maxSpeed = 15f;

   
    public float airTorque = 10f;

   
    public Transform frontWheel;
    public Transform backWheel;

    
    public GameObject carBody;       
    public ParticleSystem explosion; 
    public float explosionDuration = 2f; 

    
    public float crashRotationThreshold = 120f; 
    public string groundTag = "Ground";        

    [Header("Landing Boost")]
    public float landingBoostForce = 150f;
    public float minAirTimeForBoost = 0.2f; // Minimum seconds airborne before boost triggers

    private Rigidbody2D rb;
    private bool isDestroyed = false;
    private bool wasGrounded = true;
    private float airTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

       
        SetWheelKinematic(frontWheel);
        SetWheelKinematic(backWheel);
    }

    void FixedUpdate()
    {
        if (isDestroyed) return;

        // Auto move right
        if (rb.linearVelocity.x < maxSpeed)
            rb.AddForce(Vector2.right * acceleration);

        // Track airtime while off the ground
        if (!wasGrounded)
            airTime += Time.fixedDeltaTime;
    }

    void Update()
    {
        if (isDestroyed) return;

        HandleAirControl();
        RotateWheels();
    }

    void HandleAirControl()
    {
        if (!wasGrounded)
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
        if (frontWheel != null) frontWheel.Rotate(0, 0, -spinSpeed);
        if (backWheel != null) backWheel.Rotate(0, 0, -spinSpeed);
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.6f);
        return hit.collider != null && hit.collider.CompareTag(groundTag);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDestroyed) return;

        if (!collision.gameObject.CompareTag(groundTag)) return;

        // Apply landing boost if airborne long enough
        if (!wasGrounded && airTime >= minAirTimeForBoost)
            rb.AddForce(Vector2.right * landingBoostForce, ForceMode2D.Impulse);

        airTime = 0f;
        wasGrounded = true;

        float z = transform.eulerAngles.z;
        if (z > 180f) z -= 360f;

        if (Mathf.Abs(z) > crashRotationThreshold)
            Explode();
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(groundTag)) return;

        wasGrounded = false;
    }

    void Explode()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Show and play explosion
        if (explosion != null)
        {
            explosion.gameObject.SetActive(true);           // Ensure it's visible
            explosion.transform.parent = null;             // Detach from car
            explosion.transform.position = transform.position + Vector3.up * 0.5f;
            explosion.Play();
            StartCoroutine(FadeAndEndGame(explosionDuration));
        }

        // Hide car body
        if (carBody != null) carBody.SetActive(false);

        // Stop car physics
        rb.simulated = false;
        GetComponent<Collider2D>().enabled = false;

        // Detach and drop wheels
        DropWheel(frontWheel);
        DropWheel(backWheel);
    }

    IEnumerator FadeAndEndGame(float duration)
    {
        ParticleSystem.MainModule main = explosion.main;
        float startAlpha = main.startColor.color.a;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, timer / duration);
            main.startColor = new Color(main.startColor.color.r, main.startColor.color.g, main.startColor.color.b, alpha);
            yield return null;
        }

        // Stop the explosion and end the game
        explosion.Stop();
        GameOver();
    }

    void GameOver()
    {
        // Placeholder for end-game logic
        Debug.Log("Game Over!");
        // Example: SceneManager.LoadScene("GameOverScene");
    }

    void SetWheelKinematic(Transform wheel)
    {
        if (wheel == null) return;

        Rigidbody2D wrb = wheel.GetComponent<Rigidbody2D>();
        Collider2D wc = wheel.GetComponent<Collider2D>();

        if (wrb != null)
        {
            wrb.bodyType = RigidbodyType2D.Kinematic;
            wrb.gravityScale = 0f;
        }

        if (wc != null)
            wc.enabled = false; // <-- disable collider at start
    }

    void DropWheel(Transform wheel)
    {
        if (wheel == null) return;

        wheel.parent = null;

        Rigidbody2D wrb = wheel.GetComponent<Rigidbody2D>();
        Collider2D wc = wheel.GetComponent<Collider2D>();

        if (wc != null) wc.enabled = true;

        if (wrb != null)
        {
            wrb.bodyType = RigidbodyType2D.Dynamic;
            wrb.gravityScale = 1f;
            wrb.linearVelocity = Vector2.zero;
            wrb.angularVelocity = 0f;
            wrb.AddForce(Vector2.up * 200f);
        }

        wheel.position = new Vector3(wheel.position.x, wheel.position.y + 0.1f, wheel.position.z);
    }
}

