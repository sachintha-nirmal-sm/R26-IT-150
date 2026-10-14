using UnityEngine;

public class FrictionAppliedForceController : MonoBehaviour
{
    public static FrictionAppliedForceController Instance { get; private set; }

    [SerializeField] private float appliedForce;
    [SerializeField] private bool applying;
    [SerializeField] private float maxForce = 70f;

    public float AppliedForce => appliedForce;
    public bool IsApplying => applying;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartApplying() => applying = true;

    public void StopApplying() => applying = false;

    public void Increase(float delta)
    {
        float previous = appliedForce;
        appliedForce = Mathf.Clamp(appliedForce + Mathf.Max(0f, delta), 0f, maxForce);
        if (delta > 6f)
            FrictionFeedbackManager.Instance?.ShowInstruction("Pull more slowly so you can identify the limiting friction.");
        FrictionForceController.Instance?.SetAppliedForce(appliedForce);
        if (Mathf.Abs(appliedForce - previous) > 0.01f)
            FrictionMeasurementManager.Instance?.SampleForce(appliedForce);
    }

    public void Decrease(float delta)
    {
        appliedForce = Mathf.Clamp(appliedForce - Mathf.Max(0f, delta), 0f, maxForce);
        FrictionForceController.Instance?.SetAppliedForce(appliedForce);
    }

    public void SetForce(float value)
    {
        appliedForce = Mathf.Clamp(value, 0f, maxForce);
        FrictionForceController.Instance?.SetAppliedForce(appliedForce);
    }

    public void ResetForce()
    {
        applying = false;
        appliedForce = 0f;
        FrictionForceController.Instance?.SetAppliedForce(0f);
    }
}
