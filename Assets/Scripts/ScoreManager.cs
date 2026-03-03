using System.Collections;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public float Distance  { get; private set; }
    public int   Score     { get; private set; }
    public int   HighScore { get; private set; }
    public int   Combo     { get; private set; }

    public event System.Action<float> OnDistanceChanged;
    public event System.Action<int>   OnScoreChanged;
    public event System.Action<int>   OnComboChanged;

    /// <summary>Points awarded per frontflip (before combo multiplier).</summary>
    public int frontFlipPoints = 100;
    /// <summary>Points awarded per backflip (before combo multiplier).</summary>
    public int backFlipPoints = 150;
    /// <summary>Points for a partial flip (before combo multiplier, scaled by partialRatio).</summary>
    public int partialFlipPoints = 50;
    /// <summary>How many combo stacks a single flip landing adds.</summary>
    public int comboIncrement = 1;

    private const int MaxCombo = 10;
    private const string HighScoreKey = "HighScore";

    public Transform playerTransform;

    private float startX;
    private CarController carController;
    private int pendingScore = 0;
    private Coroutine pendingScoreCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        Combo = 1;
    }

    void Start()
    {
        if (playerTransform != null)
        {
            startX = playerTransform.position.x;
            carController = playerTransform.GetComponent<CarController>();
            if (carController != null)
            {
                carController.OnFlipCompleted += HandleFlipCompleted;
                carController.OnLanding       += HandleLanding;
                carController.OnCrash         += HandleCrash;
            }
        }
    }

    void OnDestroy()
    {
        if (carController != null)
        {
            carController.OnFlipCompleted -= HandleFlipCompleted;
            carController.OnLanding       -= HandleLanding;
            carController.OnCrash         -= HandleCrash;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        Distance = Mathf.Max(0f, playerTransform.position.x - startX);
        OnDistanceChanged?.Invoke(Distance);
    }

    /// <summary>Updates combo display each time a new full flip is completed mid-air.</summary>
    private void HandleFlipCompleted(int totalFlips)
    {
        Combo = Mathf.Min(totalFlips, MaxCombo);
        OnComboChanged?.Invoke(Combo);
    }

    /// <summary>Handles the landing event from CarController and awards score based on flips performed.</summary>
    private void HandleLanding(int frontFlips, int backFlips, float partialRatio)
    {
        int totalFlips = frontFlips + backFlips;

        if (totalFlips == 0 && partialRatio < 0.1f)
        {
            ResetCombo();
            return;
        }

        int points = (frontFlips * frontFlipPoints
                    + backFlips  * backFlipPoints
                    + Mathf.RoundToInt(partialRatio * partialFlipPoints)) * Mathf.Max(Combo, 1);

        // Defer by one frame so a crash on the same landing can cancel it.
        pendingScore = points;
        if (pendingScoreCoroutine != null) StopCoroutine(pendingScoreCoroutine);
        pendingScoreCoroutine = StartCoroutine(CommitScoreNextFrame());

        ResetCombo();
    }

    private IEnumerator CommitScoreNextFrame()
    {
        yield return null;

        Score += pendingScore;
        pendingScore = 0;
        pendingScoreCoroutine = null;

        OnScoreChanged?.Invoke(Score);

        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
        }
    }

    /// <summary>Cancels any pending score if the player crashes on the same landing.</summary>
    private void HandleCrash()
    {
        if (pendingScoreCoroutine != null)
        {
            StopCoroutine(pendingScoreCoroutine);
            pendingScoreCoroutine = null;
        }
        pendingScore = 0;
        ResetCombo();
    }

    /// <summary>Resets the combo multiplier back to 1 silently (no UI event).</summary>
    public void ResetCombo()
    {
        Combo = 1;
    }
}
