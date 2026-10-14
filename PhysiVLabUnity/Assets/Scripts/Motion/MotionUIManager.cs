using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MotionUIManager : MonoBehaviour
{
    public static MotionUIManager Instance { get; private set; }

    private TextMeshProUGUI titleText, scoreText, progressText, attemptsText, stepText, instructionText;
    private Image progressBarFill;
    private GameObject introPanel, objectivePanel, instructionBar, equipmentPanel, laboratoryPanel;
    private GameObject dataTablePanel, comparePanel, graphPanel, questionPanel, conclusionPanel, resultPanel;
    private GameObject calcPanel, accelPanel, actionRow, targetRow;
    private TextMeshProUGUI introText, objectiveText, conclusionText, compareText, questionText, dataTableText;
    private TextMeshProUGUI finalScoreText, resultDetailsText, statusText, liveReadingsText, stopwatchText, calcPromptText;
    private Button startBtn, nextBtn, resetBtn, retryBtn, viewProfileBtn, viewResultsBtn;
    private Button startExpBtn, stopExpBtn, resetRunBtn, recordBtn, directionBtn;
    private Button questionA, questionB, questionC, questionD, questionContinue, numericSubmit;
    private Button checkCalcBtn, checkCompareBtn;
    private Button target1, target2, target3, target4, target5;
    private Button lowSpeedBtn, medSpeedBtn, highSpeedBtn;
    private GameObject questionExplanationPanel, resetConfirmPanel, numericGroup, optionsGroup;
    private TextMeshProUGUI questionExplanationText, optAText, optBText, optCText, optDText;
    private TMP_InputField numericInput, calcInput, compareDistanceInput, compareDisplacementInput;
    private Button resetYes, resetNo;
    private Button[] targetButtons;

    private void Awake() => Instance = this;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void BindAll(MotionUIRefs refs, bool showWelcome)
    {
        titleText = refs.Title; scoreText = refs.Score; progressText = refs.Progress;
        attemptsText = refs.Attempts; stepText = refs.StepLabel;
        progressBarFill = refs.ProgressBar;
        instructionText = refs.Instruction;
        introPanel = refs.IntroPanel; objectivePanel = refs.ObjectivePanel;
        instructionBar = refs.InstructionBar; equipmentPanel = refs.EquipmentPanel;
        laboratoryPanel = refs.LaboratoryPanel;
        dataTablePanel = refs.DataTablePanel; comparePanel = refs.ComparePanel;
        graphPanel = refs.GraphPanel; questionPanel = refs.QuestionPanel;
        conclusionPanel = refs.ConclusionPanel; resultPanel = refs.ResultPanel;
        calcPanel = refs.CalcPanel; accelPanel = refs.AccelPanel;
        actionRow = refs.ActionRow; targetRow = refs.TargetRow;
        introText = refs.IntroText; objectiveText = refs.ObjectiveText;
        conclusionText = refs.ConclusionText; compareText = refs.CompareText;
        questionText = refs.QuestionText; dataTableText = refs.DataTableText;
        finalScoreText = refs.FinalScore; resultDetailsText = refs.ResultDetails; statusText = refs.StatusText;
        liveReadingsText = refs.LiveReadings; stopwatchText = refs.StopwatchText; calcPromptText = refs.CalcPrompt;
        startBtn = refs.StartBtn; nextBtn = refs.Next; resetBtn = refs.Reset;
        retryBtn = refs.Retry; viewProfileBtn = refs.ViewProfileBtn; viewResultsBtn = refs.ViewResultsBtn;
        startExpBtn = refs.StartExpBtn; stopExpBtn = refs.StopExpBtn;
        resetRunBtn = refs.ResetRunBtn; recordBtn = refs.RecordBtn; directionBtn = refs.DirectionBtn;
        questionA = refs.QuestionA; questionB = refs.QuestionB; questionC = refs.QuestionC; questionD = refs.QuestionD;
        questionContinue = refs.QuestionContinue; numericSubmit = refs.NumericSubmit;
        checkCalcBtn = refs.CheckCalcBtn; checkCompareBtn = refs.CheckCompareBtn;
        target1 = refs.Target1; target2 = refs.Target2; target3 = refs.Target3; target4 = refs.Target4; target5 = refs.Target5;
        lowSpeedBtn = refs.LowSpeedBtn; medSpeedBtn = refs.MedSpeedBtn; highSpeedBtn = refs.HighSpeedBtn;
        questionExplanationPanel = refs.QuestionExplanationPanel;
        questionExplanationText = refs.QuestionExplanationText;
        optAText = refs.OptAText; optBText = refs.OptBText; optCText = refs.OptCText; optDText = refs.OptDText;
        numericInput = refs.NumericInput; calcInput = refs.CalcInput;
        compareDistanceInput = refs.CompareDistanceInput; compareDisplacementInput = refs.CompareDisplacementInput;
        numericGroup = refs.NumericGroup; optionsGroup = refs.OptionsGroup;
        resetConfirmPanel = refs.ResetConfirm; resetYes = refs.ResetYes; resetNo = refs.ResetNo;
        targetButtons = new[] { target1, target2, target3, target4, target5 };
        WireButtons();
        if (showWelcome) ShowIntro();
    }

    private void WireButtons()
    {
        WireBtn(startBtn, StartPractical);
        WireBtn(nextBtn, OnNextPressed);
        WireBtn(resetBtn, () => { if (resetConfirmPanel != null) resetConfirmPanel.SetActive(true); });
        WireBtn(resetYes, () =>
        {
            resetConfirmPanel?.SetActive(false);
            MotionExperimentManager.Instance?.ResetExperiment();
            ShowIntro();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => MotionExperimentManager.Instance?.RetryExperiment());
        WireBtn(viewProfileBtn, () =>
        {
            string summary = MotionProfileManager.Instance != null ? MotionProfileManager.Instance.GetProfileSummary() : "No profile data.";
            MotionFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireBtn(startExpBtn, OnStartExperiment);
        WireBtn(stopExpBtn, OnStopExperiment);
        WireBtn(resetRunBtn, () => MotionExperimentManager.Instance?.ResetKeepTrials());
        WireBtn(recordBtn, OnRecord);
        WireBtn(directionBtn, OnConfirmSetupAction);
        WireBtn(target1, () => MotionTrialManager.Instance?.SelectTarget(1f));
        WireBtn(target2, () => MotionTrialManager.Instance?.SelectTarget(2f));
        WireBtn(target3, () => MotionTrialManager.Instance?.SelectTarget(3f));
        WireBtn(target4, () => MotionTrialManager.Instance?.SelectTarget(4f));
        WireBtn(target5, () => MotionTrialManager.Instance?.SelectTarget(5f));
        WireBtn(lowSpeedBtn, () => AccelerationExperimentManager.Instance?.SelectCondition(0));
        WireBtn(medSpeedBtn, () => AccelerationExperimentManager.Instance?.SelectCondition(1));
        WireBtn(highSpeedBtn, () => AccelerationExperimentManager.Instance?.SelectCondition(2));
        WireBtn(questionA, () => MotionQuestionManager.Instance?.Answer(0));
        WireBtn(questionB, () => MotionQuestionManager.Instance?.Answer(1));
        WireBtn(questionC, () => MotionQuestionManager.Instance?.Answer(2));
        WireBtn(questionD, () => MotionQuestionManager.Instance?.Answer(3));
        WireBtn(questionContinue, OnQuestionContinue);
        WireBtn(numericSubmit, SubmitNumericQuestion);
        WireBtn(checkCalcBtn, SubmitCalculation);
        WireBtn(checkCompareBtn, SubmitCompare);
    }

    private void WireBtn(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    public void StartPractical() => MotionExperimentManager.Instance?.StartPractical();

    private void OnNextPressed()
    {
        var step = MotionExperimentManager.Instance != null
            ? MotionExperimentManager.Instance.CurrentStep
            : MotionExperimentStep.Introduction;
        if (step == MotionExperimentStep.Introduction) { StartPractical(); return; }
        if (step == MotionExperimentStep.SelectEquipment)
        {
            MotionExperimentManager.Instance?.TryAdvanceFromEquipment();
            return;
        }
        if (step == MotionExperimentStep.Questions)
        {
            OnQuestionContinue();
            return;
        }
        MotionExperimentManager.Instance?.AdvanceStep();
    }

    private void OnConfirmSetupAction()
    {
        var step = Current();
        if (step == MotionExperimentStep.PlaceCar)
            MotionTrackController.Instance?.ConfirmCarStart();
        else
            MotionTrackController.Instance?.ConfirmDirection();
    }

    private void OnStartExperiment()
    {
        var step = Current();
        if (step == MotionExperimentStep.MotionTrials) MotionTrialManager.Instance?.StartTrial();
        else if (step == MotionExperimentStep.AccelerationExperiment) AccelerationExperimentManager.Instance?.StartRun();
        else if (step == MotionExperimentStep.Deceleration) DecelerationController.Instance?.BeginDemonstration();
        else if (step == MotionExperimentStep.DistanceVsDisplacement) MotionExperimentManager.Instance?.StartPathTask();
        else MotionFeedbackManager.Instance?.ShowInstruction("This control is used during the motion experiments.");
    }

    private void OnStopExperiment()
    {
        if (Current() == MotionExperimentStep.MotionTrials) MotionTrialManager.Instance?.StopTrial();
        else ToyCarController.Instance?.Stop();
    }

    private void OnRecord()
    {
        var step = Current();
        if (step == MotionExperimentStep.MotionTrials) MotionTrialManager.Instance?.RecordTrial();
        else if (step == MotionExperimentStep.AccelerationExperiment) AccelerationExperimentManager.Instance?.Record();
        else MotionFeedbackManager.Instance?.ShowInstruction("Record is used after a motion or acceleration run.");
    }

    private void OnQuestionContinue()
    {
        bool done = MotionQuestionManager.Instance != null && MotionQuestionManager.Instance.Advance();
        if (done) MotionExperimentManager.Instance?.AdvanceStep();
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
    }

    private void SubmitNumericQuestion()
    {
        if (numericInput == null) return;
        if (float.TryParse(numericInput.text, out float value))
            MotionQuestionManager.Instance?.AnswerNumeric(value);
        else
            MotionFeedbackManager.Instance?.ShowInstruction("Enter a number, including a minus sign if needed.");
    }

    private void SubmitCalculation()
    {
        if (calcInput == null || !float.TryParse(calcInput.text, out float value))
        {
            MotionFeedbackManager.Instance?.ShowInstruction("Enter your calculated value.");
            return;
        }
        var step = Current();
        if (step == MotionExperimentStep.SpeedCalculation) MotionExperimentManager.Instance?.CheckSpeedAnswer(value);
        else if (step == MotionExperimentStep.VelocityCalculation) MotionExperimentManager.Instance?.CheckVelocityAnswer(value);
        else if (step == MotionExperimentStep.AccelerationExperiment)
            AccelerationExperimentManager.Instance?.CheckStudentAcceleration(value, MotionExperimentManager.Instance != null ? MotionExperimentManager.Instance.MeasurementTolerance : 0.05f);
    }

    private void SubmitCompare()
    {
        float d = 0f, s = 0f;
        bool okD = compareDistanceInput != null && float.TryParse(compareDistanceInput.text, out d);
        bool okS = compareDisplacementInput != null && float.TryParse(compareDisplacementInput.text, out s);
        if (!okD || !okS)
        {
            MotionFeedbackManager.Instance?.ShowInstruction("Enter both distance and displacement.");
            return;
        }
        MotionExperimentManager.Instance?.CheckCompare(d, s);
    }

    public void ShowIntro()
    {
        HideAllContent();
        if (introPanel != null) introPanel.SetActive(true);
        if (instructionBar != null) instructionBar.SetActive(false);
        SetNextButtonVisible(false);
        if (introText != null)
        {
            introText.text =
                "MOTION\n\n" +
                "Practical:\nInvestigating Distance, Displacement, Speed, Velocity and Acceleration\n\n" +
                "In this practical you will investigate the motion of a toy car moving along a straight track. " +
                "You will measure distance and time and use the measurements to calculate speed, displacement, velocity and acceleration.";
        }
        UpdateAttemptsDisplay(MotionAttemptManager.Instance != null ? MotionAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateScoreDisplay(0);
        UpdateProgress(1, 20);
        if (stepText != null) stepText.text = "Introduction";
    }

    public void ShowStep(MotionExperimentStep step)
    {
        HideAllContent();
        if (instructionBar != null) instructionBar.SetActive(step != MotionExperimentStep.Introduction && step != MotionExperimentStep.Complete);
        SetNextButtonVisible(true);
        SetLabControls(false);
        calcPanel?.SetActive(false);
        accelPanel?.SetActive(false);

        switch (step)
        {
            case MotionExperimentStep.Introduction:
                ShowIntro();
                break;
            case MotionExperimentStep.Objective:
                objectivePanel?.SetActive(true);
                if (objectiveText != null)
                    objectiveText.text =
                        "OBJECTIVE\n\n" +
                        "To investigate the motion of an object and determine its distance, displacement, speed, velocity and acceleration using experimental measurements.\n\n" +
                        "FORMULAS\n" +
                        "speed = distance / time\n" +
                        "velocity = displacement / time\n" +
                        "acceleration = change in velocity / time";
                SetInstruction("Read the objective, then press CONTINUE.");
                SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case MotionExperimentStep.SelectEquipment:
                equipmentPanel?.SetActive(true);
                MotionEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                SetInstruction("Select all the equipment required for this practical. Drag or tap correct items into the required equipment area.");
                SetButtonLabel(nextBtn, "NEXT STEP");
                break;
            case MotionExperimentStep.SetupTrack:
                ShowLab("Drag the Straight Track onto the experiment area, then press NEXT STEP.", "NEXT STEP", false);
                SetNextButtonVisible(true);
                break;
            case MotionExperimentStep.SetupRuler:
                ShowLab("Drag the Metre Ruler onto the track, then press NEXT STEP.", "NEXT STEP", false);
                SetNextButtonVisible(true);
                break;
            case MotionExperimentStep.PlaceCar:
                ShowLab("The car must start at 0 m. Press CONFIRM START (0 m), or tap the red car. Then press NEXT STEP.", "NEXT STEP", false);
                if (actionRow != null) actionRow.SetActive(true);
                if (startExpBtn != null) startExpBtn.gameObject.SetActive(false);
                if (stopExpBtn != null) stopExpBtn.gameObject.SetActive(false);
                if (recordBtn != null) recordBtn.gameObject.SetActive(false);
                if (directionBtn != null)
                {
                    directionBtn.gameObject.SetActive(true);
                    SetButtonLabel(directionBtn, "CONFIRM START (0 m)");
                }
                SetNextButtonVisible(true);
                MotionTrackController.Instance?.EnsureCarVisibleAtStart();
                break;
            case MotionExperimentStep.PlaceMarkers:
                ShowLab("Drag each marker to 1 m, 2 m, 3 m, 4 m and 5 m. Then press NEXT STEP.", "NEXT STEP", false);
                SetNextButtonVisible(true);
                break;
            case MotionExperimentStep.SetDirection:
                ShowLab("Press CONFIRM → to choose the positive direction START to FINISH. Then press NEXT STEP.", "NEXT STEP", false);
                if (actionRow != null) actionRow.SetActive(true);
                if (startExpBtn != null) startExpBtn.gameObject.SetActive(false);
                if (stopExpBtn != null) stopExpBtn.gameObject.SetActive(false);
                if (recordBtn != null) recordBtn.gameObject.SetActive(false);
                if (directionBtn != null)
                {
                    directionBtn.gameObject.SetActive(true);
                    SetButtonLabel(directionBtn, "CONFIRM →");
                }
                SetNextButtonVisible(true);
                break;
            case MotionExperimentStep.MotionTrials:
                ShowLab("Select a target distance, press START EXPERIMENT, then RECORD the time and position. Repeat for 1 m to 5 m.", "NEXT STEP", true);
                SetTargetButtons(true);
                SetNextButtonVisible(MotionDataManager.Instance != null && MotionDataManager.Instance.CompletedTrialCount() >= 1);
                break;
            case MotionExperimentStep.DistanceVsDisplacement:
                ShowLab("The car moves 0 m → 3 m → 1 m. Distance is the whole path. Displacement is final minus initial, with direction.", "CONTINUE", true);
                SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case MotionExperimentStep.SpeedCalculation:
                ShowLab("Calculate speed = distance / time using a recorded trial.", "CONTINUE", false);
                ShowCalc("Enter speed (m/s) for your recorded trial.\nSpeed = distance / time");
                SetNextButtonVisible(false);
                break;
            case MotionExperimentStep.VelocityCalculation:
                ShowLab("Calculate velocity = displacement / time. Include direction → for forward motion.", "CONTINUE", false);
                ShowCalc("Enter velocity (m/s). Velocity = displacement / time");
                SetNextButtonVisible(false);
                break;
            case MotionExperimentStep.AccelerationExperiment:
                ShowLab("Investigating Acceleration. Choose LOW, MEDIUM or HIGH SPEED, start the run, then RECORD. a = (v − u) / t", "CONTINUE", true);
                accelPanel?.SetActive(true);
                ShowCalc("Optional: enter acceleration (m/s²) after recording.");
                SetNextButtonVisible(false);
                break;
            case MotionExperimentStep.Deceleration:
                ShowLab("Observe what happens when the moving car slows down. Negative acceleration represents deceleration.", "CONTINUE", true);
                SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case MotionExperimentStep.ObservationTable:
                dataTablePanel?.SetActive(true);
                MotionObservationTableManager.Instance?.Refresh();
                SetInstruction("Compare your recorded trials. Speed and velocity were calculated from your measurements.");
                SetButtonLabel(nextBtn, "VIEW GRAPHS");
                break;
            case MotionExperimentStep.Graphs:
                graphPanel?.SetActive(true);
                SetInstruction("These graphs are drawn from your experimental readings, not from hard-coded values.");
                SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case MotionExperimentStep.CompareDistanceDisplacement:
                comparePanel?.SetActive(true);
                if (compareText != null)
                    compareText.text =
                        "COMPARE DISTANCE AND DISPLACEMENT\n\n" +
                        "The car starts at 0 m, moves to 4 m, and then returns to 2 m.\n\n" +
                        "Calculate:\nDistance  = total path travelled\nDisplacement = final position − initial position\n\n" +
                        "Enter your answers below.";
                SetInstruction("Enter distance and displacement for this scenario.");
                SetNextButtonVisible(false);
                SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case MotionExperimentStep.Questions:
                questionPanel?.SetActive(true);
                SetInstruction("Answer the questions. Each correct answer scores +5. An incorrect answer scores −5.");
                SetNextButtonVisible(false);
                break;
            case MotionExperimentStep.Conclusion:
                conclusionPanel?.SetActive(true);
                if (conclusionText != null)
                    conclusionText.text =
                        "CONCLUSION\n\n" +
                        "Distance is the total path travelled by an object.\n\n" +
                        "Displacement depends only on the initial and final positions and has a direction.\n\n" +
                        "Speed is the rate of change of distance.  speed = distance / time\n\n" +
                        "Velocity is the rate of change of displacement.  velocity = displacement / time\n\n" +
                        "Acceleration is the rate of change of velocity.  acceleration = change in velocity / time\n\n" +
                        "Negative acceleration represents deceleration.";
                SetInstruction("Read the conclusion, then view your final score.");
                SetButtonLabel(nextBtn, "VIEW SCORE");
                break;
            case MotionExperimentStep.Complete:
                resultPanel?.SetActive(true);
                SetNextButtonVisible(false);
                break;
        }
        UpdateStepLabel(step);
    }

    private void ShowLab(string instruction, string nextLabel, bool motionControls)
    {
        laboratoryPanel?.SetActive(true);
        SetInstruction(instruction);
        SetButtonLabel(nextBtn, nextLabel);
        SetLabControls(motionControls);
    }

    private void ShowCalc(string prompt)
    {
        calcPanel?.SetActive(true);
        if (calcPromptText != null) calcPromptText.text = prompt;
        if (calcInput != null) calcInput.text = "";
    }

    public void ShowQuestion(int number, int total, string prompt, string a, string b, string c, string d)
    {
        questionPanel?.SetActive(true);
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        optionsGroup?.SetActive(true);
        numericGroup?.SetActive(false);
        if (questionText != null) questionText.text = $"QUESTION {number} / {total}\n\n{prompt}";
        SetOption(optAText, "A. " + a);
        SetOption(optBText, "B. " + b);
        SetOption(optCText, "C. " + c);
        SetOption(optDText, "D. " + d);
        EnableQuestionButtons(true);
    }

    public void ShowNumericQuestion(int number, int total, string prompt)
    {
        questionPanel?.SetActive(true);
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        optionsGroup?.SetActive(false);
        numericGroup?.SetActive(true);
        if (numericInput != null) numericInput.text = "";
        if (questionText != null) questionText.text = $"QUESTION {number} / {total}\n\n{prompt}";
    }

    public void ShowQuestionExplanation(string explanation, bool correct)
    {
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(true);
        if (questionExplanationText != null)
        {
            questionExplanationText.text = (correct ? "✓ CORRECT\n" : "✗ INCORRECT\n") + explanation;
            questionExplanationText.color = correct ? new Color(0.9f, 1f, 0.9f) : Color.white;
        }
        EnableQuestionButtons(false);
    }

    public void SetLabControls(bool motion)
    {
        if (actionRow != null) actionRow.SetActive(motion);
        if (startExpBtn != null) startExpBtn.gameObject.SetActive(motion);
        if (stopExpBtn != null) stopExpBtn.gameObject.SetActive(motion);
        if (resetRunBtn != null) resetRunBtn.gameObject.SetActive(false);
        if (recordBtn != null) recordBtn.gameObject.SetActive(motion);
        if (directionBtn != null) directionBtn.gameObject.SetActive(false);
        if (targetRow != null) targetRow.SetActive(false);
        SetTargetButtons(false);
        accelPanel?.SetActive(false);
    }

    public void SetTargetButtons(bool on)
    {
        if (targetRow != null) targetRow.SetActive(on);
        if (targetButtons == null) return;
        foreach (var b in targetButtons)
            if (b != null) b.gameObject.SetActive(on);
    }

    public void SetNextButtonVisible(bool visible)
    {
        if (nextBtn != null) nextBtn.gameObject.SetActive(visible);
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null) scoreText.text = $"SCORE {score}/100";
    }

    public void UpdateProgress(int step, int total)
    {
        if (progressText != null) progressText.text = $"Step: {step} / {total}";
        if (progressBarFill != null) progressBarFill.fillAmount = total > 0 ? step / (float)total : 0f;
    }

    public void UpdateAttemptsDisplay(int remaining)
    {
        if (attemptsText != null) attemptsText.text = $"Attempts Remaining: {remaining}";
    }

    public void UpdateStepLabel(MotionExperimentStep step)
    {
        if (stepText == null) return;
        stepText.text = step.ToString();
    }

    public void SetInstruction(string text)
    {
        if (instructionText != null) instructionText.text = text;
    }

    public void UpdateStopwatchDisplay(float seconds)
    {
        if (stopwatchText != null) stopwatchText.text = $"{seconds:00.00} s";
    }

    public void UpdateLiveReadings(float time, float position, float distance, float displacement, float speed, float velocity, float acceleration)
    {
        if (liveReadingsText == null) return;
        string dir = velocity > 0.0001f ? "→" : velocity < -0.0001f ? "←" : "•";
        liveReadingsText.text =
            $"TIME: {time:0.00} s\n" +
            $"POSITION: {position:0.00} m\n" +
            $"DISTANCE: {distance:0.00} m\n" +
            $"DISPLACEMENT: {displacement:+0.00;-0.00} m\n" +
            $"SPEED: {speed:0.00} m/s\n" +
            $"VELOCITY: {velocity:0.00} m/s {dir}\n" +
            $"ACCELERATION: {acceleration:0.00} m/s²";
    }

    public void RefreshTrialStatus()
    {
        int n = MotionDataManager.Instance != null ? MotionDataManager.Instance.CompletedTrialCount() : 0;
        SetInstruction($"Trials recorded: {n}/5. Select another target or continue when ready.");
    }

    public void ShowResult(int score, bool passed, int mistakes, MotionAttemptRecord attempt)
    {
        HideAllContent();
        resultPanel?.SetActive(true);
        instructionBar?.SetActive(false);
        SetNextButtonVisible(false);
        if (retryBtn != null)
            retryBtn.gameObject.SetActive(MotionAttemptManager.Instance == null || MotionAttemptManager.Instance.CanRetry());
        MotionResultManager.Instance?.ShowResult(score, passed, mistakes, attempt);
    }

    public void HideResult()
    {
        resultPanel?.SetActive(false);
        if (retryBtn != null) retryBtn.gameObject.SetActive(false);
    }

    private void HideAllContent()
    {
        introPanel?.SetActive(false);
        objectivePanel?.SetActive(false);
        equipmentPanel?.SetActive(false);
        laboratoryPanel?.SetActive(false);
        dataTablePanel?.SetActive(false);
        comparePanel?.SetActive(false);
        graphPanel?.SetActive(false);
        questionPanel?.SetActive(false);
        conclusionPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        calcPanel?.SetActive(false);
        accelPanel?.SetActive(false);
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
    }

    private void EnableQuestionButtons(bool on)
    {
        if (questionA != null) questionA.interactable = on;
        if (questionB != null) questionB.interactable = on;
        if (questionC != null) questionC.interactable = on;
        if (questionD != null) questionD.interactable = on;
    }

    private static void SetOption(TextMeshProUGUI tmp, string text)
    {
        if (tmp != null) tmp.text = text;
    }

    private static void SetButtonLabel(Button btn, string label)
    {
        if (btn == null) return;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = label;
    }

    private static MotionExperimentStep Current()
    {
        return MotionExperimentManager.Instance != null
            ? MotionExperimentManager.Instance.CurrentStep
            : MotionExperimentStep.Introduction;
    }
}

public class MotionIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (MotionUIManager.Instance != null) MotionUIManager.Instance.StartPractical();
    }
}

public class MotionConfirmStartClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (MotionExperimentManager.Instance != null &&
            MotionExperimentManager.Instance.CurrentStep == MotionExperimentStep.PlaceCar)
            MotionTrackController.Instance?.ConfirmCarStart();
    }
}
