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
/// Grade 9 Pressure Exerted by Solids (Figure 5.3 / Table 5.1).
/// Select equipment, drag-and-drop the lab, time the wire cutting soap.
/// </summary>
[DefaultExecutionOrder(-100)]
public class PressureSolidPracticalController : MonoBehaviour
{
    public static PressureSolidPracticalController Instance { get; private set; }

    private const int MaxBags = 4;
    private const float BagWeightN = 10f;

    private enum Stage
    {
        Intro,
        Select,
        Setup,
        Measure,
        Results
    }

    private Stage _stage = Stage.Intro;
    private bool _submitted;
    private int _score;
    private int _mistakes;
    private int _instance = 1;
    private int _bagsHung;
    private bool _plankPlaced;
    private bool _soapPlaced;
    private bool _wirePlaced;
    private bool _cutting;
    private bool _cutDone;
    private float _cutProgress;
    private float _cutDuration = 20f;
    private readonly float[] _recordedTimes = { -1f, -1f, -1f, -1f };
    private readonly HashSet<string> _selected = new HashSet<string>();

    private Font _font;
    private Text _titleText;
    private Text _stepText;
    private Text _instructionText;
    private Text _statusText;
    private Text _timerText;
    private Text _scoreText;
    private Text _watchText;
    private Transform _labRoot;
    private Transform _tray;
    private Transform _trayGrid;
    private Transform _trayBlock;
    private Transform _tableRoot;
    private InputField[] _timeFields;
    private Image[] _timeFieldBgs;
    private Text _trayHint;
    private Transform _shelf;
    private readonly HashSet<string> _trayIds = new HashSet<string>();
    private readonly Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
    private RectTransform _plankUi;
    private RectTransform _soapUi;
    private RectTransform _wireUi;
    private RectTransform _bagsUi;
    private RectTransform[] _bagSlots;
    private Button _nextButton;
    private Button _watchButton;
    private Button _recordButton;
    private Text _nextLabel;
    private Text _watchLabel;
    private Text _recordLabel;

