using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Grade 9 Activity 4.1: measure force with a Newton spring balance.
/// Step 1 hang a stone, step 2 pull a wooden block, step 3 stretch a helical spring 10 cm.
/// </summary>
[DefaultExecutionOrder(-100)]
public class ForcePracticalController : MonoBehaviour
{
    public static ForcePracticalController Instance { get; private set; }

    private const float StoneWeightN = 2.5f;
    private const float BlockStartForceN = 3.2f;
    private const float SpringConstant = 40f;
    private const float TargetExtensionM = 0.10f;

    private enum Stage
    {
        Intro,
        HangStone,
        PullBlock,
        StretchSpring,
        Results
    }

    private Stage _stage = Stage.Intro;
    private bool _submitted;
    private int _mistakes;
    private bool _stoneHung;
    private bool _blockMoved;
    private bool _springReached;
    private bool _pullHeld;
    private float _liveForce;
    private float _stoneN = -1f;
    private float _blockN = -1f;
    private float _springN = -1f;

    private Transform _stageRoot;
    private Transform _block;
    private Transform _spring;
    private Transform _stone;
    private TextMesh _balanceReading;
    private Vector3 _blockStart;

    private Font _font;
    private Text _titleText;
    private Text _stepText;
    private Text _instructionText;
    private Text _readingText;
    private Text _statusText;
    private Text _timerText;
    private Text _tableText;
    private Button _actionButton;
    private Button _recordButton;
    private Button _nextButton;
    private Text _actionLabel;
    private Text _recordLabel;
    private Text _nextLabel;

    private readonly Color _navy = new Color(0.10f, 0.22f, 0.45f);
    private readonly Color _blue = new Color(0.18f, 0.47f, 0.80f);
    private readonly Color _panel = new Color(0.84f, 0.92f, 0.98f);
    private readonly Color _ink = new Color(0.10f, 0.12f, 0.16f);
    private readonly Color _wood = new Color(0.72f, 0.48f, 0.24f);
    private readonly Color _stoneColor = new Color(0.45f, 0.48f, 0.50f);
    private readonly Color _yellow = new Color(0.96f, 0.82f, 0.22f);

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

    private void Start()
    {
        TimerManager.OnExpired += CompleteOnTimeout;
        TimerManager.OnTick += UpdateTimer;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit > 0)
        {
            TimerManager.EnsureInstance()?.StartTimer(limit);
        }

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
        if (_submitted)
        {
            return;
        }

