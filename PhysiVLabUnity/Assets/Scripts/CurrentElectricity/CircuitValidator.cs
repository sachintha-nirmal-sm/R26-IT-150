using System.Collections.Generic;
using UnityEngine;

public class CircuitValidator : MonoBehaviour
{
    public static CircuitValidator Instance { get; private set; }

    public class Result
    {
        public bool isValid;
        public string message;
        public ConnectionType detectedType = ConnectionType.Unknown;
        public bool closedLoop;
        public bool ammeterInSeries;
        public bool voltmeterInParallel;
        public bool cellsCorrect;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public Result Validate(ConnectionType expected)
    {
        var result = new Result();
        var builder = CircuitBuilder.Instance;
        if (builder == null)
        {
            result.message = "Circuit builder is not ready.";
            return result;
        }

        var cell1 = builder.GetComponentById("Cell1");
        var cell2 = builder.GetComponentById("Cell2");
        var bulb = builder.GetComponentById("Bulb");
        var ammeter = builder.GetComponentById("Ammeter");
        var voltmeter = builder.GetComponentById("Voltmeter");

        if (cell1 == null || !cell1.IsPlaced || cell2 == null || !cell2.IsPlaced)
        {
            result.message = "Place both dry cells on the circuit board.";
            return result;
        }
        if (bulb == null || !bulb.IsPlaced)
        {
            result.message = "The bulb must be part of the main circuit.";
            return result;
        }
        if (ammeter == null || !ammeter.IsPlaced)
        {
            result.message = "An ammeter is required to measure the current through the bulb. An ammeter must be connected in series.";
            return result;
        }
        if (voltmeter == null || !voltmeter.IsPlaced)
        {
            result.message = "A voltmeter is required to measure the potential difference across the bulb. A voltmeter must be connected in parallel across the bulb.";
            return result;
        }
        if (builder.Wires.Count < 5)
        {
            result.message = "Connect the terminals with conducting wires to complete the circuit.";
            return result;
        }

        var terms = IndexTerminals(cell1, cell2, bulb, ammeter, voltmeter);
        if (terms.Count < 10)
        {
            result.message = "Every component must have two terminals connected into the circuit.";
            return result;
        }

        var wires = builder.Wires;
        var ufWires = BuildUf(terms, wires, false, false, false, false);
        var ufLoop = BuildUf(terms, wires, true, false, true, false);
        var ufNoAmm = BuildUf(terms, wires, true, false, false, false);
        var ufWithVolt = BuildUf(terms, wires, true, false, true, true);

        bool loopWithoutVolt = Connected(ufLoop, terms, "bulbA", "bulbB");
        bool loopWithVolt = Connected(ufWithVolt, terms, "bulbA", "bulbB");
        bool loopWithoutAmm = Connected(ufNoAmm, terms, "bulbA", "bulbB");

        result.closedLoop = loopWithoutVolt;

        if (!loopWithoutVolt && loopWithVolt)
        {
            result.message = "Incorrect voltmeter connection.\nA voltmeter must be connected in parallel across the bulb.";
            return result;
        }
        if (!loopWithoutVolt)
        {
            result.message = "The circuit is not closed. Complete a loop through the cells, ammeter and bulb.";
            return result;
        }

        bool ammAcrossBulb = Across(ufWires, terms, "ammP", "ammN", "bulbA", "bulbB");
        if (ammAcrossBulb && loopWithoutAmm)
        {
            result.message = "Incorrect ammeter connection.\nAn ammeter must be connected in series.";
            return result;
        }
        if (loopWithoutAmm)
        {
            result.message = "Incorrect ammeter connection.\nAn ammeter must be connected in series with the bulb.";
            return result;
        }
        result.ammeterInSeries = true;

        bool voltAcrossBulb = Across(ufWires, terms, "voltP", "voltN", "bulbA", "bulbB");
        if (!voltAcrossBulb)
        {
            result.message = "Incorrect voltmeter connection.\nA voltmeter must be connected in parallel across the bulb.";
            return result;
        }
        result.voltmeterInParallel = true;

        var detected = DetectCellArrangement(ufWires, terms);
        result.detectedType = detected;
        result.cellsCorrect = detected == expected;

        if (detected == ConnectionType.Unknown)
        {
            result.message = WrongTopologyMessage(expected);
            return result;
        }
        if (detected != expected)
        {
            result.message =
                $"The cells are connected as {Label(detected)}, but this step needs {Label(expected)}.\n" +
                WrongTopologyMessage(expected);
            return result;
        }

        result.isValid = true;
        result.message = "✓ Circuit topology is correct. The ammeter is in series and the voltmeter is in parallel across the bulb.";
        return result;
    }

    private static string Label(ConnectionType type)
    {
        switch (type)
        {
            case ConnectionType.SeriesAiding: return "series aiding";
            case ConnectionType.Parallel: return "parallel";
            case ConnectionType.SeriesOpposing: return "series opposing";
            default: return "an unknown arrangement";
        }
    }

    private static string WrongTopologyMessage(ConnectionType expected)
    {
        switch (expected)
        {
            case ConnectionType.SeriesAiding:
                return "For series aiding, join the negative of one cell to the positive of the other so the voltages add.";
            case ConnectionType.Parallel:
                return "For parallel, join both positive terminals together and both negative terminals together.";
            case ConnectionType.SeriesOpposing:
                return "For series opposing, join like polarities (positive to positive, or negative to negative) so the voltages oppose.";
            default:
                return "Check the cell polarities against the textbook diagram.";
        }
    }

