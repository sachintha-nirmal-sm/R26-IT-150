using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BulbController : MonoBehaviour
{
    public static BulbController Instance { get; private set; }

    [SerializeField] private float bulbResistance = 10f;
    [SerializeField] private float brightness;
    [SerializeField] private bool isOn;
    [SerializeField] private float voltage;
    [SerializeField] private float current;
    [SerializeField] private float power;
    [SerializeField] private Image glowImage;
    [SerializeField] private Image bodyImage;
    [SerializeField] private TextMeshProUGUI statusText;

    public float BulbResistance => bulbResistance;
    public float Brightness => brightness;
    public bool IsOn => isOn;
    public float Voltage => voltage;
    public float Current => current;
    public float Power => power;

    private void Awake()
    {
        Instance = this;
    }

    public void Bind(Image glow, Image body, TextMeshProUGUI status, float resistance)
    {
        glowImage = glow;
        bodyImage = body;
        statusText = status;
        bulbResistance = resistance;
        TurnOff();
    }

    public void SetResistance(float value) => bulbResistance = Mathf.Max(0.01f, value);

    public void ApplyElectricalState(float v, float i, float p)
    {
        voltage = v;
        current = i;
        power = p;
        SetBrightness(CalculateBrightness(p));
        if (brightness <= 0.02f) TurnOff();
        else TurnOn();
    }

    public void TurnOn()
    {
        isOn = brightness > 0.02f;
        UpdateVisual();
    }

    public void TurnOff()
    {
        isOn = false;
        brightness = 0f;
        voltage = 0f;
        current = 0f;
        power = 0f;
        UpdateVisual();
    }

    public void SetBrightness(float value)
    {
        brightness = Mathf.Clamp01(value);
        UpdateVisual();
    }

    public float CalculateBrightness(float electricalPower)
    {
        float highPower = CircuitCalculationManager.Instance != null
            ? CircuitCalculationManager.Instance.HighPowerReference
            : 0.9f;
        if (electricalPower <= 0.02f) return 0f;
        return Mathf.Clamp01(electricalPower / Mathf.Max(0.05f, highPower));
    }

    public BrightnessLevel GetBrightnessLevel()
    {
        if (!isOn || brightness < 0.05f) return BrightnessLevel.Off;
        if (brightness < 0.35f) return BrightnessLevel.Dim;
        if (brightness < 0.7f) return BrightnessLevel.Medium;
        return BrightnessLevel.High;
    }

    public string BrightnessLabel()
    {
        switch (GetBrightnessLevel())
        {
            case BrightnessLevel.High: return "High";
            case BrightnessLevel.Medium: return "Medium";
            case BrightnessLevel.Dim: return "Dim";
            default: return "OFF";
        }
    }

    private void UpdateVisual()
    {
        if (glowImage != null)
        {
            glowImage.enabled = isOn;
            float a = Mathf.Lerp(0.15f, 0.95f, brightness);
            glowImage.color = new Color(1f, 0.92f, 0.35f, a);
            glowImage.transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.35f, brightness);
        }
        if (bodyImage != null)
        {
            Color off = new Color(0.72f, 0.74f, 0.78f);
            Color on = Color.Lerp(new Color(1f, 0.85f, 0.35f), new Color(1f, 0.97f, 0.55f), brightness);
            bodyImage.color = isOn ? on : off;
        }
        if (statusText != null)
            statusText.text = isOn ? "BULB STATUS: ON\nBRIGHTNESS: " + BrightnessLabel() : "BULB STATUS: OFF\nBRIGHTNESS: OFF";
    }
}
