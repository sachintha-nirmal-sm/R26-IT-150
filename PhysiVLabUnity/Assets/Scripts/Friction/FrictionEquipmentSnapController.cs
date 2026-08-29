using UnityEngine;

public class FrictionEquipmentSnapController : MonoBehaviour
{
    public static FrictionEquipmentSnapController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnItemDropped(FrictionUIDropTarget zone, FrictionDragDrop2D item)
    {
        if (zone == null || item == null) return;
        string zoneId = zone.ZoneId;
        if (zoneId == "Independent" || zoneId == "Dependent" || zoneId == "Controlled" || zoneId == "RequiredEquipment")
            return;

        if (FrictionExperimentManager.Instance != null &&
            FrictionExperimentManager.Instance.CurrentStep != FrictionExperimentStep.Setup &&
            FrictionExperimentManager.Instance.CurrentStep != FrictionExperimentStep.Pulling)
        {
            item.ReturnHome();
            FrictionFeedbackManager.Instance?.ShowInstruction("Place equipment during the experiment setup.");
            return;
        }

        string itemId = item.ItemId;

        if (zoneId == "Table" || zoneId == "SandpaperZone")
        {
            if (itemId == "Sandpaper")
            {
                SandpaperController.Instance?.Place();
                FrictionTrialManager.Instance?.NotifySandpaperPlaced();
                item.SetDraggable(false);
                return;
            }
            if (itemId == "WoodenBlock")
            {
                if (SandpaperController.Instance == null || !SandpaperController.Instance.IsPlaced)
                {
                    item.ReturnHome();
                    FrictionTrialManager.Instance?.NotifyWrongPlacement("Place the sandpaper on the table first.");
                    return;
                }
                FrictionTrialManager.Instance?.NotifyBlockPlaced();
                item.SetDraggable(false);
                return;
            }
        }

        if (zoneId == "BlockHook" || zoneId == "Block")
        {
            if (itemId == "NewtonBalance")
            {
                FrictionTrialManager.Instance?.NotifyBalanceAttached();
                item.SetDraggable(false);
                return;
            }
        }

        item.ReturnHome();
        FrictionTrialManager.Instance?.NotifyWrongPlacement("That item does not belong in this position.");
    }

    public void PlaceFromClick(FrictionDragDrop2D item)
    {
        if (item == null) return;
        if (item.ItemId == "Sandpaper")
        {
            SandpaperController.Instance?.Place();
            FrictionTrialManager.Instance?.NotifySandpaperPlaced();
            item.SetDraggable(false);
            var table = GameObject.Find("SandpaperZone");
            if (table != null) item.SnapTo(table.transform, Vector2.zero);
            return;
        }
        if (item.ItemId == "WoodenBlock")
        {
            if (SandpaperController.Instance == null || !SandpaperController.Instance.IsPlaced)
            {
                FrictionFeedbackManager.Instance?.ShowInstruction("Place the sandpaper on the table first.");
                FrictionScoreManager.Instance?.SubtractScore(5);
                return;
            }
            FrictionTrialManager.Instance?.NotifyBlockPlaced();
            item.SetDraggable(false);
            var zone = GameObject.Find("BlockZone");
            if (zone != null) item.SnapTo(zone.transform, Vector2.zero);
            return;
        }
        if (item.ItemId == "NewtonBalance")
        {
            FrictionTrialManager.Instance?.NotifyBalanceAttached();
            item.SetDraggable(false);
            var hook = GameObject.Find("HookZone");
            if (hook != null) item.SnapTo(hook.transform, Vector2.zero);
        }
    }
}
