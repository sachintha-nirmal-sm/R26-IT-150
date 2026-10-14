using System.Text;
using TMPro;
using UnityEngine;

public class PowerEnergyObservationTableManager : MonoBehaviour
{
    public static PowerEnergyObservationTableManager Instance { get; private set; }

    private TextMeshProUGUI tableText;
    private bool scored;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(TextMeshProUGUI text)
    {
        tableText = text;
    }

    public void ResetScoring()
    {
        scored = false;
        Refresh();
    }

    public void Refresh()
    {
        var list = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.Results : null;
        if (tableText == null) return;

        var sb = new StringBuilder();
        sb.AppendLine("OBSERVATION TABLE");
        sb.AppendLine();
        sb.AppendLine("Appliance     Voltage     Current      Power      Time      Energy (J)");
        sb.AppendLine("-----------------------------------------------------------------------");
        AppendRows(sb, list, false);
        sb.AppendLine();
        sb.AppendLine("kWh CONVERSION    1 kWh = 3,600,000 J");
        sb.AppendLine();
        sb.AppendLine("Appliance     Energy (J)          Energy (kWh)");
        sb.AppendLine("------------------------------------------------");
        AppendRows(sb, list, true);

        tableText.text = sb.ToString();

        int done = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.CompletedCount : 0;
        if (done >= 3 && !scored)
        {
            scored = true;
            PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Observation, 5, false);
        }
    }

    private static void AppendRows(StringBuilder sb, System.Collections.Generic.IReadOnlyList<PowerEnergyApplianceData> list, bool kwh)
    {
        if (list == null) return;
        foreach (var a in list)
        {
            if (kwh)
            {
                string j = a.completed || a.energyCalculated ? $"{a.studentEnergyJoules:0} J" : "___ J";
                string k = a.completed || a.kwhConverted ? $"{a.studentEnergyKwh:0.####} kWh" : "___ kWh";
                sb.AppendLine($"{Pad(a.shortName, 13)}{Pad(j, 20)}{k}");
            }
            else
            {
                string v = a.studentVoltage > 0 ? $"{a.studentVoltage:0.0} V" : "___ V";
                string i = a.studentCurrent > 0 ? $"{a.studentCurrent:0.000} A" : "___ A";
                string p = a.powerCalculated ? $"{a.studentPower:0.##} W" : "___ W";
                string t = a.operatingTime > 0 ? $"{a.operatingTime:0} s" : "___ s";
                string e = a.energyCalculated ? $"{a.studentEnergyJoules:0} J" : "___ J";
                sb.AppendLine($"{Pad(a.shortName, 13)}{Pad(v, 12)}{Pad(i, 13)}{Pad(p, 11)}{Pad(t, 10)}{e}");
            }
        }
    }

    private static string Pad(string s, int w)
    {
        if (s == null) s = "";
        if (s.Length >= w) return s + " ";
        return s + new string(' ', w - s.Length);
    }
}
