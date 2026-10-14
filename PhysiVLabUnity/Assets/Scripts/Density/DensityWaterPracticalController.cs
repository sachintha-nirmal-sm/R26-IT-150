using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Grade 9 Density of Water (Figure 11.2 / Table 11.1).
/// Select equipment, zero the triple-beam balance, weigh water, fill the table.
/// </summary>
[DefaultExecutionOrder(-100)]
public class DensityWaterPracticalController : MonoBehaviour
{
    public static DensityWaterPracticalController Instance { get; private set; }

    private const float EmptyBeakerG = 148.2f;
    private const float TrueDensity = 1.00f;
    private static readonly int[] Volumes = { 100, 250, 500 };

    private enum Stage
    {
        Intro,
        Select,
        Lab,
        Table,
        Results
    }

    private Stage _stage = Stage.Intro;
    private bool _submitted;
    private int _score;
    private int _mistakes;
    private int _volumeIndex;
    private bool _zeroed;
    private bool _beakerOnPan;
    private bool _emptyWeighed;
    private bool _cylinderFilled;
    private bool _poured;
    private bool _emptySaved;
    private int _conclusion;
    private readonly bool[] _rowSaved = new bool[3];
    private readonly HashSet<string> _selected = new HashSet<string>();

    private Font _font;
    private Text _titleText;
    private Text _stepText;
    private Text _instructionText;
    private Text _statusText;
    private Text _timerText;
    private Text _scoreText;
    private Text _readingText;
    private Transform _labRoot;
    private Transform _tray;
    private Transform _trayGrid;
    private Transform _trayBlock;
    private Transform _tableRoot;
    private Transform _shelf;
    private Text _trayHint;
    private RectTransform _beakerUi;
    private RectTransform _waterFillUi;
    private Image _waterFillImage;
    private Button _nextButton;
    private Button _zeroButton;
    private Button _saveButton;
    private Text _nextLabel;
    private Text _zeroLabel;
    private Text _saveLabel;
    private Transform _conclusionRow;
    private InputField _emptyField;
    private Image _emptyFieldBg;
    private readonly InputField[] _massBeakerFields = new InputField[3];
    private readonly InputField[] _massWaterFields = new InputField[3];
    private readonly InputField[] _densityFields = new InputField[3];
    private readonly Image[] _massBeakerBgs = new Image[3];
    private readonly Image[] _massWaterBgs = new Image[3];
    private readonly Image[] _densityBgs = new Image[3];
    private readonly HashSet<string> _trayIds = new HashSet<string>();
    private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();

    private readonly Color _navy = new Color(0.07f, 0.32f, 0.38f);
    private readonly Color _teal = new Color(0.12f, 0.58f, 0.62f);
    private readonly Color _panel = new Color(0.86f, 0.95f, 0.96f);
    private readonly Color _ink = new Color(0.10f, 0.14f, 0.16f);
    private readonly Color _water = new Color(0.28f, 0.62f, 0.88f, 0.72f);
    private readonly string[] _needed = { "balance", "beaker", "cyl100", "cyl250", "cyl500", "water" };

