using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PHASE 2 — Ordered experiment steps for measuring upthrust with a Eureka can.
/// Steps:
/// 1 Hang metal cube on spring balance in air (1.2 N)
/// 2 Place empty beaker under the spout (1.3 N)
/// 3 Lower to Stage (a) near surface
/// 4 Lower to Stage (b) half submerged — overflow
/// 5 Lower to Stage (c) fully immersed near surface — more overflow
/// 6 Lower to Stage (d) fully immersed deeper — reading unchanged
/// Then unlocks Phase 3 (observation table).
/// </summary>
public class UpthrustPracticalManager : MonoBehaviour
{
    public static UpthrustPracticalManager Instance { get; private set; }

    public enum StepId
    {
        HangCubeInAir = 0,
        PlaceBeakerUnderSpout = 1,
        StageA_NearSurface = 2,
        StageB_HalfSubmerged = 3,
        StageC_FullyNearSurface = 4,
        StageD_FullyDeep = 5
    }

    [Serializable]
    public class PracticalStep
    {
        public StepId stepId;
        [TextArea(2, 4)] public string instruction;
        public UpthrustSnapZone[] requiredZones;
        public UpthrustPracticalData.ImmersionStage immersion = UpthrustPracticalData.ImmersionStage.AirHang;
        public bool placesBeaker;
        public bool hangsCube;
    }

    [Header("Steps (leave empty to use book defaults)")]
    [SerializeField] private List<PracticalStep> steps = new List<PracticalStep>();

    [Header("References")]
    [SerializeField] private UpthrustSpringBalanceGauge springBalance;
    [SerializeField] private UpthrustLabVisuals labVisuals;
    [SerializeField] private UpthrustObservationTableUI observationTable;
    [SerializeField] private GameObject phase2Root;
    [SerializeField] private GameObject stepActionsRoot;
    [SerializeField] private Text liveSpringText;
    [SerializeField] private Text liveBeakerText;
    [SerializeField] private Text liveUpthrustText;
    [SerializeField] private Text liveDisplacedText;

    [Header("Runtime")]
    [SerializeField] private int currentStepIndex = -1;
    [SerializeField] private bool practicalActive;
    [SerializeField] private bool interactionAllowed;

    private readonly HashSet<int> stepsWithMistakes = new HashSet<int>();
    private readonly HashSet<UpthrustSnapZone> completedZonesThisStep = new HashSet<UpthrustSnapZone>();
    private bool observationStarted;
    private float currentBeakerWeight = UpthrustPracticalData.EmptyBeakerWeight;
    private float currentSpringReading = UpthrustPracticalData.WeightInAir;

    public int CurrentStepIndex => currentStepIndex;
    public bool IsInteractionAllowed => interactionAllowed && practicalActive;
    public bool PracticalActive => practicalActive;
    public IReadOnlyList<PracticalStep> Steps => steps;

    public event Action<int, string> OnStepChanged;
    public event Action OnPracticalCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (springBalance == null)
            springBalance = UpthrustSpringBalanceGauge.Instance;
        if (labVisuals == null)
            labVisuals = UpthrustLabVisuals.Instance;
        if (observationTable == null)
            observationTable = UpthrustObservationTableUI.Instance;

        if (steps == null || steps.Count == 0)
            BuildDefaultSteps();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Configure(
        UpthrustSpringBalanceGauge gauge,
        UpthrustLabVisuals visuals,
        UpthrustObservationTableUI table,
        GameObject phase2,
        GameObject actionsRoot,
        Text springTxt,
        Text beakerTxt,
        Text upthrustTxt,
        Text displacedTxt)
    {
        springBalance = gauge;
        labVisuals = visuals;
        observationTable = table;
        phase2Root = phase2;
        stepActionsRoot = actionsRoot;
        liveSpringText = springTxt;
        liveBeakerText = beakerTxt;
        liveUpthrustText = upthrustTxt;
        liveDisplacedText = displacedTxt;
    }

