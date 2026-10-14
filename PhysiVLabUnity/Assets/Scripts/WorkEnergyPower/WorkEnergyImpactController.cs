using UnityEngine;

public class WorkEnergyImpactController : MonoBehaviour
{
    public static WorkEnergyImpactController Instance { get; private set; }

    [SerializeField] private bool impactDetected;

    public bool ImpactDetected => impactDetected;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnImpact()
    {
        if (impactDetected) return;
        impactDetected = true;
        WorkEnergyFeedbackManager.Instance?.ShowInstruction("Impact detected.\nObserve the depression produced on the clay.");
        WorkEnergyLabWorkbench.Instance?.HandleImpact();
    }

    public void ResetImpact()
    {
        impactDetected = false;
    }
}
