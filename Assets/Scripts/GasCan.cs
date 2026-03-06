using UnityEngine;

public class GasCan : MonoBehaviour
{
    private const string PlayerTag = "Player";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerTag)) return;
        if (GasManager.Instance == null) return;

        GasManager.Instance.AddGas(GasManager.Instance.canRefillAmount);
        gameObject.SetActive(false);
    }
}