    private void BuildDefaultSteps()
    {
        steps = new List<PracticalStep>
        {
            new PracticalStep
            {
                stepId = StepId.HangCubeInAir,
                instruction = "Step 1: Hang the metal cube on the spring balance in air and record its weight (1.2 N).",
                hangsCube = true,
                immersion = UpthrustPracticalData.ImmersionStage.AirHang
            },
            new PracticalStep
            {
                stepId = StepId.PlaceBeakerUnderSpout,
                instruction = "Step 2: Place the empty beaker under the spout of the filled Eureka can.",
                placesBeaker = true
            },
            new PracticalStep
            {
                stepId = StepId.StageA_NearSurface,
                instruction = "Step 3: Lower the cube to Stage (a) — near the water surface, not submerged.",
                immersion = UpthrustPracticalData.ImmersionStage.NearSurface
            },
            new PracticalStep
            {
                stepId = StepId.StageB_HalfSubmerged,
                instruction = "Step 4: Lower the cube to Stage (b) — half submerged. Watch water overflow into the beaker.",
                immersion = UpthrustPracticalData.ImmersionStage.HalfSubmerged
            },
            new PracticalStep
            {
                stepId = StepId.StageC_FullyNearSurface,
                instruction = "Step 5: Lower the cube to Stage (c) — fully immersed, still near the surface.",
                immersion = UpthrustPracticalData.ImmersionStage.FullyNearSurface
            },
            new PracticalStep
            {
                stepId = StepId.StageD_FullyDeep,
                instruction = "Step 6: Lower the cube to Stage (d) — fully immersed, deeper. Note the reading does not change.",
                immersion = UpthrustPracticalData.ImmersionStage.FullyDeep
            }
        };
    }

    public void BeginPractical()
    {
        practicalActive = true;
        interactionAllowed = true;
        observationStarted = false;
        stepsWithMistakes.Clear();
        currentBeakerWeight = 0f;
        currentSpringReading = 0f;
        currentStepIndex = -1;

        if (phase2Root != null)
            phase2Root.SetActive(true);

        labVisuals?.ResetLab();
        springBalance?.SetReading(0f, true);
        RefreshLiveReadings(0f, 0f, 0f, 0f);
        AdvanceToStep(0);
    }

    public void AdvanceToStep(int index)
    {
        if (index < 0 || index >= steps.Count) return;

        currentStepIndex = index;
        completedZonesThisStep.Clear();

        PracticalStep step = steps[currentStepIndex];
        OnStepChanged?.Invoke(currentStepIndex, step.instruction);
        UpthrustUIManager.Instance?.ShowStepInstruction(currentStepIndex + 1, steps.Count, step.instruction);

        Debug.Log($"[UpthrustPracticalManager] {step.instruction}");
    }

    public bool IsZoneActiveForCurrentStep(UpthrustSnapZone zone)
    {
        if (!practicalActive || currentStepIndex < 0 || currentStepIndex >= steps.Count)
            return false;

        PracticalStep step = steps[currentStepIndex];
        if (step.requiredZones == null || step.requiredZones.Length == 0)
            return zone.AssociatedStepIndex == currentStepIndex;

        foreach (var z in step.requiredZones)
        {
            if (z == zone) return true;
        }

        return false;
    }

    public void NotifyCorrectPlacement(UpthrustSnapZone zone, UpthrustDraggableEquipment equipment)
    {
        if (!IsZoneActiveForCurrentStep(zone)) return;

        completedZonesThisStep.Add(zone);
        ApplyStepVisuals(steps[currentStepIndex]);
        TryCompleteCurrentStep();
    }

    public void NotifyWrongPlacement(UpthrustSnapZone zone, UpthrustDraggableEquipment equipment)
    {
        if (!practicalActive) return;

        stepsWithMistakes.Add(currentStepIndex);
        UpthrustScoreManager.Instance?.RegisterStepMistake();
        UpthrustUIManager.Instance?.ShowFeedback("Wrong apparatus or wrong snap zone for this step! −5", false);
    }

    public void NotifyOutOfOrderAttempt()
    {
        if (!practicalActive) return;

        stepsWithMistakes.Add(currentStepIndex);
        UpthrustScoreManager.Instance?.RegisterStepMistake();
        UpthrustUIManager.Instance?.ShowFeedback("Follow the steps in order! −5", false);
    }

    /// <summary>Called by Phase 2 action-choice buttons (2D lab).</summary>
    public void TryPerformCurrentStepAction(bool correctChoice)
    {
        if (!practicalActive || observationStarted) return;

        if (!correctChoice)
        {
            NotifyOutOfOrderAttempt();
            return;
        }

        ApplyStepVisuals(steps[currentStepIndex]);
        CompleteCurrentStep();
    }

    private void TryCompleteCurrentStep()
    {
        PracticalStep step = steps[currentStepIndex];

        if (step.requiredZones != null && step.requiredZones.Length > 0)
        {
            foreach (var required in step.requiredZones)
            {
                if (required != null && !completedZonesThisStep.Contains(required))
                    return;
            }
        }
        else if (completedZonesThisStep.Count < 1)
        {
            return;
        }

        CompleteCurrentStep();
    }

