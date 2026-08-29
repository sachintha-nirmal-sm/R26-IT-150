using TMPro;
using UnityEngine;

public class WavesObservationTableManager : MonoBehaviour
{
    public static WavesObservationTableManager Instance { get; private set; }

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
        bool wave = WavesWaveController.Instance != null && WavesWaveController.Instance.HasTransverseWave;
        int ribbons = WavesAssemblyManager.Instance != null ? WavesAssemblyManager.Instance.RibbonCount : 0;
        if (tableText == null) return;
        tableText.text =
            "OBSERVATION TABLE\n\n" +
            "Marker          Rest position on slinky          Motion as the pulse passes\n" +
            "--------------------------------------------------------------------------------\n" +
            Line(1, ribbons, wave) +
            Line(2, ribbons, wave) +
            Line(3, ribbons, wave) +
            Line(4, ribbons, wave) +
            Line(5, ribbons, wave) +
            "\nDirection of wave:  along the slinky, away from the hand.\n" +
            "Direction of ribbon motion:  perpendicular to the wave (across the table).\n\n" +
            "This is the property of a transverse wave.";

        if (wave && !scored)
        {
            scored = true;
            WavesScoreManager.Instance?.AddScore(5, false);
        }
    }

    private static string Line(int n, int tied, bool wave)
    {
        if (n > tied) return $"Ribbon {n}        not tied yet                     —\n";
        return wave
            ? $"Ribbon {n}        coil along the slinky            side to side (perpendicular)\n"
            : $"Ribbon {n}        coil along the slinky            at rest (no pulse yet)\n";
    }
}
