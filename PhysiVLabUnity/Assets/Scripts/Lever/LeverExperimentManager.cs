using UnityEngine;

public class LeverExperimentManager : MonoBehaviour
{
    public static LeverExperimentManager Instance { get; private set; }

    [SerializeField] private LeverExperimentStep currentStep = LeverExperimentStep.Introduction;
    [SerializeField] private int totalSteps = 16;

    public LeverExperimentStep CurrentStep => currentStep;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartExperiment()
    {
        currentStep = LeverExperimentStep.SelectEquipment;
        LeverEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        UpdateUI();
    }

    public void SetStep(LeverExperimentStep step)
    {
        currentStep = step;
        UpdateUI();
    }

    public void AdvanceStep()
    {
        if (currentStep >= LeverExperimentStep.Complete) return;

        switch (currentStep)
        {
            case LeverExperimentStep.Introduction:
                currentStep = LeverExperimentStep.SelectEquipment;
                break;
            case LeverExperimentStep.SelectEquipment:
                currentStep = LeverExperimentStep.PlacePivot;
                break;
            case LeverExperimentStep.PlacePivot:
                currentStep = LeverExperimentStep.PlaceWoodenStrip;
                break;
            case LeverExperimentStep.PlaceWoodenStrip:
                currentStep = LeverExperimentStep.PlaceBook;
                break;
            case LeverExperimentStep.PlaceBook:
                currentStep = LeverExperimentStep.MeasureDistanceA;
                break;
            case LeverExperimentStep.MeasureDistanceA:
                currentStep = LeverExperimentStep.AttachSpringBalance;
                break;
            case LeverExperimentStep.AttachSpringBalance:
                currentStep = LeverExperimentStep.SelectDistanceX;
                break;
            case LeverExperimentStep.SelectDistanceX:
                currentStep = LeverExperimentStep.PullBalance;
                break;
            case LeverExperimentStep.PullBalance:
                currentStep = LeverExperimentStep.ObserveLift;
                break;
            case LeverExperimentStep.ObserveLift:
                currentStep = LeverExperimentStep.RecordReading;
                break;
            case LeverExperimentStep.RecordReading:
                currentStep = LeverExperimentStep.NextXOrCompare;
                break;
            case LeverExperimentStep.NextXOrCompare:
                if (LeverExperimentDataManager.Instance != null && LeverExperimentDataManager.Instance.HasMoreXValues())
                {
                    LeverExperimentDataManager.Instance.AdvanceXIndex();
                    currentStep = LeverExperimentStep.SelectDistanceX;
                }
                else
                {
                    currentStep = LeverExperimentStep.CompareResults;
                }
                break;
            case LeverExperimentStep.CompareResults:
                currentStep = LeverExperimentStep.Conclusion;
                break;
            case LeverExperimentStep.Conclusion:
                currentStep = LeverExperimentStep.Conclusion2;
                break;
            case LeverExperimentStep.Conclusion2:
                currentStep = LeverExperimentStep.Challenge;
                break;
            case LeverExperimentStep.Challenge:
                currentStep = LeverExperimentStep.Complete;
                break;
            default:
                currentStep++;
                break;
        }

        UpdateUI();

        if (currentStep == LeverExperimentStep.Complete)
            LeverGameManager.Instance?.CompleteExperiment();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == LeverExperimentStep.Introduction)
            currentStep = LeverExperimentStep.SelectEquipment;

        if (currentStep != LeverExperimentStep.SelectEquipment) return;

        if (LeverEquipmentSelectionManager.Instance != null && LeverEquipmentSelectionManager.Instance.IsCompleteCheck())
            AdvanceStep();
        else
        {
            LeverScoreManager.Instance?.SubtractScore(5);
            LeverFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void ResetExperiment()
    {
        currentStep = LeverExperimentStep.SelectEquipment;
        UpdateUI();
    }

    private void UpdateUI()
    {
        LeverUIManager.Instance?.UpdateProgress(currentStep, totalSteps);
        LeverUIManager.Instance?.ShowStagePanels(currentStep);
        LeverUIManager.Instance?.UpdateInstruction(GetInstruction(currentStep));
        LeverLabWorkbench.Instance?.UpdateForStep(currentStep);

        if (currentStep == LeverExperimentStep.Conclusion || currentStep == LeverExperimentStep.Conclusion2 || currentStep == LeverExperimentStep.Challenge)
            LeverConclusionManager.Instance?.ShowConclusion();
        if (currentStep == LeverExperimentStep.CompareResults)
            LeverUIManager.Instance?.ShowCompareResults();
    }

    public string GetInstruction(LeverExperimentStep step)
    {
        switch (step)
        {
            case LeverExperimentStep.Introduction:
                return "Welcome to Lever – Activity 15.1. Press START to begin.";
            case LeverExperimentStep.SelectEquipment:
                return "Select the five pieces of equipment required for the lever experiment.";
            case LeverExperimentStep.PlacePivot:
                return "Place the support (pivot) in the correct position on the workbench.";
            case LeverExperimentStep.PlaceWoodenStrip:
                return "Place the wooden strip (lever arm) across the support pivot.";
            case LeverExperimentStep.PlaceBook:
                return "Place the book (load) on one side of the wooden strip.";
            case LeverExperimentStep.MeasureDistanceA:
                return "Use the ruler to measure distance A from the pivot to the book.";
            case LeverExperimentStep.AttachSpringBalance:
                return "Attach the Newton spring balance on the effort side of the lever.";
            case LeverExperimentStep.SelectDistanceX:
            {
                float x = LeverExperimentDataManager.Instance != null
                    ? LeverExperimentDataManager.Instance.GetCurrentX()
                    : 10f;
                int n = (LeverExperimentDataManager.Instance?.CurrentXIndex ?? 0) + 1;
                return $"Select distance X = {x:0} cm for trial {n}. Position the spring balance at this distance from the pivot.";
            }
            case LeverExperimentStep.PullBalance:
                return "Pull the spring balance gradually until the effort is enough to balance or lift the book.";
            case LeverExperimentStep.ObserveLift:
                return "Observe whether the book lifts. Note the effort reading on the spring balance.";
            case LeverExperimentStep.RecordReading:
                return "Record the effort reading, distance X, and whether the book lifted.";
            case LeverExperimentStep.NextXOrCompare:
                if (LeverExperimentDataManager.Instance != null && LeverExperimentDataManager.Instance.HasMoreXValues())
                    return "Reading recorded. Press NEXT to try another distance X.";
                return "All X values recorded. Press NEXT to compare your results.";
            case LeverExperimentStep.CompareResults:
                return "Compare your readings: as distance X increases, the required effort decreases. Press NEXT STEP.";
            case LeverExperimentStep.Conclusion:
                return "Choose the correct conclusion about how effort changes with distance X. Tap A, B, C or D.";
            case LeverExperimentStep.Conclusion2:
                return "Confirm: Effort × Distance X = Load × Distance A (principle of moments). Choose the best statement.";
            case LeverExperimentStep.Challenge:
                return "Challenge: Predict the effort needed for a new distance X using Load × A / X.";
            case LeverExperimentStep.Complete:
                return "Activity completed!";
            default:
                return "";
        }
    }
}
