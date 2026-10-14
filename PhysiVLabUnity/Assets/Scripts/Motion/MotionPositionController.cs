using UnityEngine;

public class MotionPositionController : MonoBehaviour
{
    public static MotionPositionController Instance { get; private set; }

    [SerializeField] private float trackLengthMeters = 5f;
    [SerializeField] private RectTransform trackRect;
    [SerializeField] private RectTransform carRect;

    public float TrackLengthMeters => trackLengthMeters;
    public RectTransform TrackRect => trackRect;
    public float PixelsPerMeter
    {
        get
        {
            float width = trackRect != null && trackRect.rect.width > 8f ? trackRect.rect.width : 900f;
            return width / Mathf.Max(0.01f, trackLengthMeters);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Configure(float lengthMeters)
    {
        trackLengthMeters = Mathf.Max(1f, lengthMeters);
    }

    public void Bind(RectTransform track, RectTransform car)
    {
        trackRect = track;
        carRect = car;
    }

    public float GetPositionMeters()
    {
        if (carRect == null || trackRect == null) return 0f;
        float x = carRect.anchoredPosition.x;
        float meters = x / PixelsPerMeter;
        return Mathf.Clamp(meters, 0f, trackLengthMeters);
    }

    public void SetPositionMeters(float meters)
    {
        if (carRect == null) return;
        meters = Mathf.Clamp(meters, 0f, trackLengthMeters);
        var pos = carRect.anchoredPosition;
        pos.x = meters * PixelsPerMeter;
        carRect.anchoredPosition = pos;
    }

    public void MoveToPosition(float meters)
    {
        SetPositionMeters(meters);
    }

    public void ResetPosition()
    {
        SetPositionMeters(0f);
    }

    public Vector2 AnchoredPositionForMeters(float meters)
    {
        meters = Mathf.Clamp(meters, 0f, trackLengthMeters);
        return new Vector2(meters * PixelsPerMeter, 0f);
    }

    public float MetersFromAnchoredX(float anchoredX)
    {
        return Mathf.Clamp(anchoredX / PixelsPerMeter, 0f, trackLengthMeters);
    }
}