        if (_stage == Stage.PullBlock || _stage == Stage.StretchSpring)
        {
            UpdatePull();
        }
    }

    private void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        cam.orthographic = true;
        cam.orthographicSize = 4.2f;
        cam.transform.position = new Vector3(0f, 1.15f, -10f);
        cam.backgroundColor = new Color(0.93f, 0.96f, 0.99f);
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    private void ClearStage()
    {
        if (_stageRoot != null)
        {
            Destroy(_stageRoot.gameObject);
        }

        _stageRoot = new GameObject("LabStage").transform;
        _block = null;
        _spring = null;
        _stone = null;
        _balanceReading = null;
        _liveForce = 0f;
        _pullHeld = false;
    }

    private GameObject Box(string name, Vector3 position, Vector3 scale, Color color, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_stageRoot, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = WhiteSprite();
        renderer.color = color;
        renderer.sortingOrder = order;
        return go;
    }

    private static Sprite WhiteSprite()
    {
        var tex = Texture2D.whiteTexture;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
    }

    private TextMesh WorldLabel(string name, Vector3 position, string text, int fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_stageRoot, false);
        go.transform.position = position;
        var mesh = go.AddComponent<TextMesh>();
        mesh.text = text;
        mesh.fontSize = fontSize;
        mesh.characterSize = 0.06f;
        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;
        mesh.color = color;
        mesh.font = _font;
        var renderer = go.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 20;
        if (_font != null)
        {
            renderer.sharedMaterial = _font.material;
        }
        return mesh;
    }

    private void BuildHangStone()
    {
        ClearStage();
        Box("WallMount", new Vector3(0f, 3.55f, 0f), new Vector3(1.4f, 0.25f, 1f), new Color(0.35f, 0.38f, 0.42f), 1);
        BuildSpringBalance(new Vector3(0f, 2.5f, 0f), false);
        Box("Thread", new Vector3(0f, 1.35f, 0f), new Vector3(0.06f, 0.7f, 1f), Color.black, 3);
        _stone = Box("Stone", new Vector3(0f, 0.7f, 0f), new Vector3(0.75f, 0.58f, 1f), _stoneColor, 4).transform;
        _stone.gameObject.SetActive(false);
        WorldLabel("Fig", new Vector3(0f, 0.05f, 0f), "Figure 4.2", 48, _navy);
    }

    private void BuildPullBlock()
    {
        ClearStage();
        Box("Table", new Vector3(0f, 0.45f, 0f), new Vector3(10f, 0.38f, 1f), _wood, 1);
        _block = Box("WoodenBlock", new Vector3(-2.2f, 1.1f, 0f), new Vector3(1.7f, 0.9f, 1f), new Color(0.78f, 0.55f, 0.28f), 3).transform;
        _blockStart = _block.position;
        Box("Hook", new Vector3(-1.2f, 1.1f, 0f), new Vector3(0.2f, 0.2f, 1f), new Color(0.55f, 0.55f, 0.58f), 4);
        BuildSpringBalance(new Vector3(0.45f, 1.1f, 0f), true);
        WorldLabel("Fig", new Vector3(0f, 0.0f, 0f), "Figure 4.3  — pull until the block just moves", 42, _navy);
    }

    private void BuildStretchSpring()
    {
        ClearStage();
        Box("Table", new Vector3(0f, 0.45f, 0f), new Vector3(10f, 0.38f, 1f), _wood, 1);
        _block = Box("WoodenBlock", new Vector3(-2.4f, 1.1f, 0f), new Vector3(1.7f, 0.9f, 1f), new Color(0.78f, 0.55f, 0.28f), 3).transform;
        Box("GClampArm", new Vector3(-3.15f, 0.9f, 0f), new Vector3(0.24f, 1.25f, 1f), new Color(0.82f, 0.18f, 0.16f), 5);
        Box("GClampTop", new Vector3(-2.75f, 1.45f, 0f), new Vector3(0.75f, 0.18f, 1f), new Color(0.82f, 0.18f, 0.16f), 5);
        Box("GClampBot", new Vector3(-2.75f, 0.35f, 0f), new Vector3(0.75f, 0.18f, 1f), new Color(0.82f, 0.18f, 0.16f), 5);
        _spring = Box("HelicalSpring", new Vector3(-0.5f, 1.1f, 0f), new Vector3(1.7f, 0.3f, 1f), new Color(0.75f, 0.78f, 0.82f), 4).transform;
        BuildSpringBalance(new Vector3(1.45f, 1.1f, 0f), true);
        WorldLabel("Fig", new Vector3(0f, 0.0f, 0f), "Figure 4.5  — stretch the spring by 10 cm", 42, _navy);
    }

    private void BuildSpringBalance(Vector3 position, bool horizontal)
    {
        Vector3 bodyScale = horizontal ? new Vector3(2.1f, 0.55f, 1f) : new Vector3(0.55f, 1.7f, 1f);
        Box("BalanceBody", position, bodyScale, _yellow, 6);
        Box("BalanceRing", position + (horizontal ? new Vector3(1.2f, 0f, 0f) : new Vector3(0f, 1.05f, 0f)),
            new Vector3(0.28f, 0.28f, 1f), new Color(0.55f, 0.56f, 0.60f), 7);
        Box("BalanceHook", position + (horizontal ? new Vector3(-1.2f, 0f, 0f) : new Vector3(0f, -1.0f, 0f)),
            new Vector3(0.16f, 0.22f, 1f), new Color(0.45f, 0.46f, 0.50f), 7);
        _balanceReading = WorldLabel("BalanceReading", position, "0.0 N", 72, _ink);
    }

    private void SetBalanceReading(float newtons)
    {
        string text = newtons.ToString("0.0", CultureInfo.InvariantCulture) + " N";
        if (_balanceReading != null)
        {
            _balanceReading.text = text;
        }

        if (_readingText != null)
        {
            _readingText.text = "Spring balance reading:  " + text;
        }
    }

    private void UpdatePull()
    {
        if (_pullHeld)
        {
            _liveForce = Mathf.Min(12f, _liveForce + Time.deltaTime * 3.2f);
        }
        else
        {
            _liveForce = Mathf.Max(0f, _liveForce - Time.deltaTime * 4.5f);
        }

        SetBalanceReading(_liveForce);

        if (_stage == Stage.PullBlock && _block != null)
        {
            if (_liveForce >= BlockStartForceN)
            {
                _blockMoved = true;
                float x = _blockStart.x + Mathf.Min(1.6f, (_liveForce - BlockStartForceN) * 0.45f);
                _block.position = new Vector3(x, _blockStart.y, 0f);
                _statusText.text = "The wooden block just started to move. Record this force.";
            }
            else
            {
                _block.position = _blockStart;
            }
        }

        if (_stage == Stage.StretchSpring && _spring != null)
        {
            float extension = _liveForce / SpringConstant;
            float width = 1.7f + extension * 8f;
            _spring.localScale = new Vector3(width, 0.3f, 1f);
            _spring.position = new Vector3(-0.5f + (width - 1.7f) * 0.5f, 1.1f, 0f);
            float cm = extension * 100f;
            if (cm >= 9.5f)
            {
                _springReached = true;
                _statusText.text = "Extension is " + cm.ToString("0.0") + " cm. Record the force now.";
            }
            else
            {
                _statusText.text = "Keep pulling until the helical spring extends by 10 cm. Now " + cm.ToString("0.0") + " cm.";
            }
        }

        RefreshButtons();
    }

    private void ShowIntro()
    {
        _stage = Stage.Intro;
        ClearStage();
        _titleText.text = "Activity 4.1";
        _stepText.text = "Basic Concepts Associated with Force";
        _instructionText.text =
            "You will need: a Newton spring balance, a piece of stone, a wooden block, a helical spring, a piece of thread, a metal hook and a G-clamp.\n\n"
            + "Use the spring balance to measure force in three ways. Record each reading in Table 4.1.";
        _readingText.text = "";
        _statusText.text = "Read the apparatus list, then start the activity.";
        _actionLabel.text = "Start activity";
        SetButtonVisible(_actionButton, true);
        SetButtonVisible(_recordButton, false);
        SetButtonVisible(_nextButton, false);
        RefreshTable();
    }

    private void ShowHangStone()
    {
        _stage = Stage.HangStone;
        _stoneHung = false;
        BuildHangStone();
        SetBalanceReading(0f);
        _titleText.text = "Activity 4.1";
        _stepText.text = "Step 1 of 3  ·  Figure 4.2";
        _instructionText.text = "Tie the piece of stone with the thread and hang it from the spring balance. Record the magnitude of the gravitational force acting on the stone (its weight).";
        _statusText.text = "Hang the stone, then record the reading.";
        _actionLabel.text = "Hang the stone";
        _recordLabel.text = "Record reading";
        _nextLabel.text = "Next step";
        SetButtonVisible(_actionButton, true);
        SetButtonVisible(_recordButton, true);
        SetButtonVisible(_nextButton, true);
        RefreshButtons();
        RefreshTable();
    }

    private void ShowPullBlock()
    {
        _stage = Stage.PullBlock;
        _blockMoved = false;
        _liveForce = 0f;
        BuildPullBlock();
        SetBalanceReading(0f);
        _stepText.text = "Step 2 of 3  ·  Figure 4.3";
        _instructionText.text = "Fix a hook to the wooden block. Connect the spring balance and pull horizontally until the block just starts to move. Record that force.";
        _statusText.text = "Press and hold Pull. Stop when the block just begins to move.";
        _actionLabel.text = "Hold to pull";
        RefreshButtons();
        RefreshTable();
    }

    private void ShowStretchSpring()
    {
        _stage = Stage.StretchSpring;
        _springReached = false;
        _liveForce = 0f;
        BuildStretchSpring();
        SetBalanceReading(0f);
        _stepText.text = "Step 3 of 3  ·  Figure 4.4 and 4.5";
        _instructionText.text = "Clamp the wooden block to the table with the G-clamp. Connect a helical spring between the block and the spring balance. Pull until the spring extends by 10 cm and record the reading.";
        _statusText.text = "Press and hold Pull until the spring is 10 cm longer.";
        _actionLabel.text = "Hold to pull";
        RefreshButtons();
        RefreshTable();
    }

    private void ShowResults()
    {
        _stage = Stage.Results;
        ClearStage();
        _stepText.text = "Table 4.1  ·  Record of observations";
        _instructionText.text = "Check your three readings. If a value is missing, go back and record it. Then submit the practical.";
        _readingText.text = "";
        _statusText.text = AllRecorded() ? "All three forces are recorded. You can submit." : "Some readings are still missing.";
        _actionLabel.text = "Review steps";
        _nextLabel.text = "Submit practical";
        SetButtonVisible(_actionButton, true);
        SetButtonVisible(_recordButton, false);
        SetButtonVisible(_nextButton, true);
        RefreshTable();
    }

    private void OnAction()
    {
        if (_submitted)
        {
            return;
        }

        if (_stage == Stage.Intro)
        {
            ShowHangStone();
            return;
        }

        if (_stage == Stage.HangStone)
        {
            _stoneHung = true;
            if (_stone != null)
            {
                _stone.gameObject.SetActive(true);
            }

            SetBalanceReading(StoneWeightN);
            _statusText.text = "The stone is hanging. The reading is the weight of the stone. Record it.";
            RefreshButtons();
            return;
        }

        if (_stage == Stage.Results)
        {
            ShowHangStone();
        }
    }

    private void OnRecord()
    {
        if (_submitted)
        {
            return;
        }

        if (_stage == Stage.HangStone)
        {
            if (!_stoneHung)
            {
                _mistakes++;
                _statusText.text = "Hang the stone first, then record the spring-balance reading.";
                return;
            }

            _stoneN = StoneWeightN;
            _statusText.text = "Recorded: weight of the stone = " + FormatN(_stoneN);
        }
        else if (_stage == Stage.PullBlock)
        {
            if (!_blockMoved)
            {
                _mistakes++;
                _statusText.text = "Pull until the wooden block just starts to move, then record.";
                return;
            }

            _blockN = Mathf.Max(BlockStartForceN, _liveForce);
            _statusText.text = "Recorded: force to pull the block = " + FormatN(_blockN);
        }
        else if (_stage == Stage.StretchSpring)
        {
            if (!_springReached)
            {
                _mistakes++;
                _statusText.text = "Stretch the helical spring to 10 cm, then record.";
                return;
            }

            _springN = Mathf.Max(SpringConstant * TargetExtensionM, _liveForce);
            _statusText.text = "Recorded: force on the helical spring = " + FormatN(_springN);
        }

        RefreshButtons();
        RefreshTable();
    }

    private void OnNext()
    {
        if (_submitted)
        {
            return;
        }

        if (_stage == Stage.Results)
        {
            if (!AllRecorded())
            {
                _mistakes++;
                _statusText.text = "Record all three readings in Table 4.1 before submitting.";
                return;
            }

            Finish(true);
            return;
        }

        if (_stage == Stage.HangStone)
        {
            if (_stoneN < 0f)
            {
                _mistakes++;
                _statusText.text = "Record the weight of the stone before going to the next step.";
                return;
            }

            ShowPullBlock();
            return;
        }

        if (_stage == Stage.PullBlock)
        {
            if (_blockN < 0f)
            {
                _mistakes++;
                _statusText.text = "Record the pulling force before going to the next step.";
                return;
            }

            ShowStretchSpring();
            return;
        }

        if (_stage == Stage.StretchSpring)
        {
            if (_springN < 0f)
            {
                _mistakes++;
                _statusText.text = "Record the spring force before opening Table 4.1.";
                return;
            }

            ShowResults();
        }
    }

    private void RefreshButtons()
    {
        bool pullStage = _stage == Stage.PullBlock || _stage == Stage.StretchSpring;
        _actionButton.interactable = !_submitted && (_stage != Stage.HangStone || !_stoneHung);
        if (_recordButton != null)
        {
            bool canRecord = _stage == Stage.HangStone && _stoneHung && _stoneN < 0f
                || _stage == Stage.PullBlock && _blockMoved && _blockN < 0f
                || _stage == Stage.StretchSpring && _springReached && _springN < 0f;
            _recordButton.interactable = !_submitted && canRecord;
        }

        if (_nextButton != null)
        {
            bool canNext = _stage == Stage.HangStone && _stoneN >= 0f
                || _stage == Stage.PullBlock && _blockN >= 0f
                || _stage == Stage.StretchSpring && _springN >= 0f
                || _stage == Stage.Results && AllRecorded();
            _nextButton.interactable = !_submitted && canNext;
        }

        var colors = _actionButton.colors;
        colors.normalColor = pullStage && _pullHeld ? new Color(0.15f, 0.55f, 0.32f) : _blue;
        _actionButton.colors = colors;
    }

    private bool AllRecorded()
    {
        return _stoneN >= 0f && _blockN >= 0f && _springN >= 0f;
    }

    private void RefreshTable()
    {
        _tableText.text =
            "Table 4.1  Record of observations\n"
            + "1  Weight of the piece of stone                  " + Cell(_stoneN) + "\n"
            + "2  Force applied to pull the wooden block        " + Cell(_blockN) + "\n"
            + "3  Force applied on the helical spring           " + Cell(_springN);
    }

    private static string Cell(float value)
    {
        return value < 0f ? "—  N" : FormatN(value);
    }

    private static string FormatN(float value)
    {
        return value.ToString("0.0", CultureInfo.InvariantCulture) + " N";
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
        int score = ComputeScore();
        bool passed = score >= 50;
        int timeUsed = TimerManager.Instance != null ? TimerManager.Instance.TimeUsedSeconds : 0;
        _statusText.text = (studentSubmitted ? "Submitted. " : "Time expired. ") + "Score " + score;
        RefreshButtons();

        string measurements =
            "{\"stoneWeight\":" + (_stoneN < 0f ? "0" : _stoneN.ToString(CultureInfo.InvariantCulture))
            + ",\"blockForce\":" + (_blockN < 0f ? "0" : _blockN.ToString(CultureInfo.InvariantCulture))
            + ",\"springForce\":" + (_springN < 0f ? "0" : _springN.ToString(CultureInfo.InvariantCulture))
            + "}";

        FlutterBridge.EnsureInstance()?.NotifyCompleted(
            score,
            passed,
            _mistakes,
            timeUsed,
            true,
            measurements);
    }

    private int ComputeScore()
    {
        int score = 0;
        if (_stoneN >= 0f) score += 30;
        if (_blockN >= 0f) score += 30;
        if (_springN >= 0f) score += 40;
        return Mathf.Clamp(score, 0, 100);
    }

    private void BuildCanvas()
    {
        EnsureEventSystem();

        var canvasGo = new GameObject("ForceUI");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var top = Panel("TopBar", canvasGo.transform, new Vector2(0.02f, 0.88f), new Vector2(0.98f, 0.98f), _panel);
        _titleText = Label("Title", top, new Vector2(0.02f, 0.48f), new Vector2(0.70f, 0.96f), 48, FontStyle.Bold, _navy, TextAnchor.MiddleLeft);
        _timerText = Label("Timer", top, new Vector2(0.72f, 0.48f), new Vector2(0.98f, 0.96f), 48, FontStyle.Bold, _navy, TextAnchor.MiddleRight);
        _stepText = Label("Step", top, new Vector2(0.02f, 0.04f), new Vector2(0.98f, 0.50f), 32, FontStyle.Bold, _blue, TextAnchor.MiddleLeft);

        var bottom = Panel("Bottom", canvasGo.transform, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.40f), _panel);
        _instructionText = Label("Instruction", bottom, new Vector2(0.02f, 0.70f), new Vector2(0.98f, 0.98f), 30, FontStyle.Normal, _ink, TextAnchor.UpperLeft);
        _readingText = Label("Reading", bottom, new Vector2(0.02f, 0.56f), new Vector2(0.98f, 0.70f), 40, FontStyle.Bold, _navy, TextAnchor.MiddleLeft);
        _statusText = Label("Status", bottom, new Vector2(0.02f, 0.44f), new Vector2(0.98f, 0.56f), 28, FontStyle.Normal, _ink, TextAnchor.MiddleLeft);
        _tableText = Label("Table", bottom, new Vector2(0.02f, 0.04f), new Vector2(0.52f, 0.42f), 26, FontStyle.Normal, _ink, TextAnchor.UpperLeft);

        var actions = Create("Actions", bottom, new Vector2(0.54f, 0.06f), new Vector2(0.98f, 0.42f));
        var layout = actions.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        layout.padding = new RectOffset(8, 8, 8, 8);

        _actionButton = MakeButton(actions.transform, _blue, out _actionLabel);
        _actionButton.onClick.AddListener(OnAction);
        var hold = _actionButton.gameObject.AddComponent<HoldPullButton>();
        hold.Owner = this;

        _recordButton = MakeButton(actions.transform, new Color(0.15f, 0.52f, 0.38f), out _recordLabel);
        _recordButton.onClick.AddListener(OnRecord);

        _nextButton = MakeButton(actions.transform, new Color(0.90f, 0.48f, 0.12f), out _nextLabel);
        _nextButton.onClick.AddListener(OnNext);
    }

    public void SetPullHeld(bool held)
    {
        if (_stage == Stage.PullBlock || _stage == Stage.StretchSpring)
        {
            _pullHeld = held;
        }
        else
        {
            _pullHeld = false;
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
        label = Label("Label", go.transform, Vector2.zero, Vector2.one, 32, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
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

public class HoldPullButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public ForcePracticalController Owner;

    public void OnPointerDown(PointerEventData eventData)
    {
        Owner?.SetPullHeld(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Owner?.SetPullHeld(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Owner?.SetPullHeld(false);
    }
}
