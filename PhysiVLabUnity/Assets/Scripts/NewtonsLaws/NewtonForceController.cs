using UnityEngine;

public class NewtonForceController : MonoBehaviour
{
    public static NewtonForceController Instance { get; private set; }

    [SerializeField] private float force = 1f;
    [SerializeField] private float maximumForce = 10f;

    public float Force => force;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Configure(float maxForce) => maximumForce = Mathf.Max(1f, maxForce);

    public void SetForce(float value)
    {
        force = Mathf.Clamp(value, 0f, maximumForce);
        TrolleyController.Instance?.SetForce(force);
        SpringBalanceController.Instance?.SetAppliedForce(force);
        NewtonUIManager.Instance?.HighlightForce(force);
        NewtonUIManager.Instance?.UpdateLiveNewtonReadings(
            NewtonMassController.Instance != null ? NewtonMassController.Instance.Mass : 1f,
            force,
            NewtonAccelerationCalculator.Instance != null ? NewtonAccelerationCalculator.Instance.Calculate(force, NewtonMassController.Instance != null ? NewtonMassController.Instance.Mass : 1f) : force,
            TrolleyController.Instance != null ? TrolleyController.Instance.Velocity : 0f,
            0f,
            (NewtonMassController.Instance != null ? NewtonMassController.Instance.Mass : 1f) * 9.8f,
            true);
    }
}