    private void CompleteCurrentStep()
    {
        bool firstTry = !stepsWithMistakes.Contains(currentStepIndex);
        UpthrustScoreManager.Instance?.RegisterStepSuccess(firstTry);

        UpthrustUIManager.Instance?.ShowFeedback(
            firstTry ? "Step completed! +15" : "Step completed! +15",
            true);

        int next = currentStepIndex + 1;
        if (next >= steps.Count)
            FinishExperimentPhase();
        else
            AdvanceToStep(next);
    }

    private void ApplyStepVisuals(PracticalStep step)
    {
        if (step.hangsCube)
        {
            currentSpringReading = UpthrustPracticalData.WeightInAir;
            springBalance?.SetReading(currentSpringReading);
            labVisuals?.HangCubeInAir();
            RefreshLiveReadings(currentSpringReading, currentBeakerWeight, 0f, 0f);
        }

        if (step.placesBeaker)
        {
            currentBeakerWeight = UpthrustPracticalData.EmptyBeakerWeight;
            labVisuals?.PlaceBeakerUnderSpout();
            RefreshLiveReadings(currentSpringReading, currentBeakerWeight, 0f, 0f);
        }

        if (step.immersion != UpthrustPracticalData.ImmersionStage.AirHang || step.stepId >= StepId.StageA_NearSurface)
        {
            if (step.stepId >= StepId.StageA_NearSurface)
                StartCoroutine(ApplyImmersionRoutine(step.immersion));
        }
    }

    private IEnumerator ApplyImmersionRoutine(UpthrustPracticalData.ImmersionStage stage)
    {
        var reading = UpthrustPracticalData.GetStage(stage);
        labVisuals?.MoveCubeToStage(stage);

        if (reading.overflows)
            yield return labVisuals != null
                ? labVisuals.PlayOverflow(reading.overflowAmountN)
                : null;

        currentSpringReading = reading.springBalanceN;
        currentBeakerWeight = reading.beakerWithWaterN;
        springBalance?.SetReading(currentSpringReading);
        labVisuals?.SetBeakerFill(reading.displacedWaterN);
        RefreshLiveReadings(reading.springBalanceN, reading.beakerWithWaterN, reading.upthrustN, reading.displacedWaterN);
    }

    private void RefreshLiveReadings(float springN, float beakerN, float upthrustN, float displacedN)
    {
        if (liveSpringText != null)
            liveSpringText.text = $"Spring balance: {springN:0.0} N";
        if (liveBeakerText != null)
            liveBeakerText.text = $"Beaker: {beakerN:0.0} N";
        if (liveUpthrustText != null)
            liveUpthrustText.text = $"Upthrust: {upthrustN:0.0} N";
        if (liveDisplacedText != null)
            liveDisplacedText.text = $"Displaced water: {displacedN:0.0} N";
    }

    private void FinishExperimentPhase()
    {
        practicalActive = false;
        interactionAllowed = false;
        observationStarted = true;

        if (stepActionsRoot != null)
            stepActionsRoot.SetActive(false);

        if (phase2Root != null)
            phase2Root.SetActive(false);

        if (observationTable == null)
            observationTable = UpthrustObservationTableUI.Instance;

        observationTable?.ShowTable();
        UpthrustUIManager.Instance?.ShowFeedback("Experiment complete. Fill the observation table.", true);
        Debug.Log("[UpthrustPracticalManager] Phase 2 complete — opening observation table.");
    }

    public void CompleteObservationPhase()
    {
        observationTable?.HideTable();
        UpthrustScoreManager.Instance?.FinalizeScore();
        UpthrustUIManager.Instance?.ShowEndScreen();
        OnPracticalCompleted?.Invoke();
        Debug.Log("[UpthrustPracticalManager] Practical complete.");
    }

    public void ResetPractical()
    {
        practicalActive = false;
        interactionAllowed = false;
        observationStarted = false;
        currentStepIndex = -1;
        stepsWithMistakes.Clear();
        completedZonesThisStep.Clear();
        currentBeakerWeight = 0f;
        currentSpringReading = 0f;

        foreach (var step in steps)
        {
            if (step.requiredZones == null) continue;
            foreach (var zone in step.requiredZones)
            {
                if (zone != null) zone.ResetZone();
            }
        }

        labVisuals?.ResetLab();
        springBalance?.SetReading(0f, true);
        observationTable?.ResetTable();
        RefreshLiveReadings(0f, 0f, 0f, 0f);
    }
}
