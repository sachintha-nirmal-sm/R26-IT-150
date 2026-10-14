using UnityEngine;

public class FirstLawMotionController : MonoBehaviour
{
    public static FirstLawMotionController Instance { get; private set; }

    [SerializeField] private float cruiseVelocity = 1.4f;
    [SerializeField] private bool movingMode;

    public bool MovingMode => movingMode;
    public float CruiseVelocity => cruiseVelocity;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void PrepareStationary()
    {
        movingMode = false;
        TrolleyController.Instance?.ResetTrolley();
        TrolleyController.Instance?.SetForce(0f);
        TrolleyController.Instance?.SetVelocity(0f);
    }

    public void ApplyInitialPush()
    {
        movingMode = true;
        TrolleyController.Instance?.SetPosition(0f);
        TrolleyController.Instance?.SetForce(0f);
        TrolleyController.Instance?.SetVelocity(cruiseVelocity);
        TrolleyController.Instance?.StartMotion();
    }

    public float GetNetForce()
    {
        if (!movingMode) return 0f;
        var friction = NewtonFrictionController.Instance;
        if (friction == null || friction.IsLowFriction) return 0f;
        float m = TrolleyController.Instance != null ? TrolleyController.Instance.Mass : 1f;
        return -friction.FrictionAcceleration * m;
    }

    public float GetVelocity() => TrolleyController.Instance != null ? TrolleyController.Instance.Velocity : 0f;
}
