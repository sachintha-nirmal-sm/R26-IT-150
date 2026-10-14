using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerEnergyUIManager : MonoBehaviour
{
    public static PowerEnergyUIManager Instance { get; private set; }

    private TextMeshProUGUI titleText, scoreText, progressText, attemptsText, stepText, instructionText, applianceHeaderText;
    private Image progressBarFill;
    private GameObject introPanel, objectivePanel, instructionBar, equipmentPanel, laboratoryPanel;
    private GameObject experimentPanel, dataTablePanel, comparePanel, questionPanel, conclusionPanel, resultPanel;
    private GameObject formulaPanel, graphPanel;
    private TextMeshProUGUI introText, objectiveText, conclusionText, compareText, questionText, dataTableText;
    private TextMeshProUGUI finalScoreText, resultDetailsText, statusText, liveReadingsText, physicsText, conclusionPreview;
    private TextMeshProUGUI graphTitle, formulaHelp, calcPrompt, calcHint, voltageFieldLabel, currentFieldLabel;
    private TMP_InputField calcInput, numericInput;
    private Button startBtn, nextBtn, resetBtn, retryBtn, viewProfileBtn, viewResultsBtn;
    private Button questionA, questionB, questionC, questionD, questionContinue, calcSubmit, numericCheck;
    private Button[] applianceButtons;
    private Button switchBtn, takeVBtn, takeIBtn, time10, time30, time60, timerStart, timerStop, timerReset;
    private Button anotherBtn, graphPowerBtn, graphEnergyBtn;
    private GameObject questionExplanationPanel, resetConfirmPanel, retryConfirmPanel, optionsGroup, numericGroup;
    private GameObject selectApplianceGroup, readingsGroup, calcGroup, timeGroup, timerGroup;
    private TextMeshProUGUI questionExplanationText, optAText, optBText, optCText, optDText;
    private Button resetYes, resetNo, retryYes, retryNo;
    private Button[] phraseButtons;
    private Button variableContinueBtn, conclusionContinueBtn;

    private void Awake() => Instance = this;
    private void OnDestroy() { if (Instance == this) Instance = null; }

    public void BindAll(PowerEnergyUIRefs refs, bool showWelcome)
    {
        titleText = refs.Title; scoreText = refs.Score; progressText = refs.Progress;
        attemptsText = refs.Attempts; stepText = refs.StepLabel;
        applianceHeaderText = refs.ApplianceHeader;
        progressBarFill = refs.ProgressBar;
        instructionText = refs.Instruction;
        introPanel = refs.IntroPanel; objectivePanel = refs.ObjectivePanel;
        instructionBar = refs.InstructionBar; equipmentPanel = refs.EquipmentPanel;
        laboratoryPanel = refs.LaboratoryPanel; experimentPanel = refs.ExperimentPanel;
        dataTablePanel = refs.DataTablePanel; comparePanel = refs.ComparePanel;
        questionPanel = refs.QuestionPanel; formulaPanel = refs.FormulaPanel; graphPanel = refs.GraphPanel;
        conclusionPanel = refs.ConclusionPanel; resultPanel = refs.ResultPanel;
        introText = refs.IntroText; objectiveText = refs.ObjectiveText;
        conclusionText = refs.ConclusionText; compareText = refs.CompareText;
        questionText = refs.QuestionText; dataTableText = refs.DataTableText;
        finalScoreText = refs.FinalScore; resultDetailsText = refs.ResultDetails; statusText = refs.StatusText;
        liveReadingsText = refs.LiveReadings; physicsText = refs.PhysicsText;
        conclusionPreview = refs.ConclusionPreview;
        graphTitle = refs.GraphTitle; formulaHelp = refs.FormulaHelp;
        calcPrompt = refs.CalcPrompt; calcHint = refs.CalcHint; calcInput = refs.CalcInput;
        numericInput = refs.NumericInput;
        startBtn = refs.StartBtn; nextBtn = refs.Next; resetBtn = refs.Reset;
        retryBtn = refs.Retry; viewProfileBtn = refs.ViewProfileBtn; viewResultsBtn = refs.ViewResultsBtn;
        questionA = refs.QuestionA; questionB = refs.QuestionB; questionC = refs.QuestionC; questionD = refs.QuestionD;
        questionContinue = refs.QuestionContinue; calcSubmit = refs.CalcSubmit;
        numericCheck = refs.NumericCheck;
        applianceButtons = refs.ApplianceButtons;
        switchBtn = refs.SwitchBtn; takeVBtn = refs.TakeVoltageBtn; takeIBtn = refs.TakeCurrentBtn;
        time10 = refs.Time10; time30 = refs.Time30; time60 = refs.Time60;
        timerStart = refs.TimerStart; timerStop = refs.TimerStop; timerReset = refs.TimerReset;
        anotherBtn = refs.AnotherBtn; graphPowerBtn = refs.GraphPowerBtn; graphEnergyBtn = refs.GraphEnergyBtn;
        questionExplanationPanel = refs.QuestionExplanationPanel;
        questionExplanationText = refs.QuestionExplanationText;
        optAText = refs.OptAText; optBText = refs.OptBText; optCText = refs.OptCText; optDText = refs.OptDText;
        optionsGroup = refs.OptionsGroup; numericGroup = refs.NumericGroup;
        selectApplianceGroup = refs.SelectApplianceGroup; readingsGroup = refs.ReadingsGroup;
        calcGroup = refs.CalcGroup; timeGroup = refs.TimeGroup; timerGroup = refs.TimerGroup;
        resetConfirmPanel = refs.ResetConfirm; resetYes = refs.ResetYes; resetNo = refs.ResetNo;
        retryConfirmPanel = refs.RetryConfirm; retryYes = refs.RetryYes; retryNo = refs.RetryNo;
        phraseButtons = refs.PhraseButtons;
        variableContinueBtn = refs.VariableContinue;
        conclusionContinueBtn = refs.ConclusionContinue;
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
            PowerEnergyExperimentManager.Instance?.ResetExperiment();
            ShowIntro();
        });
        WireBtn(resetNo, () => resetConfirmPanel?.SetActive(false));
        WireBtn(retryBtn, () => { if (retryConfirmPanel != null) retryConfirmPanel.SetActive(true); });
        WireBtn(retryYes, () =>
        {
            retryConfirmPanel?.SetActive(false);
            PowerEnergyExperimentManager.Instance?.RetryExperiment();
        });
        WireBtn(retryNo, () => retryConfirmPanel?.SetActive(false));
        WireBtn(viewProfileBtn, () =>
        {
            string summary = PowerEnergyProfileManager.Instance != null ? PowerEnergyProfileManager.Instance.GetProfileSummary() : "No profile data.";
            PowerEnergyFeedbackManager.Instance?.ShowInstruction(summary);
        });
        WireBtn(viewResultsBtn, () => resultPanel?.SetActive(true));
        WireBtn(questionA, () => AnswerCurrent(0));
        WireBtn(questionB, () => AnswerCurrent(1));
        WireBtn(questionC, () => AnswerCurrent(2));
        WireBtn(questionD, () => AnswerCurrent(3));
        WireBtn(questionContinue, OnQuestionContinue);
        WireBtn(calcSubmit, SubmitCalculation);
        WireBtn(numericCheck, SubmitCalculation);
        WireBtn(switchBtn, () => PowerEnergyApplianceController.Instance?.ToggleSwitch());
        WireBtn(takeVBtn, () =>
        {
            if (PowerEnergyVoltmeterController.Instance != null && PowerEnergyVoltmeterController.Instance.TakeReading())
                MaybeAdvanceToPower();
        });
        WireBtn(takeIBtn, () =>
        {
            if (PowerEnergyAmmeterController.Instance != null && PowerEnergyAmmeterController.Instance.TakeReading())
                MaybeAdvanceToPower();
        });
        WireBtn(time10, () => ChooseTime(10));
        WireBtn(time30, () => ChooseTime(30));
        WireBtn(time60, () => ChooseTime(60));
        WireBtn(timerStart, () => PowerEnergyTimerController.Instance?.StartTimer());
        WireBtn(timerStop, () => PowerEnergyTimerController.Instance?.StopTimer());
        WireBtn(timerReset, () => PowerEnergyTimerController.Instance?.ResetTimer());
        WireBtn(anotherBtn, () => PowerEnergyExperimentManager.Instance?.InvestigateAnother());
        WireBtn(graphPowerBtn, () => PowerEnergyGraphController.Instance?.ShowPower());
        WireBtn(graphEnergyBtn, () => PowerEnergyGraphController.Instance?.ShowEnergy());
        WireBtn(variableContinueBtn, () => PowerEnergyExperimentManager.Instance?.CompleteVariables());
        WireBtn(conclusionContinueBtn, () => PowerEnergyExperimentManager.Instance?.CompleteConclusion());
        if (applianceButtons != null)
        {
            string[] ids = { "Bulb", "Fan", "Iron", "Kettle" };
            for (int i = 0; i < applianceButtons.Length && i < ids.Length; i++)
            {
                int idx = i;
                WireBtn(applianceButtons[i], () =>
                {
                    PowerEnergyApplianceController.Instance?.SelectAppliance(ids[idx]);
                    PowerEnergyExperimentManager.Instance?.SetPhase(PowerEnergyExperimentPhase.TakeReadings);
                });
            }
        }
        if (phraseButtons != null)
        {
            for (int i = 0; i < phraseButtons.Length; i++)
            {
                int idx = i;
                WireBtn(phraseButtons[i], () =>
                {
                    var tmp = phraseButtons[idx] != null ? phraseButtons[idx].GetComponentInChildren<TextMeshProUGUI>() : null;
                    if (tmp != null && !string.IsNullOrEmpty(tmp.text))
                        PowerEnergyConclusionManager.Instance?.AddPhrase(tmp.text);
                });
            }
        }
    }

    private void WireBtn(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
    }

    public void StartPractical() => PowerEnergyExperimentManager.Instance?.StartPractical();

    private void OnNextPressed()
    {
        var step = Current();
        if (step == PowerEnergyExperimentStep.Introduction) { StartPractical(); return; }
        if (step == PowerEnergyExperimentStep.SelectEquipment)
        {
            PowerEnergyExperimentManager.Instance?.TryAdvanceFromEquipment();
            return;
        }
        if (step == PowerEnergyExperimentStep.IdentifyVariables || step == PowerEnergyExperimentStep.Questions || step == PowerEnergyExperimentStep.Compare)
        {
            OnQuestionContinue();
            return;
        }
        if (step == PowerEnergyExperimentStep.Conclusion)
        {
            PowerEnergyExperimentManager.Instance?.CompleteConclusion();
            return;
        }
        PowerEnergyExperimentManager.Instance?.AdvanceStep();
    }

    private void AnswerCurrent(int choice)
    {
        var step = Current();
        if (step == PowerEnergyExperimentStep.Compare)
            PowerEnergyComparisonManager.Instance?.Answer(choice);
        else
            PowerEnergyQuestionManager.Instance?.Answer(choice);
    }

    private void OnQuestionContinue()
    {
        var step = Current();
        if (step == PowerEnergyExperimentStep.Compare)
        {
            if (PowerEnergyComparisonManager.Instance == null || PowerEnergyComparisonManager.Instance.IsFinished)
            {
                PowerEnergyExperimentManager.Instance?.AdvanceStep();
                return;
            }
            if (!PowerEnergyComparisonManager.Instance.HasAnswered)
            {
                PowerEnergyFeedbackManager.Instance?.ShowInstruction("Select an answer first.");
                return;
            }
            if (PowerEnergyComparisonManager.Instance.Advance())
                PowerEnergyExperimentManager.Instance?.AdvanceStep();
            questionExplanationPanel?.SetActive(false);
            return;
        }

        if (PowerEnergyQuestionManager.Instance == null)
        {
            PowerEnergyExperimentManager.Instance?.AdvanceStep();
            return;
        }
        if (PowerEnergyQuestionManager.Instance.IsFinished)
        {
            if (step == PowerEnergyExperimentStep.IdentifyVariables)
                PowerEnergyExperimentManager.Instance?.CompleteVariables();
            else
                PowerEnergyExperimentManager.Instance?.CompleteQuestions();
            return;
        }
        if (!PowerEnergyQuestionManager.Instance.HasAnswered)
        {
            if (PowerEnergyQuestionManager.Instance.CurrentQuestion.isNumeric)
            {
                SubmitCalculation();
                return;
            }
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Select an answer, then press CONTINUE.");
            return;
        }
        bool done = PowerEnergyQuestionManager.Instance.Advance();
        questionExplanationPanel?.SetActive(false);
        if (done)
        {
            if (step == PowerEnergyExperimentStep.IdentifyVariables)
                PowerEnergyExperimentManager.Instance?.CompleteVariables();
            else
                PowerEnergyExperimentManager.Instance?.CompleteQuestions();
        }
    }

    private void SubmitCalculation()
    {
        float value = 0f;
        TMP_InputField field = numericGroup != null && numericGroup.activeInHierarchy && numericInput != null
            ? numericInput
            : calcInput;
        if (field != null)
        {
            string raw = field.text.Replace(",", "").Trim();
            float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value);
        }
        var phase = PowerEnergyExperimentManager.Instance != null ? PowerEnergyExperimentManager.Instance.Phase : PowerEnergyExperimentPhase.CalculatePower;
        var step = Current();
        if (step == PowerEnergyExperimentStep.Questions || step == PowerEnergyExperimentStep.IdentifyVariables)
        {
            PowerEnergyQuestionManager.Instance?.SubmitNumeric(value);
            return;
        }
        if (phase == PowerEnergyExperimentPhase.CalculatePower)
            PowerEnergyExperimentManager.Instance?.SubmitPower(value);
        else if (phase == PowerEnergyExperimentPhase.CalculateEnergy)
            PowerEnergyExperimentManager.Instance?.SubmitEnergy(value);
        else if (phase == PowerEnergyExperimentPhase.ConvertKwh)
            PowerEnergyExperimentManager.Instance?.SubmitKwh(value);
        if (calcInput != null) calcInput.text = "";
        if (numericInput != null) numericInput.text = "";
    }

    private void ChooseTime(float seconds)
    {
        PowerEnergyTimerController.Instance?.SetDuration(seconds);
        PowerEnergyExperimentManager.Instance?.SetPhase(PowerEnergyExperimentPhase.RunTimer);
    }

    private void MaybeAdvanceToPower()
    {
        if (PowerEnergyVoltmeterController.Instance != null && PowerEnergyVoltmeterController.Instance.ReadingTaken &&
            PowerEnergyAmmeterController.Instance != null && PowerEnergyAmmeterController.Instance.ReadingTaken)
            PowerEnergyExperimentManager.Instance?.SetPhase(PowerEnergyExperimentPhase.CalculatePower);
    }

    public void OnTimerFinished()
    {
        PowerEnergyExperimentManager.Instance?.SetPhase(PowerEnergyExperimentPhase.CalculateEnergy);
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
                "POWER AND ENERGY OF ELECTRIC APPLIANCES\n\n" +
                "Investigating Power and Electrical Energy Consumption\n\n" +
                "Different electrical appliances consume electrical energy at different rates. In this practical you will measure voltage and current, calculate power, and determine the electrical energy consumed by appliances.\n\n" +
                "Power:\nP = VI\n\n" +
                "Energy:\nE = Pt\n\n" +
                "Conversion:\n1 kWh = 3,600,000 J";
        }
        UpdateAttemptsDisplay(PowerEnergyAttemptManager.Instance != null ? PowerEnergyAttemptManager.Instance.AttemptsRemaining : 3);
        UpdateScoreDisplay(0);
        UpdateProgress(1, 13);
        if (stepText != null) stepText.text = "Introduction";
        if (titleText != null) titleText.text = "POWER & ENERGY";
    }

    public void ShowStep(PowerEnergyExperimentStep step)
    {
        HideAllContent();
        if (instructionBar != null) instructionBar.SetActive(step != PowerEnergyExperimentStep.Introduction && step != PowerEnergyExperimentStep.Complete);
        SetNextButtonVisible(step != PowerEnergyExperimentStep.Experiment && step != PowerEnergyExperimentStep.CircuitSetup);

        switch (step)
        {
            case PowerEnergyExperimentStep.Introduction:
                ShowIntro();
                return;
            case PowerEnergyExperimentStep.Objective:
                objectivePanel?.SetActive(true);
                if (objectiveText != null)
                    objectiveText.text =
                        "OBJECTIVES\n\nBy completing this practical you will be able to:\n\n" +
                        "• Identify electrical appliances.\n" +
                        "• Measure voltage and current.\n" +
                        "• Calculate electrical power.\n" +
                        "• Calculate electrical energy.\n" +
                        "• Convert joules to kilowatt hours.\n" +
                        "• Compare the energy consumption of different appliances.";
                SetInstruction("Read the objectives, then press CONTINUE.");
                SetNextLabel("CONTINUE");
                break;
            case PowerEnergyExperimentStep.SelectEquipment:
                equipmentPanel?.SetActive(true);
                PowerEnergyEquipmentSelectionManager.Instance?.EnsureCardsVisible();
                SetInstruction("Select the equipment required for this practical. Tap or drag items into the REQUIRED EQUIPMENT AREA.");
                SetNextLabel("NEXT STEP");
                break;
            case PowerEnergyExperimentStep.CircuitSetup:
                laboratoryPanel?.SetActive(true);
                SetInstruction(PowerEnergyCircuitConnectionManager.Instance != null ? PowerEnergyCircuitConnectionManager.Instance.NextHint() : "Build the simple circuit.");
                SetNextButtonVisible(PowerEnergyCircuitConnectionManager.Instance != null && PowerEnergyCircuitConnectionManager.Instance.IsComplete);
                break;
            case PowerEnergyExperimentStep.Experiment:
                experimentPanel?.SetActive(true);
                ShowPowerEnergyExperimentPhase(PowerEnergyExperimentManager.Instance != null ? PowerEnergyExperimentManager.Instance.Phase : PowerEnergyExperimentPhase.SelectAppliance);
                break;
            case PowerEnergyExperimentStep.ObservationTable:
                dataTablePanel?.SetActive(true);
                PowerEnergyObservationTableManager.Instance?.Refresh();
                SetInstruction("Study your recorded values. Higher power means more energy in the same time.");
                SetNextLabel("NEXT STEP");
                break;
            case PowerEnergyExperimentStep.Compare:
                questionPanel?.SetActive(true);
                SetInstruction("Compare the appliances using your results.");
                SetNextLabel("CONTINUE");
                break;
            case PowerEnergyExperimentStep.Graph:
                graphPanel?.SetActive(true);
                PowerEnergyGraphController.Instance?.ShowPower();
                SetInstruction("The bars use your recorded power and energy values.");
                SetNextLabel("NEXT STEP");
                break;
            case PowerEnergyExperimentStep.FormulaMatch:
                formulaPanel?.SetActive(true);
                SetInstruction("Match each idea to its formula. Tap a formula under Power, Energy or kWh.");
                SetNextButtonVisible(false);
                break;
            case PowerEnergyExperimentStep.IdentifyVariables:
            case PowerEnergyExperimentStep.Questions:
                questionPanel?.SetActive(true);
                SetNextButtonVisible(true);
                SetNextLabel("CONTINUE");
                SetInstruction("Select an answer, then press CONTINUE.");
                break;
            case PowerEnergyExperimentStep.Conclusion:
                conclusionPanel?.SetActive(true);
                SetInstruction("Tap the three sentences in the correct order.");
                SetNextButtonVisible(true);
                SetNextLabel("NEXT STEP");
                break;
            case PowerEnergyExperimentStep.Complete:
                ShowResult();
                break;
        }

        UpdateProgress((int)step + 1, 13);
        if (stepText != null) stepText.text = StepTitle(step);
        if (step != PowerEnergyExperimentStep.Objective && step != PowerEnergyExperimentStep.Questions && step != PowerEnergyExperimentStep.Compare && step != PowerEnergyExperimentStep.IdentifyVariables)
            SetNextLabel("NEXT STEP");
        UpdateLiveReadings();
    }

    public void ShowPowerEnergyExperimentPhase(PowerEnergyExperimentPhase phase)
    {
        experimentPanel?.SetActive(true);
        SetGroup(selectApplianceGroup, phase == PowerEnergyExperimentPhase.SelectAppliance || phase == PowerEnergyExperimentPhase.Recorded);
        SetGroup(readingsGroup, phase == PowerEnergyExperimentPhase.TakeReadings);
        SetGroup(calcGroup, phase == PowerEnergyExperimentPhase.CalculatePower || phase == PowerEnergyExperimentPhase.CalculateEnergy || phase == PowerEnergyExperimentPhase.ConvertKwh);
        SetGroup(timeGroup, phase == PowerEnergyExperimentPhase.SelectTime);
        SetGroup(timerGroup, phase == PowerEnergyExperimentPhase.RunTimer);
        if (anotherBtn != null) anotherBtn.gameObject.SetActive(phase == PowerEnergyExperimentPhase.Recorded);

        var app = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.Current : null;
        int done = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.CompletedCount : 0;

        switch (phase)
        {
            case PowerEnergyExperimentPhase.SelectAppliance:
                SetInstruction("Select an appliance. Investigate at least THREE appliances.");
                SetNextButtonVisible(done >= 3);
                break;
            case PowerEnergyExperimentPhase.TakeReadings:
                SetInstruction("Turn the switch ON. Then take the voltage reading and the current reading.");
                SetNextButtonVisible(false);
                break;
            case PowerEnergyExperimentPhase.CalculatePower:
                PrepareCalc("CALCULATE POWER\nP = VI", app != null ? $"Voltage = {app.voltage:0.0} V    Current = {app.current:0.000} A\nEnter power in watts." : "");
                SetInstruction("Calculate power using P = VI and enter your answer.");
                SetNextButtonVisible(false);
                break;
            case PowerEnergyExperimentPhase.SelectTime:
                SetInstruction("Choose how long the appliance operates: 10 s, 30 s or 60 s.");
                SetNextButtonVisible(false);
                break;
            case PowerEnergyExperimentPhase.RunTimer:
                SetInstruction("Press START. The timer stops automatically at the selected time.");
                SetNextButtonVisible(false);
                break;
            case PowerEnergyExperimentPhase.CalculateEnergy:
                float t = app != null ? app.operatingTime : 0f;
                float p = app != null ? (app.studentPower > 0 ? app.studentPower : app.displayPower) : 0f;
                PrepareCalc("CALCULATE ELECTRICAL ENERGY\nE = Pt", $"Power = {p:0.##} W    Time = {t:0} s\nEnter energy in joules.");
                SetInstruction("Calculate electrical energy using E = Pt.");
                SetNextButtonVisible(false);
                break;
            case PowerEnergyExperimentPhase.ConvertKwh:
                float j = app != null ? app.studentEnergyJoules : 0f;
                PrepareCalc("CONVERT TO kWh\n1 kWh = 3,600,000 J", $"Energy = {j:0} J\nEnergy in kWh = Energy in J / 3,600,000");
                SetInstruction("Convert the electrical energy into kilowatt hours.");
                SetNextButtonVisible(false);
                break;
            case PowerEnergyExperimentPhase.Recorded:
                SetInstruction(done >= 3
                    ? "At least three appliances are complete. Investigate another, or press NEXT STEP."
                    : "Recorded. Select another appliance. You need at least THREE.");
                SetNextButtonVisible(done >= 3);
                break;
        }
        UpdateLiveReadings();
    }

    private void PrepareCalc(string prompt, string hint)
    {
        if (calcPrompt != null) calcPrompt.text = prompt;
        if (calcHint != null) calcHint.text = hint;
        if (calcInput != null) calcInput.text = "";
    }

    public void ShowQuestion(int number, int total, string prompt, string a, string b, string c, string d)
    {
        questionPanel?.SetActive(true);
        optionsGroup?.SetActive(true);
        numericGroup?.SetActive(false);
        if (questionText != null) questionText.text = $"QUESTION {number} / {total}\n\n{prompt}";
        if (optAText != null) optAText.text = a;
        if (optBText != null) optBText.text = b;
        if (optCText != null) optCText.text = c;
        if (optDText != null) optDText.text = d;
        questionExplanationPanel?.SetActive(false);
        SetNextButtonVisible(true);
        SetNextLabel("CONTINUE");
    }

    public void ShowNumericQuestion(int number, int total, string prompt, string hint)
    {
        questionPanel?.SetActive(true);
        optionsGroup?.SetActive(false);
        numericGroup?.SetActive(true);
        if (questionText != null) questionText.text = $"QUESTION {number} / {total}\n\n{prompt}";
        if (numericInput != null) numericInput.text = "";
        questionExplanationPanel?.SetActive(false);
        SetNextButtonVisible(true);
        SetNextLabel("CONTINUE");
    }

    public void ShowQuestionExplanation(string explanation, bool correct)
    {
        if (questionExplanationPanel != null)
        {
            questionExplanationPanel.SetActive(true);
            questionExplanationPanel.transform.SetAsLastSibling();
        }
        SetNextButtonVisible(true);
        SetNextLabel("CONTINUE");
        if (questionExplanationText != null)
        {
            questionExplanationText.text = (correct ? "✓ CORRECT\n" : "✗ INCORRECT\n") + explanation + "\n\nPress CONTINUE at the bottom.";
            questionExplanationText.color = correct ? new Color(0.08f, 0.52f, 0.22f) : new Color(0.75f, 0.12f, 0.12f);
        }
    }

    public void SetConclusionPreview(string text)
    {
        if (conclusionPreview != null)
            conclusionPreview.text = string.IsNullOrEmpty(text)
                ? "Your conclusion will appear here."
                : text;
    }

    public void UpdateLiveReadings()
    {
        var app = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.Current : null;
        var circuit = PowerEnergyCircuitConnectionManager.Instance;
        if (liveReadingsText != null)
        {
            liveReadingsText.text =
                $"Appliance:  {(app != null ? app.shortName : "—")}\n\n" +
                $"Switch:  {(app != null && PowerEnergyApplianceController.Instance.IsOn ? "ON" : "OFF")}\n\n" +
                $"Voltage:  {(PowerEnergyVoltmeterController.Instance != null && PowerEnergyVoltmeterController.Instance.LiveValue > 0 ? PowerEnergyVoltmeterController.Instance.LiveValue.ToString("0.0") + " V" : "—")}\n\n" +
                $"Current:  {(PowerEnergyAmmeterController.Instance != null && PowerEnergyAmmeterController.Instance.LiveValue > 0 ? PowerEnergyAmmeterController.Instance.LiveValue.ToString("0.000") + " A" : "—")}\n\n" +
                $"Power:  {(app != null && app.powerCalculated ? app.studentPower.ToString("0.##") + " W" : "—")}\n\n" +
                $"Done:  {(PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.CompletedCount : 0)} / 4";
        }
        if (physicsText != null)
        {
            physicsText.text =
                "P = VI\nPower is the rate of energy use.\n1 W = 1 J/s\n\n" +
                "E = Pt\nEnergy = power × time\nUnit: joule (J)\n\n" +
                "1 kWh = 3,600,000 J";
        }
        if (titleText != null)
            titleText.text = "POWER & ENERGY";
        if (applianceHeaderText != null)
            applianceHeaderText.text = app != null ? "Appliance: " + app.shortName : "Appliance: —";
        if (scoreText != null && PowerEnergyScoreManager.Instance != null)
            scoreText.text = $"SCORE: {PowerEnergyScoreManager.Instance.GetScore()}/100";
        if (attemptsText != null && PowerEnergyAttemptManager.Instance != null)
            attemptsText.text = $"ATTEMPT: {PowerEnergyAttemptManager.Instance.CurrentAttemptNumber}/3";
        if (switchBtn != null)
        {
            var tmp = switchBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = PowerEnergyApplianceController.Instance != null && PowerEnergyApplianceController.Instance.IsOn ? "TURN OFF" : "TURN ON";
        }
        if (circuit != null && Current() == PowerEnergyExperimentStep.CircuitSetup)
            SetInstruction(circuit.NextHint());
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null) scoreText.text = $"SCORE: {score}/100";
    }

    public void UpdateAttemptsDisplay(int remaining)
    {
        if (attemptsText != null)
        {
            int used = PowerEnergyAttemptManager.Instance != null ? PowerEnergyAttemptManager.Instance.CurrentAttemptNumber : 1;
            attemptsText.text = $"ATTEMPT: {used}/3    LEFT: {remaining}";
        }
    }

    public void UpdateProgress(int current, int total)
    {
        if (progressText != null) progressText.text = $"Step: {current} / {total}";
        if (progressBarFill != null) progressBarFill.fillAmount = total > 0 ? current / (float)total : 0f;
    }

    public void SetInstruction(string text)
    {
        if (instructionText != null) instructionText.text = text;
    }

    public void SetNextButtonVisible(bool visible)
    {
        if (nextBtn != null) nextBtn.gameObject.SetActive(visible);
    }

    private void SetNextLabel(string label)
    {
        if (nextBtn == null) return;
        var tmp = nextBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = label;
    }

    public void ShowResult()
    {
        HideAllContent();
        resultPanel?.SetActive(true);
        if (retryBtn != null) retryBtn.gameObject.SetActive(true);
        SetNextButtonVisible(false);
        if (instructionBar != null) instructionBar.SetActive(false);
    }

    public void HideResult()
    {
        resultPanel?.SetActive(false);
        if (retryBtn != null) retryBtn.gameObject.SetActive(false);
    }

    private static void SetGroup(GameObject group, bool visible)
    {
        if (group != null) group.SetActive(visible);
    }

    private void HideAllContent()
    {
        introPanel?.SetActive(false);
        objectivePanel?.SetActive(false);
        equipmentPanel?.SetActive(false);
        laboratoryPanel?.SetActive(false);
        experimentPanel?.SetActive(false);
        dataTablePanel?.SetActive(false);
        comparePanel?.SetActive(false);
        questionPanel?.SetActive(false);
        formulaPanel?.SetActive(false);
        graphPanel?.SetActive(false);
        conclusionPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        if (retryBtn != null) retryBtn.gameObject.SetActive(false);
    }

    private static string StepTitle(PowerEnergyExperimentStep step)
    {
        switch (step)
        {
            case PowerEnergyExperimentStep.Objective: return "Objectives";
            case PowerEnergyExperimentStep.SelectEquipment: return "Equipment";
            case PowerEnergyExperimentStep.CircuitSetup: return "Circuit";
            case PowerEnergyExperimentStep.Experiment: return "Investigate";
            case PowerEnergyExperimentStep.ObservationTable: return "Table";
            case PowerEnergyExperimentStep.Compare: return "Compare";
            case PowerEnergyExperimentStep.Graph: return "Graph";
            case PowerEnergyExperimentStep.FormulaMatch: return "Formulas";
            case PowerEnergyExperimentStep.IdentifyVariables: return "Variables";
            case PowerEnergyExperimentStep.Questions: return "Questions";
            case PowerEnergyExperimentStep.Conclusion: return "Conclusion";
            case PowerEnergyExperimentStep.Complete: return "Results";
            default: return "Introduction";
        }
    }

    private static PowerEnergyExperimentStep Current()
    {
        return PowerEnergyExperimentManager.Instance != null
            ? PowerEnergyExperimentManager.Instance.CurrentStep
            : PowerEnergyExperimentStep.Introduction;
    }
}

public class PowerEnergyIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (PowerEnergyUIManager.Instance != null) PowerEnergyUIManager.Instance.StartPractical();
    }
}
