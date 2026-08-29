using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PHASE 3 — Observation table.
/// Recorded experiment values appear as chips; student drags them into the cells.
/// Correct cell +5, wrong cell −2.
/// </summary>
public class UpthrustObservationTableUI : MonoBehaviour
{
    public static UpthrustObservationTableUI Instance { get; private set; }

    public enum Column
    {
        SpringBalance = 0,
        BeakerWeight = 1,
        Upthrust = 2,
        DisplacedWater = 3
    }

    [Serializable]
    public class TableCell
    {
        public int stageIndex;
        public Column column;
        public InputField input;
        public Text valueLabel;
        public Image background;
        public UpthrustObservationDropCell dropCell;
        public bool scoredCorrect;
        public bool scoredWrong;
        public float placedValue = float.NaN;
    }

    [Header("Panels")]
    [SerializeField] private GameObject tablePanel;
    [SerializeField] private Button submitButton;
    [SerializeField] private Text hintText;
    [SerializeField] private RectTransform answerTray;

    [Header("Known Constants")]
    [SerializeField] private Text weightInAirText;
    [SerializeField] private Text emptyBeakerText;

    [Header("Cells")]
    [SerializeField] private TableCell[] cells = new TableCell[UpthrustPracticalData.ObservationCellCount];

    private bool tableComplete;
    private int correctCells;
    private Font uiFont;

    public bool TableComplete => tableComplete;
    public int CorrectCells => correctCells;

    public event Action OnTableCompleted;

    public static readonly Color CellNormal = new Color(0.22f, 0.30f, 0.38f, 1f);
    public static readonly Color CellGood = new Color(0.16f, 0.42f, 0.26f, 1f);
    public static readonly Color CellBad = new Color(0.52f, 0.22f, 0.22f, 1f);
    static readonly Color ChipColor = new Color(0.20f, 0.55f, 0.68f, 1f);

