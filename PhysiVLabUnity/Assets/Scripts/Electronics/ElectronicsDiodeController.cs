using UnityEngine;
using UnityEngine.UI;

public class ElectronicsDiodeController : MonoBehaviour
{
    public static ElectronicsDiodeController Instance { get; private set; }

    [SerializeField] private bool flipped;
    [SerializeField] private bool placed;
    [SerializeField] private bool forwardBiased;
    [SerializeField] private bool reverseBiased;

    private Image icon;
    private TMPro.TextMeshProUGUI anodeLabel;
    private TMPro.TextMeshProUGUI cathodeLabel;
    private TMPro.TextMeshProUGUI statusLabel;

    public bool IsPlaced => placed;
    public bool IsFlipped => flipped;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(Image img, TMPro.TextMeshProUGUI anode, TMPro.TextMeshProUGUI cathode, TMPro.TextMeshProUGUI status)
    {
        icon = img;
        anodeLabel = anode;
        cathodeLabel = cathode;
        statusLabel = status;
        UpdateVisual();
    }

    public void SetPlaced(bool value)
    {
        placed = value;
        RefreshBias();
    }

    public void FlipOrientation()
    {
        flipped = !flipped;
        RefreshBias();
        ElectronicsFeedbackManager.Instance?.ShowInstruction(flipped
            ? "Diode reversed. Anode and cathode have swapped sides."
            : "Diode returned to the standard anode → cathode direction.");
    }

    public void SetForwardBias()
    {
        forwardBiased = true;
        reverseBiased = false;
        UpdateVisual();
    }

    public void SetReverseBias()
    {
        forwardBiased = false;
        reverseBiased = true;
        UpdateVisual();
    }

    public bool IsForwardBiased() => forwardBiased;
    public bool IsReverseBiased() => reverseBiased;

    public void ResetDiode()
    {
        flipped = false;
        placed = false;
        forwardBiased = false;
        reverseBiased = false;
        UpdateVisual();
    }

    public void RefreshBias()
    {
        var circuit = ElectronicsCircuitConnectionManager.Instance;
        if (circuit != null && circuit.IsForwardBias()) SetForwardBias();
        else if (circuit != null && circuit.IsReverseBias()) SetReverseBias();
        else
        {
            forwardBiased = false;
            reverseBiased = false;
            UpdateVisual();
        }
    }

    public void UpdateVisual()
    {
        if (icon != null)
            icon.sprite = ElectronicsIconFactory.GetNamed(flipped ? "diode-rev" : "diode");
        if (anodeLabel != null)
            anodeLabel.text = flipped ? "Cathode  |<" : "Anode  >|";
        if (cathodeLabel != null)
            cathodeLabel.text = flipped ? ">|  Anode" : "|<  Cathode";
        if (statusLabel != null)
        {
            if (forwardBiased) statusLabel.text = "DIODE: FORWARD BIAS  •  current can flow";
            else if (reverseBiased) statusLabel.text = "DIODE: REVERSE BIAS  •  current blocked";
            else statusLabel.text = "IN4001 diode   Anode → |>| → Cathode";
        }
    }
}
