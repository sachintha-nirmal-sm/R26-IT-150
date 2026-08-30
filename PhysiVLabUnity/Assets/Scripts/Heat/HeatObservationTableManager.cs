using TMPro;
using UnityEngine;

public class HeatObservationTableManager : MonoBehaviour
{
    public static HeatObservationTableManager Instance { get; private set; }

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
        var vis = HeatVisualController.Instance;
        var asm = HeatAssemblyManager.Instance;
        bool a = asm != null && asm.LevelAMarked;
        bool b = vis != null && vis.ReachedLevelB;
        bool c = vis != null && vis.ReachedLevelC;

        if (tableText == null) return;
        tableText.text =
            "OBSERVATION TABLE\n\n" +
            "Mark     Liquid level in the thin tube          Reason\n" +
            "--------------------------------------------------------------------------------\n" +
            $"A        {(a ? "starting height, before heating" : "not marked yet")}     initial volume of liquid\n" +
            $"B        {(b ? "slightly below A" : "not yet observed")}                glass container expands first\n" +
            $"C        {(c ? "well above A" : "not yet observed")}                    liquid expands more than glass\n\n" +
            "Sequence:  A  →  B (brief fall)  →  C (steady rise).\n\n" +
            "Heat reaches the glass first, so the test tube expands and its volume increases.\n" +
            "The liquid level therefore falls slightly from A to B.\n" +
            "Then the liquid itself heats up and expands more than the solid glass,\n" +
            "so the level rises past A to C.";

        if (c && !scored)
        {
            scored = true;
            HeatScoreManager.Instance?.AddScore(5, false);
        }
    }
}
