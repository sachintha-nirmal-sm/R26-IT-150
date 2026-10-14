using UnityEngine;

public class EquilibriumEquipmentSnapController : MonoBehaviour
{
    public static EquilibriumEquipmentSnapController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnItemDropped(EquilibriumUIDropTarget zone, EquilibriumDragDrop2D item)
    {
        if (zone == null || item == null) return;
        string zoneId = zone.ZoneId;
        if (zoneId == "Independent" || zoneId == "Dependent" || zoneId == "Controlled" || zoneId == "RequiredEquipment")
            return;

        var step = EquilibriumExperimentManager.Instance != null
            ? EquilibriumExperimentManager.Instance.CurrentStep
            : EquilibriumExperimentStep.Introduction;

        if (item.ItemId == "Ruler" && step == EquilibriumExperimentStep.MeasureWeight)
        {
            bool ok = EquilibriumForceController.Instance != null && EquilibriumForceController.Instance.TryWeighRuler();
            if (ok)
            {
                item.SetDraggable(false);
                item.SnapTo(zone.transform, Vector2.zero);
                return;
            }
            item.ReturnHome();
            return;
        }

        if ((item.ItemId == "BandLeft" || item.ItemId == "Ruler") && step == EquilibriumExperimentStep.Equilibrium && zoneId == "LeftHang")
        {
            bool ok = EquilibriumForceController.Instance != null && EquilibriumForceController.Instance.TryHangLeft();
            if (ok)
            {
                item.SetDraggable(false);
                item.SnapTo(zone.transform, Vector2.zero);
                return;
            }
            item.ReturnHome();
            return;
        }

        if ((item.ItemId == "BandRight" || item.ItemId == "Ruler") && step == EquilibriumExperimentStep.Equilibrium && zoneId == "RightHang")
        {
            bool ok = EquilibriumForceController.Instance != null && EquilibriumForceController.Instance.TryHangRight();
            if (ok)
            {
                item.SetDraggable(false);
                item.SnapTo(zone.transform, Vector2.zero);
                return;
            }
            item.ReturnHome();
            return;
        }

        if (step != EquilibriumExperimentStep.Assembly)
        {
            item.ReturnHome();
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        bool placed = EquilibriumAssemblyManager.Instance != null && EquilibriumAssemblyManager.Instance.TryPlace(item.ItemId, zoneId);
        if (placed)
        {
            item.SetDraggable(false);
            item.SnapTo(zone.transform, Vector2.zero);
            return;
        }
        item.ReturnHome();
    }

    public void PlaceFromClick(EquilibriumDragDrop2D item)
    {
        if (item == null) return;
        var step = EquilibriumExperimentManager.Instance != null
            ? EquilibriumExperimentManager.Instance.CurrentStep
            : EquilibriumExperimentStep.Introduction;

        if (item.ItemId == "Ruler" && step == EquilibriumExperimentStep.MeasureWeight)
        {
            if (EquilibriumForceController.Instance != null && EquilibriumForceController.Instance.TryWeighRuler())
            {
                item.SetDraggable(false);
                var zone = GameObject.Find("LeftHang");
                if (zone != null) item.SnapTo(zone.transform, Vector2.zero);
            }
            return;
        }

        if (step == EquilibriumExperimentStep.Equilibrium)
        {
            if (item.ItemId == "BandLeft" || item.ItemId == "Ruler")
            {
                if (EquilibriumForceController.Instance != null && !EquilibriumForceController.Instance.LeftHung)
                    EquilibriumForceController.Instance.TryHangLeft();
                else if (EquilibriumForceController.Instance != null && !EquilibriumForceController.Instance.RightHung)
                    EquilibriumForceController.Instance.TryHangRight();
            }
            else if (item.ItemId == "BandRight")
                EquilibriumForceController.Instance?.TryHangRight();
            return;
        }

        if (step != EquilibriumExperimentStep.Assembly)
        {
            EquilibriumFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        string zoneId = DefaultZone(item.ItemId);
        bool ok = EquilibriumAssemblyManager.Instance != null && EquilibriumAssemblyManager.Instance.TryPlace(item.ItemId, zoneId);
        if (!ok) return;
        item.SetDraggable(false);
        var zoneGo = GameObject.Find(ZoneObjectName(item.ItemId));
        if (zoneGo != null) item.SnapTo(zoneGo.transform, Vector2.zero);
    }

    private static string DefaultZone(string itemId)
    {
        switch (itemId)
        {
            case "Stand": return "StandZone";
            case "Balance1": return "LeftHang";
            case "Balance2": return "RightHang";
            case "Ruler": return "RulerZone";
            case "BandLeft": return "LeftEnd";
            case "BandRight": return "RightEnd";
            default: return "";
        }
    }

    private static string ZoneObjectName(string itemId)
    {
        switch (itemId)
        {
            case "Stand": return "StandZone";
            case "Balance1": return "LeftHang";
            case "Balance2": return "RightHang";
            case "Ruler": return "RulerZone";
            case "BandLeft": return "LeftEnd";
            case "BandRight": return "RightEnd";
            default: return "";
        }
    }
}