    private ConnectionType DetectCellArrangement(Dictionary<string, int> ufWires, Dictionary<string, ElectricalTerminal> terms)
    {
        bool posTied = Connected(ufWires, terms, "c1p", "c2p");
        bool negTied = Connected(ufWires, terms, "c1n", "c2n");
        bool posNeg = Connected(ufWires, terms, "c1p", "c2n");
        bool negPos = Connected(ufWires, terms, "c1n", "c2p");

        if (posTied && negTied && !posNeg && !negPos)
            return ConnectionType.Parallel;

        if (posTied && !negTied)
            return ConnectionType.SeriesOpposing;
        if (negTied && !posTied)
            return ConnectionType.SeriesOpposing;

        if ((posNeg || negPos) && !posTied && !negTied)
            return ConnectionType.SeriesAiding;

        return ConnectionType.Unknown;
    }

    private Dictionary<string, ElectricalTerminal> IndexTerminals(
        ElectricalComponent cell1, ElectricalComponent cell2, ElectricalComponent bulb,
        ElectricalComponent ammeter, ElectricalComponent voltmeter)
    {
        var map = new Dictionary<string, ElectricalTerminal>();
        Add(map, "c1p", cell1.GetTerminal("+"));
        Add(map, "c1n", cell1.GetTerminal("-"));
        Add(map, "c2p", cell2.GetTerminal("+"));
        Add(map, "c2n", cell2.GetTerminal("-"));
        Add(map, "bulbA", bulb.TerminalA);
        Add(map, "bulbB", bulb.TerminalB);
        Add(map, "ammP", ammeter.GetTerminal("+") ?? ammeter.TerminalA);
        Add(map, "ammN", ammeter.GetTerminal("-") ?? ammeter.TerminalB);
        Add(map, "voltP", voltmeter.GetTerminal("+") ?? voltmeter.TerminalA);
        Add(map, "voltN", voltmeter.GetTerminal("-") ?? voltmeter.TerminalB);
        return map;
    }

    private static void Add(Dictionary<string, ElectricalTerminal> map, string key, ElectricalTerminal t)
    {
        if (t != null) map[key] = t;
    }

    private Dictionary<string, int> BuildUf(
        Dictionary<string, ElectricalTerminal> terms,
        IReadOnlyList<WireConnection> wires,
        bool unionCells, bool unionBulb, bool unionAmmeter, bool unionVoltmeter)
    {
        var parent = new Dictionary<ElectricalTerminal, ElectricalTerminal>();
        foreach (var t in terms.Values)
            parent[t] = t;

        foreach (var w in wires)
        {
            if (w == null || w.StartTerminal == null || w.EndTerminal == null) continue;
            if (!parent.ContainsKey(w.StartTerminal)) parent[w.StartTerminal] = w.StartTerminal;
            if (!parent.ContainsKey(w.EndTerminal)) parent[w.EndTerminal] = w.EndTerminal;
            Union(parent, w.StartTerminal, w.EndTerminal);
        }

        if (unionCells)
        {
            UnionIf(parent, terms, "c1p", "c1n");
            UnionIf(parent, terms, "c2p", "c2n");
        }
        if (unionBulb) UnionIf(parent, terms, "bulbA", "bulbB");
        if (unionAmmeter) UnionIf(parent, terms, "ammP", "ammN");
        if (unionVoltmeter) UnionIf(parent, terms, "voltP", "voltN");

        var compact = new Dictionary<string, int>();
        var ids = new Dictionary<ElectricalTerminal, int>();
        int n = 0;
        foreach (var kv in terms)
        {
            var root = Find(parent, kv.Value);
            if (!ids.ContainsKey(root)) ids[root] = n++;
            compact[kv.Key] = ids[root];
        }
        return compact;
    }

    private static void UnionIf(Dictionary<ElectricalTerminal, ElectricalTerminal> parent, Dictionary<string, ElectricalTerminal> terms, string a, string b)
    {
        if (terms.TryGetValue(a, out var ta) && terms.TryGetValue(b, out var tb))
            Union(parent, ta, tb);
    }

    private static ElectricalTerminal Find(Dictionary<ElectricalTerminal, ElectricalTerminal> parent, ElectricalTerminal x)
    {
        if (!parent.ContainsKey(x)) parent[x] = x;
        if (parent[x] != x) parent[x] = Find(parent, parent[x]);
        return parent[x];
    }

    private static void Union(Dictionary<ElectricalTerminal, ElectricalTerminal> parent, ElectricalTerminal a, ElectricalTerminal b)
    {
        var ra = Find(parent, a);
        var rb = Find(parent, b);
        if (ra != rb) parent[ra] = rb;
    }

    private static bool Connected(Dictionary<string, int> uf, Dictionary<string, ElectricalTerminal> terms, string a, string b)
    {
        return uf.ContainsKey(a) && uf.ContainsKey(b) && uf[a] == uf[b];
    }

    private static bool Across(Dictionary<string, int> uf, Dictionary<string, ElectricalTerminal> terms, string p, string n, string a, string b)
    {
        bool pnOnAB = Connected(uf, terms, p, a) && Connected(uf, terms, n, b);
        bool pnOnBA = Connected(uf, terms, p, b) && Connected(uf, terms, n, a);
        return pnOnAB || pnOnBA;
    }
}
