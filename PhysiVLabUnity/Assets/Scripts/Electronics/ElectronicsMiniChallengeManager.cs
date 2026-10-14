using UnityEngine;

public class ElectronicsMiniChallengeManager : MonoBehaviour
{
    public static ElectronicsMiniChallengeManager Instance { get; private set; }

    public bool IsComplete { get; private set; }
    private bool scored;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Begin()
    {
        IsComplete = false;
        scored = false;
        ElectronicsBatteryController.Instance?.ResetPolarity();
        ElectronicsDiodeController.Instance?.ResetDiode();
        ElectronicsDiodeController.Instance?.SetPlaced(true);
        ElectronicsBatteryController.Instance?.SetPlaced(true);
        ElectronicsSwitchController.Instance?.SetPlaced(true);
        ElectronicsBulbController.Instance?.SetPlaced(true);
        ElectronicsSwitchController.Instance?.ForceOff();
        ElectronicsUIManager.Instance?.SetInstruction("MAKE THE BULB GLOW\nBuild a forward-biased circuit. Check diode direction and battery polarity, then turn ON the switch.");
    }

    public void SetDiodeCorrect()
    {
        if (ElectronicsDiodeController.Instance != null && ElectronicsDiodeController.Instance.IsFlipped)
            ElectronicsDiodeController.Instance.FlipOrientation();
        ElectronicsFeedbackManager.Instance?.ShowInstruction("Diode set to anode → cathode (standard direction).");
    }

    public void SetBatteryNormal()
    {
        ElectronicsBatteryController.Instance?.ResetPolarity();
        ElectronicsBatteryController.Instance?.Reconnect();
        ElectronicsFeedbackManager.Instance?.ShowInstruction("Battery polarity: normal (positive toward the diode anode).");
    }

    public void OnSwitchOn()
    {
        bool ok = ElectronicsCircuitConnectionManager.Instance != null && ElectronicsCircuitConnectionManager.Instance.IsForwardBias();
        if (ok)
        {
            ElectronicsBulbController.Instance?.TurnOn();
            if (!scored)
            {
                scored = true;
                IsComplete = true;
                ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.Comparison, 5, false);
                ElectronicsFeedbackManager.Instance?.ShowMessage("✓ YOU CREATED A FORWARD-BIASED CIRCUIT!", "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
            }
            ElectronicsUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            ElectronicsBulbController.Instance?.TurnOff();
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ Check the diode direction and battery polarity.", "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
            ElectronicsSwitchController.Instance?.ForceOff();
        }
    }

    public void ResetState()
    {
        IsComplete = false;
        scored = false;
    }
}
