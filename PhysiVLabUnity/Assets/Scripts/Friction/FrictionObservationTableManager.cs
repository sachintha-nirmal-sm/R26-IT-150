using TMPro;
using UnityEngine;

public class FrictionObservationTableManager : MonoBehaviour
{
    public static FrictionObservationTableManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI tableText;
    private bool tableScored;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(TextMeshProUGUI text)
    {
        tableText = text;
        Refresh();
    }

    public void Refresh()
    {
        if (tableText == null) return;
        tableText.text = BuildTable();
        if (!tableScored && FrictionTrialManager.Instance != null && FrictionTrialManager.Instance.AllTrialsComplete())
        {
            tableScored = true;
            FrictionScoreManager.Instance?.AddScore(5, false);
        }
    }

    public void ResetScoring() => tableScored = false;

    public string BuildTable()
    {
        string s =
            "TABLE 5.4 — INVESTIGATION OF THE INFLUENCE OF SURFACE AREA ON FRICTION\n\n" +
            "Surface | Dimensions     | Area (cm²) | Weight | Limiting Friction\n" +
            "----------------------------------------------------------------------\n";
        string[] names = { "A", "B", "C" };
        string[] dims = { "30 × 20 cm", "30 × 10 cm", "20 × 10 cm" };
        float[] areas = { 600f, 300f, 200f };
        for (int i = 0; i < 3; i++)
        {
            var t = FrictionTrialManager.Instance != null ? FrictionTrialManager.Instance.GetTrial(i + 1) : null;
            string friction = t != null && t.completed ? $"{t.limitingFriction:0.0} N" : "___ N";
            float area = t != null && t.contactArea > 0f ? t.contactArea : areas[i];
            s += $"{names[i],-7} | {dims[i],-14} | {area,10:0} |  60 N  | {friction}\n";
        }
        s += "----------------------------------------------------------------------\n";
        s += "\nWeight of the wooden block = 60 N for every trial.\n";
        s += "Sandpaper roughness = CONSTANT for every trial.\n";
        return s;
    }
}
