using UnityEngine;

public class OpticsEquipmentSnapController : MonoBehaviour
{
    public static OpticsEquipmentSnapController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnItemDropped(OpticsUIDropTarget zone, OpticsDragDrop2D item)
    {
        if (zone == null || item == null) return;
        if (zone.ZoneId == "Independent" || zone.ZoneId == "Dependent" || zone.ZoneId == "Controlled" || zone.ZoneId == "RequiredEquipment")
            return;

        var step = OpticsExperimentManager.Instance != null
            ? OpticsExperimentManager.Instance.CurrentStep
            : OpticsExperimentStep.Introduction;

        if (step != OpticsExperimentStep.Assembly)
        {
            item.ReturnHome();
            OpticsFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        bool placed = OpticsAssemblyManager.Instance != null && OpticsAssemblyManager.Instance.TryPlace(item.ItemId, zone.ZoneId);
        if (placed)
        {
            item.SetDraggable(false);
            item.gameObject.SetActive(false);
            return;
        }
        item.ReturnHome();
    }

    public void PlaceFromClick(OpticsDragDrop2D item)
    {
        if (item == null) return;
        var step = OpticsExperimentManager.Instance != null
            ? OpticsExperimentManager.Instance.CurrentStep
            : OpticsExperimentStep.Introduction;
        if (step != OpticsExperimentStep.Assembly)
        {
            OpticsFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        string zone = OpticsAssemblyManager.Instance != null ? OpticsAssemblyManager.Instance.SuggestedZone(item.ItemId) : null;
        if (string.IsNullOrEmpty(zone))
        {
            OpticsFeedbackManager.Instance?.ShowInstruction("That item cannot be placed yet.");
            return;
        }

        bool placed = OpticsAssemblyManager.Instance.TryPlace(item.ItemId, zone);
        if (placed)
        {
            item.SetDraggable(false);
            item.gameObject.SetActive(false);
        }
        else item.ReturnHome();
    }
}
