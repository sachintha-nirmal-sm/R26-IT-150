using UnityEngine;

public class EquilibriumForceController : MonoBehaviour
{
    public static EquilibriumForceController Instance { get; private set; }

    public const float TrueWeightN = 1.20f;

    [SerializeField] private float tiltDeg;
    [SerializeField] private bool weighingAttached;
    [SerializeField] private bool leftHung;
    [SerializeField] private bool rightHung;
    [SerializeField] private bool weightRecorded;
    [SerializeField] private float measuredW;
    [SerializeField] private int currentTrial = 1;

    public float TiltDeg => tiltDeg;
    public bool WeighingAttached => weighingAttached;
    public bool LeftHung => leftHung;
    public bool RightHung => rightHung;
    public bool DualHung => leftHung && rightHung;
    public bool WeightRecorded => weightRecorded;
    public float MeasuredW => weightRecorded ? measuredW : 0f;
    public bool IsHorizontal => DualHung && Mathf.Abs(tiltDeg) <= 2f;
    public bool Coplanar => true;

    public float Force1N
    {
        get
        {
            if (weighingAttached && !DualHung) return TrueWeightN;
            if (!DualHung) return 0f;
            return ComputeF1();
        }
    }

    public float Force2N
    {
        get
        {
            if (!DualHung) return 0f;
            return TrueWeightN - ComputeF1();
        }
    }

    public float SumN => Force1N + Force2N;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetAll()
    {
        tiltDeg = 8f;
        weighingAttached = false;
        leftHung = false;
        rightHung = false;
        weightRecorded = false;
        measuredW = 0f;
        currentTrial = 1;
        EquilibriumVisualController.Instance?.RefreshVisuals();
        EquilibriumUIManager.Instance?.UpdateLiveReadings();
    }

    public void PrepareWeighing()
    {
        weighingAttached = false;
        leftHung = false;
        rightHung = false;
        tiltDeg = 0f;
        EquilibriumVisualController.Instance?.RefreshVisuals();
        EquilibriumUIManager.Instance?.UpdateLiveReadings();
    }

    public void PrepareTrial(int trial)
    {
        currentTrial = Mathf.Clamp(trial, 1, 3);
        leftHung = false;
        rightHung = false;
        weighingAttached = false;
        tiltDeg = currentTrial == 1 ? 10f : currentTrial == 2 ? -8f : 12f;
        EquilibriumVisualController.Instance?.RefreshVisuals();
        EquilibriumUIManager.Instance?.UpdateLiveReadings();
    }

    public bool TryWeighRuler()
    {
        var asm = EquilibriumAssemblyManager.Instance;
        if (asm == null || !asm.RulerPlaced || !asm.Balance1Placed)
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nHang the meter ruler from spring balance F1 to measure its weight.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return false;
        }
        weighingAttached = true;
        leftHung = false;
        rightHung = false;
        measuredW = TrueWeightN;
        EquilibriumVisualController.Instance?.RefreshVisuals();
        EquilibriumUIManager.Instance?.UpdateLiveReadings();
        return true;
    }

    public void ConfirmWeightReading()
    {
        if (!weighingAttached)
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nHang the ruler from F1 first, then record W.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        weightRecorded = true;
        measuredW = TrueWeightN;
        EquilibriumScoreManager.Instance?.AddScore(5, false);
        EquilibriumFeedbackManager.Instance?.ShowMessage(
            $"✓ CORRECT\nWeight of the meter ruler  W = {TrueWeightN:0.00} N\nNow suspend the ruler from both spring balances.",
            "+5 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        EquilibriumUIManager.Instance?.UpdateLiveReadings();
    }

    public bool TryHangLeft()
    {
        if (!weightRecorded)
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Measure the weight W of the ruler before hanging it from both ends.");
            return false;
        }
        var asm = EquilibriumAssemblyManager.Instance;
        if (asm == null || !asm.BandLeft)
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Loop the left rubber band onto F1.");
            return false;
        }
        leftHung = true;
        weighingAttached = false;
        EquilibriumVisualController.Instance?.RefreshVisuals();
        EquilibriumUIManager.Instance?.UpdateLiveReadings();
        EquilibriumFeedbackManager.Instance?.ShowInstruction(rightHung
            ? "Both ends hung. Adjust the ruler until it is horizontal, then RECORD READING."
            : "Left end hung on F1. Now hang the right end on F2.");
        return true;
    }

    public bool TryHangRight()
    {
        if (!weightRecorded)
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Measure the weight W of the ruler before hanging it from both ends.");
            return false;
        }
        var asm = EquilibriumAssemblyManager.Instance;
        if (asm == null || !asm.BandRight)
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Loop the right rubber band onto F2.");
            return false;
        }
        rightHung = true;
        weighingAttached = false;
        EquilibriumVisualController.Instance?.RefreshVisuals();
        EquilibriumUIManager.Instance?.UpdateLiveReadings();
        EquilibriumFeedbackManager.Instance?.ShowInstruction(leftHung
            ? "Both ends hung. Adjust the ruler until it is horizontal, then RECORD READING."
            : "Right end hung on F2. Now hang the left end on F1.");
        return true;
    }

    public void ChangeTilt(float delta)
    {
        if (!DualHung)
        {
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Hang the ruler from both spring balances before levelling it.");
            return;
        }
        tiltDeg = Mathf.Clamp(tiltDeg + delta, -20f, 20f);
        EquilibriumVisualController.Instance?.RefreshVisuals();
        EquilibriumUIManager.Instance?.UpdateLiveReadings();
        if (IsHorizontal)
            EquilibriumFeedbackManager.Instance?.ShowInstruction("The meter ruler is horizontal. Record F1 and F2.");
    }

    public bool CanRecordEquilibrium()
    {
        if (!DualHung)
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nSuspend the ruler at both ends using the rubber bands and spring balances.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return false;
        }
        if (!IsHorizontal)
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nAdjust the ruler until it is horizontal (equilibrium). Then take the readings.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return false;
        }
        return true;
    }

    private float ComputeF1()
    {
        float baseF1 = 0.60f;
        float wobble = currentTrial == 1 ? 0f : currentTrial == 2 ? 0.01f : -0.01f;
        float tiltShift = Mathf.Clamp(tiltDeg, -20f, 20f) * 0.008f;
        return Mathf.Clamp(baseF1 + wobble + tiltShift, 0.10f, TrueWeightN - 0.10f);
    }
}
