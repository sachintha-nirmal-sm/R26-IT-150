using TMPro;
using UnityEngine;

public class PowerEnergyTimerController : MonoBehaviour
{
    public static PowerEnergyTimerController Instance { get; private set; }

    private TextMeshProUGUI timeText;
    private float elapsed;
    private float targetDuration;
    private bool running;
    private bool finished;

    public float Elapsed => elapsed;
    public float TargetDuration => targetDuration;
    public bool IsRunning => running;
    public bool IsFinished => finished;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(TextMeshProUGUI label)
    {
        timeText = label;
        Refresh();
    }

    public void SetDuration(float seconds)
    {
        targetDuration = Mathf.Max(1f, seconds);
        ResetTimer();
        PowerEnergyFeedbackManager.Instance?.ShowInstruction($"Operating time set to {targetDuration:0} seconds. Press START, then wait until the timer stops.");
    }

    public void StartTimer()
    {
        var app = PowerEnergyApplianceController.Instance;
        if (app == null || app.Current == null)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Select an appliance first.");
            return;
        }
        if (!app.IsOn)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Turn the appliance ON before starting the timer.");
            return;
        }
        if (targetDuration < 1f)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Choose 10 s, 30 s or 60 s first.");
            return;
        }
        running = true;
        finished = false;
    }

    public void StopTimer()
    {
        running = false;
    }

    public void ResetTimer()
    {
        running = false;
        finished = false;
        elapsed = 0f;
        Refresh();
    }

    private void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        if (elapsed >= targetDuration)
        {
            elapsed = targetDuration;
            running = false;
            finished = true;
            var app = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.Current : null;
            if (app != null) app.operatingTime = targetDuration;
            PowerEnergyFeedbackManager.Instance?.ShowInstruction($"Time reached {targetDuration:0} s. Now calculate electrical energy using E = Pt.");
            PowerEnergyUIManager.Instance?.OnTimerFinished();
        }
        Refresh();
    }

    private void Refresh()
    {
        int total = Mathf.FloorToInt(elapsed);
        int m = total / 60;
        int s = total % 60;
        if (timeText != null) timeText.text = $"{m:00}:{s:00}";
    }
}
