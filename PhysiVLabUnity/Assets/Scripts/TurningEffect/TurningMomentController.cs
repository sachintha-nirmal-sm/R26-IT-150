using UnityEngine;

public class TurningMomentController : MonoBehaviour
{
    public static TurningMomentController Instance { get; private set; }

    [SerializeField] private float forceN;
    [SerializeField] private float angleDeg = 90f;
    [SerializeField] private int tightnessLevel = 1;
    [SerializeField] private string attachedPoint = "";
    [SerializeField] private float maxForce = 12f;
    [SerializeField] private float forceStep = 0.5f;
    [SerializeField] private float angleStep = 15f;

    public float ForceN => forceN;
    public float AngleDeg => angleDeg;
    public int TightnessLevel => tightnessLevel;
    public string AttachedPoint => attachedPoint;
    public bool BalanceAttached => attachedPoint == "A" || attachedPoint == "B" || attachedPoint == "C" || attachedPoint == "D";
    public float DistanceM => DistanceOf(attachedPoint);
    public float DistanceCm => DistanceM * 100f;
    public float PerpendicularForce => forceN * Mathf.Abs(Mathf.Sin(angleDeg * Mathf.Deg2Rad));
    public float MomentNm => PerpendicularForce * DistanceM;
    public float FrictionTorqueNm => TorqueOf(tightnessLevel);
    public float TargetForceAtD => FrictionTorqueNm / 0.60f;
    public bool StickJustMoves => BalanceAttached && MomentNm + 0.02f >= FrictionTorqueNm;
    public bool IsPerpendicular => Mathf.Abs(angleDeg - 90f) <= 10f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public static float DistanceOf(string point)
    {
        switch (point)
        {
            case "A": return 0.15f;
            case "B": return 0.30f;
            case "C": return 0.45f;
            case "D": return 0.60f;
            default: return 0f;
        }
    }

    public static float TorqueOf(int tightness)
    {
        if (tightness <= 1) return 0.90f;
        if (tightness == 2) return 1.50f;
        return 2.10f;
    }

    public bool TryAttachBalance(string zoneId)
    {
        string point = PointFromZone(zoneId);
        if (string.IsNullOrEmpty(point))
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nHook the Newton balance onto a wire loop at A, B, C or D. The textbook method uses point D.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return false;
        }
        if (point != "D")
        {
            TurningScoreManager.Instance?.SubtractScore(5);
            TurningFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nAttach the Newton balance to the loop at D (60 cm from O) as in Figure 11.4.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            return false;
        }
        attachedPoint = "D";
        TurningScoreManager.Instance?.AddScore(5, false);
        TurningFeedbackManager.Instance?.ShowMessage(
            "✓ CORRECT\nNewton balance hooked at D. Pull perpendicular to the stick until it just begins to turn.",
            "+5 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        TurningVisualController.Instance?.RefreshVisuals();
        TurningUIManager.Instance?.UpdateLiveReadings();
        return true;
    }

    public void AddForce(float delta)
    {
        forceN = Mathf.Clamp(forceN + Mathf.Sign(delta) * forceStep, 0f, maxForce);
        TurningVisualController.Instance?.RefreshReadings();
        TurningUIManager.Instance?.UpdateLiveReadings();
    }

    public void AddAngle(float delta)
    {
        angleDeg = Mathf.Clamp(angleDeg + Mathf.Sign(delta) * angleStep, 0f, 180f);
        TurningVisualController.Instance?.RefreshReadings();
        TurningUIManager.Instance?.UpdateLiveReadings();
    }

    public void SetAngle(float value)
    {
        angleDeg = Mathf.Clamp(value, 0f, 180f);
        TurningVisualController.Instance?.RefreshReadings();
        TurningUIManager.Instance?.UpdateLiveReadings();
    }

    public bool TightenScrew()
    {
        if (tightnessLevel >= 3)
        {
            TurningFeedbackManager.Instance?.ShowInstruction("The screw is already fully tightened for this practical.");
            return false;
        }
        tightnessLevel++;
        forceN = 0f;
        TurningScoreManager.Instance?.AddScore(5, false);
        TurningFeedbackManager.Instance?.ShowMessage(
            $"✓ SCREW TIGHTENED\nHalf a turn increases friction at the pivot. Tightness level is now {tightnessLevel}.",
            "+5 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        TurningVisualController.Instance?.RefreshReadings();
        TurningUIManager.Instance?.UpdateLiveReadings();
        return true;
    }

    public void ResetForces()
    {
        forceN = 0f;
        angleDeg = 90f;
        TurningVisualController.Instance?.RefreshReadings();
        TurningUIManager.Instance?.UpdateLiveReadings();
    }

    public void ResetAll()
    {
        forceN = 0f;
        angleDeg = 90f;
        tightnessLevel = 1;
        attachedPoint = "";
        TurningVisualController.Instance?.RefreshVisuals();
        TurningVisualController.Instance?.RefreshReadings();
        TurningUIManager.Instance?.UpdateLiveReadings();
    }

    public void PrepareTrial(int trial)
    {
        forceN = 0f;
        angleDeg = 90f;
        TurningVisualController.Instance?.RefreshReadings();
        TurningUIManager.Instance?.UpdateLiveReadings();
    }

    private static string PointFromZone(string zoneId)
    {
        if (string.IsNullOrEmpty(zoneId)) return "";
        if (zoneId.Contains("A")) return "A";
        if (zoneId.Contains("B")) return "B";
        if (zoneId.Contains("C")) return "C";
        if (zoneId.Contains("D")) return "D";
        return "";
    }
}
