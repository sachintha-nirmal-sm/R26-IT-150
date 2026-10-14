using TMPro;
using UnityEngine;

public class OpticsObservationTableManager : MonoBehaviour
{
    public static OpticsObservationTableManager Instance { get; private set; }

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
        var vis = OpticsVisualController.Instance;
        var asm = OpticsAssemblyManager.Instance;
        bool open = asm != null && asm.WindowOpened;
        bool mirror = asm != null && asm.MirrorPlaced;
        bool screen = asm != null && asm.ScreenPlaced;
        bool sharp = vis != null && vis.IsInFocus;
        float d = vis != null ? vis.ScreenDistanceCm : 0f;
        float f = OpticsVisualController.TrueFocalLengthCm;

        if (tableText == null) return;
        tableText.text =
            "OBSERVATION TABLE\n\n" +
            "Step                         Observation\n" +
            "--------------------------------------------------------------------------------\n" +
            $"Window                       {(open ? "open — distant outdoor scene visible" : "closed")}\n" +
            $"Concave mirror               {(mirror ? "facing the window" : "not placed")}\n" +
            $"White screen                 {(screen ? "held in front of the mirror" : "not placed")}\n" +
            $"Image on the screen          {(sharp ? "clear, real, inverted (upside down)" : screen ? "blurred / not yet focused" : "—")}\n" +
            $"Mirror–screen distance       {(sharp ? $"{d:0.0} cm" : "—")}\n" +
            $"Approximate focal length f   {(sharp ? $"{d:0.0} cm  (true f = {f:0.0} cm)" : "—")}\n\n" +
            "Reason: rays from a far-away object are treated as parallel. A concave mirror\n" +
            "converges parallel rays at its principal focus F. Therefore the sharp-image\n" +
            "distance is approximately equal to the focal length of the mirror.";

        if (sharp && !scored)
        {
            scored = true;
            OpticsScoreManager.Instance?.AddScore(5, false);
        }
    }
}
