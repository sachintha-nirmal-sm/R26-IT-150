using TMPro;
using UnityEngine;

public class ElectronicsObservationManager : MonoBehaviour
{
    public static ElectronicsObservationManager Instance { get; private set; }

    public ElectronicsDiodeObservation Forward { get; private set; } = new ElectronicsDiodeObservation { connectionType = "Forward Bias" };
    public ElectronicsDiodeObservation Reverse { get; private set; } = new ElectronicsDiodeObservation { connectionType = "Reverse Bias" };

    private bool forwardAnswered;
    private bool reverseAnswered;
    private TextMeshProUGUI tableText;

    public bool IsComplete => forwardAnswered && reverseAnswered;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(TextMeshProUGUI table)
    {
        tableText = table;
        RefreshTable();
    }

    public void StartForwardObservation()
    {
        ElectronicsUIManager.Instance?.ShowObservationChoices(
            "OBSERVATION 1  •  FORWARD BIAS",
            "What do you observe about the bulb?",
            "Bulb glows",
            "Bulb does not glow");
        RefreshTable();
    }

    public void StartReverseObservation()
    {
        ElectronicsUIManager.Instance?.ShowObservationChoices(
            "OBSERVATION 2  •  REVERSE BIAS",
            "What do you observe about the bulb?",
            "Bulb glows",
            "Bulb does not glow");
        RefreshTable();
    }

    public void AnswerForward(bool bulbGlows)
    {
        if (forwardAnswered) return;
        forwardAnswered = true;
        Forward.bulbGlowing = bulbGlows;
        Forward.currentFlowing = bulbGlows;
        Forward.observationText = bulbGlows ? "Bulb glows" : "Bulb does not glow";
        if (bulbGlows)
        {
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.ForwardObservation, 5, false);
            ElectronicsFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nDuring forward bias, the diode allows current to pass through the circuit. Therefore the bulb glows.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            ElectronicsScoreManager.Instance?.SubtractScore(5);
            ElectronicsFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nThe bulb should glow during forward bias because current can flow.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            Forward.bulbGlowing = true;
            Forward.currentFlowing = true;
            Forward.observationText = "Bulb glows";
        }
        RefreshTable();
        ElectronicsUIManager.Instance?.SetNextButtonVisible(true);
    }

    public void AnswerReverse(bool bulbGlows)
    {
        if (reverseAnswered) return;
        reverseAnswered = true;
        Reverse.bulbGlowing = bulbGlows;
        Reverse.currentFlowing = bulbGlows;
        Reverse.observationText = bulbGlows ? "Bulb glows" : "Bulb does not glow";
        if (!bulbGlows)
        {
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.ReverseObservation, 5, false);
            ElectronicsFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nDuring reverse bias, the diode blocks the current in this simple circuit. Therefore the bulb does not glow.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            ElectronicsScoreManager.Instance?.SubtractScore(5);
            ElectronicsFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nThe bulb should not glow during reverse bias because the diode blocks the current.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            Reverse.bulbGlowing = false;
            Reverse.currentFlowing = false;
            Reverse.observationText = "Bulb does not glow";
        }
        RefreshTable();
        ElectronicsUIManager.Instance?.SetNextButtonVisible(true);
    }

    public void ResetScoring()
    {
        forwardAnswered = false;
        reverseAnswered = false;
        Forward = new ElectronicsDiodeObservation { connectionType = "Forward Bias" };
        Reverse = new ElectronicsDiodeObservation { connectionType = "Reverse Bias" };
        RefreshTable();
    }

    public void RefreshTable()
    {
        if (tableText == null) return;
        string extra = "";
        var step = ElectronicsPracticalManager.Instance != null
            ? ElectronicsPracticalManager.Instance.CurrentStep
            : ElectronicsPracticalStep.Introduction;
        if (step == ElectronicsPracticalStep.ForwardObservation)
            extra = "OBSERVATION 1  •  FORWARD BIAS\nWhat do you observe about the bulb?\n\n";
        else if (step == ElectronicsPracticalStep.ReverseObservation)
            extra = "OBSERVATION 2  •  REVERSE BIAS\nWhat do you observe about the bulb?\n\n";
        tableText.text =
            extra +
            "OBSERVATION TABLE\n\n" +
            "Connection          Bulb Observation          Current Flow\n" +
            "---------------------------------------------------------------\n" +
            $"Forward Bias        {(string.IsNullOrEmpty(Forward.observationText) ? "____________" : Forward.observationText),-22}  {(Forward.currentFlowing ? "CAN FLOW" : (forwardAnswered ? "BLOCKED" : "___________"))}\n" +
            $"Reverse Bias        {(string.IsNullOrEmpty(Reverse.observationText) ? "____________" : Reverse.observationText),-22}  {(reverseAnswered ? (Reverse.currentFlowing ? "CAN FLOW" : "BLOCKED") : "___________")}";
    }
}
