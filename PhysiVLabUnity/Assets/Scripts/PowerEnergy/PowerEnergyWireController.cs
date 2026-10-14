using UnityEngine;

public class PowerEnergyWireController : MonoBehaviour
{
    public static PowerEnergyWireController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void PlaceWires()
    {
        PowerEnergyCircuitConnectionManager.Instance?.TryPlace("Wire", "WireZone");
    }

    public void ResetWires()
    {
        PowerEnergyUIManager.Instance?.UpdateLiveReadings();
    }
}
