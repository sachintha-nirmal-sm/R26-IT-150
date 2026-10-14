using UnityEngine;

public class WavesEquipmentSnapController : MonoBehaviour
{
    public static WavesEquipmentSnapController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnItemDropped(WavesUIDropTarget zone, WavesDragDrop2D item)
    {
        if (zone == null || item == null) return;
        if (zone.ZoneId == "Independent" || zone.ZoneId == "Dependent" || zone.ZoneId == "Controlled" || zone.ZoneId == "RequiredEquipment")
            return;

        var step = WavesExperimentManager.Instance != null
            ? WavesExperimentManager.Instance.CurrentStep
            : WavesExperimentStep.Introduction;

        if (step != WavesExperimentStep.Assembly)
        {
            item.ReturnHome();
            WavesFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        bool placed = WavesAssemblyManager.Instance != null && WavesAssemblyManager.Instance.TryPlace(item.ItemId, zone.ZoneId);
        if (placed)
        {
            item.SetDraggable(false);
            item.gameObject.SetActive(false);
            return;
        }
        item.ReturnHome();
    }

    public void PlaceFromClick(WavesDragDrop2D item)
    {
        if (item == null) return;
        var step = WavesExperimentManager.Instance != null
            ? WavesExperimentManager.Instance.CurrentStep
            : WavesExperimentStep.Introduction;
        if (step != WavesExperimentStep.Assembly)
        {
            WavesFeedbackManager.Instance?.ShowInstruction("Place equipment during the assembly step.");
            return;
        }

        string zone = WavesAssemblyManager.Instance != null ? WavesAssemblyManager.Instance.SuggestedZone(item.ItemId) : null;
        if (string.IsNullOrEmpty(zone))
        {
            WavesFeedbackManager.Instance?.ShowInstruction("That item cannot be placed yet.");
            return;
        }

        var refs = Object.FindAnyObjectByType<WavesUIRefs>();
        Transform target = FindZone(refs != null ? refs.LaboratoryPanel : null, zone);
        if (target == null)
        {
            item.ReturnHome();
            return;
        }

        bool placed = WavesAssemblyManager.Instance.TryPlace(item.ItemId, zone);
        if (placed)
        {
            item.SetDraggable(false);
            item.gameObject.SetActive(false);
        }
        else item.ReturnHome();
    }

    private static Transform FindZone(GameObject lab, string zoneId)
    {
        if (lab == null) return null;
        var drops = lab.GetComponentsInChildren<WavesUIDropTarget>(true);
        foreach (var d in drops)
            if (d != null && d.ZoneId == zoneId) return d.transform;
        return null;
    }
}
