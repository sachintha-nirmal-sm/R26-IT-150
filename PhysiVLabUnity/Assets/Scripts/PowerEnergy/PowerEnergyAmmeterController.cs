using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerEnergyAmmeterController : MonoBehaviour
{
    public static PowerEnergyAmmeterController Instance { get; private set; }

    private TextMeshProUGUI readingText;
    private RectTransform needle;
    private float liveValue;
    private bool readingTaken;
    private float takenValue;

    public float LiveValue => liveValue;
    public bool ReadingTaken => readingTaken;
    public float TakenValue => takenValue;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(TextMeshProUGUI reading, RectTransform needleTransform)
    {
        readingText = reading;
        needle = needleTransform;
        Refresh();
    }

    public void SetLiveValue(float current)
    {
        liveValue = current;
        Refresh();
    }

    public void ResetMeter()
    {
        liveValue = 0f;
        readingTaken = false;
        takenValue = 0f;
        Refresh();
    }

    public bool TakeReading()
    {
        var app = PowerEnergyApplianceController.Instance;
        if (app == null || app.Current == null)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Select an appliance first.");
            return false;
        }
        if (!app.IsOn)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nTurn the appliance ON before taking a current reading.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
            return false;
        }
        if (readingTaken) return true;
        takenValue = liveValue;
        readingTaken = true;
        app.Current.studentCurrent = takenValue;
        PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Current, 5, false);
        PowerEnergyFeedbackManager.Instance?.ShowMessage($"✓ CORRECT\nCurrent = {takenValue:0.000} A", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        PowerEnergyUIManager.Instance?.UpdateLiveReadings();
        return true;
    }

    public bool CheckEnteredReading(float entered)
    {
        if (!readingTaken) TakeReading();
        bool ok = Mathf.Abs(entered - liveValue) <= Mathf.Max(0.01f, 0.08f * liveValue);
        if (ok)
        {
            PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Current, 5, false);
            PowerEnergyFeedbackManager.Instance?.ShowMessage($"✓ CORRECT\nThe ammeter reads {liveValue:0.000} A.", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowMessage($"✗ INCORRECT\nRead the ammeter. Current = {liveValue:0.000} A.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        return ok;
    }

    private void Update()
    {
        if (needle == null) return;
        float target = Mathf.Lerp(-48f, 48f, Mathf.Clamp01(liveValue / 10f));
        var e = needle.localEulerAngles;
        float z = Mathf.LerpAngle(e.z, -target, Time.deltaTime * 6f);
        needle.localEulerAngles = new Vector3(0, 0, z);
    }

    private void Refresh()
    {
        if (readingText != null)
            readingText.text = liveValue <= 0.0001f ? "--- A" : $"{liveValue:0.000} A";
    }
}
