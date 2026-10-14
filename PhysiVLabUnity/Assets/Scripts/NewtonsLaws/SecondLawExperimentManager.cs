using UnityEngine;

public class SecondLawExperimentManager : MonoBehaviour
{
    public static SecondLawExperimentManager Instance { get; private set; }

    [SerializeField] private bool trolleyPlaced;
    [SerializeField] private bool setupScored;
    [SerializeField] private bool running;
    [SerializeField] private float runTime;
    [SerializeField] private bool constantMassMode = true;

    public bool TrolleyReady => trolleyPlaced;
    public bool SetupComplete =>
        trolleyPlaced &&
        PulleyController.Instance != null && PulleyController.Instance.Placed &&
        StringConnectionController.Instance != null && StringConnectionController.Instance.Connected;

    public bool HasForceSeries => NewtonDataManager.Instance != null && NewtonDataManager.Instance.ForceSeries.Count >= 3;
    public bool HasMassSeries => NewtonDataManager.Instance != null && NewtonDataManager.Instance.MassSeries.Count >= 3;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetForce(float force) => NewtonForceController.Instance?.SetForce(force);
    public void SetMass(float mass) => NewtonMassController.Instance?.SetMass(mass);

    public float CalculateAcceleration()
    {
        float f = NewtonForceController.Instance != null ? NewtonForceController.Instance.Force : 0f;
        float m = NewtonMassController.Instance != null ? NewtonMassController.Instance.Mass : 1f;
        return NewtonAccelerationCalculator.Instance != null
            ? NewtonAccelerationCalculator.Instance.Calculate(f, m)
            : (m > 0f ? f / m : 0f);
    }

    public void NotifyTrolleyPlaced()
    {
        trolleyPlaced = true;
        CheckSetup();
    }

    public void CheckSetup()
    {
        if (setupScored) return;
        if (!trolleyPlaced || PulleyController.Instance == null || !PulleyController.Instance.Placed ||
            StringConnectionController.Instance == null || !StringConnectionController.Instance.StringPlaced)
            return;
        if (!StringConnectionController.Instance.HangerAttached)
        {
            NewtonFeedbackManager.Instance?.ShowInstruction("Attach the weight hanger to complete the setup.");
            return;
        }
        setupScored = true;
        NewtonScoreManager.Instance?.AddScore(10, false);
        NewtonFeedbackManager.Instance?.ShowMessage("✓ Correct setup. Trolley — string — pulley — weight hanger.", "+10 Marks", new Color(0.08f, 0.52f, 0.22f));
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
    }

    public void MarkIncorrectSetup()
    {
        NewtonScoreManager.Instance?.SubtractScore(5);
        NewtonFeedbackManager.Instance?.ShowMessage("✗ Incorrect setup. Connect trolley — string — pulley — weight hanger.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
    }

    public void StartExperiment()
    {
        if (!SetupComplete)
        {
            MarkIncorrectSetup();
            return;
        }
        float a = CalculateAcceleration();
        TrolleyController.Instance?.ResetTrolley();
        TrolleyController.Instance?.SetMass(NewtonMassController.Instance != null ? NewtonMassController.Instance.Mass : 1f);
        TrolleyController.Instance?.SetForce(NewtonForceController.Instance != null ? NewtonForceController.Instance.Force : 1f);
        TrolleyController.Instance?.StartMotion();
        running = true;
        runTime = 0f;
        NewtonUIManager.Instance?.ShowForceArrows(true);
        NewtonFeedbackManager.Instance?.ShowInstruction($"a = F / m = {NewtonForceController.Instance.Force:0.0} / {NewtonMassController.Instance.Mass:0.0} = {a:0.00} m/s²");
    }

    public void StopExperiment()
    {
        running = false;
        TrolleyController.Instance?.Stop();
        NewtonUIManager.Instance?.ShowForceArrows(false);
    }

    public void RecordTrial()
    {
        if (!SetupComplete)
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowInstruction("Complete the setup and run the experiment before recording.");
            return;
        }
        float f = NewtonForceController.Instance != null ? NewtonForceController.Instance.Force : 0f;
        float m = NewtonMassController.Instance != null ? NewtonMassController.Instance.Mass : 1f;
        if (f <= 0f)
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowInstruction("Select an applied force first.");
            return;
        }
        float a = CalculateAcceleration();
        var data = new NewtonLawTrialData
        {
            force = f,
            mass = m,
            acceleration = a,
            velocity = TrolleyController.Instance != null ? TrolleyController.Instance.Velocity : 0f,
            time = runTime,
            observation = constantMassMode ? "Constant mass" : "Constant force"
        };
        if (constantMassMode) NewtonDataManager.Instance?.AddForceTrial(data);
        else NewtonDataManager.Instance?.AddMassTrial(data);
        NewtonScoreManager.Instance?.AddScore(5, false);
        NewtonFeedbackManager.Instance?.ShowMessage(
            $"✓ Trial recorded. F = {f:0.0} N, m = {m:0.0} kg, a = {a:0.00} m/s²",
            "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        NewtonObservationTableManager.Instance?.Refresh();
        NewtonsLawsExperimentManager.Instance?.NotifySetupChanged();
        StopExperiment();
    }

    public void SetConstantMassMode(bool on)
    {
        constantMassMode = on;
        if (on)
        {
            NewtonMassController.Instance?.SetMass(1f);
            NewtonForceController.Instance?.SetForce(1f);
        }
        else
        {
            NewtonForceController.Instance?.SetForce(4f);
            NewtonMassController.Instance?.SetMass(0.5f);
        }
    }

    public void ResetExperiment()
    {
        StopExperiment();
        TrolleyController.Instance?.ResetTrolley();
        runTime = 0f;
    }

    public void FullReset()
    {
        trolleyPlaced = false;
        setupScored = false;
        PulleyController.Instance?.ResetPulley();
        StringConnectionController.Instance?.ResetConnection();
        ResetExperiment();
    }

    private void Update()
    {
        if (running) runTime += Time.deltaTime;
    }
}
