using UnityEngine;

public class WoodenBlockController : MonoBehaviour
{
    public static WoodenBlockController Instance { get; private set; }

    public const float WeightNewtons = 60f;
    public const float LengthCm = 30f;
    public const float WidthCm = 20f;
    public const float HeightCm = 10f;

    [SerializeField] private int surfaceIndex;
    [SerializeField] private RectTransform visual;
    [SerializeField] private RectTransform homeParent;
    private Vector2 homePos;
    private Vector2 startPos;
    private bool moving;

    public int SurfaceIndex => surfaceIndex;
    public bool IsMoving => moving;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform blockVisual)
    {
        visual = blockVisual;
        if (visual != null)
        {
            homeParent = visual.parent as RectTransform;
            homePos = visual.anchoredPosition;
            startPos = homePos;
        }
        SetSurfaceA();
    }

    public void SetSurfaceA() => RotateToSurface(0);
    public void SetSurfaceB() => RotateToSurface(1);
    public void SetSurfaceC() => RotateToSurface(2);

    public void RotateToSurface(int index)
    {
        surfaceIndex = Mathf.Clamp(index, 0, 2);
        ApplyVisual();
        BlockSurfaceController.Instance?.SetSurface(surfaceIndex);
    }

    public float GetContactArea()
    {
        if (surfaceIndex == 0) return LengthCm * WidthCm;
        if (surfaceIndex == 1) return LengthCm * HeightCm;
        return WidthCm * HeightCm;
    }

    public string GetDimensions()
    {
        if (surfaceIndex == 0) return "30 cm × 20 cm";
        if (surfaceIndex == 1) return "30 cm × 10 cm";
        return "20 cm × 10 cm";
    }

    public string GetSurfaceName()
    {
        if (surfaceIndex == 0) return "A";
        if (surfaceIndex == 1) return "B";
        return "C";
    }

    public float GetWeight() => WeightNewtons;

    public void ApplyVisual()
    {
        if (visual == null) return;
        Vector2 size = surfaceIndex == 0 ? new Vector2(240, 92)
            : surfaceIndex == 1 ? new Vector2(220, 64)
            : new Vector2(150, 64);
        visual.sizeDelta = size;
    }

    public void BeginMotion()
    {
        moving = true;
        if (visual != null) startPos = visual.anchoredPosition;
    }

    public void StopMotion()
    {
        moving = false;
    }

    public void TickMotion(float deltaTime)
    {
        if (!moving || visual == null) return;
        visual.anchoredPosition += new Vector2(90f * deltaTime, 0f);
        if (visual.anchoredPosition.x > startPos.x + 220f)
            visual.anchoredPosition = new Vector2(startPos.x + 220f, visual.anchoredPosition.y);
    }

    public void ResetPosition()
    {
        moving = false;
        if (visual == null) return;
        if (homeParent != null) visual.SetParent(homeParent, false);
        visual.anchoredPosition = homePos;
        ApplyVisual();
    }

    public RectTransform Visual => visual;
}
