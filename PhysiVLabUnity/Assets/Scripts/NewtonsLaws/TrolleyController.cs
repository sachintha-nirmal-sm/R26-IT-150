using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TrolleyController : MonoBehaviour
{
    public static TrolleyController Instance { get; private set; }

    [SerializeField] private float mass = 1f;
    [SerializeField] private float force;
    [SerializeField] private float velocity;
    [SerializeField] private float position;
    [SerializeField] private float trackLength = 5f;
    [SerializeField] private bool running;

    private RectTransform trolleyRect;
    private RectTransform trackRect;
    private float pixelsPerMetre = 160f;
    private float elapsed;
    private Coroutine pulseRoutine;

    public float Mass => mass;
    public float Force => force;
    public float Velocity => velocity;
    public float Position => position;
    public float Acceleration { get; private set; }
    public bool IsRunning => running;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform trolley, RectTransform track)
    {
        trolleyRect = trolley;
        trackRect = track;
        RecalcScale();
        SetPosition(0f);
    }

    public void ConfigureLimits(float min, float max)
    {
        trackLength = Mathf.Max(1f, max - min);
        RecalcScale();
    }

    public void SetMass(float value)
    {
        mass = Mathf.Clamp(value, 0.5f, 5f);
        RecalcAcceleration();
    }

    public void SetForce(float value)
    {
        force = value;
        RecalcAcceleration();
    }

    public void SetVelocity(float value) => velocity = value;

    public void SetPosition(float metres)
    {
        position = Mathf.Clamp(metres, 0f, trackLength);
        ApplyVisual();
    }

    public void StartMotion()
    {
        RecalcAcceleration();
        running = true;
        elapsed = 0f;
        ApplyVisual();
    }

    public void PulseHighlight()
    {
        if (trolleyRect == null) return;
        var img = trolleyRect.GetComponent<Image>();
        if (img == null) return;
        trolleyRect.gameObject.SetActive(true);
        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseRoutine(img));
    }

    private IEnumerator PulseRoutine(Image img)
    {
        for (int i = 0; i < 8; i++)
        {
            img.color = i % 2 == 0 ? new Color(1f, 0.85f, 0.15f) : Color.white;
            yield return new WaitForSeconds(0.16f);
        }
        img.color = Color.white;
        pulseRoutine = null;
    }

    public void Stop()
    {
        running = false;
        velocity = 0f;
        Acceleration = 0f;
    }

    public void ResetTrolley()
    {
        running = false;
        velocity = 0f;
        Acceleration = 0f;
        force = 0f;
        SetPosition(0f);
    }

    private void RecalcAcceleration()
    {
        Acceleration = NewtonAccelerationCalculator.Instance != null
            ? NewtonAccelerationCalculator.Instance.Calculate(force, mass)
            : (mass > 0f ? force / mass : 0f);
    }

    private void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        float friction = NewtonFrictionController.Instance != null ? NewtonFrictionController.Instance.FrictionAcceleration : 0f;
        float netA = Acceleration - friction * Mathf.Sign(Mathf.Abs(velocity) < 0.01f ? (force >= 0f ? 1f : -1f) : velocity);
        if (Mathf.Abs(force) < 0.001f && NewtonFrictionController.Instance != null && NewtonFrictionController.Instance.IsLowFriction)
            netA = 0f;
        else if (Mathf.Abs(force) < 0.001f)
            netA = -friction * Mathf.Sign(velocity);

        velocity += netA * Time.deltaTime;
        if (Mathf.Abs(force) < 0.001f && NewtonFrictionController.Instance != null && !NewtonFrictionController.Instance.IsLowFriction)
        {
            if (Mathf.Abs(velocity) < 0.02f) velocity = 0f;
        }
        position += velocity * Time.deltaTime;
        if (position <= 0f)
        {
            position = 0f;
            if (velocity < 0f) velocity = 0f;
        }
        if (position >= trackLength)
        {
            position = trackLength;
            running = false;
            velocity = 0f;
        }
        ApplyVisual();
        NewtonUIManager.Instance?.UpdateLiveNewtonReadings(mass, force, Acceleration, velocity, elapsed, mass * 9.8f, velocity >= 0f);
    }

    private void RecalcScale()
    {
        if (trackRect == null) return;
        float w = trackRect.rect.width;
        if (w < 8f) w = 900f;
        pixelsPerMetre = w / Mathf.Max(0.5f, trackLength);
    }

    private void ApplyVisual()
    {
        if (trolleyRect == null) return;
        RecalcScale();
        trolleyRect.anchorMin = trolleyRect.anchorMax = new Vector2(0f, 0.52f);
        trolleyRect.pivot = new Vector2(0.5f, 0.5f);
        trolleyRect.anchoredPosition = new Vector2(40f + position * pixelsPerMetre, 0f);
        trolleyRect.gameObject.SetActive(true);
        var img = trolleyRect.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = NewtonsLawsIconFactory.GetNamed("trolley");
            img.preserveAspect = true;
        }
    }
}
