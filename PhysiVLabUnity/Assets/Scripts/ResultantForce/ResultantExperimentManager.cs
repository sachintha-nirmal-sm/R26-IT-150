using System.Globalization;
using UnityEngine;

public class ResultantExperimentManager : MonoBehaviour
{
    public static ResultantExperimentManager Instance { get; private set; }

    [SerializeField] private ResultantExperimentStep currentStep = ResultantExperimentStep.Introduction;
    [SerializeField] private int totalDisplaySteps = 12;
    [SerializeField] private int mistakeCount;
    private bool flutterSent;
    [SerializeField] private int correctScore = 5;
    [SerializeField] private int wrongPenalty = 5;
    [SerializeField] private int majorStepScore = 10;
    [SerializeField] private int maximumAttempts = 3;
    [SerializeField] private bool compareScored;
    [SerializeField] private bool completionScored;

    public ResultantExperimentStep CurrentStep => currentStep;
    public int MistakeCount => mistakeCount;
    public int TotalDisplaySteps => totalDisplaySteps;

    private void Awake() => Instance = this;

    private void Start() => ApplyInspectorSettings();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void ApplyInspectorSettings()
    {
        ResultantScoreManager.Instance?.Configure(correctScore, wrongPenalty, majorStepScore);
        ResultantAttemptManager.Instance?.Configure(maximumAttempts);
        int required = ResultantEquipmentSelectionManager.Instance != null ? ResultantEquipmentSelectionManager.Instance.RequiredCount : 7;
        ResultantScoreManager.Instance?.ConfigureMaxRaw(required);
    }

    public void RegisterMistake() => mistakeCount++;

    public void StartPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        compareScored = false;
        completionScored = false;
        ResultantScoreManager.Instance?.ResetScore();
        ResultantEquipmentSelectionManager.Instance?.EnsureCardsVisible();
        ResultantEquipmentSelectionManager.Instance?.ResetSelection();
        ResultantTrialManager.Instance?.ResetAllTrials();
        ResultantObservationTableManager.Instance?.ResetScoring();
        ResultantGraphController.Instance?.ResetScoring();
        ResultantConclusionManager.Instance?.ResetBuilder();
        ResultantVariableMatchingManager.Instance?.ResetMatching();
        ResultantAssemblyManager.Instance?.ResetAssembly();
        ResultantForceController.Instance?.ResetForces();
        ApplyInspectorSettings();
        currentStep = ResultantExperimentStep.Objective;
        ResultantUIManager.Instance?.HideResult();
        ResultantUIManager.Instance?.SetNextButtonVisible(true);
        ResultantUIManager.Instance?.UpdateAttemptsDisplay(ResultantAttemptManager.Instance != null ? ResultantAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateUI();
    }

    public void SetStep(ResultantExperimentStep step)
    {
        currentStep = step;
        EnterStep();
        UpdateUI();
        if (currentStep == ResultantExperimentStep.Complete)
            CompletePractical();
    }

    public void AdvanceStep()
    {
        if (currentStep >= ResultantExperimentStep.Complete) return;
        if (!CanLeaveCurrentStep()) return;

        if (currentStep == ResultantExperimentStep.ApplyForces)
        {
            if (!ResultantTrialManager.Instance.AllTrialsComplete())
            {
                int next = ResultantTrialManager.Instance.CurrentTrial + 1;
                StartTrial(next);
                currentStep = ResultantExperimentStep.ApplyForces;
                EnterStep();
                UpdateUI();
                return;
            }
        }

        currentStep++;
        EnterStep();
        UpdateUI();
        if (currentStep == ResultantExperimentStep.Complete)
            CompletePractical();
    }

    public void CompleteQuestions()
    {
        currentStep = ResultantExperimentStep.VariableMatching;
        EnterStep();
        UpdateUI();
    }

    public void CompleteVariableMatching()
    {
        currentStep = ResultantExperimentStep.Conclusion;
        EnterStep();
        UpdateUI();
    }

    public void CompleteConclusion()
    {
        currentStep = ResultantExperimentStep.Complete;
        EnterStep();
        UpdateUI();
        CompletePractical();
    }

    public void TryAdvanceFromEquipment()
    {
        if (currentStep == ResultantExperimentStep.Introduction || currentStep == ResultantExperimentStep.Objective)
            currentStep = ResultantExperimentStep.SelectEquipment;
        if (currentStep != ResultantExperimentStep.SelectEquipment) return;
        if (ResultantEquipmentSelectionManager.Instance != null && ResultantEquipmentSelectionManager.Instance.IsCompleteCheck())
        {
            ResultantScoreManager.Instance?.AddScore(5, false);
            AdvanceStep();
        }
        else
        {
            ResultantScoreManager.Instance?.SubtractScore(5);
            ResultantFeedbackManager.Instance?.ShowInstruction("Select all required equipment before continuing.");
        }
    }

