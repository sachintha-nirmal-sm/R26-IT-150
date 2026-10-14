using UnityEngine;

public class WorkEnergyReleaseStandController : MonoBehaviour
{
    public static WorkEnergyReleaseStandController Instance { get; private set; }

    [SerializeField] private bool standPlaced;

    public bool StandPlaced => standPlaced;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void PlaceStand() => standPlaced = true;
    public void ResetStand() => standPlaced = false;
}
