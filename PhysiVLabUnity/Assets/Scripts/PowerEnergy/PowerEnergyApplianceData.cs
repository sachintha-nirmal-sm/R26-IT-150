using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PowerEnergyApplianceData
{
    public string applianceName;
    public string shortName;
    public float voltage;
    public float current;
    public float power;
    public float displayPower;
    public float operatingTime;
    public float energyJoules;
    public float energyKwh;
    public bool completed;
    public bool powerCalculated;
    public bool energyCalculated;
    public bool kwhConverted;
    public float studentVoltage;
    public float studentCurrent;
    public float studentPower;
    public float studentEnergyJoules;
    public float studentEnergyKwh;

    public PowerEnergyApplianceData CloneDefaults()
    {
        return new PowerEnergyApplianceData
        {
            applianceName = applianceName,
            shortName = shortName,
            voltage = voltage,
            current = current,
            power = power,
            displayPower = displayPower
        };
    }
}

public static class PowerEnergyApplianceCatalog
{
    public const float SupplyVoltage = 230f;

    public static List<PowerEnergyApplianceData> CreateDefaults()
    {
        return new List<PowerEnergyApplianceData>
        {
            Make("Electric Bulb", "Bulb", 0.043f, 10f),
            Make("Electric Fan", "Fan", 0.348f, 80f),
            Make("Electric Iron", "Iron", 4.348f, 1000f),
            Make("Electric Kettle", "Kettle", 8.696f, 2000f)
        };
    }

    private static PowerEnergyApplianceData Make(string name, string shortName, float current, float displayPower)
    {
        float voltage = SupplyVoltage;
        float power = voltage * current;
        return new PowerEnergyApplianceData
        {
            applianceName = name,
            shortName = shortName,
            voltage = voltage,
            current = current,
            power = power,
            displayPower = displayPower
        };
    }

    public static PowerEnergyApplianceData Find(IList<PowerEnergyApplianceData> list, string shortName)
    {
        if (list == null) return null;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null && list[i].shortName == shortName)
                return list[i];
        }
        return null;
    }
}
