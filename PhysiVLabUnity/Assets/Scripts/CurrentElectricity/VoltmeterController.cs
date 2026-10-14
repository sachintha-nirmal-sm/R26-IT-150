using TMPro;
using UnityEngine;

public class VoltmeterController : MonoBehaviour
{
    public static VoltmeterController Instance { get; private set; }

    [SerializeField] private float displayedVoltage;
    [SerializeField] private TextMeshProUGUI readingText;
    [SerializeField] private RectTransform pointer;
    [SerializeField] private bool active;

    public float DisplayedVoltage => displayedVoltage;

    private void Awake() => Instance = this;

    public void Bind(TextMeshProUGUI reading, RectTransform needle)
    {
        readingText = reading;
        pointer = needle;
        Show(0f, false);
    }

    public void Show(float voltage, bool isActive)
    {
        displayedVoltage = voltage;
        active = isActive;
        if (readingText != null)
            readingText.text = isActive ? $"{voltage:0.00} V" : "0.00 V";
        if (pointer != null)
        {
            float t = Mathf.Clamp01(Mathf.Abs(voltage) / 4f);
            pointer.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(50f, -50f, t));
        }
    }

    public void ResetMeter() => Show(0f, false);

    private void Update()
    {
        if (!active || pointer == null) return;
        float wobble = Mathf.Sin(Time.time * 7f) * 1.1f;
        float t = Mathf.Clamp01(Mathf.Abs(displayedVoltage) / 4f);
        pointer.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(50f, -50f, t) + wobble);
    }
}
