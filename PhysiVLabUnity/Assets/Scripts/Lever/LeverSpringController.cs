using UnityEngine;

public class LeverSpringController : MonoBehaviour
{
    public static LeverSpringController Instance { get; private set; }

    [SerializeField] private RectTransform spring;
    [SerializeField] private float minHeight = 40f;
    [SerializeField] private float maxHeight = 160f;

    private float baseHeight;

    private void Awake()
    {
        Instance = this;
        if (spring == null) spring = GetComponent<RectTransform>();
        if (spring != null) baseHeight = spring.sizeDelta.y > 0f ? spring.sizeDelta.y : minHeight;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Bind(RectTransform springRect, float min = 40f, float max = 160f)
    {
        spring = springRect;
        minHeight = min;
        maxHeight = max;
        if (spring != null)
            baseHeight = spring.sizeDelta.y > 0f ? spring.sizeDelta.y : minHeight;
        SetExtension(0f, 1f);
    }

    public void SetExtension(float force, float maxForce)
    {
        if (spring == null) return;
        float t = maxForce > 0.01f ? Mathf.Clamp01(force / maxForce) : 0f;
        float height = Mathf.Lerp(minHeight, maxHeight, t);
        var size = spring.sizeDelta;
        size.y = height;
        spring.sizeDelta = size;
    }

    public void ResetSpring()
    {
        SetExtension(0f, 1f);
    }
}
