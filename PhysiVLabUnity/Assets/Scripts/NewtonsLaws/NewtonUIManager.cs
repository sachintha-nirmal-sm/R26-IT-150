using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewtonUIManager : MonoBehaviour
{
    public static NewtonUIManager Instance { get; private set; }

    private TextMeshProUGUI titleText, scoreText, progressText, attemptsText, stepText, instructionText;
    private Image progressBarFill;
    private GameObject introPanel, objectivePanel, instructionBar, equipmentPanel, laboratoryPanel;
    private GameObject dataTablePanel, graphPanel, questionPanel, conclusionPanel, resultPanel, matchingPanel;
    private GameObject calcPanel, actionRow, forceRow, massRow, frictionRow, thirdObsPanel;
    private TextMeshProUGUI introText, objectiveText, conclusionText, questionText, dataTableText;
    private TextMeshProUGUI finalScoreText, resultDetailsText, statusText, liveReadingsText, stopwatchText, calcPromptText, formulaText, springText;
    private Button startBtn, nextBtn, resetBtn, retryBtn, viewProfileBtn, viewResultsBtn;
    private Button startExpBtn, stopExpBtn, resetRunBtn, recordBtn, actionBtn;
    private Button questionA, questionB, questionC, questionD, questionContinue, numericSubmit, checkCalcBtn;
    private Button f1, f2, f3, f4, f5;
    private Button m05, m10, m15, m20, m40;
    private Button frictionLow, frictionHigh;
    private Button obsRest, obsMove, inflateBtn, hangBtn;
    private Button setupA, setupB, setupC, setupD;
    private string[] setupItemIds = { "", "", "", "" };
    private Button[] matchConcept, matchMeaning;
    private GameObject questionExplanationPanel, resetConfirmPanel, numericGroup, optionsGroup;
    private GameObject trackVisual, rulerVisual, pulleyVisual, stringVisual, hangerVisual, balloonObj, strawVisual;
    private GameObject actionArrow, reactionArrow, appliedArrow, netArrow;
    private TextMeshProUGUI questionExplanationText, optAText, optBText, optCText, optDText, matchingText;
    private TMP_InputField numericInput, calcInput;
    private Button resetYes, resetNo;
    private NewtonUIRefs bound;
    private GameObject resultBanner;
    private TextMeshProUGUI resultBannerText;
    private GameObject weightStage, weightObjectVisual;
    private TextMeshProUGUI weightReadingText;
    private float lastStartClick = -1f;

    private void Awake() => Instance = this;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void BindAll(NewtonUIRefs refs, bool showWelcome)
    {
        bound = refs;
        titleText = refs.Title; scoreText = refs.Score; progressText = refs.Progress;
        attemptsText = refs.Attempts; stepText = refs.StepLabel;
        progressBarFill = refs.ProgressBar;
        instructionText = refs.Instruction;
        introPanel = refs.IntroPanel; objectivePanel = refs.ObjectivePanel;
        instructionBar = refs.InstructionBar; equipmentPanel = refs.EquipmentPanel;
        laboratoryPanel = refs.LaboratoryPanel;
        dataTablePanel = refs.DataTablePanel;
        graphPanel = refs.GraphPanel; questionPanel = refs.QuestionPanel;
        conclusionPanel = refs.ConclusionPanel; resultPanel = refs.ResultPanel;
        matchingPanel = refs.MatchingPanel;
        calcPanel = refs.CalcPanel; actionRow = refs.ActionRow;
        forceRow = refs.ForceRow; massRow = refs.MassRow; frictionRow = refs.FrictionRow;
        thirdObsPanel = refs.ThirdObsPanel;
        introText = refs.IntroText; objectiveText = refs.ObjectiveText;
        conclusionText = refs.ConclusionText;
        questionText = refs.QuestionText; dataTableText = refs.DataTableText;
        finalScoreText = refs.FinalScore; resultDetailsText = refs.ResultDetails; statusText = refs.StatusText;
        liveReadingsText = refs.LiveReadings; stopwatchText = refs.StopwatchText; calcPromptText = refs.CalcPrompt;
        formulaText = refs.FormulaText; springText = refs.SpringText; matchingText = refs.MatchingText;
        startBtn = refs.StartBtn; nextBtn = refs.Next; resetBtn = refs.Reset;
        retryBtn = refs.Retry; viewProfileBtn = refs.ViewProfileBtn; viewResultsBtn = refs.ViewResultsBtn;
        startExpBtn = refs.StartExpBtn; stopExpBtn = refs.StopExpBtn;
        resetRunBtn = refs.ResetRunBtn; recordBtn = refs.RecordBtn; actionBtn = refs.ActionBtn;
        questionA = refs.QuestionA; questionB = refs.QuestionB; questionC = refs.QuestionC; questionD = refs.QuestionD;
        questionContinue = refs.QuestionContinue; numericSubmit = refs.NumericSubmit; checkCalcBtn = refs.CheckCalcBtn;
        f1 = refs.Force1; f2 = refs.Force2; f3 = refs.Force3; f4 = refs.Force4; f5 = refs.Force5;
        m05 = refs.Mass05; m10 = refs.Mass10; m15 = refs.Mass15; m20 = refs.Mass20; m40 = refs.Mass40;
        frictionLow = refs.FrictionLow; frictionHigh = refs.FrictionHigh;
        obsRest = refs.ObsRest; obsMove = refs.ObsMove;
        inflateBtn = refs.InflateBtn; hangBtn = refs.HangBtn;
        setupA = refs.SetupA; setupB = refs.SetupB; setupC = refs.SetupC; setupD = refs.SetupD;
        matchConcept = new[] { refs.MatchC1, refs.MatchC2, refs.MatchC3, refs.MatchC4 };
        matchMeaning = new[] { refs.MatchM1, refs.MatchM2, refs.MatchM3, refs.MatchM4 };
        questionExplanationPanel = refs.QuestionExplanationPanel;
        questionExplanationText = refs.QuestionExplanationText;
        optAText = refs.OptAText; optBText = refs.OptBText; optCText = refs.OptCText; optDText = refs.OptDText;
        numericInput = refs.NumericInput; calcInput = refs.CalcInput;
        numericGroup = refs.NumericGroup; optionsGroup = refs.OptionsGroup;
        resetConfirmPanel = refs.ResetConfirm; resetYes = refs.ResetYes; resetNo = refs.ResetNo;
        trackVisual = refs.TrackVisual; rulerVisual = refs.RulerVisual;
        pulleyVisual = refs.PulleyVisual; stringVisual = refs.StringVisual;
        hangerVisual = refs.HangerVisual; balloonObj = refs.BalloonObj; strawVisual = refs.StrawVisual;
        actionArrow = refs.ActionArrow; reactionArrow = refs.ReactionArrow;
        appliedArrow = refs.AppliedArrow; netArrow = refs.NetArrow;
        resultBanner = refs.ResultBanner;
        resultBannerText = refs.ResultBannerText;
        weightStage = refs.WeightStage;
        weightObjectVisual = refs.WeightObjectVisual;
        weightReadingText = refs.WeightReadingText;
        WireButtons();
        if (showWelcome) ShowIntro();
    }

    private void WireButtons()
    {
        WireBtn(startBtn, StartPractical);
        WireBtn(nextBtn, OnNextPressed);
        WireBtn(resetBtn, () => ShowConfirm(false));
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => ShowConfirm(true));
        WireBtn(viewProfileBtn, () =>
        {
            string summary = NewtonProfileManager.Instance != null ? NewtonProfileManager.Instance.GetProfileSummary() : "No profile data.";
            NewtonFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireBtn(startExpBtn, HandleStartExperiment);
        WireBtn(stopExpBtn, OnStopExperiment);
        WireBtn(resetRunBtn, () => NewtonsLawsExperimentManager.Instance?.ResetCurrentActivity());
        WireBtn(recordBtn, OnRecord);
        WireBtn(actionBtn, OnContextAction);
        WireBtn(f1, () => NewtonForceController.Instance?.SetForce(1f));
        WireBtn(f2, () => NewtonForceController.Instance?.SetForce(2f));
        WireBtn(f3, () => NewtonForceController.Instance?.SetForce(3f));
        WireBtn(f4, () => NewtonForceController.Instance?.SetForce(4f));
        WireBtn(f5, () => NewtonForceController.Instance?.SetForce(5f));
        WireBtn(m05, () => NewtonMassController.Instance?.SetMass(0.5f));
        WireBtn(m10, () => NewtonMassController.Instance?.SetMass(1f));
        WireBtn(m15, () => NewtonMassController.Instance?.SetMass(1.5f));
        WireBtn(m20, () => NewtonMassController.Instance?.SetMass(2f));
        WireBtn(m40, () => NewtonMassController.Instance?.SetMass(4f));
        WireBtn(frictionLow, () => FirstLawExperimentManager.Instance?.SetFriction(true));
        WireBtn(frictionHigh, () => FirstLawExperimentManager.Instance?.SetFriction(false));
        WireBtn(obsRest, () => FirstLawExperimentManager.Instance?.RecordStationaryObservation(true));
        WireBtn(obsMove, () => FirstLawExperimentManager.Instance?.RecordMovingObservation(true));
        WireBtn(inflateBtn, () => ThirdLawExperimentManager.Instance?.InflateBalloon());
        WireBtn(hangBtn, () => WeightExperimentManager.Instance?.MeasureWeight());
        WireBtn(setupA, () => PlaceSetupItem(0));
        WireBtn(setupB, () => PlaceSetupItem(1));
        WireBtn(setupC, () => PlaceSetupItem(2));
        WireBtn(setupD, () => PlaceSetupItem(3));
        WireBtn(questionA, () => OnQuestionChoice(0));
        WireBtn(questionB, () => OnQuestionChoice(1));
        WireBtn(questionC, () => OnQuestionChoice(2));
        WireBtn(questionD, () => OnQuestionChoice(3));
        WireBtn(questionContinue, OnQuestionContinue);
        WireBtn(numericSubmit, SubmitNumericQuestion);
        WireBtn(checkCalcBtn, SubmitCalculation);
        WireBtn(refsBtn(bound != null ? bound.Weight05 : null), () => WeightExperimentManager.Instance?.SelectObjectIndex(0));
        WireBtn(refsBtn(bound != null ? bound.Weight10 : null), () => WeightExperimentManager.Instance?.SelectObjectIndex(1));
        WireBtn(refsBtn(bound != null ? bound.Weight20 : null), () => WeightExperimentManager.Instance?.SelectObjectIndex(2));
        WireBtn(refsBtn(bound != null ? bound.WrongObs : null), () =>
        {
            var step = Current();
            if (step == NewtonExperimentStep.FirstLawStationary)
                FirstLawExperimentManager.Instance?.RecordStationaryObservation(false);
            else if (step == NewtonExperimentStep.FirstLawMoving)
                FirstLawExperimentManager.Instance?.RecordMovingObservation(false);
        });
        WireBtn(refsBtn(bound != null ? bound.ExplainBtn : null), () => FirstLawExperimentManager.Instance?.ConfirmExplanation());
        WireBtn(refsBtn(bound != null ? bound.RecordWeightBtn : null), () => WeightExperimentManager.Instance?.RecordMeasurement());
        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            WireBtn(matchConcept != null && i < matchConcept.Length ? matchConcept[i] : null, () => ConceptMatchingManager.Instance?.SelectConcept(idx));
            WireBtn(matchMeaning != null && i < matchMeaning.Length ? matchMeaning[i] : null, () => ConceptMatchingManager.Instance?.SelectMeaning(idx));
        }
    }

    private static Button refsBtn(Button b) => b;

    private void WireBtn(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    public void StartPractical() => NewtonsLawsExperimentManager.Instance?.StartPractical();

    private void ShowConfirm(bool retry)
    {
        if (resetConfirmPanel == null)
        {
            if (retry) NewtonsLawsExperimentManager.Instance?.RetryExperiment();
            else
            {
                NewtonsLawsExperimentManager.Instance?.ResetExperiment();
                ShowIntro();
            }
            return;
        }
        var msg = resetConfirmPanel.transform.Find("ResetMsg")?.GetComponent<TextMeshProUGUI>();
        if (msg != null)
            msg.text = retry ? "Do you want to retry this practical?" : "Are you sure you want to restart the practical?";
        resetConfirmPanel.SetActive(true);
        WireBtn(resetYes, () =>
        {
            resetConfirmPanel.SetActive(false);
            if (retry) NewtonsLawsExperimentManager.Instance?.RetryExperiment();
            else
            {
                NewtonsLawsExperimentManager.Instance?.ResetExperiment();
                ShowIntro();
            }
        });
    }

    private void OnNextPressed()
    {
        var step = Current();
        if (step == NewtonExperimentStep.Introduction) { StartPractical(); return; }
        if (step == NewtonExperimentStep.SelectEquipment)
        {
            NewtonsLawsExperimentManager.Instance?.TryAdvanceFromEquipment();
            return;
        }
        if (step == NewtonExperimentStep.Questions || step == NewtonExperimentStep.ThirdLawObservation)
        {
            OnQuestionContinue();
            return;
        }
        NewtonsLawsExperimentManager.Instance?.AdvanceStep();
    }

    public void HandleStartExperiment()
    {
        if (Time.unscaledTime - lastStartClick < 0.2f) return;
        lastStartClick = Time.unscaledTime;

        var step = Current();
        SetInstruction("START pressed — running the experiment.");
        Debug.Log("Newton's Laws: START pressed on step " + step);

        if (step == NewtonExperimentStep.FirstLawStationary || step == NewtonExperimentStep.FirstLawFriction)
            FirstLawExperimentManager.Instance?.StartFirstLawExperiment();
        else if (step == NewtonExperimentStep.FirstLawMoving)
            FirstLawExperimentManager.Instance?.ApplyInitialPush();
        else if (step == NewtonExperimentStep.SecondLawConstantMass || step == NewtonExperimentStep.SecondLawConstantForce || step == NewtonExperimentStep.SecondLawSetup)
            SecondLawExperimentManager.Instance?.StartExperiment();
        else if (step == NewtonExperimentStep.ThirdLawExperiment)
            ThirdLawExperimentManager.Instance?.ReleaseBalloon();
        else
        {
            FirstLawExperimentManager.Instance?.StartFirstLawExperiment();
            NewtonFeedbackManager.Instance?.ShowInstruction("START runs the current motion experiment.");
        }
    }

    private void OnStopExperiment()
    {
        FirstLawExperimentManager.Instance?.StopExperiment();
        SecondLawExperimentManager.Instance?.StopExperiment();
        TrolleyController.Instance?.Stop();
    }

    private void OnRecord()
    {
        var step = Current();
        if (step == NewtonExperimentStep.SecondLawConstantMass || step == NewtonExperimentStep.SecondLawConstantForce)
            SecondLawExperimentManager.Instance?.RecordTrial();
        else if (step == NewtonExperimentStep.WeightExperiment)
            WeightExperimentManager.Instance?.RecordMeasurement();
        else
            NewtonFeedbackManager.Instance?.ShowInstruction("RECORD is used after a second-law run or weight measurement.");
    }

    private void OnContextAction()
    {
        var step = Current();
        if (step == NewtonExperimentStep.FirstLawMoving) FirstLawExperimentManager.Instance?.ApplyInitialPush();
        else if (step == NewtonExperimentStep.ThirdLawExperiment) ThirdLawExperimentManager.Instance?.InflateBalloon();
        else if (step == NewtonExperimentStep.WeightExperiment) WeightExperimentManager.Instance?.MeasureWeight();
        else if (step == NewtonExperimentStep.FirstLawSetup)
            NewtonEquipmentSnapController.Instance?.AcceptItem("Trolley", true);
    }

    private void OnQuestionChoice(int index)
    {
        var step = Current();
        if (step == NewtonExperimentStep.ThirdLawObservation)
        {
            bool firstQuestion = questionText != null && questionText.text.Contains("air moves backward");
            if (firstQuestion)
            {
                bool ok = index == 0;
                ThirdLawExperimentManager.Instance?.AnswerForwardQuestion(index);
                ShowQuestionExplanation(
                    ok
                        ? "The balloon moves forward when air moves backward. Action and reaction are opposite."
                        : "The balloon moves forward — opposite to the escaping air.",
                    ok);
                SetButtonLabel(nextBtn, "NEXT QUESTION");
                SetButtonLabel(questionContinue, "NEXT QUESTION");
            }
            else
            {
                bool ok = index == 2;
                ThirdLawExperimentManager.Instance?.AnswerLawQuestion(index);
                ShowQuestionExplanation(
                    ok
                        ? "Newton's Third Law: for every action there is an equal and opposite reaction."
                        : "This is Newton's Third Law.",
                    ok);
                SetButtonLabel(nextBtn, "NEXT STEP");
                SetButtonLabel(questionContinue, "NEXT STEP");
            }
            return;
        }
        NewtonQuestionManager.Instance?.Answer(index);
    }

    private void OnQuestionContinue()
    {
        var step = Current();
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        if (step == NewtonExperimentStep.ThirdLawObservation)
        {
            if (ThirdLawExperimentManager.Instance != null && ThirdLawExperimentManager.Instance.ObservationComplete)
                NewtonsLawsExperimentManager.Instance?.AdvanceStep();
            else
                ShowThirdLawQuestion2();
            return;
        }
        bool done = NewtonQuestionManager.Instance != null && NewtonQuestionManager.Instance.Advance();
        if (done) NewtonsLawsExperimentManager.Instance?.AdvanceStep();
    }

    private void SubmitNumericQuestion()
    {
        if (numericInput == null) return;
        string raw = numericInput.text != null ? numericInput.text.Trim().Replace(',', '.') : "";
        if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float value))
            NewtonQuestionManager.Instance?.AnswerNumeric(value);
        else
            NewtonFeedbackManager.Instance?.ShowInstruction("Enter a number, for example 19.6");
    }

    private void SubmitCalculation()
    {
        if (calcInput == null || !float.TryParse(calcInput.text, out float value))
        {
            NewtonFeedbackManager.Instance?.ShowInstruction("Enter your calculated value.");
            return;
        }
        var step = Current();
        if (step == NewtonExperimentStep.WeightExperiment)
            WeightExperimentManager.Instance?.CheckStudentWeight(value);
        else if (step == NewtonExperimentStep.SecondLawConstantMass || step == NewtonExperimentStep.SecondLawConstantForce)
        {
            float expected = SecondLawExperimentManager.Instance != null ? SecondLawExperimentManager.Instance.CalculateAcceleration() : 0f;
            bool ok = NewtonAccelerationCalculator.Instance != null &&
                      NewtonAccelerationCalculator.Instance.ValidateStudentAnswer(value, expected, 0.05f);
            if (ok)
            {
                NewtonScoreManager.Instance?.AddScore(5, false);
                NewtonFeedbackManager.Instance?.ShowMessage($"✓ Correct. a = F/m = {expected:0.00} m/s²", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
                SetNextButtonVisible(true);
            }
            else
            {
                NewtonScoreManager.Instance?.SubtractScore(5);
                NewtonFeedbackManager.Instance?.ShowMessage("✗ Use a = F / m with your experimental values.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            }
        }
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
                "NEWTON'S LAWS OF MOTION\n\n" +
                "Investigating Newton's First, Second and Third Laws\n\n" +
                "In this practical you will investigate how force affects the motion of objects. " +
                "You will perform experiments to understand Newton's three laws of motion and calculate the weight of an object.";
        }
        UpdateAttemptsDisplay(NewtonAttemptManager.Instance != null ? NewtonAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateScoreDisplay(0);
        UpdateProgress(1, 21);
        if (stepText != null) stepText.text = "Introduction";
    }

    public void ShowStep(NewtonExperimentStep step)
    {
        HideAllContent();
        if (instructionBar != null) instructionBar.SetActive(step != NewtonExperimentStep.Introduction && step != NewtonExperimentStep.Complete);
        SetNextButtonVisible(true);
        SetLabControls(false);
        calcPanel?.SetActive(false);
        ShowForceArrows(false);
        ActionReactionController.Instance?.Hide();

        switch (step)
        {
            case NewtonExperimentStep.Introduction:
                ShowIntro();
                break;
            case NewtonExperimentStep.Objective:
                objectivePanel?.SetActive(true);
                if (objectiveText != null)
                    objectiveText.text =
                        "OBJECTIVES\n\n" +
                        "By completing this practical you will:\n\n" +
                        "• Investigate Newton's First Law.\n" +
                        "• Investigate the relationship between force, mass and acceleration.\n" +
                        "• Investigate Newton's Third Law.\n" +
                        "• Measure/calculate the weight of an object.\n" +
                        "• Use F = ma.\n" +
                        "• Use W = mg.\n" +
                        "• Observe action and reaction forces.\n" +
                        "• Record experimental observations.";
                SetInstruction("Read the objectives, then press CONTINUE.");
                SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case NewtonExperimentStep.SelectEquipment:
                equipmentPanel?.SetActive(true);
                NewtonEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                SetInstruction("Select all equipment required for the Newton's Laws practical. Drag or tap correct items into the required equipment area.");
                SetButtonLabel(nextBtn, "NEXT STEP");
                break;
            case NewtonExperimentStep.FirstLawSetup:
                ShowLab("Activity 1 – Newton's First Law. Place the straight track, ruler and trolley at 0 m.", "NEXT STEP", false);
                NewtonEquipmentSnapController.Instance?.RefreshForStep(NewtonExperimentStep.FirstLawSetup);
                ShowSetupButtons(
                    ("Track", "PLACE TRACK"),
                    ("Ruler", "PLACE RULER"),
                    ("Trolley", "PLACE TROLLEY"));
                SetNextButtonVisible(true);
                ShowTrackResult("Tap PLACE TRACK, PLACE RULER and PLACE TROLLEY. Then press NEXT STEP.", true);
                break;
            case NewtonExperimentStep.FirstLawStationary:
                ShowLab("CONDITION A: No push. Press the green START. The trolley stays still — that is the experiment. Then tap Remains at rest.", "NEXT STEP", true);
                if (recordBtn != null) recordBtn.gameObject.SetActive(false);
                frictionRow?.SetActive(false);
                forceRow?.SetActive(false);
                massRow?.SetActive(false);
                ShowObsButtons(true, false);
                HideThirdLawExtras();
                SetNextButtonVisible(false);
                ShowTrackResult("Press START to run Condition A (no push).", true);
                UpdateLiveNewtonReadings(1f, 0f, 0f, 0f, 0f, 9.8f, true);
                if (startExpBtn != null)
                {
                    startExpBtn.gameObject.SetActive(true);
                    startExpBtn.interactable = true;
                    startExpBtn.transform.SetAsLastSibling();
                }
                if (actionRow != null)
                {
                    actionRow.SetActive(true);
                    actionRow.transform.SetAsLastSibling();
                }
                break;
            case NewtonExperimentStep.FirstLawMoving:
                ShowLab("CONDITION B: Give an initial push, then the applied force is 0 N. On a low-friction track the trolley continues moving.", "NEXT STEP", true);
                SetButtonLabel(actionBtn, "PUSH");
                if (actionBtn != null) actionBtn.gameObject.SetActive(true);
                frictionRow?.SetActive(true);
                ShowObsButtons(false, true);
                SetNextButtonVisible(false);
                break;
            case NewtonExperimentStep.FirstLawFriction:
                ShowLab("Compare LOW FRICTION and HIGH FRICTION. Select LOW FRICTION. In real situations friction is an external force.", "NEXT STEP", true);
                frictionRow?.SetActive(true);
                SetButtonLabel(actionBtn, "PUSH");
                if (actionBtn != null) actionBtn.gameObject.SetActive(true);
                break;
            case NewtonExperimentStep.FirstLawObservation:
                ShowLab("Select the correct observations, then confirm the explanation of Newton's First Law.", "CONTINUE", false);
                ShowObsButtons(true, true);
                if (bound != null && bound.ExplainBtn != null) bound.ExplainBtn.gameObject.SetActive(true);
                SetNextButtonVisible(false);
                SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case NewtonExperimentStep.SecondLawSetup:
                ShowLab("Activity 2 – Force, Mass and Acceleration. Place trolley, pulley, string and weight hanger.", "NEXT STEP", false);
                NewtonEquipmentSnapController.Instance?.RefreshForStep(NewtonExperimentStep.SecondLawSetup);
                ShowSetupButtons(
                    ("Trolley", "PLACE TROLLEY"),
                    ("Pulley", "PLACE PULLEY"),
                    ("String", "PLACE STRING"),
                    ("Hanger", "PLACE HANGER"));
                bool secondReady = SecondLawExperimentManager.Instance != null && SecondLawExperimentManager.Instance.SetupComplete;
                SetNextButtonVisible(true);
                ShowTrackResult(
                    secondReady
                        ? "Setup complete. Press NEXT STEP."
                        : "Tap PLACE TROLLEY, PULLEY, STRING and HANGER. Then press NEXT STEP.",
                    true);
                break;
            case NewtonExperimentStep.SecondLawConstantMass:
                ShowLab("Keep mass = 1 kg. Test forces 1 N, 2 N, 3 N and 4 N. START, then RECORD. a = F / m", "NEXT STEP", true);
                forceRow?.SetActive(true);
                massRow?.SetActive(true);
                NewtonMassController.Instance?.SetMass(1f);
                ShowCalc("Optional: enter acceleration a = F/m (m/s²)");
                SetNextButtonVisible(false);
                break;
            case NewtonExperimentStep.SecondLawConstantForce:
                ShowLab("Keep force = 4 N. Change mass to 0.5, 1, 2 and 4 kg. As mass increases, acceleration decreases.", "NEXT STEP", true);
                forceRow?.SetActive(true);
                massRow?.SetActive(true);
                NewtonForceController.Instance?.SetForce(4f);
                ShowCalc("Optional: enter acceleration a = F/m (m/s²)");
                SetNextButtonVisible(false);
                break;
            case NewtonExperimentStep.SecondLawGraphs:
                graphPanel?.SetActive(true);
                SetInstruction("Graphs use your experimental readings. Force vs acceleration should be a straight line through the origin.");
                SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case NewtonExperimentStep.ThirdLawSetup:
                ShowLab("Activity 3 – Action and Reaction. Place string, straw, then attach the balloon.", "NEXT STEP", false);
                NewtonEquipmentSnapController.Instance?.RefreshForStep(NewtonExperimentStep.ThirdLawSetup);
                ShowSetupButtons(
                    ("String", "PLACE STRING"),
                    ("Straw", "PLACE STRAW"),
                    ("Balloon", "PLACE BALLOON"));
                SetNextButtonVisible(true);
                ShowTrackResult("Tap PLACE STRING, then STRAW, then BALLOON. Then press NEXT STEP.", true);
                break;
            case NewtonExperimentStep.ThirdLawExperiment:
                ShowLab("Inflate the balloon, then RELEASE. Air moves backward. Balloon moves forward.", "NEXT STEP", true);
                if (inflateBtn != null) inflateBtn.gameObject.SetActive(true);
                SetButtonLabel(startExpBtn, "RELEASE");
                SetButtonLabel(actionBtn, "INFLATE");
                if (actionBtn != null) actionBtn.gameObject.SetActive(true);
                SetNextButtonVisible(false);
                break;
            case NewtonExperimentStep.ThirdLawObservation:
                questionPanel?.SetActive(true);
                ShowQuestion(1, 2, "What happens when the air moves backward?",
                    "Balloon moves forward.", "Balloon remains stationary.", "Balloon moves backward.", "Balloon becomes heavier.");
                SetInstruction("Answer the action-reaction questions.");
                SetNextButtonVisible(false);
                break;
            case NewtonExperimentStep.WeightExperiment:
                ShowLab("Measuring and Calculating Weight. Select 0.5 kg, 1.0 kg or 2.0 kg, hang it, then calculate W = mg.", "CONTINUE", false);
                NewtonEquipmentSnapController.Instance?.RefreshForStep(NewtonExperimentStep.WeightExperiment);
                TrolleyController.Instance?.Stop();
                ShowWeightStage(true);
                ShowSetupButtons(
                    ("Mass05", "0.5 kg"),
                    ("Mass10", "1.0 kg"),
                    ("Mass20", "2.0 kg"),
                    ("Hang", "HANG OBJECT"));
                if (recordBtn != null)
                {
                    recordBtn.gameObject.SetActive(true);
                    SetButtonLabel(recordBtn, "RECORD WEIGHT");
                }
                ShowCalc("Enter calculated weight W = mg (N). g = 9.8 m/s^2");
                SetNextButtonVisible(false);
                ShowTrackResult("Tap 0.5 kg, 1.0 kg or 2.0 kg, then HANG OBJECT. Then enter W = mg and CHECK.", true);
                UpdateLiveNewtonReadings(0.5f, 0f, 0f, 0f, 0f, 4.9f, true);
                break;
            case NewtonExperimentStep.ObservationTables:
                dataTablePanel?.SetActive(true);
                NewtonObservationTableManager.Instance?.Refresh();
                SetInstruction("These tables use your experimental data.");
                SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case NewtonExperimentStep.Questions:
                questionPanel?.SetActive(true);
                SetInstruction("Answer the questions. Each correct answer scores +5. An incorrect answer scores −5.");
                SetNextButtonVisible(false);
                break;
            case NewtonExperimentStep.ConceptMatching:
                matchingPanel?.SetActive(true);
                SetInstruction("Match each law or quantity with its meaning.");
                SetNextButtonVisible(false);
                SetButtonLabel(nextBtn, "CONTINUE");
                break;
            case NewtonExperimentStep.Conclusion:
                conclusionPanel?.SetActive(true);
                if (conclusionText != null)
                    conclusionText.text =
                        "CONCLUSION\n\n" +
                        "Newton's First Law explains that an object remains at rest or continues with uniform velocity when no unbalanced force acts on it.\n\n" +
                        "Newton's Second Law shows that acceleration depends on the force and mass of an object.\n\n" +
                        "Newton's Third Law states that forces occur in equal and opposite pairs.\n\n" +
                        "The weight of an object is the gravitational force acting on it and is calculated using W = mg.";
                SetInstruction("Read the conclusion, then view your final score.");
                SetButtonLabel(nextBtn, "VIEW SCORE");
                break;
            case NewtonExperimentStep.Complete:
                resultPanel?.SetActive(true);
                SetNextButtonVisible(false);
                break;
        }
        UpdateStepLabel(step);
        UpdateFormula(step);
    }

    private void ShowThirdLawQuestion2()
    {
        ShowQuestion(2, 2, "Which law explains this?",
            "Newton's First Law", "Newton's Second Law", "Newton's Third Law", "W = mg");
    }

    private void ShowLab(string instruction, string nextLabel, bool motionControls)
    {
        laboratoryPanel?.SetActive(true);
        SetInstruction(instruction);
        SetButtonLabel(nextBtn, nextLabel);
        SetLabControls(motionControls);
        HideThirdLawExtras();
        if (Current() != NewtonExperimentStep.WeightExperiment)
            ShowWeightStage(false);
    }

    private void HideThirdLawExtras()
    {
        balloonObj?.SetActive(false);
        strawVisual?.SetActive(false);
        if (bound != null && bound.SpringVisual != null)
            bound.SpringVisual.SetActive(false);
    }

    private void ShowCalc(string prompt)
    {
        calcPanel?.SetActive(true);
        if (calcPromptText != null) calcPromptText.text = prompt;
        if (calcInput != null) calcInput.text = "";
    }

    private void ShowObsButtons(bool rest, bool move)
    {
        if (obsRest != null && obsRest.transform.parent != null)
            obsRest.transform.parent.gameObject.SetActive(rest || move);
        if (obsRest != null) obsRest.gameObject.SetActive(rest);
        if (obsMove != null) obsMove.gameObject.SetActive(move);
        if (bound != null && bound.WrongObs != null) bound.WrongObs.gameObject.SetActive(rest || move);
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
        if (questionContinue != null) questionContinue.gameObject.SetActive(false);
        SetNextButtonVisible(false);
        SetButtonLabel(nextBtn, "CONTINUE");
        SetButtonLabel(questionContinue, "CONTINUE");
    }

    public void ShowNumericQuestion(int number, int total, string prompt)
    {
        questionPanel?.SetActive(true);
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        optionsGroup?.SetActive(false);
        numericGroup?.SetActive(true);
        numericGroup?.transform.SetAsLastSibling();
        if (numericInput != null)
        {
            numericInput.text = "";
            numericInput.interactable = true;
            numericInput.gameObject.SetActive(true);
            numericInput.Select();
            numericInput.ActivateInputField();
        }
        if (numericSubmit != null)
        {
            numericSubmit.gameObject.SetActive(true);
            numericSubmit.interactable = true;
        }
        if (questionText != null) questionText.text = $"QUESTION {number} / {total}\n\n{prompt}";
        if (questionContinue != null) questionContinue.gameObject.SetActive(false);
        SetNextButtonVisible(false);
        SetInstruction("Type the number in the box, then press SUBMIT.");
    }

    public void ShowQuestionExplanation(string explanation, bool correct)
    {
        if (questionExplanationPanel != null)
        {
            questionExplanationPanel.SetActive(true);
            questionExplanationPanel.transform.SetAsLastSibling();
        }
        if (questionExplanationText != null)
        {
            questionExplanationText.text = (correct ? "✓ CORRECT\n" : "✗ INCORRECT\n") + explanation;
            questionExplanationText.color = correct ? new Color(0.9f, 1f, 0.9f) : Color.white;
        }
        EnableQuestionButtons(false);
        if (questionContinue != null)
        {
            questionContinue.gameObject.SetActive(true);
            questionContinue.interactable = true;
            questionContinue.transform.SetAsLastSibling();
        }
        SetNextButtonVisible(true);
        SetButtonLabel(nextBtn, "CONTINUE");
        SetButtonLabel(questionContinue, "CONTINUE");
        SetInstruction("Answer recorded. Press CONTINUE to go to the next question.");
    }

    public void ShowMatching(string[] concepts, string[] meanings, bool[] matched)
    {
        matchingPanel?.SetActive(true);
        if (matchingText != null)
            matchingText.text = "FINAL CONCEPT MATCHING\nTap a law on the left, then its meaning on the right.";
        for (int i = 0; i < 4; i++)
        {
            if (matchConcept != null && i < matchConcept.Length && matchConcept[i] != null)
            {
                SetButtonLabel(matchConcept[i], concepts[i] + (matched[i] ? "  ✓" : ""));
                matchConcept[i].interactable = !matched[i];
            }
            if (matchMeaning != null && i < matchMeaning.Length && matchMeaning[i] != null)
            {
                SetButtonLabel(matchMeaning[i], meanings[i] + (matched[i] ? "  ✓" : ""));
                matchMeaning[i].interactable = !matched[i];
            }
        }
    }

    private void PlaceSetupItem(int index)
    {
        if (setupItemIds == null || index < 0 || index >= setupItemIds.Length) return;
        string id = setupItemIds[index];
        if (string.IsNullOrEmpty(id)) return;
        if (id == "Hang")
        {
            WeightExperimentManager.Instance?.MeasureWeight();
            ShowTrackResult("Object hanging. Enter W = mg in the box, then press CHECK.", true);
            return;
        }
        if (id == "Mass05") { WeightExperimentManager.Instance?.SelectObjectIndex(0); ShowWeightObject(0.5f); return; }
        if (id == "Mass10") { WeightExperimentManager.Instance?.SelectObjectIndex(1); ShowWeightObject(1f); return; }
        if (id == "Mass20") { WeightExperimentManager.Instance?.SelectObjectIndex(2); ShowWeightObject(2f); return; }
        NewtonEquipmentSnapController.Instance?.PlaceById(id);
        var step = Current();
        if (step == NewtonExperimentStep.SecondLawSetup && SecondLawExperimentManager.Instance != null && SecondLawExperimentManager.Instance.SetupComplete)
            ShowTrackResult("Setup complete. Press NEXT STEP.", true);
        else if (step == NewtonExperimentStep.FirstLawSetup && FirstLawExperimentManager.Instance != null && FirstLawExperimentManager.Instance.SetupComplete)
            ShowTrackResult("Setup complete. Press NEXT STEP.", true);
        else if (step == NewtonExperimentStep.ThirdLawSetup && ThirdLawExperimentManager.Instance != null && ThirdLawExperimentManager.Instance.SetupComplete)
            ShowTrackResult("Setup complete. Press NEXT STEP.", true);
    }

    private void ShowSetupButtons(params (string id, string label)[] items)
    {
        if (actionRow != null)
        {
            actionRow.SetActive(true);
            actionRow.transform.SetAsLastSibling();
        }
        if (startExpBtn != null) startExpBtn.gameObject.SetActive(false);
        if (stopExpBtn != null) stopExpBtn.gameObject.SetActive(false);
        if (recordBtn != null) recordBtn.gameObject.SetActive(false);
        if (resetRunBtn != null) resetRunBtn.gameObject.SetActive(false);
        if (actionBtn != null) actionBtn.gameObject.SetActive(false);
        if (inflateBtn != null) inflateBtn.gameObject.SetActive(false);
        if (hangBtn != null) hangBtn.gameObject.SetActive(false);

        Button[] buttons = { setupA, setupB, setupC, setupD };
        for (int i = 0; i < buttons.Length; i++)
        {
            bool on = items != null && i < items.Length;
            if (buttons[i] != null) buttons[i].gameObject.SetActive(on);
            setupItemIds[i] = on ? items[i].id : "";
            if (on) SetButtonLabel(buttons[i], items[i].label);
        }
    }

    private void HideSetupButtons()
    {
        if (setupA != null) setupA.gameObject.SetActive(false);
        if (setupB != null) setupB.gameObject.SetActive(false);
        if (setupC != null) setupC.gameObject.SetActive(false);
        if (setupD != null) setupD.gameObject.SetActive(false);
        setupItemIds[0] = setupItemIds[1] = setupItemIds[2] = setupItemIds[3] = "";
    }

    public void SetLabControls(bool motion)
    {
        if (actionRow != null) actionRow.SetActive(motion);
        if (startExpBtn != null) startExpBtn.gameObject.SetActive(motion);
        if (stopExpBtn != null) stopExpBtn.gameObject.SetActive(motion);
        if (resetRunBtn != null) resetRunBtn.gameObject.SetActive(true);
        if (recordBtn != null) recordBtn.gameObject.SetActive(motion);
        if (actionBtn != null) actionBtn.gameObject.SetActive(false);
        forceRow?.SetActive(false);
        massRow?.SetActive(false);
        frictionRow?.SetActive(false);
        if (inflateBtn != null) inflateBtn.gameObject.SetActive(false);
        if (hangBtn != null) hangBtn.gameObject.SetActive(false);
        HideSetupButtons();
        ShowObsButtons(false, false);
        if (bound != null)
        {
            if (bound.Weight05 != null) bound.Weight05.gameObject.SetActive(false);
            if (bound.Weight10 != null) bound.Weight10.gameObject.SetActive(false);
            if (bound.Weight20 != null) bound.Weight20.gameObject.SetActive(false);
            if (bound.RecordWeightBtn != null) bound.RecordWeightBtn.gameObject.SetActive(false);
            if (bound.ExplainBtn != null) bound.ExplainBtn.gameObject.SetActive(false);
            if (bound.WrongObs != null) bound.WrongObs.gameObject.SetActive(false);
        }
        SetButtonLabel(startExpBtn, "START");
        SetButtonLabel(resetRunBtn, "RESET ACTIVITY");
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

    public void UpdateStepLabel(NewtonExperimentStep step)
    {
        if (stepText == null) return;
        switch (step)
        {
            case NewtonExperimentStep.FirstLawSetup:
            case NewtonExperimentStep.FirstLawStationary:
            case NewtonExperimentStep.FirstLawMoving:
            case NewtonExperimentStep.FirstLawFriction:
            case NewtonExperimentStep.FirstLawObservation:
                stepText.text = "Activity: Newton's First Law"; break;
            case NewtonExperimentStep.SecondLawSetup:
            case NewtonExperimentStep.SecondLawConstantMass:
            case NewtonExperimentStep.SecondLawConstantForce:
            case NewtonExperimentStep.SecondLawGraphs:
                stepText.text = "Activity: Newton's Second Law"; break;
            case NewtonExperimentStep.ThirdLawSetup:
            case NewtonExperimentStep.ThirdLawExperiment:
            case NewtonExperimentStep.ThirdLawObservation:
                stepText.text = "Activity: Newton's Third Law"; break;
            case NewtonExperimentStep.WeightExperiment:
                stepText.text = "Activity: Weight"; break;
            default:
                stepText.text = step.ToString(); break;
        }
    }

    public void SetInstruction(string text)
    {
        if (instructionText != null) instructionText.text = text;
    }

    public void ShowTrackResult(string text, bool show)
    {
        if (resultBanner != null) resultBanner.SetActive(show);
        if (resultBannerText != null && !string.IsNullOrEmpty(text)) resultBannerText.text = text;
    }

    public void ShowWeightStage(bool on)
    {
        if (weightStage != null) weightStage.SetActive(on);
        if (bound != null && bound.CarRect != null) bound.CarRect.gameObject.SetActive(!on);
        pulleyVisual?.SetActive(false);
        stringVisual?.SetActive(false);
        hangerVisual?.SetActive(false);
        balloonObj?.SetActive(false);
        strawVisual?.SetActive(false);
        if (on)
        {
            if (bound != null && bound.WeightSpring != null)
                SpringBalanceController.Instance?.Bind(bound.WeightSpring, null);
            else if (bound != null)
                SpringBalanceController.Instance?.Bind(bound.SpringVisual, bound.Pointer);
            ShowWeightObject(0f);
            if (weightReadingText != null) weightReadingText.text = "Reading: 0.00 N";
        }
    }

    public void ShowWeightObject(float massKg)
    {
        if (weightObjectVisual == null) return;
        weightObjectVisual.SetActive(massKg > 0.01f);
        var label = weightObjectVisual.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null && massKg > 0.01f) label.text = $"{massKg:0.0} kg";
    }

    public void UpdateSpringReading(float force)
    {
        if (springText != null) springText.text = $"Force: {force:0.00} N";
        if (weightReadingText != null) weightReadingText.text = $"Reading: {force:0.00} N";
        if (weightObjectVisual != null && force > 0.01f) weightObjectVisual.SetActive(true);
    }

    public void UpdateLiveNewtonReadings(float mass, float force, float acc, float vel, float time, float weight, bool forward)
    {
        if (liveReadingsText == null) return;
        string dir = vel > 0.01f ? "→" : vel < -0.01f ? "←" : "•";
        liveReadingsText.text =
            "LIVE READINGS\n\n" +
            $"Mass            {mass:0.00} kg\n" +
            $"Force           {force:0.00} N\n" +
            $"Acceleration    {acc:0.00} m/s^2\n" +
            $"Velocity        {vel:0.00} m/s  {dir}\n" +
            $"Time            {time:0.00} s\n" +
            $"Weight          {weight:0.00} N\n" +
            $"Direction       {(forward ? "right ->" : "left <-")}\n" +
            $"Net force       {force:0.00} N";
        if (stopwatchText != null) stopwatchText.text = $"{time:00.00} s";
    }

    public void HighlightForce(float force)
    {
        ColorOn(f1, Mathf.Abs(force - 1f) < 0.01f);
        ColorOn(f2, Mathf.Abs(force - 2f) < 0.01f);
        ColorOn(f3, Mathf.Abs(force - 3f) < 0.01f);
        ColorOn(f4, Mathf.Abs(force - 4f) < 0.01f);
        ColorOn(f5, Mathf.Abs(force - 5f) < 0.01f);
    }

    public void HighlightMass(float mass)
    {
        ColorOn(m05, Mathf.Abs(mass - 0.5f) < 0.01f);
        ColorOn(m10, Mathf.Abs(mass - 1f) < 0.01f);
        ColorOn(m15, Mathf.Abs(mass - 1.5f) < 0.01f);
        ColorOn(m20, Mathf.Abs(mass - 2f) < 0.01f);
        ColorOn(m40, Mathf.Abs(mass - 4f) < 0.01f);
    }

    public void HighlightFriction(bool low)
    {
        ColorOn(frictionLow, low);
        ColorOn(frictionHigh, !low);
    }

    public void HighlightWeightMass(float mass)
    {
        if (bound == null) return;
        ColorOn(bound.Weight05, Mathf.Abs(mass - 0.5f) < 0.01f);
        ColorOn(bound.Weight10, Mathf.Abs(mass - 1f) < 0.01f);
        ColorOn(bound.Weight20, Mathf.Abs(mass - 2f) < 0.01f);
    }

    public void ShowForceArrows(bool on)
    {
        if (appliedArrow != null) appliedArrow.SetActive(on);
        if (netArrow != null) netArrow.SetActive(on);
        if (bound != null && bound.ResistArrow != null) bound.ResistArrow.SetActive(on);
    }

    public void SetThirdLawVisual(string id, bool on)
    {
        if (id == "string" && stringVisual != null) stringVisual.SetActive(on);
        if (id == "straw" && strawVisual != null) strawVisual.SetActive(on);
        if (id == "balloon" && balloonObj != null) balloonObj.SetActive(on);
    }

    private void UpdateFormula(NewtonExperimentStep step)
    {
        if (formulaText == null) return;
        formulaText.text =
            "FORMULAS\n" +
            "2nd law:  F = ma     a = F/m\n" +
            "3rd law:  Action = Reaction\n" +
            "Weight:   W = mg     g = 9.8";
    }

    public void ShowResult(int score, bool passed, int mistakes, NewtonAttemptRecord attempt)
    {
        HideAllContent();
        resultPanel?.SetActive(true);
        instructionBar?.SetActive(false);
        SetNextButtonVisible(false);
        if (retryBtn != null)
            retryBtn.gameObject.SetActive(NewtonAttemptManager.Instance == null || NewtonAttemptManager.Instance.CanRetry());
        NewtonResultManager.Instance?.ShowResult(score, passed, mistakes, attempt);
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
        graphPanel?.SetActive(false);
        questionPanel?.SetActive(false);
        conclusionPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        matchingPanel?.SetActive(false);
        calcPanel?.SetActive(false);
        if (questionExplanationPanel != null) questionExplanationPanel.SetActive(false);
        ShowTrackResult("", false);
        ShowWeightStage(false);
    }

    private void EnableQuestionButtons(bool on)
    {
        if (questionA != null) questionA.interactable = on;
        if (questionB != null) questionB.interactable = on;
        if (questionC != null) questionC.interactable = on;
        if (questionD != null) questionD.interactable = on;
    }

    private static void ColorOn(Button btn, bool on)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = on ? new Color(0.12f, 0.62f, 0.35f) : new Color(0.15f, 0.48f, 0.78f);
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

    private static NewtonExperimentStep Current()
    {
        return NewtonsLawsExperimentManager.Instance != null
            ? NewtonsLawsExperimentManager.Instance.CurrentStep
            : NewtonExperimentStep.Introduction;
    }
}

public class NewtonIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (NewtonUIManager.Instance != null) NewtonUIManager.Instance.StartPractical();
    }
}

public class NewtonLabStartClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        NewtonUIManager.Instance?.HandleStartExperiment();
    }
}
