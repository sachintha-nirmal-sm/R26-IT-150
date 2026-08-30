using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkEnergyPowerChallengeManager : MonoBehaviour
{
    public static WorkEnergyPowerChallengeManager Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_InputField answerInput;
    [SerializeField] private Button submitBtn;
    [SerializeField] private Button skipBtn;
    [SerializeField] private TextMeshProUGUI promptText;

    private bool completed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(GameObject challengePanel, TMP_InputField input, Button submit, Button skip, TextMeshProUGUI prompt)
    {
        panel = challengePanel;
        answerInput = input;
        submitBtn = submit;
        skipBtn = skip;
        promptText = prompt;
        if (submitBtn != null)
        {
            submitBtn.onClick.RemoveAllListeners();
            submitBtn.onClick.AddListener(Submit);
        }
        if (skipBtn != null)
        {
            skipBtn.onClick.RemoveAllListeners();
            skipBtn.onClick.AddListener(Skip);
        }
        if (panel != null) panel.SetActive(false);
    }

    public void ShowChallenge()
    {
        completed = false;
        if (panel != null) panel.SetActive(true);
        if (promptText != null)
        {
            promptText.text =
                "OPTIONAL CHALLENGE — POWER\n\n" +
                "Power is the rate at which work is done.\nP = W / t\n\n" +
                "If 4.9 J of work is done in 2 seconds, calculate the power.\n\n" +
                "Enter your answer in watts (W).";
        }
        if (answerInput != null) answerInput.text = "";
        WorkEnergyUIManager.Instance?.SetNextButtonVisible(true);
        WorkEnergyUIManager.Instance?.SetNextButtonLabel("SKIP / NEXT");
    }

    public void ResetChallenge()
    {
        completed = false;
        if (panel != null) panel.SetActive(false);
        if (answerInput != null) answerInput.text = "";
    }

    private void Submit()
    {
        if (completed) return;
        float value = 0f;
        string raw = answerInput != null ? answerInput.text.Trim() : "";
        if (!float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
            float.TryParse(raw, out value);

        if (Mathf.Abs(value - 2.45f) <= 0.05f)
        {
            completed = true;
            WorkEnergyScoreManager.Instance?.AddScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("✓ P = W/t = 4.9 / 2 = 2.45 W");
            WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
        }
        else
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Use P = W / t. Work = 4.9 J and time = 2 s.");
        }
    }

    private void Skip()
    {
        WorkEnergyPowerExperimentManager.Instance?.AdvanceStep();
    }
}
