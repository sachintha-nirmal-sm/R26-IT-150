using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CircuitBuilder : MonoBehaviour
{
    public static CircuitBuilder Instance { get; private set; }

    [SerializeField] private RectTransform board;
    [SerializeField] private RectTransform tray;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI meterInfoText;
    [SerializeField] private TMP_FontAsset font;

    private readonly List<ElectricalComponent> components = new List<ElectricalComponent>();
    private readonly List<WireConnection> wires = new List<WireConnection>();
    private readonly Dictionary<string, Vector2> suggested = new Dictionary<string, Vector2>();
    private CircuitLabPhase phase = CircuitLabPhase.Build;
    private ConnectionType expected = ConnectionType.SeriesAiding;
    private int connectionNumber = 1;
    private bool checkAwarded;
    private CircuitReading liveReading;
    private TMP_InputField voltageInput;
    private TMP_InputField currentInput;
    private string selectedBrightness = "";

    public IReadOnlyList<WireConnection> Wires => wires;
    public CircuitLabPhase Phase => phase;
    public CircuitReading LiveReading => liveReading;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform circuitBoard, RectTransform equipmentTray, TextMeshProUGUI hint, TextMeshProUGUI meterInfo, TMP_FontAsset tmpFont)
    {
        board = circuitBoard;
        tray = equipmentTray;
        hintText = hint;
        meterInfoText = meterInfo;
        font = tmpFont;
        WireDragController.Instance?.Bind(board);
        if (components.Count == 0) BuildLabComponents();
    }

    public void BindRecordFields(TMP_InputField vInput, TMP_InputField iInput)
    {
        voltageInput = vInput;
        currentInput = iInput;
    }

    public void StartConnection(int number)
    {
        connectionNumber = number;
        expected = TypeFor(number);
        phase = CircuitLabPhase.Build;
        checkAwarded = false;
        liveReading = null;
        selectedBrightness = "";
        ResetCurrentCircuit();
        SetSuggestedLayout(number);
        if (hintText != null)
            hintText.text = HintFor(number);
        SetRecordFieldsVisible(false);
        UpdateMeterInfo();
        ElecUIManager.Instance?.SetLabButtons(true, false, false, false, false, true);
    }

    public ElectricalComponent GetComponentById(string id)
    {
        foreach (var c in components)
            if (c != null && c.ComponentId == id) return c;
        return null;
    }

    public List<ElectricalTerminal> AllTerminals()
    {
        var list = new List<ElectricalTerminal>();
        foreach (var c in components)
        {
            if (c == null) continue;
            foreach (var t in c.Terminals)
                if (t != null) list.Add(t);
        }
        return list;
    }

    public bool IsPointOnBoard(Vector2 screen, Camera cam)
    {
        if (board == null) return false;
        return RectTransformUtility.RectangleContainsScreenPoint(board, screen, cam);
    }

    public void PlaceFromClick(ElecDragDrop2D item)
    {
        if (item == null || board == null) return;
        Vector2 pos = suggested.ContainsKey(item.ItemId) ? suggested[item.ItemId] : Vector2.zero;
        item.SnapTo(board, pos);
        OnItemDropped("CircuitBoard", item);
    }

    public void PlaceFree(ElecDragDrop2D item, PointerEventData eventData)
    {
        if (item == null || board == null) return;
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(board, eventData.position, eventData.pressEventCamera, out local);
        item.SnapTo(board, local);
        OnItemDropped("CircuitBoard", item);
    }

    public void OnItemDropped(string zoneId, ElecDragDrop2D item)
    {
        if (item == null) return;
        var comp = item.GetComponent<ElectricalComponent>();
        if (comp != null) comp.MarkPlaced(true);
        if (zoneId == "CircuitBoard")
            ElecFeedbackManager.Instance?.ShowInstruction(comp != null ? comp.DisplayName() + " placed on the circuit board. Connect its terminals with wires." : "Component placed.");
        RefreshWires();
    }

    public void TryConnect(ElectricalTerminal a, ElectricalTerminal b)
    {
        if (a == null || b == null || a == b) return;
        if (a.Owner == b.Owner)
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            ElecFeedbackManager.Instance?.ShowMessage("✗ Incorrect connection.\nDo not short a component by joining its own two terminals.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        foreach (var w in wires)
        {
            if (w != null && w.Connects(a, b))
            {
                ElecFeedbackManager.Instance?.ShowInstruction("Those terminals are already connected.");
                return;
            }
        }

        var wireObj = new GameObject("Wire");
        wireObj.transform.SetParent(board, false);
        var img = wireObj.AddComponent<Image>();
        img.sprite = ElecIconFactory.White();
        img.raycastTarget = false;
        var wire = wireObj.AddComponent<WireConnection>();
        wire.Bind(a, b, board);
        wires.Add(wire);
        ElecScoreManager.Instance?.AddScore(5, false);
        ElecFeedbackManager.Instance?.ShowMessage("✓ Wire connected.", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
    }

    public void UndoLastWire()
    {
        if (wires.Count == 0) return;
        var last = wires[wires.Count - 1];
        wires.RemoveAt(wires.Count - 1);
        if (last != null) Destroy(last.gameObject);
        PowerDown();
        phase = CircuitLabPhase.Build;
        ElecUIManager.Instance?.SetLabButtons(true, false, false, false, false, true);
        ElecFeedbackManager.Instance?.ShowInstruction("Last wire removed.");
    }

    public void RotateSelectedCell()
    {
        ElectricalComponent cell = GetComponentById("Cell2");
        if (cell != null && cell.IsPlaced) { cell.ToggleFlip(); return; }
        cell = GetComponentById("Cell1");
        cell?.ToggleFlip();
    }

    public void RotateCell(string id)
    {
        GetComponentById(id)?.ToggleFlip();
    }

    public void CheckCircuit()
    {
        if (phase != CircuitLabPhase.Build && phase != CircuitLabPhase.Checked)
        {
            ElecFeedbackManager.Instance?.ShowInstruction("Reset or finish recording before checking again.");
            return;
        }

        var result = CircuitValidator.Instance != null
            ? CircuitValidator.Instance.Validate(expected)
            : new CircuitValidator.Result { message = "Validator missing." };

        if (!result.isValid)
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            ElecFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\n" + result.message, "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            return;
        }

        phase = CircuitLabPhase.Checked;
        if (!checkAwarded)
        {
            checkAwarded = true;
            ElecScoreManager.Instance?.AddScore(10);
        }
        ElecFeedbackManager.Instance?.ShowMessage(
            $"✓ CONNECTION {connectionNumber} COMPLETE\n{result.message}",
            "+10 Marks",
            new Color(0.08f, 0.52f, 0.22f));
        ElecUIManager.Instance?.SetLabButtons(true, true, false, false, false, true);
    }

    public void TestCircuit()
    {
        if (phase < CircuitLabPhase.Checked)
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            ElecFeedbackManager.Instance?.ShowInstruction("Check the circuit before testing. The circuit must be valid.");
            return;
        }

        var calc = CircuitCalculationManager.Instance;
        var cfg = ConfigFor(connectionNumber);
        liveReading = calc != null
            ? calc.Evaluate(expected, connectionNumber, cfg != null ? cfg.ArrangementName : expected.ToString())
            : new CircuitReading { connectionNumber = connectionNumber, arrangement = expected.ToString() };

        foreach (var w in wires) w.SetEnergized(liveReading.voltage > 0.05f);
        RefreshWires();

        var bulb = GetComponentById("Bulb")?.GetComponent<BulbController>() ?? BulbController.Instance;
        bulb?.ApplyElectricalState(liveReading.voltage, liveReading.current, liveReading.power);
        AmmeterController.Instance?.Show(0f, false);
        VoltmeterController.Instance?.Show(0f, false);

        phase = CircuitLabPhase.Tested;
        UpdateMeterInfo();
        ElecUIManager.Instance?.SetLabButtons(false, false, true, false, false, true);

        string extra = expected == ConnectionType.SeriesOpposing
            ? "\nThe two equal cell voltages oppose each other, so the net potential difference is approximately zero in this simplified model."
            : "\nObserve the brightness of the bulb.";
        ElecFeedbackManager.Instance?.ShowInstruction(
            $"Bulb status: {(liveReading.voltage > 0.05f ? "ON" : "OFF")}\nBrightness: {liveReading.brightnessLabel}" + extra);
    }

    public void MeasureVoltage()
    {
        if (phase < CircuitLabPhase.Tested)
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            ElecFeedbackManager.Instance?.ShowInstruction("Test the circuit before measuring.");
            return;
        }
        float v = VoltageMeasurementManager.Instance != null ? VoltageMeasurementManager.Instance.Measure() : 0f;
        phase = CircuitLabPhase.MeasuredVoltage;
        UpdateMeterInfo();
        ElecUIManager.Instance?.SetLabButtons(false, false, false, true, false, true);
        ElecFeedbackManager.Instance?.ShowInstruction($"Potential Difference Across Bulb:\n{v:0.00} V");
    }

    public void MeasureCurrent()
    {
        if (phase < CircuitLabPhase.MeasuredVoltage)
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            ElecFeedbackManager.Instance?.ShowInstruction("Measure the voltage first, then the current.");
            return;
        }
        float i = CurrentMeasurementManager.Instance != null ? CurrentMeasurementManager.Instance.Measure() : 0f;
        phase = CircuitLabPhase.MeasuredCurrent;
        SetRecordFieldsVisible(true);
        UpdateMeterInfo();
        ElecUIManager.Instance?.SetLabButtons(false, false, false, false, true, true);
        ElecFeedbackManager.Instance?.ShowInstruction($"Current Through Bulb:\n{i:0.00} A");
    }

    public void SetBrightnessChoice(string label)
    {
        selectedBrightness = label;
        UpdateMeterInfo();
    }

    public bool TryRecord()
    {
        if (phase < CircuitLabPhase.MeasuredCurrent)
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            ElecFeedbackManager.Instance?.ShowInstruction("Measure voltage and current before recording.");
            return false;
        }
        if (liveReading == null)
        {
            ElecFeedbackManager.Instance?.ShowInstruction("Test the circuit first.");
            return false;
        }

        float enteredV = ParseField(voltageInput);
        float enteredI = ParseField(currentInput);
        bool vOk = !float.IsNaN(enteredV) && (VoltageMeasurementManager.Instance == null || VoltageMeasurementManager.Instance.IsWithinTolerance(enteredV));
        bool iOk = !float.IsNaN(enteredI) && (CurrentMeasurementManager.Instance == null || CurrentMeasurementManager.Instance.IsWithinTolerance(enteredI));
        bool bOk = string.Equals(NormalizeBright(selectedBrightness), NormalizeBright(liveReading.brightnessLabel));

        if (vOk) ElecScoreManager.Instance?.AddScore(5);
        else
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            ElecFeedbackManager.Instance?.ShowInstruction($"Incorrect voltage. Simulated value is {liveReading.voltage:0.00} V.");
        }
        if (iOk) ElecScoreManager.Instance?.AddScore(5);
        else
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            ElecFeedbackManager.Instance?.ShowInstruction($"Incorrect current. Simulated value is {liveReading.current:0.00} A.");
        }
        if (bOk) ElecScoreManager.Instance?.AddScore(5);
        else
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            ElecFeedbackManager.Instance?.ShowInstruction($"Incorrect brightness. Observed brightness is {liveReading.brightnessLabel}.");
        }

        liveReading.recordedVoltage = float.IsNaN(enteredV) ? 0f : enteredV;
        liveReading.recordedCurrent = float.IsNaN(enteredI) ? 0f : enteredI;
        liveReading.recordedBrightness = selectedBrightness;
        ElecExperimentDataManager.Instance?.Store(liveReading);
        ElecObservationTableManager.Instance?.Refresh();
        phase = CircuitLabPhase.Recorded;
        ElecFeedbackManager.Instance?.ShowInstruction($"Connection {connectionNumber} completed. Readings saved.");
        return true;
    }

    public void ResetCurrentCircuit()
    {
        ClearWires();
        foreach (var c in components)
            c?.ReturnHome();
        PowerDown();
        VoltageMeasurementManager.Instance?.ResetMeasurement();
        CurrentMeasurementManager.Instance?.ResetMeasurement();
        if (voltageInput != null) voltageInput.text = "";
        if (currentInput != null) currentInput.text = "";
        SetRecordFieldsVisible(false);
        selectedBrightness = "";
        WireDragController.Instance?.CancelPending();
        phase = CircuitLabPhase.Build;
        checkAwarded = false;
        liveReading = null;
        UpdateMeterInfo();
    }

    public void RefreshWires()
    {
        foreach (var w in wires)
            w?.RefreshLayout();
    }

    public void ClearWires()
    {
        for (int i = wires.Count - 1; i >= 0; i--)
            if (wires[i] != null) Destroy(wires[i].gameObject);
        wires.Clear();
    }

    private void PowerDown()
    {
        foreach (var w in wires) w?.SetEnergized(false);
        GetComponentById("Bulb")?.GetComponent<BulbController>()?.TurnOff();
        BulbController.Instance?.TurnOff();
        AmmeterController.Instance?.ResetMeter();
        VoltmeterController.Instance?.ResetMeter();
    }

    private void BuildLabComponents()
    {
        if (tray == null || board == null) return;
        components.Clear();
        float v = CircuitCalculationManager.Instance != null ? CircuitCalculationManager.Instance.CellVoltage : 1.5f;
        float r = CircuitCalculationManager.Instance != null ? CircuitCalculationManager.Instance.BulbResistance : 10f;

        components.Add(CreateComponent("Cell1", ElectricalComponentType.DryCell, "Dry Cell 1", v));
        components.Add(CreateComponent("Cell2", ElectricalComponentType.DryCell, "Dry Cell 2", v));
        components.Add(CreateComponent("Bulb", ElectricalComponentType.Bulb, "Bulb", r));
        components.Add(CreateComponent("Ammeter", ElectricalComponentType.Ammeter, "Ammeter", 0f));
        components.Add(CreateComponent("Voltmeter", ElectricalComponentType.Voltmeter, "Voltmeter", 0f));
        LayoutTray();
        board.GetComponent<ElecUIDropTarget>()?.Configure("CircuitBoard", "Any", Vector2.zero);
    }

    private ElectricalComponent CreateComponent(string id, ElectricalComponentType type, string label, float value)
    {
        var obj = new GameObject(id);
        obj.transform.SetParent(tray, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(118f, 80f);
        obj.AddComponent<CanvasGroup>();
        var bg = obj.AddComponent<Image>();
        bg.sprite = ElecIconFactory.White();
        bg.color = new Color(1f, 1f, 1f, 0.96f);
        bg.raycastTarget = true;

        var body = new GameObject("Body");
        body.transform.SetParent(obj.transform, false);
        var brt = body.AddComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.08f, 0.28f);
        brt.anchorMax = new Vector2(0.92f, 0.96f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;
        var bimg = body.AddComponent<Image>();
        bimg.sprite = ElecIconFactory.GetComponentSprite(type);
        bimg.preserveAspect = true;
        bimg.raycastTarget = false;

        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(obj.transform, false);
        var lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0f);
        lrt.anchorMax = new Vector2(1f, 0.28f);
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = label;
        tmp.fontSize = 14;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.12f, 0.16f, 0.22f);
        tmp.raycastTarget = false;

        var le = obj.AddComponent<LayoutElement>();
        le.preferredWidth = 118f;
        le.preferredHeight = 80f;
        le.minWidth = 88f;
        le.minHeight = 64f;
        le.flexibleWidth = 1f;

        var comp = obj.AddComponent<ElectricalComponent>();
        comp.Configure(id, type);
        comp.StoreHome();

        if (type == ElectricalComponentType.DryCell)
        {
            var cell = obj.AddComponent<BatteryCellController>();
            cell.Bind(comp, id, value);
            AddRotateButton(obj, id);
        }
        else if (type == ElectricalComponentType.Bulb)
        {
            var glow = new GameObject("Glow");
            glow.transform.SetParent(body.transform, false);
            var grt = glow.AddComponent<RectTransform>();
            grt.anchorMin = Vector2.zero;
            grt.anchorMax = Vector2.one;
            grt.offsetMin = grt.offsetMax = Vector2.zero;
            var gimg = glow.AddComponent<Image>();
            gimg.sprite = ElecIconFactory.White();
            gimg.color = new Color(1f, 0.92f, 0.35f, 0f);
            gimg.raycastTarget = false;
            var bulb = obj.AddComponent<BulbController>();
            bulb.Bind(gimg, bimg, null, value);
        }
        else if (type == ElectricalComponentType.Ammeter)
        {
            var reading = CreateMeterText(body.transform, "0.00 A");
            var needle = CreateNeedle(body.transform);
            var meter = obj.AddComponent<AmmeterController>();
            meter.Bind(reading, needle);
        }
        else if (type == ElectricalComponentType.Voltmeter)
        {
            var reading = CreateMeterText(body.transform, "0.00 V");
            var needle = CreateNeedle(body.transform);
            var meter = obj.AddComponent<VoltmeterController>();
            meter.Bind(reading, needle);
        }

        return comp;
    }

    private void AddRotateButton(GameObject owner, string id)
    {
        var obj = new GameObject("Rotate");
        obj.transform.SetParent(owner.transform, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(36f, 28f);
        rt.anchoredPosition = new Vector2(-2f, -2f);
        obj.AddComponent<Image>().color = new Color(0.18f, 0.45f, 0.72f);
        var btn = obj.AddComponent<Button>();
        var t = new GameObject("T");
        t.transform.SetParent(obj.transform, false);
        var trt = t.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = trt.offsetMax = Vector2.zero;
        var tmp = t.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = "↻";
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        string captured = id;
        btn.onClick.AddListener(() => RotateCell(captured));
    }

    private TextMeshProUGUI CreateMeterText(Transform parent, string value)
    {
        var obj = new GameObject("Reading");
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.08f);
        rt.anchorMax = new Vector2(0.9f, 0.42f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = value;
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.1f, 0.15f, 0.2f);
        tmp.raycastTarget = false;
        return tmp;
    }

    private RectTransform CreateNeedle(Transform parent)
    {
        var obj = new GameObject("Needle");
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.62f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(4f, 28f);
        obj.AddComponent<Image>().color = new Color(0.75f, 0.12f, 0.12f);
        return rt;
    }

    private void LayoutTray()
    {
        if (tray == null) return;
        var layout = tray.GetComponent<HorizontalLayoutGroup>() ?? tray.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 6f;
        layout.padding = new RectOffset(6, 6, 4, 4);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
    }

    private void SetSuggestedLayout(int number)
    {
        suggested.Clear();
        if (number == 2)
        {
            suggested["Cell1"] = new Vector2(-220f, 90f);
            suggested["Cell2"] = new Vector2(-220f, -10f);
            suggested["Ammeter"] = new Vector2(40f, 40f);
            suggested["Bulb"] = new Vector2(240f, 40f);
            suggested["Voltmeter"] = new Vector2(240f, -90f);
        }
        else
        {
            suggested["Cell1"] = new Vector2(-260f, 50f);
            suggested["Cell2"] = new Vector2(-80f, 50f);
            suggested["Ammeter"] = new Vector2(90f, 50f);
            suggested["Bulb"] = new Vector2(260f, 50f);
            suggested["Voltmeter"] = new Vector2(260f, -90f);
        }
    }

    private void UpdateMeterInfo()
    {
        if (meterInfoText == null) return;
        if (liveReading == null)
        {
            meterInfoText.text = HowToText(connectionNumber);
            return;
        }
        string vLine = VoltageMeasurementManager.Instance != null && VoltageMeasurementManager.Instance.HasMeasured
            ? $"Potential Difference Across Bulb: {liveReading.voltage:0.00} V"
            : "Potential Difference Across Bulb: — V";
        string iLine = CurrentMeasurementManager.Instance != null && CurrentMeasurementManager.Instance.HasMeasured
            ? $"Current Through Bulb: {liveReading.current:0.00} A"
            : "Current Through Bulb: — A";
        meterInfoText.text =
            $"CONNECTION {connectionNumber}  |  {liveReading.arrangement}\n" +
            $"BULB STATUS: {(liveReading.voltage > 0.05f ? "ON" : "OFF")}\n" +
            $"BRIGHTNESS: {liveReading.brightnessLabel}\n" +
            $"{vLine}\n{iLine}\n" +
            $"Power P = VI = {liveReading.power:0.00} W\n" +
            (string.IsNullOrEmpty(selectedBrightness) ? "Select brightness: High / Medium / OFF" : "Selected brightness: " + selectedBrightness) +
            "\n\nSimulation values are based on a simplified circuit model.";
    }

    private static float ParseField(TMP_InputField field)
    {
        if (field == null || string.IsNullOrWhiteSpace(field.text)) return float.NaN;
        if (float.TryParse(field.text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v))
            return v;
        if (float.TryParse(field.text, out v)) return v;
        return float.NaN;
    }

    private static string NormalizeBright(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        value = value.Trim().ToLowerInvariant();
        if (value == "off" || value == "0") return "off";
        if (value.Contains("high") || value.Contains("bright")) return "high";
        if (value.Contains("med")) return "medium";
        if (value.Contains("dim") || value.Contains("low")) return "dim";
        return value;
    }

    private static ConnectionType TypeFor(int n)
    {
        if (n == 2) return ConnectionType.Parallel;
        if (n == 3) return ConnectionType.SeriesOpposing;
        return ConnectionType.SeriesAiding;
    }

    private static ConnectionConfiguration ConfigFor(int n)
    {
        if (n == 2) return Connection2Manager.Instance != null ? Connection2Manager.Instance.Config : ConnectionConfiguration.Parallel();
        if (n == 3) return Connection3Manager.Instance != null ? Connection3Manager.Instance.Config : ConnectionConfiguration.SeriesOpposing();
        return Connection1Manager.Instance != null ? Connection1Manager.Instance.Config : ConnectionConfiguration.SeriesAiding();
    }

    private static string HintFor(int n)
    {
        if (n == 1)
            return "Do this in order: 1) Drag parts onto the board  2) Join Cell1 − to Cell2 +  3) Ammeter in the loop  4) Voltmeter across the bulb  5) CHECK CIRCUIT";
        if (n == 2)
            return "Do this in order: 1) Drag parts onto the board  2) Join both + together and both − together  3) Ammeter in the loop  4) Voltmeter across the bulb  5) CHECK CIRCUIT";
        return "Do this in order: 1) Drag parts onto the board  2) Reverse one cell (↻) so like poles face  3) Ammeter in the loop  4) Voltmeter across the bulb  5) CHECK CIRCUIT";
    }

    private static string HowToText(int n)
    {
        if (n == 1)
            return
                "CONNECTION 1 — Series aiding\n" +
                "Voltages ADD  (about 3.0 V)\n\n" +
                "1. Drag Dry Cell 1, Dry Cell 2, Bulb,\n    Ammeter and Voltmeter onto the board.\n\n" +
                "2. Place cells in a line:\n    Cell 1  [+] [−]  Cell 2  [+] [−]\n    Wire: Cell 1 (−)  →  Cell 2 (+)\n\n" +
                "3. Ammeter goes IN THE LOOP\n    (series with the bulb).\n\n" +
                "4. Voltmeter goes ACROSS THE BULB\n    (one wire to each bulb terminal).\n\n" +
                "5. Tap a red/black terminal, then tap\n    the next terminal to add a wire.\n\n" +
                "6. Press CHECK CIRCUIT.";
        if (n == 2)
            return
                "CONNECTION 2 — Parallel\n" +
                "Voltage stays about 1.5 V\n\n" +
                "1. Drag the same 5 parts onto the board.\n\n" +
                "2. Join both + terminals together.\n    Join both − terminals together.\n\n" +
                "3. Ammeter in the loop with the bulb.\n\n" +
                "4. Voltmeter across the bulb only.\n\n" +
                "5. Press CHECK CIRCUIT.";
        return
            "CONNECTION 3 — Series opposing\n" +
            "Voltages CANCEL  (about 0 V)\n\n" +
            "1. Drag the same 5 parts onto the board.\n\n" +
            "2. Press ↻ on one cell so like poles face:\n    [+] [−]    [−] [+]\n\n" +
            "3. Ammeter in the loop with the bulb.\n\n" +
            "4. Voltmeter across the bulb only.\n\n" +
            "5. Press CHECK CIRCUIT.\n\n" +
            "The bulb should stay OFF.";
    }

    private void SetRecordFieldsVisible(bool on)
    {
        if (voltageInput != null) voltageInput.gameObject.SetActive(on);
        if (currentInput != null) currentInput.gameObject.SetActive(on);
    }
}
