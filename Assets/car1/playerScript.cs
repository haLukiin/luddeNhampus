using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("Gravity")]
    public float groundedGravityScale = 4f;
    public float airGravityScale = 2f;

    [Header("Landing Boost")]
    public float landingBoostForce = 80f;
    public float minAirTimeForBoost = 0.2f;

    [Header("Flip Scoring")]
    public float frontFlipBoostBonus = 15f;
    public float backFlipBoostBonus = 10f;
    public float partialFlipBoostBonus = 5f;

    /// <summary>Fired on landing. frontFlips/backFlips are full 360° rotations; partialRatio is 0–1 of the remaining arc.</summary>
    public event System.Action<int, int, float> OnLanding;

    /// <summary>Fired each time a new full flip is completed in the air. Passes the running total flip count.</summary>
    public event System.Action<int> OnFlipCompleted;

    /// <summary>Fired when the car crashes and explodes.</summary>
    public event System.Action OnCrash;

    private Rigidbody2D rb;
    private bool isDestroyed = false;
    private bool wasGrounded = true;
    private float airTime = 0f;
    private float airRotationAccumulator = 0f;
    private int lastFlipCount = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = groundedGravityScale;
        SetWheelKinematic(frontWheel);
        SetWheelKinematic(backWheel);
    }

    void FixedUpdate()
    {
        if (isDestroyed) return;

        if (rb.linearVelocity.x < maxSpeed)
            rb.AddForce(Vector2.right * acceleration);

        if (!wasGrounded)
        {
            airTime += Time.fixedDeltaTime;
            airRotationAccumulator += rb.angularVelocity * Time.fixedDeltaTime;

            int currentFlips = Mathf.FloorToInt(Mathf.Abs(airRotationAccumulator) / 360f);
            if (currentFlips > lastFlipCount)
            {
                lastFlipCount = currentFlips;
                OnFlipCompleted?.Invoke(currentFlips);
            }
        }

        HandleAirControl();
    }

    void Update()
    {
        if (isDestroyed) return;
    }

    void HandleAirControl()
    {
        if (!wasGrounded)
        {
            float input = 0f;
            if (Keyboard.current.aKey.isPressed) input =  1f;
            if (Keyboard.current.dKey.isPressed) input = -1f;

            if (input != 0f)
                rb.AddTorque(input * airTorque);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDestroyed) return;
        if (!collision.gameObject.CompareTag(groundTag)) return;

        float z = transform.eulerAngles.z;
        if (z > 180f) z -= 360f;
        bool isCrash = Mathf.Abs(z) > crashRotationThreshold;

        float totalDegrees = Mathf.Abs(airRotationAccumulator);
        int fullFlips = Mathf.FloorToInt(totalDegrees / 360f);
        float partialRatio = (totalDegrees % 360f) / 360f;
        int frontFlips = airRotationAccumulator < 0 ? fullFlips : 0;
        int backFlips  = airRotationAccumulator > 0 ? fullFlips : 0;

        airRotationAccumulator = 0f;
        airTime = 0f;
        wasGrounded = true;

        if (isCrash)
        {
            Explode();
            return;
        }        OnLanding?.Invoke(frontFlips, backFlips, partialRatio);

        if (airTime >= minAirTimeForBoost)
            rb.AddForce(Vector2.right * landingBoostForce, ForceMode2D.Impulse);

        rb.gravityScale = groundedGravityScale;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(groundTag)) return;
        wasGrounded = false;
        airRotationAccumulator = 0f;
        lastFlipCount = 0;
        rb.gravityScale = airGravityScale;
    }

    void Explode()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        OnCrash?.Invoke();

        if (explosion != null)
        {
            explosion.gameObject.SetActive(true);
            explosion.transform.parent = null;
            explosion.transform.position = transform.position + Vector3.up * 0.5f;
            explosion.Play();
            StartCoroutine(FadeAndEndGame(explosionDuration));
        }

        if (carBody != null) carBody.SetActive(false);

        rb.simulated = false;
        GetComponent<Collider2D>().enabled = false;

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

        explosion.Stop();
        GameOver();
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
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
            wc.enabled = false;
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
