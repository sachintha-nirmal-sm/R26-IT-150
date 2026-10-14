using UnityEngine;

public class WeightExperimentManager : MonoBehaviour
{
    public static WeightExperimentManager Instance { get; private set; }

    [SerializeField] private float selectedMass = 0.5f;
    [SerializeField] private bool objectSelected;
    [SerializeField] private bool hung;
    [SerializeField] private bool calculated;
    [SerializeField] private bool measurementScored;

    private static readonly float[] ObjectMasses = { 0.5f, 1.0f, 2.0f };

    public float SelectedMass => selectedMass;
    public float MeasuredWeight => SpringBalanceController.Instance != null ? SpringBalanceController.Instance.Reading : selectedMass * 9.8f;
    public bool HasMeasurement => NewtonDataManager.Instance != null && NewtonDataManager.Instance.WeightSeries.Count >= 1;
    public bool CalculationDone => calculated;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetMass(float mass)
    {
        selectedMass = Mathf.Clamp(mass, 0.5f, 5f);
        objectSelected = true;
        NewtonUIManager.Instance?.HighlightWeightMass(selectedMass);
        if (hung) MeasureWeight();
    }

    public float CalculateWeight()
    {
        return NewtonForceCalculator.Instance != null
            ? NewtonForceCalculator.Instance.CalculateWeight(selectedMass)
            : selectedMass * 9.8f;
    }

    public void MeasureWeight()
    {
        if (!objectSelected)
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowInstruction("Select an object first.");
            return;
        }
        hung = true;
        SpringBalanceController.Instance?.AttachObject(selectedMass);
        float w = CalculateWeight();
        if (!measurementScored)
        {
            measurementScored = true;
            NewtonScoreManager.Instance?.AddScore(5, false);
            NewtonFeedbackManager.Instance?.ShowMessage($"✓ Spring balance reading ≈ {w:0.0} N  (W = mg)", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        }
        NewtonUIManager.Instance?.UpdateLiveNewtonReadings(selectedMass, w, 0f, 0f, 0f, w, true);
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void RecordMeasurement()
    {
        if (!hung)
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowInstruction("Hang the object on the spring balance first.");
            return;
        }
        float w = CalculateWeight();
        NewtonDataManager.Instance?.AddWeightTrial(new NewtonLawTrialData
        {
            mass = selectedMass,
            weight = w,
            force = w,
            observation = "W = mg"
        });
        NewtonScoreManager.Instance?.AddScore(5, false);
        NewtonFeedbackManager.Instance?.ShowMessage($"✓ Recorded: mass {selectedMass:0.0} kg, weight {w:0.0} N", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        NewtonObservationTableManager.Instance?.Refresh();
        NewtonDataManager.Instance.WeightComplete = NewtonDataManager.Instance.WeightSeries.Count >= 1;
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void CheckStudentWeight(float studentValue)
    {
        float expected = CalculateWeight();
        bool ok = NewtonForceCalculator.Instance != null
            ? NewtonForceCalculator.Instance.ValidateStudentAnswer(studentValue, expected, NewtonsLawsExperimentManager.Instance != null ? NewtonsLawsExperimentManager.Instance.MeasurementTolerance : 0.05f)
            : Mathf.Abs(studentValue - expected) <= 0.05f;
        if (ok)
        {
            if (!calculated)
            {
                calculated = true;
                NewtonScoreManager.Instance?.AddScore(5, false);
            }
            NewtonFeedbackManager.Instance?.ShowMessage($"✓ Correct. W = mg = {selectedMass:0.0} × 9.8 = {expected:0.0} N", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
            NewtonDataManager.Instance.WeightComplete = true;
            NewtonUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage("✗ Incorrect. Weight is calculated using W = mg, with g = 9.8 m/s².", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void SelectObjectIndex(int index)
    {
        if (index < 0 || index >= ObjectMasses.Length)
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            return;
        }
        SetMass(ObjectMasses[index]);
        NewtonFeedbackManager.Instance?.ShowInstruction($"Object selected: mass = {selectedMass:0.0} kg. Hang it on the spring balance.");
    }

    public void ResetExperiment()
    {
        hung = false;
        calculated = false;
        measurementScored = false;
        objectSelected = false;
        selectedMass = 0.5f;
        SpringBalanceController.Instance?.ResetBalance();
    }
}
