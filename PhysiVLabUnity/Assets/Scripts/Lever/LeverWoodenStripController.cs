using UnityEngine;

public class LeverWoodenStripController : MonoBehaviour
{
    public static LeverWoodenStripController Instance { get; private set; }

    [SerializeField] private RectTransform strip;
    [SerializeField] private float maxTiltDegrees = 8f;

    private Quaternion restRotation;
    private bool bound;

    private void Awake()
    {
        Instance = this;
        if (strip == null) strip = GetComponent<RectTransform>();
        CaptureRest();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Bind(RectTransform stripRect)
    {
        strip = stripRect;
        CaptureRest();
        ResetStrip();
        bound = true;
    }

    public void CaptureRest()
    {
        if (strip != null) restRotation = strip.localRotation;
    }

    /// <summary>
    /// Tilts the strip based on force ratio (0 = level, 1 = max tilt toward lift).
    /// </summary>
    public void SetRotationFromForceRatio(float forceRatio)
    {
        if (strip == null) return;
        float t = Mathf.Clamp01(forceRatio);
        float angle = -maxTiltDegrees * t;
        strip.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void ApplyForce(float currentForce, float requiredForce)
    {
        float ratio = requiredForce > 0.01f ? currentForce / requiredForce : 0f;
        SetRotationFromForceRatio(ratio);
    }

    public void ResetStrip()
    {
        if (strip == null) return;
        strip.localRotation = bound || restRotation != default ? restRotation : Quaternion.identity;
    }
}
