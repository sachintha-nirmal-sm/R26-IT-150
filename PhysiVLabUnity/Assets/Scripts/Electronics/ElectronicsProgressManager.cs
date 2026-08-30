using UnityEngine;

public class ElectronicsProgressManager : MonoBehaviour
{
    public static ElectronicsProgressManager Instance { get; private set; }

    [SerializeField] private int totalSteps = 10;
    [SerializeField] private int currentDisplayStep = 1;

    public int TotalSteps => totalSteps;
    public int CurrentDisplayStep => currentDisplayStep;
    public float Progress01 => totalSteps <= 0 ? 0f : Mathf.Clamp01(currentDisplayStep / (float)totalSteps);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetFromPracticalStep(ElectronicsPracticalStep step)
    {
        currentDisplayStep = DisplayIndex(step);
        RefreshHeader();
    }

    public void ResetProgress()
    {
        currentDisplayStep = 1;
        RefreshHeader();
    }

    public void RefreshHeader()
    {
        ElectronicsUIManager.Instance?.UpdateProgressDisplay(currentDisplayStep, totalSteps, Progress01);
    }

    public static int DisplayIndex(ElectronicsPracticalStep step)
    {
        switch (step)
        {
            case ElectronicsPracticalStep.Introduction:
            case ElectronicsPracticalStep.Theory:
                return 1;
            case ElectronicsPracticalStep.EquipmentSelection:
                return 2;
            case ElectronicsPracticalStep.CircuitSetup:
                return 3;
            case ElectronicsPracticalStep.ForwardBias:
                return 4;
            case ElectronicsPracticalStep.ForwardObservation:
                return 5;
            case ElectronicsPracticalStep.BatteryDisconnect:
            case ElectronicsPracticalStep.BatteryReverse:
                return 6;
            case ElectronicsPracticalStep.ReverseBias:
                return 7;
            case ElectronicsPracticalStep.ReverseObservation:
                return 8;
            case ElectronicsPracticalStep.Comparison:
            case ElectronicsPracticalStep.Matching:
            case ElectronicsPracticalStep.Challenge:
            case ElectronicsPracticalStep.Questions:
                return 9;
            default:
                return 10;
        }
    }
}
