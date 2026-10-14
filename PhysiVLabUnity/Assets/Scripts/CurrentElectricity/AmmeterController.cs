using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmeterController : MonoBehaviour
{
    public static AmmeterController Instance { get; private set; }

    [SerializeField] private float displayedCurrent;
    [SerializeField] private TextMeshProUGUI readingText;
    [SerializeField] private RectTransform pointer;
    [SerializeField] private bool active;

    public float DisplayedCurrent => displayedCurrent;

    private void Awake() => Instance = this;

    public void Bind(TextMeshProUGUI reading, RectTransform needle)
    {
        readingText = reading;
        pointer = needle;
        Show(0f, false);
    }

    public void Show(float current, bool isActive)
    {
        displayedCurrent = current;
        active = isActive;
        if (readingText != null)
            readingText.text = isActive ? $"{current:0.00} A" : "0.00 A";
        UpdatePointer(isActive ? current : 0f);
    }

    public void ResetMeter() => Show(0f, false);

    private void Update()
    {
        if (!active || pointer == null) return;
        float wobble = Mathf.Sin(Time.time * 8f) * 1.2f;
        float angle = CurrentToAngle(displayedCurrent) + wobble;
        pointer.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void UpdatePointer(float current)
    {
        if (pointer == null) return;
        pointer.localRotation = Quaternion.Euler(0f, 0f, CurrentToAngle(current));
    }

    private float CurrentToAngle(float current)
    {
        float max = 0.5f;
        float t = Mathf.Clamp01(Mathf.Abs(current) / max);
        return Mathf.Lerp(50f, -50f, t);
    }
}
