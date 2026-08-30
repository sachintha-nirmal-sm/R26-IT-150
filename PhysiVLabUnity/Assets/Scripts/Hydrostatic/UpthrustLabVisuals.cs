using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 2D lab visuals: retort stand, spring balance, metal cube (centre line),
/// Eureka can, overflow stream, and beaker water fill.
/// </summary>
public class UpthrustLabVisuals : MonoBehaviour
{
    public static UpthrustLabVisuals Instance { get; private set; }

    [Header("Moving Parts")]
    [SerializeField] private RectTransform cube;
    [SerializeField] private RectTransform hangingAssembly;
    [SerializeField] private Image beakerWater;
    [SerializeField] private Image eurekaWater;
    [SerializeField] private RectTransform overflowStream;
    [SerializeField] private GameObject beakerObject;
    [SerializeField] private GameObject cubeObject;
    [SerializeField] private Text stageLabel;

    [Header("Cube Anchored Y (lab local space)")]
    [SerializeField] private float airY = 210f;
    [SerializeField] private float nearSurfaceY = 70f;
    [SerializeField] private float halfSubmergedY = 10f;
    [SerializeField] private float fullyNearY = -40f;
    [SerializeField] private float fullyDeepY = -90f;
    [SerializeField] private float moveDuration = 0.55f;

    private Coroutine moveRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Configure(
        RectTransform cubeRt,
        RectTransform assembly,
        Image beakerFill,
        Image eurekaFill,
        RectTransform overflow,
        GameObject beaker,
        GameObject cubeGo,
        Text stage)
    {
        cube = cubeRt;
        hangingAssembly = assembly;
        beakerWater = beakerFill;
        eurekaWater = eurekaFill;
        overflowStream = overflow;
        beakerObject = beaker;
        cubeObject = cubeGo;
        stageLabel = stage;
        ResetLab();
    }

    public void ResetLab()
    {
        if (cubeObject != null) cubeObject.SetActive(false);
        if (beakerObject != null) beakerObject.SetActive(false);
        if (overflowStream != null) overflowStream.gameObject.SetActive(false);
        SetBeakerFill(0f);
        SetEurekaFill(1f);
        SetStageLabel("Ready");
        SetCubeY(airY, true);
    }

    public void HangCubeInAir()
    {
        if (cubeObject != null) cubeObject.SetActive(true);
        SetCubeY(airY, false);
        SetStageLabel("In air  •  1.2 N");
    }

    public void PlaceBeakerUnderSpout()
    {
        if (beakerObject != null) beakerObject.SetActive(true);
        SetBeakerFill(0f);
        SetStageLabel("Beaker under spout  •  1.3 N");
    }

    public void MoveCubeToStage(UpthrustPracticalData.ImmersionStage stage)
    {
        float y = airY;
        string label = "In air";

        switch (stage)
        {
            case UpthrustPracticalData.ImmersionStage.NearSurface:
                y = nearSurfaceY;
                label = "Stage (a)  near surface  •  Upthrust 0 N";
                break;
            case UpthrustPracticalData.ImmersionStage.HalfSubmerged:
                y = halfSubmergedY;
                label = "Stage (b)  half submerged  •  Upthrust 0.3 N";
                break;
            case UpthrustPracticalData.ImmersionStage.FullyNearSurface:
                y = fullyNearY;
                label = "Stage (c)  fully immersed  •  Upthrust 0.6 N";
                break;
            case UpthrustPracticalData.ImmersionStage.FullyDeep:
                y = fullyDeepY;
                label = "Stage (d)  deeper  •  Upthrust still 0.6 N";
                break;
        }

        SetCubeY(y, false);
        SetStageLabel(label);
    }

    public void SetBeakerFill(float displacedNewton)
    {
        if (beakerWater == null) return;

        float t = Mathf.Clamp01(displacedNewton / 0.6f);
        beakerWater.fillAmount = Mathf.Lerp(0.08f, 0.72f, t);
        beakerWater.gameObject.SetActive(displacedNewton > 0.01f || (beakerObject != null && beakerObject.activeSelf));
        if (displacedNewton <= 0.01f)
            beakerWater.fillAmount = 0.06f;
    }

    public void SetEurekaFill(float normalized)
    {
        if (eurekaWater == null) return;
        eurekaWater.fillAmount = Mathf.Clamp01(normalized);
    }

    public IEnumerator PlayOverflow(float amountN)
    {
        if (overflowStream == null)
            yield break;

        overflowStream.gameObject.SetActive(true);
        Vector2 start = overflowStream.anchoredPosition;
        float elapsed = 0f;
        const float duration = 0.7f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 3f, 1f);
            overflowStream.localScale = new Vector3(1f, 0.4f + t * 0.8f, 1f);
            yield return null;
        }

        overflowStream.anchoredPosition = start;
        overflowStream.localScale = Vector3.one;
        overflowStream.gameObject.SetActive(false);
    }

    private void SetStageLabel(string text)
    {
        if (stageLabel != null)
            stageLabel.text = text;
    }

    private void SetCubeY(float y, bool instant)
    {
        RectTransform target = hangingAssembly != null ? hangingAssembly : cube;
        if (target == null) return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        if (instant)
        {
            Vector2 p = target.anchoredPosition;
            p.y = y;
            target.anchoredPosition = p;
            return;
        }

        moveRoutine = StartCoroutine(MoveToY(target, y));
    }

    private IEnumerator MoveToY(RectTransform target, float y)
    {
        Vector2 start = target.anchoredPosition;
        Vector2 end = new Vector2(start.x, y);
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
            target.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        target.anchoredPosition = end;
        moveRoutine = null;
    }
}
