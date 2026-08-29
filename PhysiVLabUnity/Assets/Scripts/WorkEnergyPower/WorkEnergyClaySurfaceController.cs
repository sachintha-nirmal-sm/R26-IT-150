using UnityEngine;
using UnityEngine.UI;

public class WorkEnergyClaySurfaceController : MonoBehaviour
{
    public static WorkEnergyClaySurfaceController Instance { get; private set; }

    [SerializeField] private Image clayImage;
    [SerializeField] private Image depressionImage;
    [SerializeField] private RectTransform clayRect;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(Image clay, Image depression, RectTransform rect)
    {
        clayImage = clay;
        depressionImage = depression;
        clayRect = rect;
        SetFlat();
    }

    public void SetFlat()
    {
        if (clayImage != null) clayImage.color = new Color(0.72f, 0.48f, 0.28f, 1f);
        if (depressionImage != null)
        {
            depressionImage.gameObject.SetActive(false);
            depressionImage.rectTransform.sizeDelta = new Vector2(70f, 8f);
        }
    }

    public void ShowPrepared()
    {
        if (clayImage != null) clayImage.color = new Color(0.78f, 0.52f, 0.30f, 1f);
    }

    public void ShowDepression(float depthCm, float maximumDepth)
    {
        if (depressionImage == null) return;
        depressionImage.gameObject.SetActive(true);
        float t = maximumDepth > 0f ? Mathf.Clamp01(depthCm / maximumDepth) : 0.3f;
        float width = Mathf.Lerp(48f, 110f, t);
        float height = Mathf.Lerp(10f, 52f, t);
        depressionImage.rectTransform.sizeDelta = new Vector2(width, height);
        depressionImage.color = Color.Lerp(new Color(0.45f, 0.28f, 0.14f), new Color(0.28f, 0.16f, 0.08f), t);
    }
}