    public void StartTrial(int trial)
    {
        ResultantTrialManager.Instance?.BeginTrial(trial);
        currentStep = ResultantExperimentStep.ApplyForces;
        ResultantForceController.Instance?.ResetForces();
        EnterStep();
        UpdateUI();
    }

    public void ConfirmSetup()
    {
        if (currentStep != ResultantExperimentStep.Assembly)
        {
            ResultantFeedbackManager.Instance?.ShowInstruction("Confirm setup during the assembly step.");
            ResultantScoreManager.Instance?.SubtractScore(5);
            return;
        }
        if (ResultantAssemblyManager.Instance != null && ResultantAssemblyManager.Instance.ConfirmSetup())
        {
            StartTrial(1);
        }
    }

    public void ChangeForceB(float delta) => ResultantForceController.Instance?.AddForceB(delta);
    public void ChangeForceC(float delta) => ResultantForceController.Instance?.AddForceC(delta);

    public void RecordReading()
    {
        if (currentStep != ResultantExperimentStep.ApplyForces)
        {
            ResultantFeedbackManager.Instance?.ShowInstruction("Record the readings while applying forces B and C.");
            return;
        }

        var force = ResultantForceController.Instance;
        if (force == null || force.ForceB < 0.5f || force.ForceC < 0.5f)
        {
            ResultantScoreManager.Instance?.SubtractScore(5);
            ResultantFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nPull both balances B and C so that two forces act on the trolley.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return;
        }

        float b = force.ForceB;
        float c = force.ForceC;
        float a = force.ForceA;
        var trial = ResultantTrialManager.Instance;
        float targetB = trial != null ? trial.TargetB : b;
        float targetC = trial != null ? trial.TargetC : c;
        bool nearTarget = Mathf.Abs(b - targetB) <= 1.0f && Mathf.Abs(c - targetC) <= 1.0f;

        ResultantTrialManager.Instance?.RecordCurrentReading(b, c, a);
        if (nearTarget)
        {
            ResultantScoreManager.Instance?.AddScore(5, false);
            ResultantFeedbackManager.Instance?.ShowMessage(
                $"✓ CORRECT\nA = {a:0.0} N,  B = {b:0.0} N,  C = {c:0.0} N\nResultant A equals B + C.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            ResultantScoreManager.Instance?.AddScore(5, false);
            ResultantFeedbackManager.Instance?.ShowMessage(
                $"✓ Readings recorded.\nA = {a:0.0} N  (= B + C)\nTarget was about B = {targetB:0} N and C = {targetC:0} N.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
        }
        CompleteTrial();
    }

    public void CompleteTrial()
    {
        if (ResultantTrialManager.Instance != null && ResultantTrialManager.Instance.AllTrialsComplete())
        {
            currentStep = ResultantExperimentStep.ObservationTable;
            EnterStep();
            UpdateUI();
            return;
        }
        ResultantUIManager.Instance?.SetNextButtonVisible(true);
        ResultantFeedbackManager.Instance?.ShowInstruction("Reading recorded. Press NEXT STEP to start the next trial.");
    }

    public void AnswerCompare(int choice)
    {
        if (currentStep != ResultantExperimentStep.CompareResults) return;
        if (choice == 2)
        {
            if (!compareScored)
            {
                compareScored = true;
                ResultantScoreManager.Instance?.AddScore(5, false);
            }
            ResultantFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT\nWhen two forces act in the same direction, the resultant force A equals B + C.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            ResultantUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            ResultantScoreManager.Instance?.SubtractScore(5);
            ResultantFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nLook at the table. Force A is always equal to Force B plus Force C.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public void CompletePractical()
    {
        if (completionScored) return;
        completionScored = true;
        int score = ResultantScoreManager.Instance != null ? ResultantScoreManager.Instance.FinalizeScore() : 0;
        bool passed = score >= 50;
        var attempt = ResultantAttemptManager.Instance != null
            ? ResultantAttemptManager.Instance.RegisterAttempt(score, mistakeCount, ResultantTrialManager.Instance != null ? ResultantTrialManager.Instance.GetAllTrials() : null, passed ? "Completed" : "Needs Improvement")
            : null;
        ResultantProfileManager.Instance?.UpdatePracticalResult(score, mistakeCount, passed, attempt);
        ResultantResultManager.Instance?.ShowResult(score, passed, mistakeCount, attempt);
        ResultantUIManager.Instance?.ShowResult();
        ResultantSaveManager.Instance?.Save(ResultantProfileManager.Instance != null ? ResultantProfileManager.Instance.ProfileData : null);
        SendToFlutter(score, passed);
    }

    public void CompleteExperiment()
    {
        if (!completionScored)
        {
            CompletePractical();
            return;
        }

        int score = ResultantScoreManager.Instance != null ? ResultantScoreManager.Instance.GetScore() : 0;
        SendToFlutter(score, score >= 50);
    }

    private void SendToFlutter(int score, bool passed)
    {
        if (flutterSent)
        {
            return;
        }

        flutterSent = true;
        TimerManager.Instance?.Stop();
        int timeUsed = TimerManager.Instance != null ? TimerManager.Instance.TimeUsedSeconds : 0;
        int trials = 0;
        if (ResultantTrialManager.Instance != null)
        {
            var list = ResultantTrialManager.Instance.GetAllTrials();
            trials = list != null ? list.Count : 0;
        }

        string measurements =
            "{\"trials\":" + trials.ToString(CultureInfo.InvariantCulture)
            + ",\"mistakes\":" + mistakeCount.ToString(CultureInfo.InvariantCulture)
            + "}";
        FlutterBridge.EnsureInstance()?.NotifyCompleted(
            score,
            passed,
            mistakeCount,
            timeUsed,
            true,
            measurements);
    }

    public void ResetPractical()
    {
        mistakeCount = 0;
        flutterSent = false;
        compareScored = false;
        completionScored = false;
        currentStep = ResultantExperimentStep.Introduction;
        ResultantScoreManager.Instance?.ResetScore();
        ResultantTrialManager.Instance?.ResetAllTrials();
        ResultantEquipmentSelectionManager.Instance?.ResetSelection();
        ResultantConclusionManager.Instance?.ResetBuilder();
        ResultantVariableMatchingManager.Instance?.ResetMatching();
        ResultantObservationTableManager.Instance?.ResetScoring();
        ResultantGraphController.Instance?.ResetScoring();
        ResultantAssemblyManager.Instance?.ResetAssembly();
        ResultantForceController.Instance?.ResetForces();
        ResultantEquipmentManager.Instance?.ResetTray();
        UpdateUI();
    }

    public void ResetCurrentTrial()
    {
        ResultantTrialManager.Instance?.ResetCurrentTrialKeepData();
        ResultantForceController.Instance?.ResetForces();
        ResultantUIManager.Instance?.UpdateLiveReadings();
    }

    public void ResetExperiment() => ResetPractical();

    public void RetryExperiment()
    {
        if (ResultantAttemptManager.Instance != null && !ResultantAttemptManager.Instance.CanRetry())
        {
            ResultantFeedbackManager.Instance?.ShowInstruction("No attempts remaining.");
            return;
        }
        ResetPractical();
        StartPractical();
    }

    private bool CanLeaveCurrentStep()
    {
        switch (currentStep)
        {
            case ResultantExperimentStep.SelectEquipment:
                if (ResultantEquipmentSelectionManager.Instance == null || !ResultantEquipmentSelectionManager.Instance.IsCompleteCheck())
                {
                    ResultantScoreManager.Instance?.SubtractScore(5);
                    ResultantFeedbackManager.Instance?.ShowInstruction("Select all required equipment first.");
                    return false;
                }
                return true;
            case ResultantExperimentStep.Assembly:
                ResultantFeedbackManager.Instance?.ShowInstruction("Place all apparatus, then press CONFIRM SETUP.");
                return false;
            case ResultantExperimentStep.ApplyForces:
                if (ResultantTrialManager.Instance != null && ResultantTrialManager.Instance.GetTrial(ResultantTrialManager.Instance.CurrentTrial) != null &&
                    ResultantTrialManager.Instance.GetTrial(ResultantTrialManager.Instance.CurrentTrial).completed)
                    return true;
                ResultantScoreManager.Instance?.SubtractScore(5);
                ResultantFeedbackManager.Instance?.ShowInstruction("Record the readings of A, B and C before continuing.");
                return false;
            case ResultantExperimentStep.Questions:
                if (ResultantQuestionManager.Instance != null && ResultantQuestionManager.Instance.IsFinished) return true;
                ResultantFeedbackManager.Instance?.ShowInstruction("Select an answer, then press CONTINUE.");
                return false;
            default:
                return true;
        }
    }

    private void EnterStep()
    {
        switch (currentStep)
        {
            case ResultantExperimentStep.Assembly:
                ResultantEquipmentManager.Instance?.ResetTray();
                ResultantAssemblyManager.Instance?.ResetAssembly();
                break;
            case ResultantExperimentStep.ApplyForces:
                if (ResultantTrialManager.Instance != null && ResultantTrialManager.Instance.CurrentTrial < 1)
                    StartTrial(1);
                break;
            case ResultantExperimentStep.ObservationTable:
                ResultantObservationTableManager.Instance?.Refresh();
                break;
            case ResultantExperimentStep.Graph:
                ResultantGraphController.Instance?.ShowGraphs();
                break;
            case ResultantExperimentStep.Questions:
                ResultantQuestionManager.Instance?.StartQuiz();
                break;
            case ResultantExperimentStep.VariableMatching:
                ResultantVariableMatchingManager.Instance?.ResetMatching();
                break;
            case ResultantExperimentStep.Conclusion:
                ResultantConclusionManager.Instance?.ResetBuilder();
                break;
        }
    }

    public void UpdateUI() => ResultantUIManager.Instance?.ShowStep(currentStep);
}
