using UnityEngine;




using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour
{
    [Header("Auto Drive")]
    public float acceleration = 30f;
    public float maxSpeed = 15f;

    [Header("Air Control")]
    public float airTorque = 10f;

    [Header("Wheels")]
    public Transform frontWheel;
    public Transform backWheel;

    [Header("Body & Explosion")]
    public GameObject carBody;       // Drag the Body child object here
    public ParticleSystem explosion; // Drag the explosion ParticleSystem here
    public float explosionDuration = 2f; // How long the explosion should last before ending

    [Header("Crash Settings")]
    public float crashRotationThreshold = 120f; // Degrees to detect upside-down
    public string groundTag = "Ground";         // Tag for the Tilemap ground

    private Rigidbody2D rb;
    private bool isDestroyed = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Make wheels kinematic at start so they don't interfere with ground
        SetWheelKinematic(frontWheel);
        SetWheelKinematic(backWheel);
    }

    void FixedUpdate()
    {
        if (isDestroyed) return;

        // Auto move right
        if (rb.linearVelocity.x < maxSpeed)
            rb.AddForce(Vector2.right * acceleration);
    }

    void Update()
    {
        if (isDestroyed) return;

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

        float z = transform.eulerAngles.z;
        if (z > 180f) z -= 360f;

        if (Mathf.Abs(z) > crashRotationThreshold)
        {
            Explode();
        }
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

