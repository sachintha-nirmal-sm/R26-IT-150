using TMPro;
using UnityEngine;

public class PowerEnergyResultManager : MonoBehaviour
{
    public static PowerEnergyResultManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI resultDetailsText;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(TextMeshProUGUI score, TextMeshProUGUI details, TextMeshProUGUI status)
    {
        finalScoreText = score;
        resultDetailsText = details;
        statusText = status;
    }

    public void ShowResult(int score, bool passed, int mistakes, PowerEnergyAttemptRecord attempt)
    {
        string perf = PowerEnergyProfileManager.Instance != null ? PowerEnergyProfileManager.Instance.GetPerformanceLabel(score) : "";
        int best = PowerEnergyProfileManager.Instance != null && PowerEnergyProfileManager.Instance.ProfileData != null
            ? PowerEnergyProfileManager.Instance.ProfileData.bestScore
            : score;
        int attempts = attempt != null ? attempt.attemptNumber : 1;
        int apps = attempt != null ? attempt.appliancesCompleted : 0;
        int power = attempt != null ? attempt.powerCalculations : 0;
        int energy = attempt != null ? attempt.energyCalculations : 0;
        int kwh = attempt != null ? attempt.kwhConversions : 0;

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "PRACTICAL COMPLETED\n\n" +
                "Power and Energy of Electric Appliances\n\n" +
                $"Score:  {score} / 100\n" +
                $"Percentage:  {score}%\n" +
                $"Performance:  {perf}\n" +
                $"Best Score:  {best} / 100\n" +
                $"Attempts:  {attempts}\n\n" +
                $"Appliances:  {apps} / 4\n" +
                $"Power calculations:  {power} / 4\n" +
                $"Energy calculations:  {energy} / 4\n" +
                $"kWh conversions:  {kwh} / 4\n" +
                "Conclusion:  " + (attempt != null && attempt.conclusionCompleted ? "Completed" : "—");
        }

        if (resultDetailsText != null)
            resultDetailsText.text = BuildApplianceResults();

        if (statusText != null)
        {
            statusText.text = passed ? "STATUS: COMPLETED" : "STATUS: NEEDS IMPROVEMENT";
            statusText.color = passed ? new Color(0.1f, 0.55f, 0.2f) : new Color(0.75f, 0.15f, 0.15f);
        }
    }

    private static string BuildApplianceResults()
    {
        var list = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.Results : null;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("RESULTS");
        sb.AppendLine();
        if (list == null) return sb.ToString();
        foreach (var a in list)
        {
            sb.AppendLine(a.applianceName);
            sb.AppendLine($"  V = {(a.studentVoltage > 0 ? a.studentVoltage.ToString("0.0") : "—")} V");
            sb.AppendLine($"  I = {(a.studentCurrent > 0 ? a.studentCurrent.ToString("0.000") : "—")} A");
            sb.AppendLine($"  P = {(a.powerCalculated ? a.studentPower.ToString("0.##") : "—")} W");
            sb.AppendLine($"  E = {(a.energyCalculated ? a.studentEnergyJoules.ToString("0") : "—")} J");
            sb.AppendLine($"      {(a.kwhConverted ? a.studentEnergyKwh.ToString("0.####") : "—")} kWh");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
