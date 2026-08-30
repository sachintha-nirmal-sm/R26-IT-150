using TMPro;
using UnityEngine;

public class BlockSurfaceController : MonoBehaviour
{
    public static BlockSurfaceController Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI surfaceLabel;
    [SerializeField] private TextMeshProUGUI areaLabel;
    [SerializeField] private int currentSurface;

    public int CurrentSurface => currentSurface;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(TextMeshProUGUI surface, TextMeshProUGUI area)
    {
        surfaceLabel = surface;
        areaLabel = area;
        SetSurface(0);
    }

    public void SetSurface(int index)
    {
        currentSurface = Mathf.Clamp(index, 0, 2);
        string name = index == 0 ? "A" : index == 1 ? "B" : "C";
        string dim = index == 0 ? "30 cm × 20 cm" : index == 1 ? "30 cm × 10 cm" : "20 cm × 10 cm";
        float area = index == 0 ? 600f : index == 1 ? 300f : 200f;
        if (surfaceLabel != null)
            surfaceLabel.text = $"Surface {name}\n{dim}";
        if (areaLabel != null)
            areaLabel.text = $"Area = {area:0} cm²\nWeight = 60 N";
    }

    public bool IsExpectedSurface(int expected) => currentSurface == expected;
}
