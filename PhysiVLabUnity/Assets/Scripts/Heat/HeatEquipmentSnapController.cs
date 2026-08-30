using UnityEngine;

public class HeatEquipmentSnapController : MonoBehaviour
{
    public static HeatEquipmentSnapController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnItemDropped(HeatUIDropTarget zone, HeatDragDrop2D item)
    {
        if (zone == null || item == null) return;
        if (zone.ZoneId == "Independent" || zone.ZoneId == "Dependent" || zone.ZoneId == "Controlled" || zone.ZoneId == "RequiredEquipment")
            return;

        var step = HeatExperimentManager.Instance != null
            ? HeatExperimentManager.Instance.CurrentStep
            : HeatExperimentStep.Introduction;

        if (step != HeatExperimentStep.Assembly)
        {
            item.ReturnHome();
            HeatFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        bool placed = HeatAssemblyManager.Instance != null && HeatAssemblyManager.Instance.TryPlace(item.ItemId, zone.ZoneId);
        if (placed)
        {
            item.SetDraggable(false);
            item.gameObject.SetActive(false);
            return;
        }
        item.ReturnHome();
    }

    public void PlaceFromClick(HeatDragDrop2D item)
    {
        if (item == null) return;
        var step = HeatExperimentManager.Instance != null
            ? HeatExperimentManager.Instance.CurrentStep
            : HeatExperimentStep.Introduction;
        if (step != HeatExperimentStep.Assembly)
        {
            HeatFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        string zone = HeatAssemblyManager.Instance != null ? HeatAssemblyManager.Instance.SuggestedZone(item.ItemId) : null;
        if (string.IsNullOrEmpty(zone))
        {
            HeatFeedbackManager.Instance?.ShowInstruction("That item cannot be placed yet.");
            return;
        }

        bool placed = HeatAssemblyManager.Instance.TryPlace(item.ItemId, zone);
        if (placed)
        {
            item.SetDraggable(false);
            item.gameObject.SetActive(false);
        }
        else item.ReturnHome();
    }
}
