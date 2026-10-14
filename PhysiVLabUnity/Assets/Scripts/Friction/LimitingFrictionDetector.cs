using UnityEngine;

public class LimitingFrictionDetector : MonoBehaviour
{
    public static LimitingFrictionDetector Instance { get; private set; }

    [SerializeField] private float limitingFriction = 18f;
    [SerializeField] private bool limitingFrictionDetected;
    [SerializeField] private float detectedReading;

    public bool LimitingFrictionDetected => limitingFrictionDetected;
    public float DetectedReading => detectedReading;
    public float TargetLimitingFriction => limitingFriction;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetDetection(float target)
    {
        limitingFriction = target;
        limitingFrictionDetected = false;
        detectedReading = 0f;
    }

    public void Evaluate(float appliedForce)
    {
        if (appliedForce < limitingFriction)
        {
            FrictionForceController.Instance?.SetMoving(false);
            return;
        }

        if (!limitingFrictionDetected)
        {
            limitingFrictionDetected = true;
            detectedReading = limitingFriction;
            FrictionMeasurementManager.Instance?.FreezeLimitingReading(detectedReading);
            FrictionFeedbackManager.Instance?.ShowMessage(
                $"✓ BLOCK JUST STARTED MOVING\nLIMITING FRICTION: {detectedReading:0.0} N",
                "",
                new Color(0.08f, 0.52f, 0.22f));
        }
        FrictionForceController.Instance?.SetMoving(true);
    }
}
