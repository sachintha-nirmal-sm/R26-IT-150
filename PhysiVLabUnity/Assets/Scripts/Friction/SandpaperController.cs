using TMPro;
using UnityEngine;

public class SandpaperController : MonoBehaviour
{
    public static SandpaperController Instance { get; private set; }

    public const float Roughness = 1f;

    [SerializeField] private bool placed;
    [SerializeField] private RectTransform visual;
    [SerializeField] private TextMeshProUGUI roughnessLabel;

    public bool IsPlaced => placed;
    public float SurfaceRoughness => Roughness;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform sandpaperVisual, TextMeshProUGUI label)
    {
        visual = sandpaperVisual;
        roughnessLabel = label;
        if (roughnessLabel != null)
            roughnessLabel.text = "Surface roughness:\nCONSTANT";
        ResetPlacement();
    }

    public void Place()
    {
        placed = true;
        if (visual != null) visual.gameObject.SetActive(true);
        if (roughnessLabel != null)
            roughnessLabel.text = "Surface roughness:\nCONSTANT";
    }

    public void ResetPlacement()
    {
        placed = false;
        if (visual != null) visual.gameObject.SetActive(false);
    }
}
