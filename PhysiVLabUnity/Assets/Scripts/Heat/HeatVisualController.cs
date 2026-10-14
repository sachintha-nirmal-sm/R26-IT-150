using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeatVisualController : MonoBehaviour
{
    public static HeatVisualController Instance { get; private set; }

    public const float LevelA = 0.48f;
    public const float LevelB = 0.40f;
    public const float LevelC = 0.78f;

    private GameObject testTubeVisual;
    private GameObject waterVisual;
    private GameObject stopperVisual;
    private GameObject thinTubeVisual;
    private GameObject tripodVisual;
    private GameObject beakerVisual;
    private GameObject burnerVisual;
    private GameObject flameVisual;
    private GameObject standVisual;
    private GameObject markA;
    private GameObject markB;
    private GameObject markC;
    private RectTransform liquidColumn;
    private TextMeshProUGUI levelLabel;
    private TextMeshProUGUI statusLabel;
    private float liquidT = LevelA;
    private bool heating;
    private bool reachedB;
    private bool reachedC;
    private bool dropScored;
    private bool riseScored;
    private Coroutine heatRoutine;

    public bool ReachedLevelB => reachedB;
    public bool ReachedLevelC => reachedC;
    public bool IsHeating => heating;
    public float LiquidT => liquidT;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(
        GameObject tube, GameObject water, GameObject stopper, GameObject thin,
        GameObject tripod, GameObject beaker, GameObject burner, GameObject flame, GameObject stand,
        GameObject a, GameObject b, GameObject c, RectTransform column,
        TextMeshProUGUI level, TextMeshProUGUI status)
    {
        testTubeVisual = tube;
        waterVisual = water;
        stopperVisual = stopper;
        thinTubeVisual = thin;
        tripodVisual = tripod;
        beakerVisual = beaker;
        burnerVisual = burner;
        flameVisual = flame;
        standVisual = stand;
        markA = a;
        markB = b;
        markC = c;
        liquidColumn = column;
        levelLabel = level;
        statusLabel = status;
        ResetVisuals();
    }

    public void ResetVisuals()
    {
        if (heatRoutine != null)
        {
            StopCoroutine(heatRoutine);
            heatRoutine = null;
        }
        heating = false;
        reachedB = false;
        reachedC = false;
        dropScored = false;
        riseScored = false;
        liquidT = LevelA;
        ShowTestTube(false);
        ShowWater(false);
        ShowStopper(false);
        ShowThinTube(false);
        ShowTripod(false);
        ShowBeaker(false);
        ShowBurner(false);
        ShowStand(false);
        ShowMarkA(false);
        if (markB != null) markB.SetActive(false);
        if (markC != null) markC.SetActive(false);
        if (flameVisual != null) flameVisual.SetActive(false);
        ApplyLiquid();
        RefreshLabels();
    }

    public void ShowTestTube(bool show) { if (testTubeVisual != null) testTubeVisual.SetActive(show); }
    public void ShowWater(bool show) { if (waterVisual != null) waterVisual.SetActive(show); ApplyLiquid(); }
    public void ShowStopper(bool show) { if (stopperVisual != null) stopperVisual.SetActive(show); }
    public void ShowThinTube(bool show)
    {
        if (thinTubeVisual != null) thinTubeVisual.SetActive(show);
        ApplyLiquid();
        RefreshLabels();
    }
    public void ShowTripod(bool show) { if (tripodVisual != null) tripodVisual.SetActive(show); }
    public void ShowBeaker(bool show) { if (beakerVisual != null) beakerVisual.SetActive(show); }
    public void ShowBurner(bool show) { if (burnerVisual != null) burnerVisual.SetActive(show); }
    public void ShowStand(bool show) { if (standVisual != null) standVisual.SetActive(show); }
    public void ShowMarkA(bool show)
    {
        if (markA != null) markA.SetActive(show);
        RefreshLabels();
    }

    public void StartHeating()
    {
        var asm = HeatAssemblyManager.Instance;
        if (asm == null || !asm.SetupConfirmed)
        {
            HeatFeedbackManager.Instance?.ShowInstruction("Confirm the setup before heating.");
            return;
        }
        if (heating || reachedC) return;
        heating = true;
        if (flameVisual != null) flameVisual.SetActive(true);
        HeatFeedbackManager.Instance?.ShowInstruction("Watch the thin tube: the liquid will fall slightly, then rise.");
        heatRoutine = StartCoroutine(HeatSequence());
        HeatUIManager.Instance?.UpdateLiveReadings();
    }

    private IEnumerator HeatSequence()
    {
        yield return new WaitForSeconds(0.6f);
        yield return AnimateLevel(LevelA, LevelB, 1.15f);
        reachedB = true;
        if (markB != null) markB.SetActive(true);
        if (!dropScored)
        {
            dropScored = true;
            HeatScoreManager.Instance?.AddScore(10, false);
        }
        HeatFeedbackManager.Instance?.ShowMessage(
            "✓ LEVEL FALLS TO B\nThe glass test tube expands first, so its volume increases and the liquid level drops slightly from A to B.",
            "+10 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        HeatUIManager.Instance?.UpdateLiveReadings();
        yield return new WaitForSeconds(0.9f);
        yield return AnimateLevel(LevelB, LevelC, 2.4f);
        reachedC = true;
        heating = false;
        if (markC != null) markC.SetActive(true);
        if (!riseScored)
        {
            riseScored = true;
            HeatScoreManager.Instance?.AddScore(10, false);
        }
        HeatFeedbackManager.Instance?.ShowMessage(
            "✓ LEVEL RISES TO C\nThe liquid now expands more than the glass, so the level rises past A to C.",
            "+10 MARKS",
            new Color(0.08f, 0.52f, 0.22f));
        HeatUIManager.Instance?.UpdateLiveReadings();
        HeatUIManager.Instance?.SetNextButtonVisible(true);
    }

    private IEnumerator AnimateLevel(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            liquidT = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
            ApplyLiquid();
            RefreshLabels();
            yield return null;
        }
        liquidT = to;
        ApplyLiquid();
        RefreshLabels();
    }

    private void ApplyLiquid()
    {
        if (liquidColumn == null) return;
        bool show = thinTubeVisual != null && thinTubeVisual.activeInHierarchy;
        liquidColumn.gameObject.SetActive(show);
        if (!show) return;
        liquidColumn.anchorMin = new Vector2(0.42f, 0.08f);
        liquidColumn.anchorMax = new Vector2(0.58f, liquidT);
        liquidColumn.offsetMin = liquidColumn.offsetMax = Vector2.zero;
    }

    private void RefreshLabels()
    {
        string levelName = "—";
        if (thinTubeVisual != null && thinTubeVisual.activeInHierarchy)
        {
            if (reachedC) levelName = "C  (liquid expanded)";
            else if (reachedB && heating) levelName = "rising towards C";
            else if (reachedB) levelName = "B  (glass expanded first)";
            else if (HeatAssemblyManager.Instance != null && HeatAssemblyManager.Instance.LevelAMarked) levelName = "A  (starting level)";
            else levelName = "liquid in thin tube";
        }
        if (levelLabel != null)
            levelLabel.text = "Liquid level:  " + levelName;
        if (statusLabel != null)
        {
            if (reachedC) statusLabel.text = "Observation:  fell A → B, then rose to C";
            else if (heating) statusLabel.text = "Heating:  watch the thin tube";
            else if (HeatAssemblyManager.Instance != null && HeatAssemblyManager.Instance.SetupConfirmed)
                statusLabel.text = "Ready to heat the water bath";
            else statusLabel.text = "Assemble the apparatus";
        }
    }
}
