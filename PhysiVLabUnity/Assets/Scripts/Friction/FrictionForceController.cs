using UnityEngine;

public class FrictionForceController : MonoBehaviour
{
    public static FrictionForceController Instance { get; private set; }

    [SerializeField] private float weight = 60f;
    [SerializeField] private float limitingFriction = 18f;
    [SerializeField] private float appliedForce;
    [SerializeField] private float frictionForce;

    public float Weight => weight;
    public float LimitingFriction => limitingFriction;
    public float AppliedForce => appliedForce;
    public float FrictionForce => frictionForce;
    public bool BlockMoving { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        weight = 60f;
    }

    public void PrepareTrial(int trialNumber)
    {
        weight = 60f;
        float[] bases = { 18.0f, 18.2f, 17.8f };
        int i = Mathf.Clamp(trialNumber - 1, 0, 2);
        limitingFriction = bases[i] + Random.Range(-0.12f, 0.12f);
        appliedForce = 0f;
        frictionForce = 0f;
        BlockMoving = false;
        LimitingFrictionDetector.Instance?.ResetDetection(limitingFriction);
    }

    public void SetAppliedForce(float force)
    {
        appliedForce = Mathf.Max(0f, force);
        LimitingFrictionDetector.Instance?.Evaluate(appliedForce);
        if (BlockMoving)
        {
            frictionForce = limitingFriction * 0.92f;
            WoodenBlockController.Instance?.BeginMotion();
        }
        else
        {
            frictionForce = Mathf.Min(appliedForce, limitingFriction);
            WoodenBlockController.Instance?.StopMotion();
        }
        FrictionUIManager.Instance?.UpdateLiveReadings();
    }

    public void SetMoving(bool moving)
    {
        BlockMoving = moving;
        if (moving) WoodenBlockController.Instance?.BeginMotion();
        else WoodenBlockController.Instance?.StopMotion();
    }

    public void ResetForces()
    {
        appliedForce = 0f;
        frictionForce = 0f;
        BlockMoving = false;
        WoodenBlockController.Instance?.StopMotion();
    }

    private void Update()
    {
        if (BlockMoving) WoodenBlockController.Instance?.TickMotion(Time.deltaTime);
    }
}
