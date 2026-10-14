using UnityEngine;
using UnityEngine.UI;

public class PulleyController : MonoBehaviour
{
    public static PulleyController Instance { get; private set; }

    [SerializeField] private bool placed;
    private GameObject pulleyVisual;

    public bool Placed => placed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(GameObject visual)
    {
        pulleyVisual = visual;
        if (pulleyVisual != null) pulleyVisual.SetActive(placed);
    }

    public void Place()
    {
        placed = true;
        if (pulleyVisual != null)
        {
            pulleyVisual.SetActive(true);
            var img = pulleyVisual.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = NewtonsLawsIconFactory.GetNamed("pulley");
                img.preserveAspect = true;
            }
        }
    }

    public void ResetPulley()
    {
        placed = false;
        if (pulleyVisual != null) pulleyVisual.SetActive(false);
    }
}
