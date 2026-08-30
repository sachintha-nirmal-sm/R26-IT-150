using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ElectronicsBulbController : MonoBehaviour
{
    public static ElectronicsBulbController Instance { get; private set; }

    [SerializeField] private bool glowing;
    [SerializeField] private bool placed;

    private Image bulbImage;
    private Image glowImage;
    private Image raysImage;
    private TMPro.TextMeshProUGUI statusLabel;
    private Coroutine pulseRoutine;

    public bool IsPlaced => placed;
    public bool IsGlowing => glowing;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(Image bulb, Image glow, Image rays, TMPro.TextMeshProUGUI status)
    {
        bulbImage = bulb;
        glowImage = glow;
        raysImage = rays;
        statusLabel = status;
        SetGlow(false);
    }

    public void SetPlaced(bool value)
    {
        placed = value;
        if (!value) SetGlow(false);
    }

    public void TurnOn() => SetGlow(true);
    public void TurnOff() => SetGlow(false);

    public void SetGlow(bool state)
    {
        glowing = state;
        if (bulbImage != null)
            bulbImage.sprite = ElectronicsIconFactory.GetNamed(state ? "bulb-on" : "bulb-off");
        if (glowImage != null)
        {
            glowImage.enabled = state;
            glowImage.color = state ? new Color(1f, 0.92f, 0.35f, 0.55f) : Color.clear;
        }
        if (raysImage != null)
        {
            raysImage.enabled = state;
            raysImage.sprite = ElectronicsIconFactory.GetNamed("rays");
            raysImage.color = state ? new Color(1f, 0.95f, 0.55f, 0.8f) : Color.clear;
        }
        if (statusLabel != null)
            statusLabel.text = state ? "BULB STATUS: GLOWING" : "BULB STATUS: NOT GLOWING";

        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        if (state && glowImage != null) pulseRoutine = StartCoroutine(Pulse());
    }

    public void ResetBulb()
    {
        placed = false;
        SetGlow(false);
    }

    private IEnumerator Pulse()
    {
        while (glowing && glowImage != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                float a = 0.35f + 0.25f * Mathf.Sin(t * 4f);
                glowImage.color = new Color(1f, 0.92f, 0.35f, a);
                if (raysImage != null)
                    raysImage.transform.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(t * 4f));
                yield return null;
            }
        }
    }
}
