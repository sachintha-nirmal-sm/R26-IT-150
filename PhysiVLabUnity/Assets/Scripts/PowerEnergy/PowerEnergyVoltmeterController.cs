using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerEnergyVoltmeterController : MonoBehaviour
{
    public static PowerEnergyVoltmeterController Instance { get; private set; }

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

    public void SetLiveValue(float voltage)
    {
        liveValue = voltage;
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
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nTurn the appliance ON before taking a voltage reading.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
            return false;
        }
        if (readingTaken) return true;
        takenValue = liveValue;
        readingTaken = true;
        app.Current.studentVoltage = takenValue;
        PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Voltage, 5, false);
        PowerEnergyFeedbackManager.Instance?.ShowMessage($"✓ CORRECT\nPotential difference = {takenValue:0.0} V", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        PowerEnergyUIManager.Instance?.UpdateLiveReadings();
        return true;
    }

    public bool CheckEnteredReading(float entered)
    {
        if (!readingTaken) TakeReading();
        bool ok = Mathf.Abs(entered - liveValue) <= 1.0f;
        if (ok)
        {
            PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Voltage, 5, false);
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✓ CORRECT\nThe voltmeter reads 230.0 V.", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nRead the voltmeter carefully. The supply is 230.0 V.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
        }
        return ok;
    }

    private void Update()
    {
        if (needle == null) return;
        float target = Mathf.Lerp(-48f, 48f, Mathf.Clamp01(liveValue / 250f));
        var e = needle.localEulerAngles;
        float z = Mathf.LerpAngle(e.z, -target, Time.deltaTime * 6f);
        needle.localEulerAngles = new Vector3(0, 0, z);
    }

    private void Refresh()
    {
        if (readingText != null)
            readingText.text = liveValue <= 0.001f ? "--- V" : $"{liveValue:0.0} V";
    }
}
