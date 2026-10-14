using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerEnergyApplianceController : MonoBehaviour
{
    public static PowerEnergyApplianceController Instance { get; private set; }

    private readonly List<PowerEnergyApplianceData> appliances = new List<PowerEnergyApplianceData>();
    private PowerEnergyApplianceData current;
    private Image applianceVisual;
    private TextMeshProUGUI nameLabel;
    private TextMeshProUGUI statusLabel;
    private bool switchOn;

    public IReadOnlyList<PowerEnergyApplianceData> Results => appliances;
    public PowerEnergyApplianceData Current => current;
    public bool IsOn => switchOn;
    public int CompletedCount
    {
        get
        {
            int n = 0;
            foreach (var a in appliances) if (a.completed) n++;
            return n;
        }
    }
    public int PowerCount
    {
        get
        {
            int n = 0;
            foreach (var a in appliances) if (a.powerCalculated) n++;
            return n;
        }
    }
    public int EnergyCount
    {
        get
        {
            int n = 0;
            foreach (var a in appliances) if (a.energyCalculated) n++;
            return n;
        }
    }
    public int KwhCount
    {
        get
        {
            int n = 0;
            foreach (var a in appliances) if (a.kwhConverted) n++;
            return n;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ResetAll();
    }

    public void Bind(Image visual, TextMeshProUGUI name, TextMeshProUGUI status)
    {
        applianceVisual = visual;
        nameLabel = name;
        statusLabel = status;
        RefreshVisual();
    }

    public void ResetAll()
    {
        appliances.Clear();
        appliances.AddRange(PowerEnergyApplianceCatalog.CreateDefaults());
        current = null;
        switchOn = false;
        RefreshVisual();
    }

    public void ResetCurrentAppliance()
    {
        switchOn = false;
        if (current != null && !current.completed)
        {
            current.operatingTime = 0;
            current.energyJoules = 0;
            current.energyKwh = 0;
            current.powerCalculated = false;
            current.energyCalculated = false;
            current.kwhConverted = false;
            current.studentVoltage = 0;
            current.studentCurrent = 0;
            current.studentPower = 0;
            current.studentEnergyJoules = 0;
            current.studentEnergyKwh = 0;
        }
        PowerEnergyTimerController.Instance?.ResetTimer();
        PowerEnergyVoltmeterController.Instance?.ResetMeter();
        PowerEnergyAmmeterController.Instance?.ResetMeter();
        RefreshVisual();
        PowerEnergyUIManager.Instance?.UpdateLiveReadings();
    }

    public bool SelectAppliance(string shortName)
    {
        var found = PowerEnergyApplianceCatalog.Find(appliances, shortName);
        if (found == null) return false;
        current = found;
        switchOn = false;
        PowerEnergyTimerController.Instance?.ResetTimer();
        PowerEnergyVoltmeterController.Instance?.ResetMeter();
        PowerEnergyAmmeterController.Instance?.ResetMeter();
        RefreshVisual();
        PowerEnergyFeedbackManager.Instance?.ShowInstruction("Selected " + found.applianceName + ". Turn the switch ON, then take voltage and current readings.");
        PowerEnergyUIManager.Instance?.UpdateLiveReadings();
        return true;
    }

    public void SetSwitch(bool on)
    {
        if (current == null)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Select an appliance before turning the switch ON.");
            return;
        }
        if (PowerEnergyCircuitConnectionManager.Instance == null || !PowerEnergyCircuitConnectionManager.Instance.IsComplete)
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowInstruction("Complete the circuit setup before starting the appliance.");
            return;
        }
        switchOn = on;
        if (switchOn)
        {
            PowerEnergyVoltmeterController.Instance?.SetLiveValue(current.voltage);
            PowerEnergyAmmeterController.Instance?.SetLiveValue(current.current);
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✓ APPLIANCE ON\nThe circuit is live. Read the voltmeter and ammeter.", "", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            PowerEnergyVoltmeterController.Instance?.SetLiveValue(0f);
            PowerEnergyAmmeterController.Instance?.SetLiveValue(0f);
            PowerEnergyTimerController.Instance?.StopTimer();
        }
        RefreshVisual();
        PowerEnergyUIManager.Instance?.UpdateLiveReadings();
    }

    public void ToggleSwitch() => SetSwitch(!switchOn);

    public PowerEnergyApplianceData GetByIndex(int index)
    {
        if (index < 0 || index >= appliances.Count) return null;
        return appliances[index];
    }

    public string BuildSummary()
    {
        var lines = new System.Text.StringBuilder();
        foreach (var a in appliances)
        {
            if (!a.completed && !a.powerCalculated) continue;
            lines.AppendLine($"{a.shortName}: V={a.studentVoltage:0.##} V, I={a.studentCurrent:0.###} A, P={a.studentPower:0.##} W, E={a.studentEnergyJoules:0} J, {a.studentEnergyKwh:0.####} kWh");
        }
        return lines.ToString();
    }

    private void RefreshVisual()
    {
        if (applianceVisual != null)
        {
            var type = current == null ? PowerEnergyEquipmentType.ElectricalAppliance : TypeOf(current.shortName);
            applianceVisual.sprite = PowerEnergyIconFactory.GetSprite(type);
            applianceVisual.preserveAspect = true;
            applianceVisual.color = switchOn ? Color.white : new Color(0.85f, 0.88f, 0.92f);
        }
        if (nameLabel != null)
            nameLabel.text = current != null ? current.applianceName : "Select an appliance";
        if (statusLabel != null)
            statusLabel.text = current == null ? "OFF" : (switchOn ? "ON" : "OFF");
    }

    private static PowerEnergyEquipmentType TypeOf(string shortName)
    {
        switch (shortName)
        {
            case "Fan": return PowerEnergyEquipmentType.Fan;
            case "Iron": return PowerEnergyEquipmentType.Iron;
            case "Kettle": return PowerEnergyEquipmentType.Kettle;
            default: return PowerEnergyEquipmentType.Bulb;
        }
    }
}