    private readonly Color _navy = new Color(0.10f, 0.22f, 0.45f);
    private readonly Color _blue = new Color(0.18f, 0.47f, 0.80f);
    private readonly Color _panel = new Color(0.86f, 0.93f, 0.98f);
    private readonly Color _ink = new Color(0.10f, 0.12f, 0.16f);
    private readonly Color _wood = new Color(0.76f, 0.52f, 0.28f);
    private readonly Color _soap = new Color(0.98f, 0.84f, 0.28f);
    private readonly Color _bag = new Color(0.62f, 0.38f, 0.18f);
    private readonly string[] _needed = { "plank", "soap", "wire", "sandbags", "stopwatch" };

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
        Gizmos.color = new Color(0.18f, 0.47f, 0.80f, 0.9f);
        Gizmos.DrawWireCube(transform.position, new Vector3(10f, 6f, 0.1f));
    }

    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            return;
        }

        var rect = new Rect(20f, 20f, 520f, 70f);
        GUI.Box(rect, "");
        GUI.Label(
            new Rect(32f, 32f, 500f, 50f),
            "Pressure lab is empty here on purpose.\nPress Play — the drag-and-drop UI builds at runtime.");
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

    private void Update()
    {
        if (_submitted || !_cutting)
        {
            return;
        }

        _cutProgress += Time.unscaledDeltaTime / _cutDuration;
        float depth = Mathf.Clamp01(_cutProgress);
        if (_soapUi != null)
        {
            _soapUi.localScale = new Vector3(1f, Mathf.Max(0.18f, 1f - depth * 0.75f), 1f);
        }

        if (_watchText != null)
        {
            _watchText.text = "Stopwatch  " + (_cutProgress * _cutDuration).ToString("0.0", CultureInfo.InvariantCulture) + " s";
        }

        if (depth >= 1f)
        {
            _cutting = false;
            _cutDone = true;
            _statusText.text = "The wire passed through the soap. Type that time in Table 5.1, then Record.";
            RefreshTable();
            RefreshButtons();
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
        cam.backgroundColor = new Color(0.90f, 0.95f, 0.98f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    private void ShowIntro()
    {
        _stage = Stage.Intro;
        _stepText.text = "Welcome";
        _instructionText.text =
            "Pressure Exerted by Solids  •  Figure 5.3\n"
            + "A thin wire and sandbags cut through soap. More weight → more pressure → less time.";
        _statusText.text = "Tap Next, then pick only the equipment this lab needs.";
        _watchText.text = "";
        SetLabVisible(false);
        SetShelfVisible(false);
        ShowTrayOrTable();
        RefreshButtons();
        RefreshTable();
        RefreshScore();
    }

    private void ShowSelect()
    {
        _stage = Stage.Select;
        _stepText.text = "Step 1  •  Choose equipment";
        _instructionText.text =
            "Drag the correct items into Your tray. Wrong tools lose marks.\n"
            + "You need: cakes of soap, thin metal wire, 10 N sandbags, wooden plank, stopwatch.";
        _statusText.text = "Hint: a Newton balance is for Force, not this pressure lab.";
        SetLabVisible(false);
        SetShelfVisible(true);
        ShowTrayOrTable();
        RefreshButtons();
    }

    private void ShowSetup()
    {
        _stage = Stage.Setup;
        _stepText.text = "Step 2  •  Build Figure 5.3";
        _instructionText.text =
            "Drop onto the tables: plank across both tables, soap on the plank, then the wire over the soap.";
        _statusText.text = "Put the soap on the plank that sits on the tables.";
        SetShelfVisible(false);
        SetLabVisible(true);
        ShowTrayOrTable();
        RefreshButtons();
    }

    private void ShowMeasure()
    {
        _stage = Stage.Measure;
        _bagsHung = 0;
        _cutting = false;
        _cutDone = false;
        _cutProgress = 0f;
        if (_soapUi != null)
        {
            _soapUi.localScale = Vector3.one;
        }

        _stepText.text = "Step 3  •  Instance 0" + _instance;
        _instructionText.text =
            "Hang " + _instance + " sandbag(s) (" + (_instance * BagWeightN).ToString("0") + " N), start the stopwatch, then type the time in Table 5.1.";
        _statusText.text = "Use a fresh cake of soap. You fill Table 5.1 yourself.";
        _watchText.text = "Stopwatch  0.0 s";
        ShowTrayOrTable();
        RefreshButtons();
        RefreshTable();
    }

    private void ShowResults()
    {
        _stage = Stage.Results;
        _stepText.text = "Table 5.1  •  Finish";
        _instructionText.text =
            "Check Table 5.1. More sandbags → more pressure → the soap cuts in less time.";
        _statusText.text = "Submit to save your marks to your profile.";
        ShowTrayOrTable();
        RefreshButtons();
        RefreshTable();
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
                AddScore(8);
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

    public void OnSetupDropped(string slot)
    {
        if (_submitted || _stage != Stage.Setup)
        {
            return;
        }

        if (slot == "plank" && !_plankPlaced)
        {
            _plankPlaced = true;
            Show(_plankUi, true);
            AddScore(12);
            _statusText.text = "Plank is across the tables.";
        }
        else if (slot == "soap")
        {
            if (!_plankPlaced)
            {
                Penalize("Place the plank first.");
                return;
            }

            if (!_soapPlaced)
            {
                _soapPlaced = true;
                Show(_soapUi, true);
                AddScore(12);
                _statusText.text = "Soap is over the gap.";
            }
        }
        else if (slot == "wire")
        {
            if (!_soapPlaced)
            {
                Penalize("Place the soap before the wire.");
                return;
            }

            if (!_wirePlaced)
            {
                _wirePlaced = true;
                Show(_wireUi, true);
                AddScore(12);
                _statusText.text = "Wire is looped over the soap. Next: hang sandbags.";
            }
        }
        else
        {
            Penalize("Drop that on the matching place in Figure 5.3.");
            return;
        }

        RefreshButtons();
    }

    public void OnBagDropped()
    {
        if (_submitted || _stage != Stage.Measure || _cutting || _cutDone)
        {
            return;
        }

        if (_bagsHung >= _instance)
        {
            Penalize("This instance needs only " + _instance + " bag(s).");
            return;
        }

        _bagsHung++;
        Show(_bagsUi, true);
        StretchBags();
        AddScore(4);
        _statusText.text = _bagsHung + " bag(s) hanging. Weight = " + (_bagsHung * BagWeightN).ToString("0") + " N.";
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

            ShowSetup();
            return;
        }

        if (_stage == Stage.Setup)
        {
            if (!_plankPlaced || !_soapPlaced || !_wirePlaced)
            {
                Penalize("Finish Figure 5.3: plank, soap, then wire.");
                return;
            }

            ShowMeasure();
            return;
        }

        if (_stage == Stage.Measure)
        {
            if (_recordedTimes[_instance - 1] < 0f)
            {
                Penalize("Record the cutting time before the next instance.");
                return;
            }

            if (_instance < MaxBags)
            {
                _instance++;
                ShowMeasure();
                return;
            }

            ShowResults();
            return;
        }

        if (_stage == Stage.Results)
        {
            Finish(true);
        }
    }

    private void OnWatch()
    {
        if (_submitted || _stage != Stage.Measure || _cutting || _cutDone)
        {
            return;
        }

        if (_bagsHung != _instance)
        {
            Penalize("Hang exactly " + _instance + " sandbag(s) before timing.");
            return;
        }

        _cutDuration = 20f / _instance;
        _cutProgress = 0f;
        _cutting = true;
        _statusText.text = "Watch the wire cut through. Greater weight → shorter time.";
        RefreshButtons();
    }

    private void OnRecord()
    {
        if (_submitted || _stage != Stage.Measure || !_cutDone)
        {
            if (_stage == Stage.Measure)
            {
                Penalize("Wait until the wire has cut through, then type the time in Table 5.1.");
            }

            return;
        }

        if (_recordedTimes[_instance - 1] >= 0f)
        {
            return;
        }

        InputField field = _timeFields != null ? _timeFields[_instance - 1] : null;
        string raw = field != null ? field.text.Trim() : "";
        float entered;
        if (string.IsNullOrEmpty(raw)
            || (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out entered)
                && !float.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out entered)))
        {
            Penalize("Type the stopwatch time (seconds) in Table 5.1 first.");
            return;
        }

        if (entered <= 0f || entered > 120f)
        {
            Penalize("That time is not realistic. Use the stopwatch reading.");
            return;
        }

        _recordedTimes[_instance - 1] = entered;
        float error = Mathf.Abs(entered - _cutDuration);
        if (error <= 1.0f)
        {
            AddScore(8);
            _statusText.text = "Table 5.1 instance 0" + _instance + " saved. Close to the stopwatch.";
        }
        else if (error <= 2.5f)
        {
            AddScore(4);
            _statusText.text = "Saved, but check the stopwatch more carefully next time.";
        }
        else
        {
            AddScore(1);
            _statusText.text = "Saved. The stopwatch showed " + _cutDuration.ToString("0.0", CultureInfo.InvariantCulture) + " s.";
        }

        if (field != null)
        {
            field.text = entered.ToString("0.0", CultureInfo.InvariantCulture);
        }

        RefreshTable();
        RefreshButtons();
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

    private void RefreshButtons()
    {
        bool selectReady = _stage == Stage.Select && _selected.Count >= _needed.Length;
        bool setupReady = _stage == Stage.Setup && _plankPlaced && _soapPlaced && _wirePlaced;
        bool measureReady = _stage == Stage.Measure && _recordedTimes[_instance - 1] >= 0f;
        _nextButton.interactable = !_submitted && (
            _stage == Stage.Intro
            || selectReady
            || setupReady
            || measureReady
            || _stage == Stage.Results);
        _watchButton.interactable = !_submitted && _stage == Stage.Measure && !_cutting && !_cutDone && _bagsHung == _instance;
        _recordButton.interactable = !_submitted && _stage == Stage.Measure && _cutDone && _recordedTimes[_instance - 1] < 0f;

        if (_submitted)
        {
            _nextLabel.text = "Completed";
            _nextButton.interactable = false;
            var done = _nextButton.colors;
            done.disabledColor = new Color(0.15f, 0.52f, 0.38f);
            _nextButton.colors = done;
            SetButtonVisible(_watchButton, false);
            SetButtonVisible(_recordButton, false);
            return;
        }

        if (_stage == Stage.Intro) _nextLabel.text = "Start lab";
        else if (_stage == Stage.Select) _nextLabel.text = "Go to setup";
        else if (_stage == Stage.Setup) _nextLabel.text = "Hang bags";
        else if (_stage == Stage.Measure && _instance < MaxBags) _nextLabel.text = "Next instance";
        else if (_stage == Stage.Measure) _nextLabel.text = "Open table";
        else _nextLabel.text = "Submit";

        _watchLabel.text = "Start stopwatch";
        _recordLabel.text = "Save in Table 5.1";
        SetButtonVisible(_watchButton, _stage == Stage.Measure);
        SetButtonVisible(_recordButton, _stage == Stage.Measure);
    }

    private void RefreshTable()
    {
        if (_timeFields == null)
        {
            return;
        }

        for (int i = 0; i < MaxBags; i++)
        {
            bool filled = _recordedTimes[i] >= 0f;
            bool active = !_submitted && _stage == Stage.Measure && i == _instance - 1 && _cutDone && !filled;
            _timeFields[i].interactable = active;
            if (filled)
            {
                _timeFields[i].text = _recordedTimes[i].ToString("0.0", CultureInfo.InvariantCulture);
            }

            if (_timeFieldBgs[i] != null)
            {
                _timeFieldBgs[i].color = filled
                    ? new Color(0.82f, 0.95f, 0.86f)
                    : active
                        ? new Color(1f, 0.96f, 0.72f)
                        : Color.white;
            }
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
        int recorded = 0;
        for (int i = 0; i < _recordedTimes.Length; i++)
        {
            if (_recordedTimes[i] >= 0f)
            {
                recorded++;
            }
        }

        bool passed = _score >= 50 && recorded >= 2;
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

        string measurements =
            "{\"instance1\":" + Num(_recordedTimes[0])
            + ",\"instance2\":" + Num(_recordedTimes[1])
            + ",\"instance3\":" + Num(_recordedTimes[2])
            + ",\"instance4\":" + Num(_recordedTimes[3])
            + ",\"selected\":" + _selected.Count
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

    private static string Num(float value)
    {
        return value < 0f ? "null" : value.ToString("0.0", CultureInfo.InvariantCulture);
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
            case "plank": return "Wooden plank";
            case "soap": return "Cake of soap";
            case "wire": return "Thin metal wire";
            case "sandbags": return "10 N sandbags";
            case "stopwatch": return "Stopwatch";
            case "balance": return "Newton spring balance";
            case "spring": return "Helical spring";
            case "beaker": return "Beaker";
            default: return id;
        }
    }

    private void BuildCanvas()
    {
        EnsureEventSystem();
        var canvasGo = new GameObject("PressureUI");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var top = Panel("TopBar", canvasGo.transform, new Vector2(0.02f, 0.88f), new Vector2(0.98f, 0.98f), _panel);
        _titleText = Label("Title", top, new Vector2(0.02f, 0.48f), new Vector2(0.62f, 0.96f), 36, FontStyle.Bold, _navy, TextAnchor.MiddleLeft);
        _titleText.text = "Pressure Exerted by Solids";
        _scoreText = Label("Score", top, new Vector2(0.62f, 0.48f), new Vector2(0.82f, 0.96f), 28, FontStyle.Bold, _blue, TextAnchor.MiddleCenter);
        _timerText = Label("Timer", top, new Vector2(0.82f, 0.48f), new Vector2(0.98f, 0.96f), 36, FontStyle.Bold, _navy, TextAnchor.MiddleRight);
        _stepText = Label("Step", top, new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.50f), 26, FontStyle.Bold, _blue, TextAnchor.MiddleLeft);

        _labRoot = Panel("Lab", canvasGo.transform, new Vector2(0.02f, 0.42f), new Vector2(0.62f, 0.86f), new Color(0.93f, 0.96f, 0.99f)).transform;
        BuildLabFigure(_labRoot);

        _shelf = Panel("Shelf", canvasGo.transform, new Vector2(0.02f, 0.42f), new Vector2(0.62f, 0.86f), new Color(0.93f, 0.96f, 0.99f)).transform;
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
        _tray.gameObject.AddComponent<PressureDropZone>().Kind = "tray";
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
        BuildTable51(_tableRoot);

        var bottom = Panel("Bottom", canvasGo.transform, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.40f), _panel);
        _instructionText = Label("Instruction", bottom, new Vector2(0.02f, 0.72f), new Vector2(0.98f, 0.98f), 26, FontStyle.Normal, _ink, TextAnchor.UpperLeft);
        _statusText = Label("Status", bottom, new Vector2(0.02f, 0.58f), new Vector2(0.98f, 0.72f), 24, FontStyle.Bold, _navy, TextAnchor.MiddleLeft);
        _watchText = Label("Watch", bottom, new Vector2(0.02f, 0.46f), new Vector2(0.50f, 0.58f), 26, FontStyle.Bold, _blue, TextAnchor.MiddleLeft);

        var actions = Create("Actions", bottom, new Vector2(0.54f, 0.08f), new Vector2(0.98f, 0.52f));
        var layout = actions.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.padding = new RectOffset(6, 6, 6, 6);

        _watchButton = MakeButton(actions.transform, new Color(0.15f, 0.52f, 0.38f), out _watchLabel);
        _watchButton.onClick.AddListener(OnWatch);
        _recordButton = MakeButton(actions.transform, new Color(0.90f, 0.48f, 0.12f), out _recordLabel);
        _recordButton.onClick.AddListener(OnRecord);
        _nextButton = MakeButton(actions.transform, _blue, out _nextLabel);
        _nextButton.onClick.AddListener(OnNext);
    }

    private void BuildShelf(Transform parent)
    {
        string[] ids = { "plank", "soap", "wire", "sandbags", "stopwatch", "balance", "spring", "beaker" };
        Color[] colors =
        {
            _wood, _soap, new Color(0.55f, 0.58f, 0.62f), _bag, new Color(0.20f, 0.55f, 0.45f),
            new Color(0.55f, 0.35f, 0.70f), new Color(0.35f, 0.62f, 0.80f), new Color(0.75f, 0.78f, 0.82f)
        };

        for (int i = 0; i < ids.Length; i++)
        {
            int col = i % 4;
            int row = i / 4;
            float x0 = 0.04f + col * 0.24f;
            float y1 = 0.92f - row * 0.46f;
            MakeEquipmentCard(
                parent,
                ids[i],
                NiceName(ids[i]),
                colors[i],
                new Vector2(x0, y1 - 0.40f),
                new Vector2(x0 + 0.22f, y1),
                "equipment");
        }
    }

    private void BuildLabFigure(Transform parent)
    {
        var tableColor = new Color(0.42f, 0.28f, 0.14f);
        var tableTop = new Color(0.58f, 0.40f, 0.20f);
        Panel("TableL", parent, new Vector2(0.02f, 0.06f), new Vector2(0.34f, 0.40f), tableColor);
        Panel("TableR", parent, new Vector2(0.66f, 0.06f), new Vector2(0.98f, 0.40f), tableColor);
        Panel("TopL", parent, new Vector2(0.02f, 0.36f), new Vector2(0.34f, 0.41f), tableTop);
        Panel("TopR", parent, new Vector2(0.66f, 0.36f), new Vector2(0.98f, 0.41f), tableTop);
        Label("gap", parent, new Vector2(0.36f, 0.22f), new Vector2(0.64f, 0.34f), 18, FontStyle.Italic, _ink, TextAnchor.MiddleCenter).text = "gap";

        _plankUi = Panel("Plank", parent, new Vector2(0.05f, 0.37f), new Vector2(0.95f, 0.50f), _wood).GetComponent<RectTransform>();
        Panel("PlankEdge", _plankUi, new Vector2(0f, 0f), new Vector2(1f, 0.28f), new Color(0.52f, 0.32f, 0.14f));
        Label("plankName", _plankUi, new Vector2(0.03f, 0.30f), new Vector2(0.28f, 0.95f), 18, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft).text = "plank";

        _soapUi = Panel("Soap", parent, new Vector2(0.36f, 0.42f), new Vector2(0.64f, 0.58f), _soap).GetComponent<RectTransform>();
        AddSpriteFill(_soapUi, "soap");

        _wireUi = Create("Wire", parent, new Vector2(0.39f, 0.12f), new Vector2(0.61f, 0.74f)).GetComponent<RectTransform>();
        Panel("WireLoop", _wireUi, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.96f), new Color(0.38f, 0.40f, 0.45f));
        Panel("WireStem", _wireUi, new Vector2(0.46f, 0.00f), new Vector2(0.54f, 0.90f), new Color(0.38f, 0.40f, 0.45f));

        _bagsUi = Create("Bags", parent, new Vector2(0.32f, 0.01f), new Vector2(0.68f, 0.20f)).GetComponent<RectTransform>();
        _bagSlots = new RectTransform[MaxBags];
        for (int i = 0; i < MaxBags; i++)
        {
            float x0 = i * 0.25f;
            var slot = Panel("bag" + i, _bagsUi, new Vector2(x0 + 0.01f, 0f), new Vector2(x0 + 0.24f, 1f), new Color(1f, 1f, 1f, 0f)).GetComponent<RectTransform>();
            AddSpriteFill(slot, "sandbags");
            _bagSlots[i] = slot;
            Show(slot, false);
        }

        Show(_plankUi, false);
        Show(_soapUi, false);
        Show(_wireUi, false);
        Show(_bagsUi, false);

        MakeDrop(parent, "plank", new Vector2(0.04f, 0.34f), new Vector2(0.96f, 0.54f));
        MakeDrop(parent, "soap", new Vector2(0.34f, 0.34f), new Vector2(0.66f, 0.62f));
        MakeDrop(parent, "wire", new Vector2(0.38f, 0.12f), new Vector2(0.62f, 0.76f));
        MakeDrop(parent, "bag", new Vector2(0.30f, 0.00f), new Vector2(0.70f, 0.22f));
        MakeDrop(parent, "lab", new Vector2(0.01f, 0.00f), new Vector2(0.99f, 0.76f));

        var kit = Panel("Kit", parent, new Vector2(0.02f, 0.78f), new Vector2(0.98f, 0.98f), Color.white);
        MakeKitItem(kit, "plank", "Plank", _wood, 0f);
        MakeKitItem(kit, "soap", "Soap", _soap, 0.25f);
        MakeKitItem(kit, "wire", "Wire", new Color(0.45f, 0.48f, 0.52f), 0.50f);
        MakeKitItem(kit, "bag", "Sandbag 10 N", _bag, 0.75f);
    }

    private void MakeKitItem(Transform parent, string id, string label, Color color, float x)
    {
        MakeEquipmentCard(
            parent,
            id,
            label,
            color,
            new Vector2(0.02f + x, 0.10f),
            new Vector2(0.24f + x, 0.90f),
            "setup");
    }

    private void MakeDrop(Transform parent, string kind, Vector2 min, Vector2 max)
    {
        var zone = Create("drop-" + kind, parent, min, max);
        var image = zone.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.04f);
        var drop = zone.AddComponent<PressureDropZone>();
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
            _tableRoot.gameObject.SetActive(_stage == Stage.Setup || _stage == Stage.Measure || _stage == Stage.Results);
        }
    }

    private void BuildTable51(Transform parent)
    {
        Label("TableTitle", parent, new Vector2(0.04f, 0.88f), new Vector2(0.96f, 0.98f), 22, FontStyle.Bold, _navy, TextAnchor.MiddleLeft).text = "Table 5.1";
        Label("TableSub", parent, new Vector2(0.04f, 0.80f), new Vector2(0.96f, 0.88f), 16, FontStyle.Italic, _ink, TextAnchor.MiddleLeft).text = "Time to cut through the soap";
        Label("Hint", parent, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.12f), 16, FontStyle.Normal, _blue, TextAnchor.MiddleLeft).text = "Type the stopwatch time (s) yourself.";

        var header = Panel("Header", parent, new Vector2(0.04f, 0.70f), new Vector2(0.96f, 0.80f), _navy);
        TableHeader(header, "Inst", 0.00f, 0.16f);
        TableHeader(header, "Bags", 0.16f, 0.36f);
        TableHeader(header, "Weight", 0.36f, 0.58f);
        TableHeader(header, "Time / s", 0.58f, 1.00f);

        _timeFields = new InputField[MaxBags];
        _timeFieldBgs = new Image[MaxBags];
        for (int i = 0; i < MaxBags; i++)
        {
            float y1 = 0.70f - i * 0.14f;
            float y0 = y1 - 0.13f;
            var row = Panel("row" + i, parent, new Vector2(0.04f, y0), new Vector2(0.96f, y1), i % 2 == 0 ? new Color(0.95f, 0.97f, 1f) : Color.white);
            Label("i", row, new Vector2(0.00f, 0f), new Vector2(0.16f, 1f), 18, FontStyle.Bold, _ink, TextAnchor.MiddleCenter).text = (i + 1).ToString("00");
            Label("b", row, new Vector2(0.16f, 0f), new Vector2(0.36f, 1f), 18, FontStyle.Normal, _ink, TextAnchor.MiddleCenter).text = (i + 1).ToString();
            Label("w", row, new Vector2(0.36f, 0f), new Vector2(0.58f, 1f), 18, FontStyle.Normal, _ink, TextAnchor.MiddleCenter).text = ((i + 1) * 10) + " N";
            _timeFields[i] = MakeTimeField(row, new Vector2(0.60f, 0.12f), new Vector2(0.98f, 0.88f), out _timeFieldBgs[i]);
        }
    }

    private void TableHeader(Transform parent, string text, float x0, float x1)
    {
        Label("h", parent, new Vector2(x0, 0f), new Vector2(x1, 1f), 16, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter).text = text;
    }

    private InputField MakeTimeField(Transform parent, Vector2 min, Vector2 max, out Image background)
    {
        var go = Create("time", parent, min, max);
        background = go.AddComponent<Image>();
        background.color = Color.white;
        background.raycastTarget = true;
        var input = go.AddComponent<InputField>();
        var text = Label("text", go.transform, new Vector2(0.06f, 0f), new Vector2(0.94f, 1f), 20, FontStyle.Bold, _navy, TextAnchor.MiddleCenter);
        var placeholder = Label("ph", go.transform, new Vector2(0.06f, 0f), new Vector2(0.94f, 1f), 18, FontStyle.Italic, new Color(0.55f, 0.58f, 0.62f), TextAnchor.MiddleCenter);
        placeholder.text = "type s";
        input.textComponent = text;
        input.placeholder = placeholder;
        input.contentType = InputField.ContentType.DecimalNumber;
        input.characterLimit = 6;
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

    private void StretchBags()
    {
        if (_bagSlots == null)
        {
            return;
        }

        for (int i = 0; i < _bagSlots.Length; i++)
        {
            Show(_bagSlots[i], i < _bagsHung);
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
        label = Label("Label", go.transform, Vector2.zero, Vector2.one, 26, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
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
        Label("n", card, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.30f), 18, FontStyle.Bold, _ink, TextAnchor.MiddleCenter).text = label;
        var drag = card.gameObject.AddComponent<PressureDragItem>();
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

        Label("n", go.transform, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.28f), 16, FontStyle.Bold, needed ? _navy : new Color(0.72f, 0.14f, 0.14f), TextAnchor.MiddleCenter).text = NiceName(id);
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
        string key = id == "bag" ? "sandbags" : id;
        Sprite sprite;
        if (_sprites.TryGetValue(key, out sprite) && sprite != null)
        {
            return sprite;
        }

        Texture2D tex = null;
        string path = Path.Combine(Application.dataPath, "Resources", "PressureEquipment", key + ".png");
        if (File.Exists(path))
        {
            tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.name = key;
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.LoadImage(File.ReadAllBytes(path), false);
        }

        if (tex == null)
        {
            tex = Resources.Load<Texture2D>("PressureEquipment/" + key);
        }

        if (tex == null)
        {
            tex = MakeIcon(key);
        }

        sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = key;
        _sprites[key] = sprite;
        return sprite;
    }

    private Texture2D MakeIcon(string id)
    {
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color fg = _wood;
        switch (id)
        {
            case "soap": fg = _soap; break;
            case "wire": fg = new Color(0.55f, 0.58f, 0.62f); break;
            case "sandbags": fg = _bag; break;
            case "stopwatch": fg = new Color(0.20f, 0.55f, 0.45f); break;
            case "balance": fg = new Color(0.55f, 0.35f, 0.70f); break;
            case "spring": fg = new Color(0.35f, 0.62f, 0.80f); break;
            case "beaker": fg = new Color(0.55f, 0.70f, 0.82f); break;
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

public class PressureDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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

    public bool TryDropOn(PressureDropZone zone)
    {
        if (_dropped || zone == null || PressureSolidPracticalController.Instance == null)
        {
            return false;
        }

        if (Kind == "equipment" && zone.Kind == "tray")
        {
            PressureSolidPracticalController.Instance.OnEquipmentDropped(ItemId, true);
            _dropped = true;
            return true;
        }

        if (Kind == "setup" && IsLabZone(zone.Kind))
        {
            if (ItemId == "bag")
            {
                PressureSolidPracticalController.Instance.OnBagDropped();
            }
            else
            {
                PressureSolidPracticalController.Instance.OnSetupDropped(ItemId);
            }

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

    private static PressureDropZone FindZone(PointerEventData eventData)
    {
        if (eventData.pointerEnter != null)
        {
            var zone = eventData.pointerEnter.GetComponentInParent<PressureDropZone>();
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

            var zone = eventData.hovered[i].GetComponentInParent<PressureDropZone>();
            if (zone != null)
            {
                return zone;
            }
        }

        return null;
    }

    private static bool IsLabZone(string kind)
    {
        return kind == "lab" || kind == "plank" || kind == "soap" || kind == "wire" || kind == "bag";
    }
}

public class PressureDropZone : MonoBehaviour, IDropHandler
{
    public string Kind;

    public void OnDrop(PointerEventData eventData)
    {
        var item = eventData.pointerDrag != null
            ? eventData.pointerDrag.GetComponent<PressureDragItem>()
            : null;
        if (item != null)
        {
            item.TryDropOn(this);
        }
    }
}
