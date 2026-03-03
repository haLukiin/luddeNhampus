using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Forward Follow")]
    public float forwardOffset = 3f;
    public float heightOffset = 1.5f;
    public float smoothSpeed = 3f;

    [Header("Dynamic Zoom")]
    public float minZoom = 5f;
    public float maxZoom = 8f;
    public float zoomSpeed = 2f;

    private float highestXPosition;
    private Camera cam;
    private Rigidbody2D targetRb;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetRb = target.GetComponent<Rigidbody2D>();
        highestXPosition = transform.position.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        FollowForwardOnly();
        HandleZoom();
    }

    void FollowForwardOnly()
    {
        float targetX = target.position.x + forwardOffset;

        // Only move forward
        if (targetX > highestXPosition)
        {
            highestXPosition = targetX;
        }

        Vector3 desiredPosition = new Vector3(
            highestXPosition,
            target.position.y + heightOffset,
            -10f
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    void HandleZoom()
    {
        float speed = targetRb.linearVelocity.magnitude;

        float targetZoom = Mathf.Lerp(minZoom, maxZoom, speed / 20f);
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetZoom,
            zoomSpeed * Time.deltaTime
        );
    }
}
