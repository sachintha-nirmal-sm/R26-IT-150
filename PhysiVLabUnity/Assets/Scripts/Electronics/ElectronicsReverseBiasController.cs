using UnityEngine;

public class ElectronicsReverseBiasController : MonoBehaviour
{
    public static ElectronicsReverseBiasController Instance { get; private set; }

    public bool IsValidated { get; private set; }
    public bool DarkScored { get; private set; }
    public bool IsCompleted => DarkScored;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Begin()
    {
        ElectronicsSwitchController.Instance?.ForceOff();
        ElectronicsUIManager.Instance?.SetInstruction("EXPERIMENT 2  •  REVERSE BIAS\nThe battery polarity is reversed. The diode now blocks current. Turn ON the switch.");
        ElectronicsCircuitConnectionManager.Instance?.ScoreReverseIfReady();
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
        if (!circuit.IsReverseBias())
        {
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ The diode is not reverse biased. Reverse only the battery.", "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
            return;
        }

        if (!IsValidated) circuit.ScoreReverseIfReady();
        ElectronicsBulbController.Instance?.TurnOff();
        if (!DarkScored)
        {
            DarkScored = true;
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.ReverseObservation, 5, false);
            ElectronicsFeedbackManager.Instance?.ShowMessage(
                "BULB STATUS: NOT GLOWING\nCURRENT: BLOCKED\nREVERSE BIAS\n\nThe diode blocks current in this simple circuit.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        ElectronicsUIManager.Instance?.SetNextButtonVisible(true);
    }

    public void ResetState()
    {
        IsValidated = false;
        DarkScored = false;
    }
}
