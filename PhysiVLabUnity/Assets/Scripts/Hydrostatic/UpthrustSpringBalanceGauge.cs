using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animates a 0–5 N spring-balance pointer and digital readout from the current apparent weight.
/// Inspector: assign needle RectTransform (rotates around Z) and optional scale / readout texts.
/// </summary>
public class UpthrustSpringBalanceGauge : MonoBehaviour
{
    public static UpthrustSpringBalanceGauge Instance { get; private set; }

    [Header("Scale")]
    [SerializeField] private float minNewton = 0f;
    [SerializeField] private float maxNewton = UpthrustPracticalData.SpringScaleMax;
    [SerializeField] private float needleAngleAtMin = 90f;
    [SerializeField] private float needleAngleAtMax = -90f;

    [Header("Visuals")]
    [SerializeField] private RectTransform needle;
    [SerializeField] private Text digitalReadout;
    [SerializeField] private Text scaleCaption;
    [SerializeField] private Image springCoil;
    [SerializeField] private float animationSpeed = 6f;

    private float displayedNewton;
    private float targetNewton = UpthrustPracticalData.WeightInAir;

    public float CurrentReading => displayedNewton;
    public float TargetReading => targetNewton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        displayedNewton = targetNewton;
        ApplyVisuals(displayedNewton);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Configure(RectTransform needleTransform, Text readout, Text caption, Image coil)
    {
        needle = needleTransform;
        digitalReadout = readout;
        scaleCaption = caption;
        springCoil = coil;
        ApplyVisuals(displayedNewton);
    }

    /// <summary>Sets the target apparent weight in newtons (book values: 1.2, 0.9, 0.6).</summary>
    public void SetReading(float newtons, bool instant = false)
    {
        targetNewton = Mathf.Clamp(newtons, minNewton, maxNewton);
        if (instant)
        {
            displayedNewton = targetNewton;
            ApplyVisuals(displayedNewton);
        }
    }

    private void Update()
    {
        if (Mathf.Abs(displayedNewton - targetNewton) < 0.001f) return;

        displayedNewton = Mathf.Lerp(displayedNewton, targetNewton, Time.deltaTime * animationSpeed);
        if (Mathf.Abs(displayedNewton - targetNewton) < 0.01f)
            displayedNewton = targetNewton;

        ApplyVisuals(displayedNewton);
    }

    private void ApplyVisuals(float newtons)
    {
        float t = Mathf.InverseLerp(minNewton, maxNewton, newtons);
        float angle = Mathf.Lerp(needleAngleAtMin, needleAngleAtMax, t);

        if (needle != null)
            needle.localEulerAngles = new Vector3(0f, 0f, angle);

        if (digitalReadout != null)
            digitalReadout.text = $"{newtons:0.0} N";

        if (scaleCaption != null)
            scaleCaption.text = "Spring Balance  0 – 5 N";

        if (springCoil != null)
        {
            // Stretch the coil slightly as load increases.
            float stretch = Mathf.Lerp(0.85f, 1.25f, t);
            springCoil.rectTransform.localScale = new Vector3(1f, stretch, 1f);
        }
    }
}
