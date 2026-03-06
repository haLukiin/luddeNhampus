using UnityEngine;

public class GasManager : MonoBehaviour
{
    public static GasManager Instance { get; private set; }

    [Header("Gas Settings")]
    public float maxGas = 100f;
    public float depletionRate = 5f;
    public float canRefillAmount = 40f;

    public float CurrentGas { get; private set; }

    /// <summary>Fired whenever gas changes. Passes normalized value 0-1.</summary>
    public event System.Action<float> OnGasChanged;

    /// <summary>Fired when the car runs out of gas.</summary>
    public event System.Action OnGasEmpty;

    private bool isEmpty = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CurrentGas = maxGas;
    }

    void Update()
    {
        if (isEmpty) return;

        CurrentGas -= depletionRate * Time.deltaTime;
        CurrentGas  = Mathf.Clamp(CurrentGas, 0f, maxGas);

        OnGasChanged?.Invoke(CurrentGas / maxGas);

        if (CurrentGas <= 0f)
        {
            isEmpty = true;
            OnGasEmpty?.Invoke();
        }
    }

    /// <summary>Adds gas from a pickup, clamped to the max tank capacity.</summary>
    public void AddGas(float amount)
    {
        if (isEmpty) return;

        CurrentGas = Mathf.Min(CurrentGas + amount, maxGas);
        OnGasChanged?.Invoke(CurrentGas / maxGas);
    }
}
