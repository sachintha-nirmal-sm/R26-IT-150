using UnityEngine;

public class WorkEnergyReleaseMechanismController : MonoBehaviour
{
    public static WorkEnergyReleaseMechanismController Instance { get; private set; }

    [SerializeField] private bool mechanismReady;
    [SerializeField] private float currentHeight = 0.5f;
    [SerializeField] private float heightTolerance = 0.02f;

    public bool MechanismReady => mechanismReady;
    public float CurrentHeight => currentHeight;
    public float HeightTolerance => heightTolerance;

    private RectTransform holder;
    private RectTransform scaleRoot;
    private float clayY;
    private float oneMetreY;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform holderRect, RectTransform scale, float clayNormalized, float oneMetreNormalized)
    {
        holder = holderRect;
        scaleRoot = scale;
        if (scaleRoot == null) return;
        float h = scaleRoot.rect.height > 1f ? scaleRoot.rect.height : 520f;
        clayY = clayNormalized * h;
        oneMetreY = oneMetreNormalized * h;
    }

    public void SetReady(bool ready) => mechanismReady = ready;

    public void SetHeight(float heightMetres)
    {
        currentHeight = Mathf.Clamp(heightMetres, 0f, 1.05f);
        ApplyHolderPosition();
    }

    public bool IsWithinTolerance(float target)
    {
        return Mathf.Abs(currentHeight - target) <= heightTolerance;
    }

    public float HeightFromHolderPosition()
    {
        if (holder == null || scaleRoot == null) return currentHeight;
        float localY = holder.anchoredPosition.y;
        float span = oneMetreY - clayY;
        if (Mathf.Abs(span) < 0.001f) return currentHeight;
        currentHeight = Mathf.Clamp((localY - clayY) / span, 0f, 1.05f);
        return currentHeight;
    }

    public void ApplyHolderPosition()
    {
        if (holder == null || scaleRoot == null) return;
        float span = oneMetreY - clayY;
        float y = clayY + currentHeight * span;
        var pos = holder.anchoredPosition;
        pos.y = y;
        holder.anchoredPosition = pos;
    }

    public void ResetMechanism()
    {
        mechanismReady = false;
        currentHeight = 0.5f;
        ApplyHolderPosition();
    }
}
