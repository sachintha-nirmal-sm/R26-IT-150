using UnityEngine;
using UnityEngine.UI;

public class ElectronicsSwitchController : MonoBehaviour
{
    public static ElectronicsSwitchController Instance { get; private set; }

    [SerializeField] private bool isOn;
    [SerializeField] private bool placed;

    private Image leverImage;
    private TMPro.TextMeshProUGUI label;
    private Button toggleButton;

    public bool IsPlaced => placed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(Image lever, TMPro.TextMeshProUGUI text, Button button)
    {
        leverImage = lever;
        label = text;
        toggleButton = button;
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(ToggleSwitch);
        }
        UpdateVisual();
    }

    public void SetPlaced(bool value)
    {
        placed = value;
        if (!value) isOn = false;
        UpdateVisual();
    }

    public bool IsOn() => isOn;

    public void TurnOn()
    {
        if (!CanOperate()) return;
        isOn = true;
        UpdateVisual();
        ElectronicsCircuitConnectionManager.Instance?.OnSwitchChanged(true);
    }

    public void TurnOff()
    {
        isOn = false;
        UpdateVisual();
        ElectronicsCircuitConnectionManager.Instance?.OnSwitchChanged(false);
    }

    public void ToggleSwitch()
    {
        if (isOn)
        {
            TurnOff();
            return;
        }
        TurnOn();
    }

    public bool CanOperate()
    {
        var step = ElectronicsPracticalManager.Instance != null
            ? ElectronicsPracticalManager.Instance.CurrentStep
            : ElectronicsPracticalStep.Introduction;
        var circuit = ElectronicsCircuitConnectionManager.Instance;
        if (circuit == null || !circuit.IsCircuitComplete())
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Complete the circuit before turning ON the switch.");
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            return false;
        }
        if (step == ElectronicsPracticalStep.CircuitSetup)
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Press NEXT STEP, then turn ON the switch for the forward-bias experiment.");
            return false;
        }
        return true;
    }

    public void ResetSwitch()
    {
        isOn = false;
        placed = false;
        UpdateVisual();
    }

    public void ForceOff()
    {
        isOn = false;
        UpdateVisual();
        ElectronicsBulbController.Instance?.TurnOff();
    }

    private void UpdateVisual()
    {
        if (leverImage != null)
            leverImage.sprite = ElectronicsIconFactory.GetNamed(isOn ? "switch-on" : "switch-off");
        if (label != null)
            label.text = isOn ? "SWITCH: ON" : "SWITCH: OFF";
        if (toggleButton != null)
        {
            var txt = toggleButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (txt != null) txt.text = isOn ? "TURN OFF" : "TURN ON";
        }
    }
}
