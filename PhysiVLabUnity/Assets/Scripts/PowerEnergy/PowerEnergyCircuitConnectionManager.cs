using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerEnergyCircuitConnectionManager : MonoBehaviour
{
    public static PowerEnergyCircuitConnectionManager Instance { get; private set; }

    private readonly HashSet<string> placed = new HashSet<string>();
    private readonly Dictionary<string, PowerEnergyUIDropTarget> zones = new Dictionary<string, PowerEnergyUIDropTarget>();
    private Image seriesWire;
    private Image parallelWire;
    private TextMeshProUGUI statusLabel;
    private bool scoredComplete;

    public bool IsComplete =>
        placed.Contains("PowerSupply") &&
        placed.Contains("Ammeter") &&
        placed.Contains("Appliance") &&
        placed.Contains("Voltmeter") &&
        placed.Contains("Switch") &&
        placed.Contains("Wire");

    public bool PowerSupplyPlaced => placed.Contains("PowerSupply");
    public bool AmmeterInSeries => placed.Contains("Ammeter");
    public bool VoltmeterInParallel => placed.Contains("Voltmeter");
    public bool AppliancePlaced => placed.Contains("Appliance");
    public bool SwitchPlaced => placed.Contains("Switch");
    public bool WiresPlaced => placed.Contains("Wire");

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(PowerEnergyUIDropTarget supply, PowerEnergyUIDropTarget ammeter, PowerEnergyUIDropTarget appliance, PowerEnergyUIDropTarget voltmeter, PowerEnergyUIDropTarget wrongSeries, PowerEnergyUIDropTarget wrongParallel, PowerEnergyUIDropTarget switchZone, PowerEnergyUIDropTarget wireZone, Image series, Image parallel, TextMeshProUGUI status)
    {
        zones.Clear();
        Register("SupplyZone", supply);
        Register("AmmeterZone", ammeter);
        Register("ApplianceZone", appliance);
        Register("VoltmeterZone", voltmeter);
        Register("WrongVoltmeterSeries", wrongSeries);
        Register("WrongAmmeterParallel", wrongParallel);
        Register("SwitchZone", switchZone);
        Register("WireZone", wireZone);
        seriesWire = series;
        parallelWire = parallel;
        statusLabel = status;
        RefreshVisuals();
    }

    public PowerEnergyUIDropTarget FindZone(string zoneId)
    {
        return zones.TryGetValue(zoneId, out var z) ? z : null;
    }

    public string SuggestedZone(string itemId)
    {
        switch (itemId)
        {
            case "PowerSupply": return "SupplyZone";
            case "Ammeter": return "AmmeterZone";
            case "Appliance":
            case "Bulb":
            case "Fan":
            case "Iron":
            case "Kettle": return "ApplianceZone";
            case "Voltmeter": return "VoltmeterZone";
            case "Switch": return "SwitchZone";
            case "Wire": return "WireZone";
            default: return null;
        }
    }

    public bool TryPlace(string itemId, string zoneId)
    {
        if (zoneId == "WrongVoltmeterSeries" || (itemId == "Voltmeter" && zoneId == "AmmeterZone"))
        {
            NotifyWrongVoltmeterSeries();
            return false;
        }
        if (zoneId == "WrongAmmeterParallel" || (itemId == "Ammeter" && zoneId == "VoltmeterZone"))
        {
            NotifyWrongAmmeterParallel();
            return false;
        }

        string expected = SuggestedZone(itemId);
        if (string.IsNullOrEmpty(expected) || expected != zoneId)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✗ INCORRECT CONNECTION\nPlace each instrument in its labelled position.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
            return false;
        }

        string key = Canonical(itemId);
        if (placed.Contains(key)) return true;
        placed.Add(key);
        HideZoneChrome(zoneId);
        PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Circuit, 5, false);
        PowerEnergyFeedbackManager.Instance?.ShowMessage("✓ CORRECT CONNECTION\n" + Explain(key), "+5 MARKS", new Color(0.08f, 0.52f, 0.22f));
        RefreshVisuals();
        if (IsComplete && !scoredComplete)
        {
            scoredComplete = true;
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Circuit complete. The ammeter is in series and the voltmeter is in parallel. Press NEXT STEP.");
            PowerEnergyUIManager.Instance?.SetNextButtonVisible(true);
        }
        return true;
    }

    public void NotifyWrongDrop(PowerEnergyDragDrop2D item, PowerEnergyUIDropTarget zone)
    {
        if (item == null || zone == null) return;
        if (item.ItemId == "Voltmeter" && (zone.ZoneId == "AmmeterZone" || zone.ZoneId == "WrongVoltmeterSeries" || zone.ZoneId == "SupplyZone"))
        {
            NotifyWrongVoltmeterSeries();
            return;
        }
        if (item.ItemId == "Ammeter" && (zone.ZoneId == "VoltmeterZone" || zone.ZoneId == "WrongAmmeterParallel"))
        {
            NotifyWrongAmmeterParallel();
            return;
        }
        PowerEnergyScoreManager.Instance?.SubtractScore(5);
        PowerEnergyFeedbackManager.Instance?.ShowMessage("✗ INCORRECT CONNECTION\nLook at the labels: ammeter in series, voltmeter across the appliance.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
    }

    public void ResetCircuit()
    {
        placed.Clear();
        scoredComplete = false;
        foreach (var pair in zones) ShowZoneChrome(pair.Value);
        RefreshVisuals();
        PowerEnergyElectricalEquipmentManager.Instance?.ResetTray();
    }

    public string NextHint()
    {
        if (!placed.Contains("PowerSupply")) return "Drag the power supply onto the POWER SUPPLY position.";
        if (!placed.Contains("Ammeter")) return "Connect the ammeter in SERIES with the appliance.";
        if (!placed.Contains("Appliance")) return "Place an appliance in the APPLIANCE position.";
        if (!placed.Contains("Voltmeter")) return "Connect the voltmeter in PARALLEL, across the appliance.";
        if (!placed.Contains("Switch")) return "Place the switch in the circuit.";
        if (!placed.Contains("Wire")) return "Drag the wires to complete the circuit.";
        return "Circuit ready. Press NEXT STEP to select an appliance and take readings.";
    }

    private void NotifyWrongVoltmeterSeries()
    {
        PowerEnergyScoreManager.Instance?.SubtractScore(5);
        PowerEnergyFeedbackManager.Instance?.ShowMessage("✗ INCORRECT CONNECTION\nIncorrect. A voltmeter is connected in parallel.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
    }

    private void NotifyWrongAmmeterParallel()
    {
        PowerEnergyScoreManager.Instance?.SubtractScore(5);
        PowerEnergyFeedbackManager.Instance?.ShowMessage("✗ INCORRECT CONNECTION\nIncorrect. An ammeter is connected in series.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
    }

    private void HideZoneChrome(string zoneId)
    {
        var zone = FindZone(zoneId);
        if (zone == null) return;
        var icon = zone.transform.Find("Icon");
        var hint = zone.transform.Find("Hint");
        if (icon != null) icon.gameObject.SetActive(false);
        if (hint != null) hint.gameObject.SetActive(false);
        var img = zone.GetComponent<Image>();
        if (img != null) img.color = new Color(0.82f, 0.93f, 0.86f, 1f);
    }

    private static void ShowZoneChrome(PowerEnergyUIDropTarget zone)
    {
        if (zone == null) return;
        var icon = zone.transform.Find("Icon");
        var hint = zone.transform.Find("Hint");
        if (icon != null) icon.gameObject.SetActive(true);
        if (hint != null) hint.gameObject.SetActive(true);
        var img = zone.GetComponent<Image>();
        if (img != null && zone.ZoneId != null && zone.ZoneId.StartsWith("Wrong"))
            img.color = new Color(1f, 1f, 1f, 0.01f);
        else if (img != null)
            img.color = new Color(1f, 1f, 1f, 0.92f);
    }

    private void RefreshVisuals()
    {
        if (seriesWire != null) seriesWire.enabled = false;
        if (parallelWire != null) parallelWire.enabled = false;
        if (statusLabel != null)
            statusLabel.text = IsComplete
                ? "Circuit: READY  •  Ammeter in series  •  Voltmeter in parallel"
                : NextHint();
        PowerEnergyUIManager.Instance?.UpdateLiveReadings();
    }

    private void Register(string id, PowerEnergyUIDropTarget zone)
    {
        if (zone == null) return;
        zones[id] = zone;
    }

    private static string Canonical(string itemId)
    {
        if (itemId == "Bulb" || itemId == "Fan" || itemId == "Iron" || itemId == "Kettle") return "Appliance";
        return itemId;
    }

    private static string Explain(string key)
    {
        switch (key)
        {
            case "PowerSupply": return "The power supply provides 230 V for the appliance.";
            case "Ammeter": return "Correct. The ammeter is connected in series.";
            case "Appliance": return "The appliance is now in the circuit.";
            case "Voltmeter": return "Correct. The voltmeter is connected across the appliance.";
            case "Switch": return "The switch will turn the appliance ON and OFF.";
            default: return "The wires complete the simple series circuit.";
        }
    }
}