    static readonly float[] RecordedValues =
    {
        1.2f, 0.9f, 0.6f,
        1.3f, 1.6f, 1.9f,
        0.0f, 0.3f
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (tablePanel != null)
            tablePanel.SetActive(false);

        if (submitButton != null)
        {
            submitButton.interactable = false;
            submitButton.onClick.AddListener(SubmitTable);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Configure(
        GameObject panel,
        Button submit,
        Text hint,
        Text wAir,
        Text wBeaker,
        TableCell[] builtCells,
        RectTransform tray)
    {
        tablePanel = panel;
        submitButton = submit;
        hintText = hint;
        weightInAirText = wAir;
        emptyBeakerText = wBeaker;
        cells = builtCells;
        answerTray = tray;

        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(SubmitTable);
            submitButton.onClick.AddListener(SubmitTable);
            submitButton.interactable = false;
        }

        BindDropCells();
        RefreshKnownConstants();
    }

    public void ShowTable()
    {
        if (tablePanel != null)
            tablePanel.SetActive(true);

        RefreshKnownConstants();
        BindDropCells();
        BuildAnswerChips();

        if (hintText != null)
        {
            hintText.text =
                "Drag the recorded answers into the correct cells.  " +
                "Upthrust = W_air − W_apparent.  Displaced water = W_beaker − W_empty.";
        }

        UpthrustUIManager.Instance?.ShowStepInstruction(3, 4,
            "Phase 3: Drag recorded readings into the observation table.");
        UpthrustUIManager.Instance?.ShowProgress("Fill every cell, then press Submit");
    }

    public void HideTable()
    {
        if (tablePanel != null)
            tablePanel.SetActive(false);
    }

    public void ResetTable()
    {
        tableComplete = false;
        correctCells = 0;

        if (cells != null)
        {
            foreach (var cell in cells)
            {
                if (cell == null) continue;
                cell.scoredCorrect = false;
                cell.scoredWrong = false;
                cell.placedValue = float.NaN;
                SetCellText(cell, string.Empty);
                SetCellColor(cell, CellNormal);
            }
        }

        if (submitButton != null)
            submitButton.interactable = false;

        HideTable();
    }

    public void ApplyDroppedValue(UpthrustObservationDropCell drop, float valueN)
    {
        if (tableComplete || drop == null) return;

        TableCell cell = FindCell(drop);
        if (cell == null) return;

        cell.placedValue = valueN;
        SetCellText(cell, UpthrustPracticalData.FormatNewton(valueN) + " N");
        VerifyCell(cell);
    }

    public bool IsCellLocked(UpthrustObservationDropCell drop)
    {
        TableCell cell = FindCell(drop);
        return cell != null && cell.scoredCorrect;
    }

    public void RestoreCellColor(UpthrustObservationDropCell drop)
    {
        TableCell cell = FindCell(drop);
        if (cell == null) return;

        if (cell.scoredCorrect) SetCellColor(cell, CellGood);
        else if (cell.scoredWrong) SetCellColor(cell, CellBad);
        else SetCellColor(cell, CellNormal);
    }

    public void VerifyCell(TableCell cell)
    {
        if (tableComplete || cell == null) return;

        float entered;
        if (!float.IsNaN(cell.placedValue))
            entered = cell.placedValue;
        else if (cell.input != null && TryParseNewton(cell.input.text, out entered))
        { }
        else if (cell.valueLabel != null && TryParseNewton(cell.valueLabel.text, out entered))
        { }
        else
        {
            UpthrustUIManager.Instance?.ShowFeedback("Drop a recorded value into this cell.", false);
            return;
        }

        float expected = ExpectedValue(cell.stageIndex, cell.column);
        bool match = UpthrustPracticalData.ValuesMatch(entered, expected);

        if (match && cell.scoredCorrect)
            return;

        if (match)
        {
            if (!cell.scoredCorrect)
            {
                cell.scoredCorrect = true;
                cell.scoredWrong = false;
                UpthrustScoreManager.Instance?.RegisterCorrectTableCell();
                correctCells++;
            }

            SetCellColor(cell, CellGood);
            SetCellText(cell, UpthrustPracticalData.FormatNewton(expected) + " N");
            UpthrustUIManager.Instance?.ShowFeedback(
                $"Correct! +5   ({correctCells}/{UpthrustPracticalData.ObservationCellCount})", true);
        }
        else
        {
            if (cell.scoredCorrect)
            {
                cell.scoredCorrect = false;
                correctCells = Mathf.Max(0, correctCells - 1);
            }

            if (!cell.scoredWrong)
            {
                cell.scoredWrong = true;
                UpthrustScoreManager.Instance?.RegisterWrongTableCell();
            }

            SetCellColor(cell, CellBad);
            UpthrustUIManager.Instance?.ShowFeedback("Wrong cell for that reading. −2", false);
        }

        if (submitButton != null)
            submitButton.interactable = AllCellsFilled();

        int filled = 0;
        if (cells != null)
        {
            foreach (var c in cells)
                if (c != null && !float.IsNaN(c.placedValue)) filled++;
        }
        UpthrustUIManager.Instance?.ShowProgress($"Cells filled: {filled} / {UpthrustPracticalData.ObservationCellCount}  — then Submit");
    }

    public void SubmitTable()
    {
        if (tableComplete) return;

        if (!AllCellsFilled())
        {
            UpthrustUIManager.Instance?.ShowFeedback("Drop a value into every cell first.", false);
            return;
        }

        tableComplete = true;
        if (submitButton != null)
            submitButton.interactable = false;

        OnTableCompleted?.Invoke();
        UpthrustPracticalManager.Instance?.CompleteObservationPhase();
    }

    public bool AllCellsFilled()
    {
        if (cells == null || cells.Length == 0) return false;
        foreach (var cell in cells)
        {
            if (cell == null || float.IsNaN(cell.placedValue))
                return false;
        }
        return true;
    }

    public bool AllCellsCorrect()
    {
        if (cells == null || cells.Length == 0) return false;
        foreach (var cell in cells)
        {
            if (cell == null || !cell.scoredCorrect)
                return false;
        }
        return true;
    }

    public TableCell GetCell(int stageIndex, Column column)
    {
        if (cells == null) return null;
        foreach (var cell in cells)
        {
            if (cell != null && cell.stageIndex == stageIndex && cell.column == column)
                return cell;
        }
        return null;
    }

    public void PopulateReview(Transform reviewRoot)
    {
        if (reviewRoot == null) return;

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                var yours = FindNamedText(reviewRoot, $"Your_{row}_{col}");
                var correct = FindNamedText(reviewRoot, $"Correct_{row}_{col}");
                float expected = ExpectedValue(row, (Column)col);
                var cell = GetCell(row, (Column)col);

                string student = cell != null && !float.IsNaN(cell.placedValue)
                    ? UpthrustPracticalData.FormatNewton(cell.placedValue) + " N"
                    : "—";
                string answer = UpthrustPracticalData.FormatNewton(expected) + " N";

                if (yours != null)
                {
                    yours.text = student;
                    yours.fontSize = 22;
                    yours.alignment = TextAnchor.MiddleCenter;
                    bool ok = cell != null && cell.scoredCorrect;
                    yours.color = ok ? new Color(0.45f, 1f, 0.55f) : new Color(1f, 0.45f, 0.45f);
                    var bg = yours.transform.parent != null ? yours.transform.parent.GetComponent<Image>() : null;
                    if (bg != null)
                        bg.color = ok ? CellGood : CellBad;
                }

                if (correct != null)
                {
                    correct.text = answer;
                    correct.fontSize = 22;
                    correct.alignment = TextAnchor.MiddleCenter;
                    correct.color = Color.white;
                }
            }
        }
    }

    private static Text FindNamedText(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Text>(true))
            if (t.gameObject.name == name) return t;
        return null;
    }

    public static float ExpectedValue(int stageIndex, Column column)
    {
        var reading = UpthrustPracticalData.Stages[Mathf.Clamp(stageIndex, 0, UpthrustPracticalData.Stages.Length - 1)];
        switch (column)
        {
            case Column.SpringBalance: return reading.springBalanceN;
            case Column.BeakerWeight: return reading.beakerWithWaterN;
            case Column.Upthrust: return reading.upthrustN;
            case Column.DisplacedWater: return reading.displacedWaterN;
            default: return 0f;
        }
    }

    public static bool TryParseNewton(string text, out float value)
    {
        value = 0f;
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (text.IndexOf("drop", StringComparison.OrdinalIgnoreCase) >= 0) return false;

        text = text.Trim().Replace("N", string.Empty).Replace("n", string.Empty).Trim();
        return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            || float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
    }

    private void BindDropCells()
    {
        if (cells == null) return;

        foreach (var cell in cells)
        {
            if (cell == null) continue;

            if (cell.dropCell == null && cell.background != null)
                cell.dropCell = cell.background.GetComponent<UpthrustObservationDropCell>()
                                ?? cell.background.gameObject.AddComponent<UpthrustObservationDropCell>();

            if (cell.dropCell != null)
            {
                cell.dropCell.Configure(cell.stageIndex, cell.column, cell.valueLabel, cell.background);
                if (float.IsNaN(cell.placedValue))
                    cell.dropCell.SetDisplay("drop here");
            }

            if (cell.input != null)
                cell.input.interactable = false;
        }
    }

    private void BuildAnswerChips()
    {
        if (answerTray == null) return;

        for (int i = answerTray.childCount - 1; i >= 0; i--)
        {
            var child = answerTray.GetChild(i);
            if (child.name.StartsWith("Chip_", StringComparison.Ordinal))
                Destroy(child.gameObject);
        }

        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null)
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        foreach (float value in RecordedValues)
            CreateChip(value);
    }

    private void CreateChip(float value)
    {
        var go = new GameObject("Chip_" + value.ToString("0.0"), typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        go.transform.SetParent(answerTray, false);

        var img = go.GetComponent<Image>();
        img.color = ChipColor;
        img.raycastTarget = true;

        var le = go.GetComponent<LayoutElement>();
        le.preferredWidth = 150;
        le.preferredHeight = 58;
        le.minWidth = 140;
        le.minHeight = 54;

        var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(go.transform, false);
        var text = textGo.GetComponent<Text>();
        text.font = uiFont;
        text.fontSize = 26;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        text.text = UpthrustPracticalData.FormatNewton(value) + " N";
        var trt = text.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        var chip = go.AddComponent<RecordedAnswerChip>();
        chip.Configure(value, text, ChipColor);
    }

    private TableCell FindCell(UpthrustObservationDropCell drop)
    {
        if (drop == null || cells == null) return null;
        foreach (var cell in cells)
        {
            if (cell == null) continue;
            if (cell.dropCell == drop) return cell;
            if (cell.stageIndex == drop.StageIndex && cell.column == drop.Column)
                return cell;
        }
        return null;
    }

    private void RefreshKnownConstants()
    {
        if (weightInAirText != null)
            weightInAirText.text = $"W (air) = {UpthrustPracticalData.FormatNewton(UpthrustPracticalData.WeightInAir)} N";

        if (emptyBeakerText != null)
            emptyBeakerText.text = $"W (empty beaker) = {UpthrustPracticalData.FormatNewton(UpthrustPracticalData.EmptyBeakerWeight)} N";
    }

    private static void SetCellText(TableCell cell, string text)
    {
        if (cell.valueLabel != null)
            cell.valueLabel.text = string.IsNullOrEmpty(text) ? "drop here" : text;
        if (cell.input != null)
            cell.input.text = text;
        cell.dropCell?.SetDisplay(string.IsNullOrEmpty(text) ? "drop here" : text);
    }

    private static void SetCellColor(TableCell cell, Color color)
    {
        if (cell.background != null)
            cell.background.color = color;
    }
}