    private void Awake()
    {
        Instance = this;
        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();
        _font = Font.CreateDynamicFontFromOSFont(new[] { "Segoe UI", "Arial", "Helvetica" }, 64);
        if (_font == null)
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        SetupCamera();
        BuildCanvas();
        ShowIntro();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.12f, 0.58f, 0.62f, 0.9f);
        Gizmos.DrawWireCube(transform.position, new Vector3(10f, 6f, 0.1f));
    }

    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            return;
        }

        GUI.Box(new Rect(20f, 20f, 540f, 70f), "");
        GUI.Label(
            new Rect(32f, 32f, 520f, 50f),
            "Density lab is empty here on purpose.\nPress Play — the drag-and-drop UI builds at runtime.");
    }

    private void Start()
    {
        TimerManager.OnExpired += CompleteOnTimeout;
        TimerManager.OnTick += UpdateTimer;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);
        UpdateTimer(TimerManager.Instance != null ? TimerManager.Instance.RemainingSeconds : limit);
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.OnTick -= UpdateTimer;
        TimerManager.HideOnGui = false;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            cam = go.AddComponent<Camera>();
            go.tag = "MainCamera";
        }

        cam.orthographic = true;
        cam.orthographicSize = 5.4f;
        cam.backgroundColor = new Color(0.90f, 0.96f, 0.97f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    private void ShowIntro()
    {
        _stage = Stage.Intro;
        _stepText.text = "Welcome";
        _instructionText.text =
            "Density of Water  •  Figure 11.2  •  Table 11.1\n"
            + "Zero the triple-beam balance, weigh an empty 500 ml beaker, then 100, 250 and 500 ml of water.";
        _statusText.text = "Tap Next, then pick only the equipment this lab needs.";
        RefreshReading();
        SetLabVisible(false);
        SetShelfVisible(false);
        ShowTrayOrTable();
        RefreshButtons();
        RefreshScore();
    }

    private void ShowSelect()
    {
        _stage = Stage.Select;
        _stepText.text = "Step 1  •  Choose equipment";
        _instructionText.text =
            "Drag the correct items into Your tray. Wrong tools lose marks.\n"
            + "You need: 100 / 250 / 500 ml cylinders, 500 ml beaker, triple-beam balance, water.";
        _statusText.text = "Hint: a thermometer or ruler is not in this method.";
        SetLabVisible(false);
        SetShelfVisible(true);
        ShowTrayOrTable();
        RefreshButtons();
    }

    private void ShowLab()
    {
        _stage = Stage.Lab;
        _volumeIndex = 0;
        _cylinderFilled = false;
        _poured = false;
        _stepText.text = "Step 2  •  Figure 11.2";
        _instructionText.text =
            "Zero the balance, put the dry beaker on the pan, then measure 100 ml of water with the 100 ml cylinder.";
        _statusText.text = "Adjust the triple-beam balance to the zero mark first.";
        SetShelfVisible(false);
        SetLabVisible(true);
        ShowTrayOrTable();
        RefreshReading();
        RefreshWaterFill();
        RefreshButtons();
    }

    private void ShowTable()
    {
        _stage = Stage.Table;
        _stepText.text = "Step 3  •  Table 11.1";
        _instructionText.text =
            "Copy the balance readings. Mass of water = beaker with water − empty beaker. Density = mass / volume.  1 ml = 1 cm³.";
        _statusText.text = "Fill every cell, then Save table.";
        ShowTrayOrTable();
        RefreshButtons();
        RefreshTableHighlight();
    }

    private void ShowResults()
    {
        _stage = Stage.Results;
        _stepText.text = "Conclusion";
        _instructionText.text =
            "What can be concluded? Density of water stays about 1 g cm⁻³ for every volume you tried.";
        _statusText.text = "Pick the conclusion, then Submit to save marks to your profile.";
        ShowTrayOrTable();
        RefreshButtons();
    }

    public void OnEquipmentDropped(string id, bool intoTray)
    {
        if (_submitted || _stage != Stage.Select || !intoTray)
        {
            return;
        }

        bool needed = IsNeeded(id);
        if (needed)
        {
            if (_selected.Add(id))
            {
                AddScore(6);
                _statusText.text = NiceName(id) + " is needed. Nice.";
            }
        }
        else
        {
            _mistakes++;
            AddScore(-8);
            _statusText.text = NiceName(id) + " is not for this lab. −8";
        }

        AddTrayChip(id, needed);
        RefreshButtons();
    }

    public void OnLabDropped(string itemId, string zone)
    {
        if (_submitted || _stage != Stage.Lab)
        {
            return;
        }

        if (itemId == "beaker")
        {
            PlaceBeaker();
            return;
        }

        if (IsCylinder(itemId))
        {
            if (zone == "water" || (!_cylinderFilled && zone == "lab"))
            {
            FillCylinder(itemId);
                return;
            }

            PourCylinder(itemId);
            return;
        }

        if (itemId == "water" && (zone == "beaker" || zone == "pan"))
        {
            Penalize("Measure the water with the correct cylinder first. Do not pour from the jug.");
            return;
        }

        Penalize("Drop that on the matching place: pan, water jug, or beaker.");
    }

    private void PlaceBeaker()
    {
        if (_beakerOnPan)
        {
            return;
        }

        _beakerOnPan = true;
        Show(_beakerUi, true);
        AddScore(6);
        if (!_zeroed)
        {
            _statusText.text = "Beaker is on the pan, but the balance is not on zero yet.";
        }
        else
        {
            CaptureEmptyMass();
        }

        RefreshReading();
        RefreshButtons();
    }

    private void FillCylinder(string itemId)
    {
        if (!_beakerOnPan || !_emptyWeighed)
        {
            Penalize("Weigh the empty beaker before you measure water.");
            return;
        }

        string needed = NeededCylinder();
        if (itemId != needed)
        {
            Penalize("Use the " + Volumes[_volumeIndex] + " ml cylinder for this volume.");
            return;
        }

        if (_cylinderFilled)
        {
            return;
        }

        _cylinderFilled = true;
        AddScore(6);
        _statusText.text = "You measured " + Volumes[_volumeIndex] + " ml. Pour it into the beaker.";
        RefreshButtons();
    }

    private void PourCylinder(string itemId)
    {
        if (!_cylinderFilled)
        {
            Penalize("Fill the cylinder from the water jug first.");
            return;
        }

        if (itemId != NeededCylinder())
        {
            Penalize("Pour the cylinder that matches " + Volumes[_volumeIndex] + " ml.");
            return;
        }

        if (_poured)
        {
            return;
        }

        _poured = true;
        AddScore(6);
        _statusText.text =
            "Beaker now holds " + Volumes[_volumeIndex] + " ml. Read the balance, then go to the next volume.";
        RefreshReading();
        RefreshWaterFill();
        RefreshButtons();
    }

    private void CaptureEmptyMass()
    {
        if (_emptyWeighed || !_zeroed || !_beakerOnPan)
        {
            return;
        }

        _emptyWeighed = true;
        AddScore(8);
        _statusText.text = "Empty beaker = " + EmptyBeakerG.ToString("0.0", CultureInfo.InvariantCulture)
            + " g. Now measure 100 ml of water.";
    }

    private void OnZero()
    {
        if (_submitted || _stage != Stage.Lab || _zeroed)
        {
            return;
        }

        _zeroed = true;
        AddScore(8);
        _statusText.text = "Balance is on the zero mark. Place the dry 500 ml beaker on the pan.";
        if (_beakerOnPan)
        {
            CaptureEmptyMass();
        }

        RefreshReading();
        RefreshButtons();
    }

    private void OnNext()
    {
        if (_submitted)
        {
            return;
        }

        if (_stage == Stage.Intro)
        {
            ShowSelect();
            return;
        }

        if (_stage == Stage.Select)
        {
            if (_selected.Count < _needed.Length)
            {
                Penalize("Select every required item first.");
                return;
            }

            ShowLab();
            return;
        }

        if (_stage == Stage.Lab)
        {
            if (!_poured)
            {
                Penalize("Finish this volume before the next step.");
                return;
            }

            if (_volumeIndex < Volumes.Length - 1)
            {
                _volumeIndex++;
                _cylinderFilled = false;
                _poured = false;
                RefreshWaterFill();
                RefreshReading();
                _stepText.text = "Step 2  •  " + Volumes[_volumeIndex] + " ml";
                _instructionText.text =
                    "Empty the beaker in your mind, then measure " + Volumes[_volumeIndex]
                    + " ml with the matching cylinder and weigh again.";
                _statusText.text = "Use the " + Volumes[_volumeIndex] + " ml measuring cylinder this time.";
                RefreshButtons();
                return;
            }

            ShowTable();
            return;
        }

        if (_stage == Stage.Table)
        {
            if (!_emptySaved || !_rowSaved[0] || !_rowSaved[1] || !_rowSaved[2])
            {
                Penalize("Save Table 11.1 before the conclusion.");
                return;
            }

            ShowResults();
            return;
        }

        if (_stage == Stage.Results)
        {
            if (_conclusion == 0)
            {
                Penalize("Pick a conclusion first.");
                return;
            }

            Finish(true);
        }
    }

    private void OnSaveTable()
    {
        if (_submitted || _stage != Stage.Table)
        {
            return;
        }

        float emptyEntered;
        if (!TryRead(_emptyField, out emptyEntered))
        {
            Penalize("Type the mass of the empty beaker first.");
            return;
        }

        if (!_emptySaved)
        {
            _emptySaved = true;
            ScoreValue(emptyEntered, EmptyBeakerG, 1.5f, 8, "Empty beaker mass");
        }

        for (int i = 0; i < 3; i++)
        {
            if (_rowSaved[i])
            {
                continue;
            }

            float beakerWater;
            float waterMass;
            float density;
            if (!TryRead(_massBeakerFields[i], out beakerWater)
                || !TryRead(_massWaterFields[i], out waterMass)
                || !TryRead(_densityFields[i], out density))
            {
                Penalize("Fill every cell for " + Volumes[i] + " cm³.");
                return;
            }

            _rowSaved[i] = true;
            float trueBeaker = EmptyBeakerG + Volumes[i];
            float trueWater = Volumes[i];
            ScoreValue(beakerWater, trueBeaker, 1.5f, 4, Volumes[i] + " ml beaker + water");
            ScoreValue(waterMass, trueWater, 2.0f, 4, "Mass of water");
            ScoreValue(density, TrueDensity, 0.08f, 6, "Density");
        }

        _statusText.text = "Table 11.1 saved. Density should be about 1.00 g cm⁻³ each time.";
        RefreshTableHighlight();
        RefreshButtons();
    }

    public void OnConclusion(int choice)
    {
        if (_submitted || _stage != Stage.Results || _conclusion != 0)
        {
            return;
        }

        _conclusion = choice;
        if (choice == 1)
        {
            AddScore(8);
            _statusText.text = "Yes — density of water is about 1 g cm⁻³, independent of the volume you used.";
        }
        else
        {
            Penalize("Look at the last column. The ratio stayed near 1 g cm⁻³.");
        }

        RefreshButtons();
    }

    private void ScoreValue(float entered, float truth, float tolerance, int marks, string label)
    {
        float error = Mathf.Abs(entered - truth);
        if (error <= tolerance)
        {
            AddScore(marks);
        }
        else if (error <= tolerance * 2.5f)
        {
            AddScore(Mathf.Max(1, marks / 2));
            _statusText.text = label + " is a little off. Check the balance.";
        }
        else
        {
            AddScore(1);
            _statusText.text = label + " should be near " + truth.ToString("0.00", CultureInfo.InvariantCulture) + ".";
        }
    }

    private static bool TryRead(InputField field, out float value)
    {
        value = 0f;
        if (field == null)
        {
            return false;
        }

        string raw = field.text.Trim();
        return !string.IsNullOrEmpty(raw)
            && (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || float.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value));
    }

    private void Penalize(string message)
    {
        _mistakes++;
        AddScore(-6);
        _statusText.text = message + "  −6";
        RefreshScore();
    }

    private void AddScore(int delta)
    {
        _score = Mathf.Clamp(_score + delta, 0, 100);
        RefreshScore();
    }

    private void RefreshScore()
    {
        if (_scoreText != null)
        {
            _scoreText.text = "Score  " + _score + " / 100";
        }
    }

    private void RefreshReading()
    {
        if (_readingText == null)
        {
            return;
        }

        if (!_zeroed)
        {
            _readingText.text = "Balance  not zeroed";
            return;
        }

        if (!_beakerOnPan)
        {
            _readingText.text = "Balance  0.0 g";
            return;
        }

        float mass = EmptyBeakerG;
        if (_poured)
        {
            mass += Volumes[_volumeIndex];
        }

        _readingText.text = "Balance  " + mass.ToString("0.0", CultureInfo.InvariantCulture) + " g";
    }

    private void RefreshWaterFill()
    {
        if (_waterFillUi == null)
        {
            return;
        }

        bool show = _beakerOnPan && _poured;
        Show(_waterFillUi, show);
        if (!show || _waterFillImage == null)
        {
            return;
        }

        float t = Volumes[_volumeIndex] / 500f;
        var rect = _waterFillUi;
        rect.anchorMin = new Vector2(0.18f, 0.12f);
        rect.anchorMax = new Vector2(0.82f, 0.12f + 0.70f * t);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        _waterFillImage.color = Color.Lerp(new Color(0.45f, 0.75f, 0.95f, 0.55f), _water, t);
    }

    private void RefreshButtons()
    {
        bool selectReady = _stage == Stage.Select && _selected.Count >= _needed.Length;
        bool labReady = _stage == Stage.Lab && _poured;
        bool tableReady = _stage == Stage.Table && _emptySaved && _rowSaved[0] && _rowSaved[1] && _rowSaved[2];
        bool resultsReady = _stage == Stage.Results && _conclusion != 0;
        _nextButton.interactable = !_submitted && (
            _stage == Stage.Intro || selectReady || labReady || tableReady || resultsReady);
        _zeroButton.interactable = !_submitted && _stage == Stage.Lab && !_zeroed;
        _saveButton.interactable = !_submitted && _stage == Stage.Table && !tableReady;

        if (_submitted)
        {
            _nextLabel.text = "Completed";
            _nextButton.interactable = false;
            var done = _nextButton.colors;
            done.disabledColor = new Color(0.15f, 0.52f, 0.38f);
            _nextButton.colors = done;
            SetButtonVisible(_zeroButton, false);
            SetButtonVisible(_saveButton, false);
            return;
        }

        if (_stage == Stage.Intro) _nextLabel.text = "Start lab";
        else if (_stage == Stage.Select) _nextLabel.text = "Go to bench";
        else if (_stage == Stage.Lab && _volumeIndex < Volumes.Length - 1) _nextLabel.text = "Next volume";
        else if (_stage == Stage.Lab) _nextLabel.text = "Open Table 11.1";
        else if (_stage == Stage.Table) _nextLabel.text = "Conclusion";
        else _nextLabel.text = "Submit";

        _zeroLabel.text = "Zero balance";
        _saveLabel.text = "Save Table 11.1";
        SetButtonVisible(_zeroButton, _stage == Stage.Lab);
        SetButtonVisible(_saveButton, _stage == Stage.Table);
    }

    private void RefreshTableHighlight()
    {
        PaintField(_emptyField, _emptyFieldBg, _emptySaved, _stage == Stage.Table && !_emptySaved);
        for (int i = 0; i < 3; i++)
        {
            bool active = _stage == Stage.Table && !_rowSaved[i];
            PaintField(_massBeakerFields[i], _massBeakerBgs[i], _rowSaved[i], active);
            PaintField(_massWaterFields[i], _massWaterBgs[i], _rowSaved[i], active);
            PaintField(_densityFields[i], _densityBgs[i], _rowSaved[i], active);
        }
    }

    private void PaintField(InputField field, Image bg, bool saved, bool active)
    {
        if (field == null)
        {
            return;
        }

        field.interactable = !_submitted && active;
        if (bg != null)
        {
            bg.color = saved
                ? new Color(0.82f, 0.95f, 0.86f)
                : active
                    ? new Color(1f, 0.96f, 0.72f)
                    : Color.white;
        }
    }

    private void UpdateTimer(int remaining)
    {
        if (_timerText == null)
        {
            return;
        }

        int minutes = Mathf.Max(0, remaining) / 60;
        int seconds = Mathf.Max(0, remaining) % 60;
        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        _timerText.color = remaining <= 30 ? new Color(0.75f, 0.12f, 0.12f) : _navy;
    }

    public void CompleteOnTimeout()
    {
        Finish(false);
    }

    private void Finish(bool studentSubmitted)
    {
        if (_submitted)
        {
            return;
        }

        _submitted = true;
        TimerManager.Instance?.Stop();
        bool passed = _score >= 50 && _emptySaved && _rowSaved[0];
        int timeUsed = TimerManager.Instance != null ? TimerManager.Instance.TimeUsedSeconds : 0;
        _statusText.text = (studentSubmitted ? "Submitted. " : "Time expired. ") + "Score " + _score + " / 100";
        if (!Application.isEditor)
        {
            _instructionText.text = "Your marks were sent to the app profile. You can leave this lab.";
        }
        else
        {
            _instructionText.text = "Score saved in this Play session. Unity Editor will stop Play in a moment.";
        }

        RefreshButtons();
        RefreshTableHighlight();

        string measurements =
            "{\"emptyBeaker\":" + EmptyBeakerG.ToString("0.0", CultureInfo.InvariantCulture)
            + ",\"v100\":100,\"v250\":250,\"v500\":500"
            + ",\"density\":" + TrueDensity.ToString("0.00", CultureInfo.InvariantCulture)
            + ",\"selected\":" + _selected.Count
            + ",\"conclusion\":" + _conclusion
            + "}";

        FlutterBridge.EnsureInstance()?.NotifyCompleted(
            _score,
            passed,
            _mistakes,
            timeUsed,
            true,
            measurements);

        StartCoroutine(AfterSubmit());
    }

    private IEnumerator AfterSubmit()
    {
        yield return new WaitForSecondsRealtime(1.2f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private string NeededCylinder()
    {
        switch (Volumes[_volumeIndex])
        {
            case 100: return "cyl100";
            case 250: return "cyl250";
            default: return "cyl500";
        }
    }

    private static bool IsCylinder(string id)
    {
        return id == "cyl100" || id == "cyl250" || id == "cyl500";
    }

    private bool IsNeeded(string id)
    {
        for (int i = 0; i < _needed.Length; i++)
        {
            if (_needed[i] == id)
            {
                return true;
            }
        }

        return false;
    }

    private static string NiceName(string id)
    {
        switch (id)
        {
            case "balance": return "Triple-beam balance";
            case "beaker": return "500 ml beaker";
            case "cyl100": return "100 ml cylinder";
            case "cyl250": return "250 ml cylinder";
            case "cyl500": return "500 ml cylinder";
            case "water": return "Water";
            case "thermo": return "Thermometer";
            case "ruler": return "Metre ruler";
            case "spring": return "Helical spring";
            case "stopwatch": return "Stopwatch";
            default: return id;
        }
    }

    private void BuildCanvas()
    {
        EnsureEventSystem();
        var canvasGo = new GameObject("DensityUI");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var top = Panel("TopBar", canvasGo.transform, new Vector2(0.02f, 0.88f), new Vector2(0.98f, 0.98f), _panel);
        _titleText = Label("Title", top, new Vector2(0.02f, 0.48f), new Vector2(0.62f, 0.96f), 34, FontStyle.Bold, _navy, TextAnchor.MiddleLeft);
        _titleText.text = "Density of Water";
        _scoreText = Label("Score", top, new Vector2(0.62f, 0.48f), new Vector2(0.82f, 0.96f), 28, FontStyle.Bold, _teal, TextAnchor.MiddleCenter);
        _timerText = Label("Timer", top, new Vector2(0.82f, 0.48f), new Vector2(0.98f, 0.96f), 36, FontStyle.Bold, _navy, TextAnchor.MiddleRight);
        _stepText = Label("Step", top, new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.50f), 26, FontStyle.Bold, _teal, TextAnchor.MiddleLeft);

        _labRoot = Panel("Lab", canvasGo.transform, new Vector2(0.02f, 0.42f), new Vector2(0.62f, 0.86f), new Color(0.93f, 0.97f, 0.98f)).transform;
        BuildLabFigure(_labRoot);

        _shelf = Panel("Shelf", canvasGo.transform, new Vector2(0.02f, 0.42f), new Vector2(0.62f, 0.86f), new Color(0.93f, 0.97f, 0.98f)).transform;
        BuildShelf(_shelf);

        var side = Panel("Side", canvasGo.transform, new Vector2(0.64f, 0.42f), new Vector2(0.98f, 0.86f), _panel);
        _trayBlock = Create("TrayBlock", side, Vector2.zero, Vector2.one).transform;
        Label("TrayTitle", _trayBlock, new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.96f), 24, FontStyle.Bold, _navy, TextAnchor.MiddleLeft).text = "Your tray  (drop here)";
        _tray = Panel("Tray", _trayBlock, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.80f), Color.white).transform;
        var trayImage = _tray.GetComponent<Image>();
        if (trayImage != null)
        {
            trayImage.raycastTarget = true;
        }

        _tray.gameObject.AddComponent<DensityDropZone>().Kind = "tray";
        _trayHint = Label("TrayHint", _tray, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.62f), 22, FontStyle.Italic, new Color(0.45f, 0.50f, 0.58f), TextAnchor.MiddleCenter);
        _trayHint.text = "Drop equipment here";
        _trayGrid = Create("TrayGrid", _tray, Vector2.zero, Vector2.one).transform;
        var grid = _trayGrid.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(96f, 104f);
        grid.spacing = new Vector2(8f, 8f);
        grid.padding = new RectOffset(10, 10, 10, 10);
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        _tableRoot = Create("TableBlock", side, Vector2.zero, Vector2.one).transform;
        BuildTable11(_tableRoot);

        var bottom = Panel("Bottom", canvasGo.transform, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.40f), _panel);
        _instructionText = Label("Instruction", bottom, new Vector2(0.02f, 0.72f), new Vector2(0.98f, 0.98f), 24, FontStyle.Normal, _ink, TextAnchor.UpperLeft);
        _statusText = Label("Status", bottom, new Vector2(0.02f, 0.58f), new Vector2(0.98f, 0.72f), 22, FontStyle.Bold, _navy, TextAnchor.MiddleLeft);
        _readingText = Label("Reading", bottom, new Vector2(0.02f, 0.44f), new Vector2(0.50f, 0.58f), 26, FontStyle.Bold, _teal, TextAnchor.MiddleLeft);

        var actions = Create("Actions", bottom, new Vector2(0.50f, 0.06f), new Vector2(0.98f, 0.50f));
        var layout = actions.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.padding = new RectOffset(6, 6, 6, 6);

        _zeroButton = MakeButton(actions.transform, new Color(0.15f, 0.52f, 0.38f), out _zeroLabel);
        _zeroButton.onClick.AddListener(OnZero);
        _saveButton = MakeButton(actions.transform, new Color(0.90f, 0.48f, 0.12f), out _saveLabel);
        _saveButton.onClick.AddListener(OnSaveTable);
        _nextButton = MakeButton(actions.transform, _teal, out _nextLabel);
        _nextButton.onClick.AddListener(OnNext);

        BuildConclusion(bottom);
    }

    private void BuildConclusion(Transform bottom)
    {
        var row = Create("Conclusion", bottom, new Vector2(0.02f, 0.06f), new Vector2(0.48f, 0.42f));
        var layout = row.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 6f;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = true;
        MakeConclusion(row.transform, 1, "Density ≈ 1 g cm⁻³ for every volume");
        MakeConclusion(row.transform, 2, "Density changes a lot with volume");
        MakeConclusion(row.transform, 3, "Mass of water equals volume in litres");
        row.SetActive(false);
        _conclusionRow = row.transform;
    }

    private void MakeConclusion(Transform parent, int id, string label)
    {
        var go = new GameObject("c" + id);
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.95f);
        var button = go.AddComponent<Button>();
        int captured = id;
        button.onClick.AddListener(() => OnConclusion(captured));
        Label("t", go.transform, new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), 16, FontStyle.Bold, _navy, TextAnchor.MiddleLeft).text = label;
    }

    private void BuildShelf(Transform parent)
    {
        string[] ids =
        {
            "balance", "beaker", "cyl100", "cyl250", "cyl500",
            "water", "thermo", "ruler", "spring", "stopwatch"
        };
        Color[] colors =
        {
            new Color(0.22f, 0.48f, 0.78f), new Color(0.75f, 0.82f, 0.86f),
            new Color(0.40f, 0.70f, 0.78f), new Color(0.32f, 0.62f, 0.72f), new Color(0.22f, 0.52f, 0.64f),
            new Color(0.28f, 0.62f, 0.88f), new Color(0.85f, 0.35f, 0.32f), new Color(0.72f, 0.62f, 0.28f),
            new Color(0.55f, 0.35f, 0.70f), new Color(0.20f, 0.55f, 0.45f)
        };

        for (int i = 0; i < ids.Length; i++)
        {
            int col = i % 5;
            int row = i / 5;
            float x0 = 0.02f + col * 0.196f;
            float y1 = 0.94f - row * 0.46f;
            MakeEquipmentCard(
                parent,
                ids[i],
                NiceName(ids[i]),
                colors[i],
                new Vector2(x0, y1 - 0.40f),
                new Vector2(x0 + 0.184f, y1),
                "equipment");
        }
    }

    private void BuildLabFigure(Transform parent)
    {
        var bench = Panel("Bench", parent, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.76f), new Color(0.90f, 0.94f, 0.96f));
        AddSpriteFill(Create("BalancePic", bench, new Vector2(0.02f, 0.08f), new Vector2(0.62f, 0.96f)).transform, "balance");

        Label("panHint", bench, new Vector2(0.04f, 0.02f), new Vector2(0.40f, 0.14f), 16, FontStyle.Italic, _ink, TextAnchor.MiddleLeft).text = "drop beaker on pan";

        _beakerUi = Panel("BeakerOnPan", bench, new Vector2(0.08f, 0.34f), new Vector2(0.30f, 0.78f), new Color(1f, 1f, 1f, 0.08f)).GetComponent<RectTransform>();
        AddSpriteFill(_beakerUi, "beaker");
        _waterFillUi = Panel("WaterFill", _beakerUi, new Vector2(0.18f, 0.12f), new Vector2(0.82f, 0.55f), _water).GetComponent<RectTransform>();
        _waterFillImage = _waterFillUi.GetComponent<Image>();
        if (_waterFillImage != null)
        {
            _waterFillImage.raycastTarget = false;
        }

        Show(_beakerUi, false);
        Show(_waterFillUi, false);

        var jug = Panel("Jug", bench, new Vector2(0.64f, 0.08f), new Vector2(0.96f, 0.72f), Color.white);
        AddSpriteFill(jug, "water");
        Label("jugHint", jug, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.18f), 16, FontStyle.Bold, _navy, TextAnchor.MiddleCenter).text = "water";

        MakeDrop(parent, "pan", new Vector2(0.04f, 0.08f), new Vector2(0.40f, 0.72f));
        MakeDrop(parent, "beaker", new Vector2(0.08f, 0.30f), new Vector2(0.32f, 0.76f));
        MakeDrop(parent, "water", new Vector2(0.64f, 0.08f), new Vector2(0.96f, 0.72f));
        MakeDrop(parent, "lab", new Vector2(0.01f, 0.00f), new Vector2(0.99f, 0.76f));

        var kit = Panel("Kit", parent, new Vector2(0.02f, 0.78f), new Vector2(0.98f, 0.98f), Color.white);
        MakeKitItem(kit, "beaker", "Beaker", new Color(0.75f, 0.82f, 0.86f), 0.00f);
        MakeKitItem(kit, "cyl100", "100 ml", new Color(0.40f, 0.70f, 0.78f), 0.20f);
        MakeKitItem(kit, "cyl250", "250 ml", new Color(0.32f, 0.62f, 0.72f), 0.40f);
        MakeKitItem(kit, "cyl500", "500 ml", new Color(0.22f, 0.52f, 0.64f), 0.60f);
        MakeKitItem(kit, "water", "Water", _water, 0.80f);
    }

    private void MakeKitItem(Transform parent, string id, string label, Color color, float x)
    {
        MakeEquipmentCard(
            parent,
            id,
            label,
            color,
            new Vector2(0.01f + x, 0.08f),
            new Vector2(0.19f + x, 0.92f),
            "setup");
    }

    private void MakeDrop(Transform parent, string kind, Vector2 min, Vector2 max)
    {
        var zone = Create("drop-" + kind, parent, min, max);
        var image = zone.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.04f);
        var drop = zone.AddComponent<DensityDropZone>();
        drop.Kind = kind;
    }

    private void SetLabVisible(bool visible)
    {
        if (_labRoot != null)
        {
            _labRoot.gameObject.SetActive(visible);
        }
    }

    private void ShowTrayOrTable()
    {
        if (_trayBlock != null)
        {
            _trayBlock.gameObject.SetActive(_stage == Stage.Select);
        }

        if (_tableRoot != null)
        {
            _tableRoot.gameObject.SetActive(_stage == Stage.Lab || _stage == Stage.Table || _stage == Stage.Results);
        }

        if (_conclusionRow != null)
        {
            _conclusionRow.gameObject.SetActive(_stage == Stage.Results);
        }
    }

    private void BuildTable11(Transform parent)
    {
        Label("TableTitle", parent, new Vector2(0.04f, 0.90f), new Vector2(0.96f, 0.99f), 20, FontStyle.Bold, _navy, TextAnchor.MiddleLeft).text = "Table 11.1";
        Label("EmptyLbl", parent, new Vector2(0.04f, 0.80f), new Vector2(0.58f, 0.90f), 14, FontStyle.Normal, _ink, TextAnchor.MiddleLeft).text = "Mass of empty beaker / g";
        _emptyField = MakeTimeField(parent, new Vector2(0.60f, 0.81f), new Vector2(0.96f, 0.89f), out _emptyFieldBg, "g");

        var header = Panel("Header", parent, new Vector2(0.04f, 0.70f), new Vector2(0.96f, 0.80f), _navy);
        TableHeader(header, "V/cm³", 0.00f, 0.18f);
        TableHeader(header, "Beaker+w", 0.18f, 0.46f);
        TableHeader(header, "Water/g", 0.46f, 0.70f);
        TableHeader(header, "g cm⁻³", 0.70f, 1.00f);

        for (int i = 0; i < 3; i++)
        {
            float y1 = 0.70f - i * 0.18f;
            float y0 = y1 - 0.17f;
            var row = Panel("row" + i, parent, new Vector2(0.04f, y0), new Vector2(0.96f, y1), i % 2 == 0 ? new Color(0.94f, 0.98f, 0.99f) : Color.white);
            Label("v", row, new Vector2(0.00f, 0f), new Vector2(0.18f, 1f), 16, FontStyle.Bold, _ink, TextAnchor.MiddleCenter).text = Volumes[i].ToString();
            _massBeakerFields[i] = MakeTimeField(row, new Vector2(0.19f, 0.12f), new Vector2(0.45f, 0.88f), out _massBeakerBgs[i], "g");
            _massWaterFields[i] = MakeTimeField(row, new Vector2(0.47f, 0.12f), new Vector2(0.69f, 0.88f), out _massWaterBgs[i], "g");
            _densityFields[i] = MakeTimeField(row, new Vector2(0.71f, 0.12f), new Vector2(0.98f, 0.88f), out _densityBgs[i], "ρ");
        }

        Label("Hint", parent, new Vector2(0.04f, 0.01f), new Vector2(0.96f, 0.12f), 14, FontStyle.Italic, _teal, TextAnchor.MiddleLeft).text = "1 ml = 1 cm³   •   type the readings yourself";
    }

    private void TableHeader(Transform parent, string text, float x0, float x1)
    {
        Label("h", parent, new Vector2(x0, 0f), new Vector2(x1, 1f), 13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter).text = text;
    }

    private InputField MakeTimeField(Transform parent, Vector2 min, Vector2 max, out Image background, string placeholder)
    {
        var go = Create("field", parent, min, max);
        background = go.AddComponent<Image>();
        background.color = Color.white;
        background.raycastTarget = true;
        var input = go.AddComponent<InputField>();
        var text = Label("text", go.transform, new Vector2(0.06f, 0f), new Vector2(0.94f, 1f), 16, FontStyle.Bold, _navy, TextAnchor.MiddleCenter);
        var ph = Label("ph", go.transform, new Vector2(0.06f, 0f), new Vector2(0.94f, 1f), 14, FontStyle.Italic, new Color(0.55f, 0.58f, 0.62f), TextAnchor.MiddleCenter);
        ph.text = placeholder;
        input.textComponent = text;
        input.placeholder = ph;
        input.contentType = InputField.ContentType.DecimalNumber;
        input.characterLimit = 8;
        input.lineType = InputField.LineType.SingleLine;
        input.interactable = false;
        return input;
    }

    private void SetShelfVisible(bool visible)
    {
        if (_shelf != null)
        {
            _shelf.gameObject.SetActive(visible);
        }
    }

    private static void Show(RectTransform rect, bool visible)
    {
        if (rect != null)
        {
            rect.gameObject.SetActive(visible);
        }
    }

    private Button MakeButton(Transform parent, Color color, out Text label)
    {
        var go = new GameObject("Button");
        go.transform.SetParent(parent, false);
        var image = go.AddComponent<Image>();
        image.color = color;
        var button = go.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = color * 1.08f;
        colors.pressedColor = color * 0.85f;
        colors.disabledColor = new Color(0.65f, 0.68f, 0.72f);
        button.colors = colors;
        label = Label("Label", go.transform, Vector2.zero, Vector2.one, 22, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        label.raycastTarget = false;
        return button;
    }

    private Text Label(string name, Transform parent, Vector2 min, Vector2 max, int size, FontStyle style, Color color, TextAnchor anchor)
    {
        var go = Create(name, parent, min, max);
        var text = go.AddComponent<Text>();
        text.font = _font;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private void MakeEquipmentCard(Transform parent, string id, string label, Color tint, Vector2 min, Vector2 max, string dragKind)
    {
        var card = Panel(id, parent, min, max, Color.Lerp(tint, Color.white, 0.78f));
        var iconGo = Create("Icon", card, new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.96f));
        var icon = iconGo.AddComponent<Image>();
        icon.sprite = GetSprite(id);
        icon.preserveAspect = true;
        icon.color = Color.white;
        icon.raycastTarget = false;
        icon.type = Image.Type.Simple;
        Label("n", card, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.30f), 15, FontStyle.Bold, _ink, TextAnchor.MiddleCenter).text = label;
        var drag = card.gameObject.AddComponent<DensityDragItem>();
        drag.ItemId = id;
        drag.Kind = dragKind;
    }

    private void AddTrayChip(string id, bool needed)
    {
        if (_trayGrid == null || !_trayIds.Add(id))
        {
            return;
        }

        if (_trayHint != null)
        {
            _trayHint.gameObject.SetActive(false);
        }

        var go = new GameObject("chip-" + id);
        go.transform.SetParent(_trayGrid, false);
        var bg = go.AddComponent<Image>();
        bg.color = needed ? new Color(0.93f, 0.98f, 0.93f) : new Color(1f, 0.90f, 0.90f);
        bg.raycastTarget = false;

        var iconGo = new GameObject("icon");
        iconGo.transform.SetParent(go.transform, false);
        var iconRect = iconGo.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.08f, 0.28f);
        iconRect.anchorMax = new Vector2(0.92f, 0.96f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        var icon = iconGo.AddComponent<Image>();
        icon.sprite = GetSprite(id);
        icon.preserveAspect = true;
        icon.color = Color.white;
        icon.raycastTarget = false;

        Label("n", go.transform, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.28f), 14, FontStyle.Bold, needed ? _navy : new Color(0.72f, 0.14f, 0.14f), TextAnchor.MiddleCenter).text = ShortName(id);
    }

    private static string ShortName(string id)
    {
        switch (id)
        {
            case "cyl100": return "100 ml";
            case "cyl250": return "250 ml";
            case "cyl500": return "500 ml";
            case "balance": return "Balance";
            case "beaker": return "Beaker";
            case "water": return "Water";
            case "thermo": return "Thermo";
            case "ruler": return "Ruler";
            case "spring": return "Spring";
            case "stopwatch": return "Watch";
            default: return id;
        }
    }

    private void AddSpriteFill(Transform parent, string id)
    {
        var go = Create("pic", parent, Vector2.zero, Vector2.one);
        var image = go.AddComponent<Image>();
        image.sprite = GetSprite(id);
        image.preserveAspect = true;
        image.color = Color.white;
        image.raycastTarget = false;
        image.type = Image.Type.Simple;
    }

    private Sprite GetSprite(string id)
    {
        Sprite sprite;
        if (_sprites.TryGetValue(id, out sprite) && sprite != null)
        {
            return sprite;
        }

        Texture2D tex = LoadPng("DensityEquipment", id);
        if (tex == null)
        {
            tex = LoadPng("PressureEquipment", id);
        }

        if (tex == null)
        {
            tex = MakeIcon(id);
        }

        sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = id;
        _sprites[id] = sprite;
        return sprite;
    }

    private static Texture2D LoadPng(string folder, string id)
    {
        string path = Path.Combine(Application.dataPath, "Resources", folder, id + ".png");
        if (File.Exists(path))
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.name = id;
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.LoadImage(File.ReadAllBytes(path), false);
            return tex;
        }

        return Resources.Load<Texture2D>(folder + "/" + id);
    }

    private Texture2D MakeIcon(string id)
    {
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color fg = _teal;
        switch (id)
        {
            case "water": fg = new Color(0.28f, 0.62f, 0.88f); break;
            case "beaker": fg = new Color(0.70f, 0.80f, 0.86f); break;
            case "thermo": fg = new Color(0.85f, 0.35f, 0.32f); break;
            case "ruler": fg = new Color(0.72f, 0.62f, 0.28f); break;
            case "spring": fg = new Color(0.55f, 0.35f, 0.70f); break;
            case "stopwatch": fg = new Color(0.20f, 0.55f, 0.45f); break;
        }

        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        for (int y = 18; y < size - 18; y++)
        {
            for (int x = 18; x < size - 18; x++)
            {
                pixels[y * size + x] = fg;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return tex;
    }

    private Transform Panel(string name, Transform parent, Vector2 min, Vector2 max, Color color)
    {
        var go = Create(name, parent, min, max);
        var image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return go.transform;
    }

    private static GameObject Create(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return go;
    }

    private static void SetButtonVisible(Button button, bool visible)
    {
        if (button != null)
        {
            button.gameObject.SetActive(visible);
        }
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }
}

public class DensityDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string ItemId;
    public string Kind;
    private Canvas _canvas;
    private RectTransform _rect;
    private Vector2 _start;
    private CanvasGroup _group;
    private bool _dropped;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        _group = gameObject.AddComponent<CanvasGroup>();
        var image = GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dropped = false;
        _start = _rect.anchoredPosition;
        _group.blocksRaycasts = false;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_canvas == null)
        {
            return;
        }

        _rect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public bool TryDropOn(DensityDropZone zone)
    {
        if (_dropped || zone == null || DensityWaterPracticalController.Instance == null)
        {
            return false;
        }

        if (Kind == "equipment" && zone.Kind == "tray")
        {
            DensityWaterPracticalController.Instance.OnEquipmentDropped(ItemId, true);
            _dropped = true;
            return true;
        }

        if (Kind == "setup" && IsLabZone(zone.Kind))
        {
            DensityWaterPracticalController.Instance.OnLabDropped(ItemId, zone.Kind);
            _dropped = true;
            return true;
        }

        return false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_dropped)
        {
            TryDropOn(FindZone(eventData));
        }

        _group.blocksRaycasts = true;
        _rect.anchoredPosition = _start;
    }

    private static DensityDropZone FindZone(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null)
        {
            var zone = eventData.pointerEnter.GetComponentInParent<DensityDropZone>();
            if (zone != null)
            {
                return zone;
            }
        }

        if (eventData.hovered == null)
        {
            return null;
        }

        for (int i = 0; i < eventData.hovered.Count; i++)
        {
            if (eventData.hovered[i] == null)
            {
                continue;
            }

            var found = eventData.hovered[i].GetComponentInParent<DensityDropZone>();
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static bool IsLabZone(string kind)
    {
        return kind == "lab" || kind == "pan" || kind == "beaker" || kind == "water";
    }
}

public class DensityDropZone : MonoBehaviour, IDropHandler
{
    public string Kind;

    public void OnDrop(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<DensityDragItem>()
            : null;
        if (item != null)
        {
            item.TryDropOn(this);
        }
    }
}
