using UnityEngine;

public class NewtonMassController : MonoBehaviour
{
    public static NewtonMassController Instance { get; private set; }

    [SerializeField] private float mass = 1f;
    [SerializeField] private float minimumMass = 0.5f;
    [SerializeField] private float maximumMass = 5f;

    public float Mass => mass;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Configure(float min, float max)
    {
        minimumMass = Mathf.Max(0.1f, min);
        maximumMass = Mathf.Max(minimumMass, max);
    }

    public void SetMass(float value)
    {
        mass = Mathf.Clamp(value, minimumMass, maximumMass);
        TrolleyController.Instance?.SetMass(mass);
        NewtonUIManager.Instance?.HighlightMass(mass);
        float f = NewtonForceController.Instance != null ? NewtonForceController.Instance.Force : 0f;
        NewtonUIManager.Instance?.UpdateLiveNewtonReadings(
            mass, f,
            NewtonAccelerationCalculator.Instance != null ? NewtonAccelerationCalculator.Instance.Calculate(f, mass) : 0f,
            TrolleyController.Instance != null ? TrolleyController.Instance.Velocity : 0f,
            0f, mass * 9.8f, true);
    }

    public void AddMassBlock(float amount) => SetMass(mass + amount);
    public void RemoveMassBlock(float amount) => SetMass(mass - amount);
}
