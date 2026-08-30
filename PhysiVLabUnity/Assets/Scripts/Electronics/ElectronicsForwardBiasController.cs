using UnityEngine;

public class ElectronicsForwardBiasController : MonoBehaviour
{
    public static ElectronicsForwardBiasController Instance { get; private set; }

    public bool IsValidated { get; private set; }
    public bool GlowScored { get; private set; }
    public bool IsCompleted => GlowScored;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Begin()
    {
        ElectronicsSwitchController.Instance?.ForceOff();
        ElectronicsUIManager.Instance?.SetInstruction("EXPERIMENT 1  •  FORWARD BIAS\nConnect the battery so that the diode is forward biased. Then turn ON the switch.");
        ElectronicsCircuitConnectionManager.Instance?.ScoreForwardIfReady();
    }

    public void MarkValidated() => IsValidated = true;

    public void OnSwitchOn()
    {
        var circuit = ElectronicsCircuitConnectionManager.Instance;
        if (circuit == null || !circuit.IsCircuitComplete())
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Complete the circuit before turning ON the switch.");
            ElectronicsSwitchController.Instance?.ForceOff();
            return;
        }
        if (!circuit.IsForwardBias())
        {
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ Incorrect battery polarity or diode direction.", "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
            ElectronicsBulbController.Instance?.TurnOff();
            return;
        }

        if (!IsValidated) circuit.ScoreForwardIfReady();
        ElectronicsBulbController.Instance?.TurnOn();
        if (!GlowScored)
        {
            GlowScored = true;
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.ForwardObservation, 5, false);
            ElectronicsFeedbackManager.Instance?.ShowMessage(
                "BULB STATUS: GLOWING\nCURRENT CAN FLOW\nFORWARD BIAS\n\nThe diode allows current to pass through the circuit.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        ElectronicsUIManager.Instance?.SetNextButtonVisible(true);
    }

    public void ResetState()
    {
        IsValidated = false;
        GlowScored = false;
    }
}
