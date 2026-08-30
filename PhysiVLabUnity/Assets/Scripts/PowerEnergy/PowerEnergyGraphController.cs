using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerEnergyGraphController : MonoBehaviour
{
    public static PowerEnergyGraphController Instance { get; private set; }

    private Transform powerBars;
    private Transform energyBars;
    private TextMeshProUGUI titleText;
    private bool showingEnergy;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(Transform powerRoot, Transform energyRoot, TextMeshProUGUI title)
    {
        powerBars = powerRoot;
        energyBars = energyRoot;
        titleText = title;
        Refresh();
    }

    public void ShowPower()
    {
        showingEnergy = false;
        Refresh();
    }

    public void ShowEnergy()
    {
        showingEnergy = true;
        Refresh();
    }

    public void Refresh()
    {
        var list = PowerEnergyApplianceController.Instance != null ? PowerEnergyApplianceController.Instance.Results : null;
        if (titleText != null)
            titleText.text = showingEnergy ? "APPLIANCE vs ENERGY (J)" : "APPLIANCE vs POWER (W)";
        if (powerBars != null) powerBars.gameObject.SetActive(!showingEnergy);
        if (energyBars != null) energyBars.gameObject.SetActive(showingEnergy);
        Fill(showingEnergy ? energyBars : powerBars, list, showingEnergy);
    }

    private static void Fill(Transform root, IReadOnlyList<PowerEnergyApplianceData> list, bool energy)
    {
        if (root == null || list == null) return;
        float max = 1f;
        foreach (var a in list)
        {
            float v = energy ? Mathf.Max(0f, a.studentEnergyJoules) : Mathf.Max(0f, a.studentPower);
            if (v > max) max = v;
        }
        for (int i = 0; i < root.childCount && i < list.Count; i++)
        {
            var row = root.GetChild(i);
            var fillTransform = row.Find("Track/Fill") ?? row.Find("Fill");
            var fill = fillTransform != null ? fillTransform.GetComponent<Image>() : null;
            var label = row.Find("Label")?.GetComponent<TextMeshProUGUI>();
            var value = row.Find("Value")?.GetComponent<TextMeshProUGUI>();
            var a = list[i];
            float v = energy ? a.studentEnergyJoules : a.studentPower;
            if (fill != null)
            {
                fill.fillAmount = max > 0 ? Mathf.Clamp01(v / max) : 0f;
                fill.color = BarColor(i);
            }
            if (label != null) label.text = a.shortName;
            if (value != null)
                value.text = energy
                    ? (a.energyCalculated ? $"{a.studentEnergyJoules:0} J" : "—")
                    : (a.powerCalculated ? $"{a.studentPower:0.##} W" : "—");
        }
    }

    private static Color BarColor(int i)
    {
        switch (i)
        {
            case 0: return new Color(0.95f, 0.78f, 0.22f);
            case 1: return new Color(0.20f, 0.62f, 0.85f);
            case 2: return new Color(0.85f, 0.32f, 0.32f);
            default: return new Color(0.18f, 0.62f, 0.48f);
        }
    }
}
