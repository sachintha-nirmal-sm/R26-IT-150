using UnityEngine;
using UnityEngine.UI;

public class ElectronicsBatteryController : MonoBehaviour
{
    public static ElectronicsBatteryController Instance { get; private set; }

    [SerializeField] private bool reversed;
    [SerializeField] private bool connected = true;
    [SerializeField] private bool placed;

    private RectTransform visual;
    private Image icon;
    private TMPro.TextMeshProUGUI polarityLabel;
    private TMPro.TextMeshProUGUI voltageLabel;

    public bool IsPlaced => placed;
    public bool IsConnected => connected && placed;
    public float Voltage => 3f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform vis, Image img, TMPro.TextMeshProUGUI polarity, TMPro.TextMeshProUGUI voltage)
    {
        visual = vis;
        icon = img;
        polarityLabel = polarity;
        voltageLabel = voltage;
        UpdateVisual();
    }

    public void SetPlaced(bool value)
    {
        placed = value;
        if (value) connected = true;
        UpdateVisual();
    }

    public void ReversePolarity()
    {
        reversed = true;
        UpdateVisual();
        ElectronicsFeedbackManager.Instance?.ShowInstruction("Battery polarity reversed.");
    }

    public void ResetPolarity()
    {
        reversed = false;
        UpdateVisual();
    }

    public bool IsNormalPolarity() => !reversed;
    public bool IsReversedPolarity() => reversed;

    public void Disconnect()
    {
        connected = false;
        UpdateVisual();
        ElectronicsCircuitConnectionManager.Instance?.RefreshState();
    }

    public void Reconnect()
    {
        if (!placed)
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Place the battery on the circuit board first.");
            return;
        }
        connected = true;
        UpdateVisual();
        ElectronicsCircuitConnectionManager.Instance?.RefreshState();
    }

    public void ResetBattery()
    {
        reversed = false;
        connected = true;
        placed = false;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (visual != null)
            visual.localRotation = Quaternion.Euler(0f, 0f, reversed ? 180f : 0f);
        if (icon != null)
            icon.sprite = ElectronicsIconFactory.GetNamed(reversed ? "battery-rev" : "battery");
        if (polarityLabel != null)
        {
            polarityLabel.text = reversed
                ? "Battery:  −  toward diode   +  toward bulb"
                : "Battery:  +  toward switch / diode   −  return";
        }
        if (voltageLabel != null)
            voltageLabel.text = placed ? "Battery: 1.5 V + 1.5 V = 3 V" : "Battery: two 1.5 V dry cells";
        if (icon != null)
            icon.color = connected ? Color.white : new Color(1f, 1f, 1f, 0.35f);
    }
}
