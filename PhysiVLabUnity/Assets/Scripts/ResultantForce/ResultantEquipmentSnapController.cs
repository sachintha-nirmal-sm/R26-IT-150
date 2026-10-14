using UnityEngine;

public class ResultantEquipmentSnapController : MonoBehaviour
{
    public static ResultantEquipmentSnapController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnItemDropped(ResultantUIDropTarget zone, ResultantDragDrop2D item)
    {
        if (zone == null || item == null) return;
        string zoneId = zone.ZoneId;
        if (zoneId == "Independent" || zoneId == "Dependent" || zoneId == "Controlled" || zoneId == "RequiredEquipment")
            return;

        if (ResultantExperimentManager.Instance != null &&
            ResultantExperimentManager.Instance.CurrentStep != ResultantExperimentStep.Assembly)
        {
            item.ReturnHome();
            ResultantFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        bool ok = ResultantAssemblyManager.Instance != null && ResultantAssemblyManager.Instance.TryPlace(item.ItemId, zoneId);
        if (ok)
        {
            item.SetDraggable(false);
            SnapVisual(item, zone);
            return;
        }
        item.ReturnHome();
    }

    public void PlaceFromClick(ResultantDragDrop2D item)
    {
        if (item == null) return;
        if (ResultantExperimentManager.Instance != null &&
            ResultantExperimentManager.Instance.CurrentStep != ResultantExperimentStep.Assembly)
        {
            ResultantFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        string zoneId = DefaultZone(item.ItemId);
        bool ok = ResultantAssemblyManager.Instance != null && ResultantAssemblyManager.Instance.TryPlace(item.ItemId, zoneId);
        if (!ok) return;
        item.SetDraggable(false);
        var zone = GameObject.Find(ZoneObjectName(item.ItemId));
        if (zone != null) item.SnapTo(zone.transform, Vector2.zero);
    }

    private static void SnapVisual(ResultantDragDrop2D item, ResultantUIDropTarget zone)
    {
        if (zone != null) item.SnapTo(zone.transform, Vector2.zero);
    }

    private static string DefaultZone(string itemId)
    {
        switch (itemId)
        {
            case "Trolley": return "TrolleyZone";
            case "Ring": return "RingZone";
            case "Strings": return "StringZone";
            case "Pulley1": return "PulleyZone1";
            case "Pulley2": return "PulleyZone2";
            case "BalanceB": return "HangB";
            case "BalanceC": return "HangC";
            case "BalanceA": return "BalanceAZone";
            default: return "";
        }
    }

    private static string ZoneObjectName(string itemId)
    {
        switch (itemId)
        {
            case "Trolley": return "TrolleyZone";
            case "Ring": return "RingZone";
            case "Strings": return "StringZone";
            case "Pulley1": return "PulleyZone1";
            case "Pulley2": return "PulleyZone2";
            case "BalanceB": return "HangB";
            case "BalanceC": return "HangC";
            case "BalanceA": return "BalanceAZone";
            default: return "";
        }
    }
}
