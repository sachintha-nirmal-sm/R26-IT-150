using UnityEngine;

public class WorkEnergyDepressionController : MonoBehaviour
{
    public static WorkEnergyDepressionController Instance { get; private set; }

    [SerializeField] private float baseDepth = 0f;
    [SerializeField] private float energyFactor = 0.306122f;
    [SerializeField] private float maximumDepth = 3f;
    [SerializeField] private float lastDepth;

    public float LastDepth => lastDepth;
    public float MaximumDepth => maximumDepth;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float CalculateDepth(float potentialEnergy)
    {
        lastDepth = Mathf.Clamp(baseDepth + energyFactor * potentialEnergy, 0.05f, maximumDepth);
        lastDepth = Mathf.Round(lastDepth * 10f) / 10f;
        return lastDepth;
    }

    public void DisplayDepression(float depthCm)
    {
        lastDepth = depthCm;
        WorkEnergyClaySurfaceController.Instance?.ShowDepression(depthCm, maximumDepth);
    }

    public void ResetDepression()
    {
        lastDepth = 0f;
        WorkEnergyClaySurfaceController.Instance?.SetFlat();
        if (WorkEnergyClayController.Instance != null && WorkEnergyClayController.Instance.ClayPrepared)
            WorkEnergyClaySurfaceController.Instance?.ShowPrepared();
    }
}
