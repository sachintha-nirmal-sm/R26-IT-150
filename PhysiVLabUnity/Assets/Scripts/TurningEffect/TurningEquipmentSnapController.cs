using UnityEngine;

public class TurningEquipmentSnapController : MonoBehaviour
{
    public static TurningEquipmentSnapController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnItemDropped(TurningUIDropTarget zone, TurningDragDrop2D item)
    {
        if (zone == null || item == null) return;
        string zoneId = zone.ZoneId;
        if (zoneId == "Independent" || zoneId == "Dependent" || zoneId == "Controlled" || zoneId == "RequiredEquipment")
            return;

        var step = TurningExperimentManager.Instance != null ? TurningExperimentManager.Instance.CurrentStep : TurningExperimentStep.Introduction;
        if (item.ItemId == "NewtonBalance")
        {
            if (step != TurningExperimentStep.ApplyForce)
            {
                item.ReturnHome();
                TurningFeedbackManager.Instance?.ShowInstruction("Hook the Newton balance after the apparatus is assembled.");
                return;
            }
            bool hooked = TurningMomentController.Instance != null && TurningMomentController.Instance.TryAttachBalance(zoneId);
            if (hooked)
            {
                item.SetDraggable(false);
                item.SnapTo(zone.transform, Vector2.zero);
                return;
            }
            item.ReturnHome();
            return;
        }

        if (step != TurningExperimentStep.Assembly)
        {
            item.ReturnHome();
            TurningFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        bool ok = TurningAssemblyManager.Instance != null && TurningAssemblyManager.Instance.TryPlace(item.ItemId, zoneId);
        if (ok)
        {
            bool reusable = item.ItemId == "Drill";
            if (reusable)
            {
                item.ReturnHome();
                return;
            }
            item.SetDraggable(false);
            item.SnapTo(zone.transform, Vector2.zero);
            return;
        }
        item.ReturnHome();
    }

    public void PlaceFromClick(TurningDragDrop2D item)
    {
        if (item == null) return;
        var step = TurningExperimentManager.Instance != null ? TurningExperimentManager.Instance.CurrentStep : TurningExperimentStep.Introduction;

        if (item.ItemId == "NewtonBalance")
        {
            if (step != TurningExperimentStep.ApplyForce)
            {
                TurningFeedbackManager.Instance?.ShowInstruction("Hook the Newton balance after the apparatus is assembled.");
                return;
            }
            bool hooked = TurningMomentController.Instance != null && TurningMomentController.Instance.TryAttachBalance("LoopD");
            if (!hooked) return;
            item.SetDraggable(false);
            var zone = GameObject.Find("LoopD");
            if (zone != null) item.SnapTo(zone.transform, Vector2.zero);
            return;
        }

        if (step != TurningExperimentStep.Assembly)
        {
            TurningFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        string zoneId = DefaultZone(item.ItemId);
        bool ok = TurningAssemblyManager.Instance != null && TurningAssemblyManager.Instance.TryPlace(item.ItemId, zoneId);
        if (!ok) return;
        if (item.ItemId == "Drill") return;
        item.SetDraggable(false);
        var zoneGo = GameObject.Find(ZoneObjectName(item.ItemId));
        if (zoneGo != null) item.SnapTo(zoneGo.transform, Vector2.zero);
    }

    private static string DefaultZone(string itemId)
    {
        switch (itemId)
        {
            case "Table": return "TableZone";
            case "Stick": return "StickZone";
            case "Drill": return NextDrillZone();
            case "Washer1": return "PivotO";
            case "ScrewNail": return "PivotO";
            case "Washer2": return "PivotO";
            case "Wire": return "LoopA";
            default: return "";
        }
    }

    private static string NextDrillZone()
    {
        var a = TurningAssemblyManager.Instance;
        if (a == null) return "PivotO";
        if (!a.HoleODrilled) return "PivotO";
        if (!a.HoleADrilled) return "HoleA";
        if (!a.HoleBDrilled) return "HoleB";
        if (!a.HoleCDrilled) return "HoleC";
        if (!a.HoleDDrilled) return "HoleD";
        return "PivotO";
    }

    private static string ZoneObjectName(string itemId)
    {
        switch (itemId)
        {
            case "Table": return "TableZone";
            case "Stick": return "StickZone";
            case "Washer1":
            case "ScrewNail":
            case "Washer2": return "PivotO";
            case "Wire": return "LoopA";
            default: return "";
        }
    }
}
