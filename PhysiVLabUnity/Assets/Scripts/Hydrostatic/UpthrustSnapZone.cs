using UnityEngine;

/// <summary>
/// Trigger volume / UI drop target for a practical step.
/// 3D setup: empty GameObject + Box Collider (Is Trigger = true).
/// UI setup: RectTransform + this script; UpthrustDraggableEquipment calls TryAccept.
/// </summary>
public class UpthrustSnapZone : MonoBehaviour
{
    [Header("Accepted Apparatus")]
    [SerializeField] private UpthrustApparatusType acceptedType;
    [SerializeField] private int associatedStepIndex;

    [Header("Snap")]
    [SerializeField] private Transform snapPoint;
    [SerializeField] private bool hideZoneVisualOnSnap = true;
    [SerializeField] private GameObject zoneVisual;

    [Header("State")]
    [SerializeField] private bool isOccupied;

    public UpthrustApparatusType AcceptedType => acceptedType;
    public int AssociatedStepIndex => associatedStepIndex;
    public bool IsOccupied => isOccupied;
    public Transform SnapPoint => snapPoint != null ? snapPoint : transform;

    public event System.Action<UpthrustSnapZone, UpthrustDraggableEquipment> OnCorrectSnap;
    public event System.Action<UpthrustSnapZone, UpthrustDraggableEquipment> OnWrongSnap;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    public void Configure(UpthrustApparatusType type, int stepIndex, Transform point, GameObject visual)
    {
        acceptedType = type;
        associatedStepIndex = stepIndex;
        snapPoint = point;
        zoneVisual = visual;
    }

    private void OnTriggerEnter(Collider other)
    {
        UpthrustDraggableEquipment drag = other.GetComponent<UpthrustDraggableEquipment>();
        if (drag == null)
            drag = other.GetComponentInParent<UpthrustDraggableEquipment>();

        if (drag != null)
            TryAccept(drag);
    }

    /// <summary>Used by UI drag-drop and 3D triggers.</summary>
    public bool TryAccept(UpthrustDraggableEquipment drag)
    {
        if (drag == null || isOccupied) return false;

        UpthrustPracticalManager steps = UpthrustPracticalManager.Instance;
        if (steps == null || !steps.IsZoneActiveForCurrentStep(this))
        {
            steps?.NotifyOutOfOrderAttempt();
            return false;
        }

        if (drag.Type == acceptedType)
        {
            SnapObject(drag);
            OnCorrectSnap?.Invoke(this, drag);
            steps.NotifyCorrectPlacement(this, drag);
            return true;
        }

        OnWrongSnap?.Invoke(this, drag);
        steps.NotifyWrongPlacement(this, drag);
        return false;
    }

    public void SnapObject(UpthrustDraggableEquipment drag)
    {
        isOccupied = true;
        drag.SnapTo(SnapPoint);

        if (hideZoneVisualOnSnap && zoneVisual != null)
            zoneVisual.SetActive(false);
    }

    public void ResetZone()
    {
        isOccupied = false;
        if (zoneVisual != null)
            zoneVisual.SetActive(true);
    }
}
