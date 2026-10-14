using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElectronicsWireController : MonoBehaviour
{
    public static ElectronicsWireController Instance { get; private set; }

    private readonly HashSet<string> connected = new HashSet<string>();
    private readonly Dictionary<string, Image> wireImages = new Dictionary<string, Image>();
    private string pendingTerminal;

    public const string BatteryToSwitch = "BatteryToSwitch";
    public const string SwitchToDiode = "SwitchToDiode";
    public const string DiodeToBulb = "DiodeToBulb";
    public const string BulbToBattery = "BulbToBattery";

    public int ConnectionCount => connected.Count;
    public bool AllWiresConnected =>
        connected.Contains(BatteryToSwitch) &&
        connected.Contains(SwitchToDiode) &&
        connected.Contains(DiodeToBulb) &&
        connected.Contains(BulbToBattery);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void BindWire(string id, Image image)
    {
        if (image == null) return;
        wireImages[id] = image;
        bool on = connected.Contains(id);
        image.gameObject.SetActive(on);
        image.enabled = true;
        image.raycastTarget = false;
        foreach (var child in image.GetComponentsInChildren<Image>(true))
            child.raycastTarget = false;
    }

    public bool IsConnected(string id) => connected.Contains(id);

    public bool TryConnect(string connectionId)
    {
        if (connected.Contains(connectionId)) return true;
        if (!IsValidId(connectionId))
        {
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT CONNECTION\nConnect Battery → Switch → Diode → Bulb → Battery.", "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
            return false;
        }

        connected.Add(connectionId);
        if (wireImages.TryGetValue(connectionId, out var img) && img != null)
        {
            img.gameObject.SetActive(true);
            img.enabled = true;
        }

        ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.ForwardCircuit, 3, false);
        ElectronicsFeedbackManager.Instance?.ShowMessage("✓ CONNECTION CORRECT\n" + Explain(connectionId), "+3 MARKS", new Color(0.08f, 0.52f, 0.22f));
        ElectronicsCircuitConnectionManager.Instance?.RefreshState();
        return true;
    }

    public void TapTerminal(string terminalId)
    {
        if (string.IsNullOrEmpty(pendingTerminal))
        {
            pendingTerminal = terminalId;
            ElectronicsFeedbackManager.Instance?.ShowInstruction("Start point: " + Label(terminalId) + ". Now tap the end point.");
            return;
        }

        string start = pendingTerminal;
        string end = terminalId;
        pendingTerminal = null;
        string id = MatchPair(start, end);
        if (id == null)
        {
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ INCORRECT CONNECTION\nConnect Battery → Switch, Switch → Diode, Diode → Bulb, Bulb → Battery.", "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        TryConnect(id);
    }

    public void ResetWires()
    {
        connected.Clear();
        pendingTerminal = null;
        foreach (var pair in wireImages)
            if (pair.Value != null) pair.Value.gameObject.SetActive(false);
    }

    private static bool IsValidId(string id)
    {
        return id == BatteryToSwitch || id == SwitchToDiode || id == DiodeToBulb || id == BulbToBattery;
    }

    private static string MatchPair(string a, string b)
    {
        if (Pair(a, b, "Battery+", "SwitchIn") || Pair(a, b, "BatteryOut", "SwitchIn")) return BatteryToSwitch;
        if (Pair(a, b, "SwitchOut", "DiodeIn") || Pair(a, b, "SwitchOut", "DiodeAnode")) return SwitchToDiode;
        if (Pair(a, b, "DiodeOut", "BulbIn") || Pair(a, b, "DiodeCathode", "BulbIn")) return DiodeToBulb;
        if (Pair(a, b, "BulbOut", "Battery-") || Pair(a, b, "BulbOut", "BatteryReturn")) return BulbToBattery;
        return null;
    }

    private static bool Pair(string a, string b, string x, string y)
    {
        return (a == x && b == y) || (a == y && b == x);
    }

    private static string Explain(string id)
    {
        switch (id)
        {
            case BatteryToSwitch: return "Battery connected to the switch.";
            case SwitchToDiode: return "Switch connected to the diode.";
            case DiodeToBulb: return "Diode connected to the bulb.";
            default: return "Bulb connected back to the battery. The loop is closing.";
        }
    }

    private static string Label(string id)
    {
        switch (id)
        {
            case "Battery+": return "Battery +";
            case "Battery-": return "Battery −";
            case "SwitchIn": return "Switch input";
            case "SwitchOut": return "Switch output";
            case "DiodeIn":
            case "DiodeAnode": return "Diode anode";
            case "DiodeOut":
            case "DiodeCathode": return "Diode cathode";
            case "BulbIn": return "Bulb input";
            case "BulbOut": return "Bulb output";
            default: return id;
        }
    }
}
