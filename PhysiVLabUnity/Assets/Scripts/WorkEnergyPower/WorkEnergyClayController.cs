using UnityEngine;

public class WorkEnergyClayController : MonoBehaviour
{
    public static WorkEnergyClayController Instance { get; private set; }

    [SerializeField] private float clayThickness = 3f;
    [SerializeField] private bool clayPrepared;

    public float ClayThickness => clayThickness;
    public bool ClayPrepared => clayPrepared;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool TryPrepareClay()
    {
        if (clayPrepared) return true;
        clayPrepared = true;
        return true;
    }

    public void ResetClayPrep()
    {
        clayPrepared = false;
        WorkEnergyDepressionController.Instance?.ResetDepression();
        WorkEnergyClaySurfaceController.Instance?.SetFlat();
    }
}
