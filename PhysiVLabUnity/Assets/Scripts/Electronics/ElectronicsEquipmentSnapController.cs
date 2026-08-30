using UnityEngine;

public class ElectronicsEquipmentSnapController : MonoBehaviour
{
    public static ElectronicsEquipmentSnapController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnItemDropped(ElectronicsUIDropTarget zone, ElectronicsDragDrop2D item)
    {
        if (zone == null || item == null) return;
        if (zone.ZoneId == "RequiredEquipment") return;
        if (zone.ZoneId != null && zone.ZoneId.StartsWith("Match"))
            return;

        var step = ElectronicsPracticalManager.Instance != null
            ? ElectronicsPracticalManager.Instance.CurrentStep
            : ElectronicsPracticalStep.Introduction;

        bool inCircuit = step == ElectronicsPracticalStep.CircuitSetup
                         || step == ElectronicsPracticalStep.ForwardBias
                         || step == ElectronicsPracticalStep.BatteryDisconnect
                         || step == ElectronicsPracticalStep.BatteryReverse
                         || step == ElectronicsPracticalStep.ReverseBias
                         || step == ElectronicsPracticalStep.Challenge;

        if (!inCircuit)
        {
            item.ReturnHome();
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Incorrect position.");
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            return;
        }

        bool placed = ElectronicsCircuitConnectionManager.Instance != null && ElectronicsCircuitConnectionManager.Instance.TryPlace(item.ItemId, zone.ZoneId);
        if (placed)
        {
            bool isWire = item.ItemId == "Wire" || item.ItemId == "Wires";
            if (isWire)
            {
                item.ReturnHome();
                item.gameObject.SetActive(false);
            }
            else
                item.SetDraggable(false);
            return;
        }
        item.ReturnHome();
    }

    public void PlaceFromClick(ElectronicsDragDrop2D item)
    {
        if (item == null) return;
        var step = ElectronicsPracticalManager.Instance != null
            ? ElectronicsPracticalManager.Instance.CurrentStep
            : ElectronicsPracticalStep.Introduction;
        if (step != ElectronicsPracticalStep.CircuitSetup && step != ElectronicsPracticalStep.Challenge)
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Place circuit equipment on the breadboard first.");
            return;
        }

        string zone = ElectronicsCircuitConnectionManager.Instance != null ? ElectronicsCircuitConnectionManager.Instance.SuggestedZone(item.ItemId) : null;
        if (string.IsNullOrEmpty(zone))
        {
            ElectronicsFeedbackManager.Instance?.ShowInstruction("That item cannot be placed yet.");
            return;
        }

        bool placed = ElectronicsCircuitConnectionManager.Instance.TryPlace(item.ItemId, zone);
        if (placed)
        {
            bool isWire = item.ItemId == "Wire" || item.ItemId == "Wires";
            if (isWire)
            {
                item.ReturnHome();
                item.gameObject.SetActive(false);
            }
            else
            {
                var target = ElectronicsCircuitConnectionManager.Instance.FindZone(zone);
                if (target != null) item.SnapTo(target.transform, Vector2.zero);
                item.SetDraggable(false);
            }
        }
        else item.ReturnHome();
    }
}
