using UnityEngine;

public class PowerEnergyEquipmentSnapController : MonoBehaviour
{
    public static PowerEnergyEquipmentSnapController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnItemDropped(PowerEnergyUIDropTarget zone, PowerEnergyDragDrop2D item)
    {
        if (zone == null || item == null) return;
        if (zone.ZoneId == "RequiredEquipment" || zone.ZoneId == "FormulaPower" || zone.ZoneId == "FormulaEnergy" || zone.ZoneId == "FormulaKwh")
            return;

        var step = PowerEnergyExperimentManager.Instance != null
            ? PowerEnergyExperimentManager.Instance.CurrentStep
            : PowerEnergyExperimentStep.Introduction;

        if (step != PowerEnergyExperimentStep.CircuitSetup)
        {
            item.ReturnHome();
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Place circuit equipment during the circuit setup step.");
            return;
        }

        bool placed = PowerEnergyCircuitConnectionManager.Instance != null && PowerEnergyCircuitConnectionManager.Instance.TryPlace(item.ItemId, zone.ZoneId);
        if (placed)
        {
            item.SetDraggable(false);
            return;
        }
        item.ReturnHome();
    }

    public void PlaceFromClick(PowerEnergyDragDrop2D item)
    {
        if (item == null) return;
        var step = PowerEnergyExperimentManager.Instance != null
            ? PowerEnergyExperimentManager.Instance.CurrentStep
            : PowerEnergyExperimentStep.Introduction;
        if (step != PowerEnergyExperimentStep.CircuitSetup)
        {
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Place circuit equipment during the circuit setup step.");
            return;
        }

        string zone = PowerEnergyCircuitConnectionManager.Instance != null ? PowerEnergyCircuitConnectionManager.Instance.SuggestedZone(item.ItemId) : null;
        if (string.IsNullOrEmpty(zone))
        {
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("That item cannot be placed yet.");
            return;
        }

        bool placed = PowerEnergyCircuitConnectionManager.Instance.TryPlace(item.ItemId, zone);
        if (placed)
        {
            var target = PowerEnergyCircuitConnectionManager.Instance.FindZone(zone);
            if (target != null) item.SnapTo(target.transform, Vector2.zero);
            item.SetDraggable(false);
        }
        else item.ReturnHome();
    }
}
