using UnityEngine;

public class wheelScript2 : MonoBehaviour
{

    public Rigidbody2D carRb;
    public float rotationFactor = 50f;

    void Update()
    {
        if (carRb != null)
        {
            float spin = carRb.linearVelocity.magnitude * rotationFactor;
            transform.Rotate(0, 0, -spin * Time.deltaTime);
        }
    }


}
