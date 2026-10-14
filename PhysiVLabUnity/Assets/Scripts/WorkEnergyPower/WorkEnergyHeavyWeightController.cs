using UnityEngine;

public class WorkEnergyHeavyWeightController : MonoBehaviour
{
    public static WorkEnergyHeavyWeightController Instance { get; private set; }

    [SerializeField] private float weightMass = 1.0f;
    [SerializeField] private bool weightPlaced;
    [SerializeField] private bool massLocked = true;

    public float WeightMass => weightMass;
    public bool WeightPlaced => weightPlaced;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (WorkEnergyExperimentDataManager.Instance != null)
            weightMass = WorkEnergyExperimentDataManager.Instance.WeightMass;
    }

    public void PlaceWeight()
    {
        weightPlaced = true;
        massLocked = true;
    }

    public bool TryChangeMass(float newMass)
    {
        if (massLocked && weightPlaced)
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("Keep the same weight for every reading.");
            return false;
        }
        if (newMass <= 0f) return false;
        weightMass = newMass;
        WorkEnergyExperimentDataManager.Instance?.SetMass(newMass);
        return true;
    }

    public void ResetPlacement()
    {
        weightPlaced = false;
    }

    public void ResetForNextReading()
    {
        massLocked = true;
    }
}
