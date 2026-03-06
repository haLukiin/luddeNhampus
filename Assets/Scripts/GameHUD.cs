using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public TextMeshProUGUI distanceLabel;
    public TextMeshProUGUI scoreLabel;
    public TextMeshProUGUI highScoreLabel;
    public TextMeshProUGUI comboLabel;
    public float comboFadeDuration = 1.5f;

    [Header("Gas")]
    public Image gasFillImage;

    private Coroutine comboFadeCoroutine;
    private CarController carController;

    void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnDistanceChanged += OnDistanceChanged;
            ScoreManager.Instance.OnScoreChanged    += OnScoreChanged;
            ScoreManager.Instance.OnComboChanged    += OnComboChanged;

            if (ScoreManager.Instance.playerTransform != null)
            {
                carController = ScoreManager.Instance.playerTransform.GetComponent<CarController>();
                if (carController != null)
                    carController.OnCrash += OnCrash;
            }
        }

        if (distanceLabel  != null) distanceLabel.text  = "0 m";
        if (scoreLabel     != null) scoreLabel.text     = "Score: 0";
        if (highScoreLabel != null) highScoreLabel.text = $"Best: {ScoreManager.Instance?.HighScore}";
        if (comboLabel     != null) comboLabel.alpha    = 0f;
        if (gasFillImage   != null) gasFillImage.fillAmount = 1f;

        if (GasManager.Instance != null)
            GasManager.Instance.OnGasChanged += OnGasChanged;
    }

    void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnDistanceChanged -= OnDistanceChanged;
            ScoreManager.Instance.OnScoreChanged    -= OnScoreChanged;
            ScoreManager.Instance.OnComboChanged    -= OnComboChanged;
        }

        if (carController != null)
            carController.OnCrash -= OnCrash;

        if (GasManager.Instance != null)
            GasManager.Instance.OnGasChanged -= OnGasChanged;
    }

    private void OnDistanceChanged(float distance)
    {
        if (distanceLabel != null)
            distanceLabel.text = $"{distance:F0} m";
    }

    private void OnScoreChanged(int score)
    {
        if (scoreLabel != null)
            scoreLabel.text = $"Score: {score}";

        if (highScoreLabel != null)
            highScoreLabel.text = $"Best: {ScoreManager.Instance.HighScore}";
    }

    private void OnComboChanged(int combo)
    {
        if (comboLabel == null || combo < 1) return;

        comboLabel.text  = $"x{combo} COMBO!";
        comboLabel.alpha = 1f;

        if (comboFadeCoroutine != null)
            StopCoroutine(comboFadeCoroutine);

        comboFadeCoroutine = StartCoroutine(FadeComboLabel());
    }

    private void OnGasChanged(float normalizedGas)
    {
        if (gasFillImage == null) return;

        gasFillImage.fillAmount = normalizedGas;
        gasFillImage.color = normalizedGas < 0.25f
            ? Color.Lerp(Color.red, Color.yellow, normalizedGas / 0.25f)
            : Color.green;
    }

    private IEnumerator FadeComboLabel()
    {
        yield return new WaitForSeconds(0.5f);

        float timer = 0f;
        while (timer < comboFadeDuration)
        {
            timer += Time.deltaTime;
            comboLabel.alpha = Mathf.Lerp(1f, 0f, timer / comboFadeDuration);
            yield return null;
        }

        comboLabel.alpha = 0f;
    }

    private void OnCrash()
    {
        if (comboLabel == null) return;

        if (comboFadeCoroutine != null)
            StopCoroutine(comboFadeCoroutine);

        comboLabel.alpha = 0f;
    }
}
