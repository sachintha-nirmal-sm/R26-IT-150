using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElectronicsCircuitConnectionManager : MonoBehaviour
{
    public static ElectronicsCircuitConnectionManager Instance { get; private set; }

    private readonly HashSet<string> placed = new HashSet<string>();
    private readonly Dictionary<string, ElectronicsUIDropTarget> zones = new Dictionary<string, ElectronicsUIDropTarget>();
    private TextMeshProUGUI statusLabel;
    private bool forwardScored;
    private bool reverseScored;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(
        ElectronicsUIDropTarget board, ElectronicsUIDropTarget battery, ElectronicsUIDropTarget sw, ElectronicsUIDropTarget diode, ElectronicsUIDropTarget bulb,
        ElectronicsUIDropTarget wire1, ElectronicsUIDropTarget wire2, ElectronicsUIDropTarget wire3, ElectronicsUIDropTarget wire4,
        TextMeshProUGUI status)
    {
        zones.Clear();
        Register("BoardZone", board);
        Register("BatteryZone", battery);
        Register("SwitchZone", sw);
        Register("DiodeZone", diode);
        Register("BulbZone", bulb);
        Register("WireBatterySwitch", wire1);
        Register("WireSwitchDiode", wire2);
        Register("WireDiodeBulb", wire3);
        Register("WireBulbBattery", wire4);
        statusLabel = status;
        RefreshState();
    }

    public ElectronicsUIDropTarget FindZone(string zoneId) => zones.TryGetValue(zoneId, out var z) ? z : null;

    public string SuggestedZone(string itemId)
    {
        switch (itemId)
        {
            case "Breadboard": return "BoardZone";
            case "Battery":
            case "DryCells": return "BatteryZone";
            case "Switch": return "SwitchZone";
            case "Diode": return "DiodeZone";
            case "Bulb": return "BulbZone";
            case "Wire":
            case "Wires":
                if (ElectronicsWireController.Instance == null) return "WireBatterySwitch";
                if (!ElectronicsWireController.Instance.IsConnected(ElectronicsWireController.BatteryToSwitch)) return "WireBatterySwitch";
                if (!ElectronicsWireController.Instance.IsConnected(ElectronicsWireController.SwitchToDiode)) return "WireSwitchDiode";
                if (!ElectronicsWireController.Instance.IsConnected(ElectronicsWireController.DiodeToBulb)) return "WireDiodeBulb";
                return "WireBulbBattery";
            default: return null;
        }
    }

    public bool TryPlace(string itemId, string zoneId)
    {
        string expected = SuggestedZone(itemId);
        if (itemId == "Wire" || itemId == "Wires")
        {
            string conn = WireFromZone(zoneId);
            if (conn == null)
            {
                ElectronicsScoreManager.Instance?.SubtractScore(3);
                ElectronicsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT CONNECTION\nDrop the wire onto a labelled connection slot.", "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
                return false;
            }
            bool ok = ElectronicsWireController.Instance != null && ElectronicsWireController.Instance.TryConnect(conn);
            if (ok) HideZoneChrome(zoneId);
            return ok;
        }

        if (string.IsNullOrEmpty(expected) || expected != zoneId)
        {
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT CONNECTION\nIncorrect position. Place each component on its labelled pad.", "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
            return false;
        }

        string key = Canonical(itemId);
        if (placed.Contains(key)) return true;
        placed.Add(key);
        HideZoneChrome(zoneId);
        ApplyPlacement(key);
        ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.Placement, 1, false);
        ElectronicsFeedbackManager.Instance?.ShowMessage("✓ CONNECTION CORRECT\n" + Explain(key), "+3 MARKS", new Color(0.08f, 0.52f, 0.22f));
        RefreshState();
        return true;
    }

    public void NotifyWrongDrop(ElectronicsDragDrop2D item, ElectronicsUIDropTarget zone)
    {
        ElectronicsScoreManager.Instance?.SubtractScore(3);
        ElectronicsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT CONNECTION\nIncorrect position.", "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
    }

    public bool CheckCircuit() => IsCircuitComplete();

    public bool IsCircuitComplete()
    {
        return placed.Contains("Breadboard") &&
               placed.Contains("Battery") &&
               placed.Contains("Switch") &&
               placed.Contains("Diode") &&
               placed.Contains("Bulb") &&
               ElectronicsWireController.Instance != null &&
               ElectronicsWireController.Instance.AllWiresConnected &&
               ElectronicsBatteryController.Instance != null &&
               ElectronicsBatteryController.Instance.IsConnected;
    }

    public bool IsForwardBias()
    {
        if (!IsCircuitComplete()) return false;
        if (ElectronicsDiodeController.Instance == null || ElectronicsBatteryController.Instance == null) return false;
        return ElectronicsBatteryController.Instance.IsNormalPolarity() && !ElectronicsDiodeController.Instance.IsFlipped;
    }

    public bool IsReverseBias()
    {
        if (!IsCircuitComplete()) return false;
        if (ElectronicsDiodeController.Instance == null || ElectronicsBatteryController.Instance == null) return false;
        return ElectronicsBatteryController.Instance.IsReversedPolarity() && !ElectronicsDiodeController.Instance.IsFlipped;
    }

    public void ResetCircuit()
    {
        placed.Clear();
        forwardScored = false;
        reverseScored = false;
        foreach (var pair in zones) ShowZoneChrome(pair.Value);
        ElectronicsBatteryController.Instance?.ResetBattery();
        ElectronicsDiodeController.Instance?.ResetDiode();
        ElectronicsBulbController.Instance?.ResetBulb();
        ElectronicsSwitchController.Instance?.ResetSwitch();
        ElectronicsWireController.Instance?.ResetWires();
        ElectronicsCircuitBoardManager.Instance?.ResetBoard();
        ElectronicsLabEquipmentTray.Instance?.ResetTray();
        RefreshState();
    }

    public void RefreshState()
    {
        ElectronicsDiodeController.Instance?.RefreshBias();
        ElectronicsCircuitBoardManager.Instance?.Refresh();
        if (statusLabel != null) statusLabel.text = NextHint();
        ElectronicsUIManager.Instance?.UpdateCircuitStatus(NextHint());
        UpdateCurrentFlow();
    }

    public void OnSwitchChanged(bool on)
    {
        if (!on)
        {
            ElectronicsBulbController.Instance?.TurnOff();
            RefreshState();
            return;
        }

        var step = ElectronicsPracticalManager.Instance != null
            ? ElectronicsPracticalManager.Instance.CurrentStep
            : ElectronicsPracticalStep.Introduction;

        if (step == ElectronicsPracticalStep.ForwardBias || step == ElectronicsPracticalStep.ForwardObservation)
            ElectronicsForwardBiasController.Instance?.OnSwitchOn();
        else if (step == ElectronicsPracticalStep.ReverseBias || step == ElectronicsPracticalStep.ReverseObservation)
            ElectronicsReverseBiasController.Instance?.OnSwitchOn();
        else if (step == ElectronicsPracticalStep.Challenge)
            ElectronicsMiniChallengeManager.Instance?.OnSwitchOn();
        else
            UpdateCurrentFlow();
    }

    public void ScoreForwardIfReady()
    {
        if (forwardScored) return;
        if (!IsForwardBias())
        {
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ Incorrect battery polarity or diode direction.", "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            return;
        }
        forwardScored = true;
        ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.ForwardCircuit, 10, false);
        ElectronicsFeedbackManager.Instance?.ShowMessage("✓ FORWARD BIAS CONNECTION CORRECT", "+10 MARKS", new Color(0.08f, 0.52f, 0.22f));
        ElectronicsForwardBiasController.Instance?.MarkValidated();
    }

    public void ScoreReverseIfReady()
    {
        if (reverseScored) return;
        if (!IsReverseBias())
        {
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ Incorrect. Reverse only the battery. Leave the diode unchanged.", "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            return;
        }
        reverseScored = true;
        ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.ReverseCircuit, 10, false);
        ElectronicsFeedbackManager.Instance?.ShowMessage("✓ REVERSE BIAS CONNECTION CORRECT", "+10 MARKS", new Color(0.08f, 0.52f, 0.22f));
        ElectronicsReverseBiasController.Instance?.MarkValidated();
    }

    public string NextHint()
    {
        if (!placed.Contains("Breadboard")) return "Drag the circuit board onto the BREADBOARD area.";
        if (!placed.Contains("Battery")) return "Place the two 1.5 V dry cells on the BATTERY pad. Total: 3 V.";
        if (!placed.Contains("Switch")) return "Place the switch on the SWITCH pad.";
        if (!placed.Contains("Diode")) return "Place the IN4001 diode with anode toward the battery positive.";
        if (!placed.Contains("Bulb")) return "Place the 2.5 V torch bulb on the BULB pad.";
        if (ElectronicsWireController.Instance == null || !ElectronicsWireController.Instance.AllWiresConnected)
            return "Connect wires: Battery → Switch → Diode → Bulb → Battery.";
        if (ElectronicsBatteryController.Instance != null && !ElectronicsBatteryController.Instance.IsConnected)
            return "Reconnect the battery to close the circuit.";
        if (IsForwardBias()) return "Circuit ready. Forward bias: diode allows current. You may turn ON the switch.";
        if (IsReverseBias()) return "Circuit ready. Reverse bias: diode blocks current. You may turn ON the switch.";
        return "Check diode direction and battery polarity.";
    }

    public bool HasComponent(string key) => placed.Contains(key);

    private void UpdateCurrentFlow()
    {
        bool on = ElectronicsSwitchController.Instance != null && ElectronicsSwitchController.Instance.IsOn();
        bool flow = on && IsForwardBias();
        if (flow) ElectronicsBulbController.Instance?.TurnOn();
        else ElectronicsBulbController.Instance?.TurnOff();
    }

    private void ApplyPlacement(string key)
    {
        switch (key)
        {
            case "Breadboard":
                ElectronicsCircuitBoardManager.Instance?.SetBoardPlaced(true);
                break;
            case "Battery":
                ElectronicsBatteryController.Instance?.SetPlaced(true);
                break;
            case "Switch":
                ElectronicsSwitchController.Instance?.SetPlaced(true);
                break;
            case "Diode":
                ElectronicsDiodeController.Instance?.SetPlaced(true);
                break;
            case "Bulb":
                ElectronicsBulbController.Instance?.SetPlaced(true);
                break;
        }
    }

    private static string WireFromZone(string zoneId)
    {
        switch (zoneId)
        {
            case "WireBatterySwitch": return ElectronicsWireController.BatteryToSwitch;
            case "WireSwitchDiode": return ElectronicsWireController.SwitchToDiode;
            case "WireDiodeBulb": return ElectronicsWireController.DiodeToBulb;
            case "WireBulbBattery": return ElectronicsWireController.BulbToBattery;
            default: return null;
        }
    }

    private void Register(string id, ElectronicsUIDropTarget zone)
    {
        if (zone == null) return;
        zones[id] = zone;
    }

    private void HideZoneChrome(string zoneId)
    {
        var zone = FindZone(zoneId);
        if (zone == null) return;
        var hint = zone.transform.Find("Hint");
        if (hint != null) hint.gameObject.SetActive(false);
        var img = zone.GetComponent<Image>();
        if (img != null) img.color = new Color(0.82f, 0.93f, 0.86f, 1f);
    }

    private static void ShowZoneChrome(ElectronicsUIDropTarget zone)
    {
        if (zone == null) return;
        var hint = zone.transform.Find("Hint");
        if (hint != null) hint.gameObject.SetActive(true);
        var img = zone.GetComponent<Image>();
        if (img != null) img.color = new Color(1f, 1f, 1f, 0.92f);
    }

    private static string Canonical(string itemId)
    {
        if (itemId == "DryCells") return "Battery";
        if (itemId == "Wires") return "Wire";
        return itemId;
    }

    private static string Explain(string key)
    {
        switch (key)
        {
            case "Breadboard": return "Circuit board placed.";
            case "Battery": return "Two 1.5 V cells. Total voltage = 3 V. + and − are labelled.";
            case "Switch": return "Switch placed. Default state is OFF.";
            case "Diode": return "IN4001 diode placed. Anode → |>| → Cathode.";
            default: return "2.5 V torch bulb placed.";
        }
    }
}
