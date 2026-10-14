using UnityEngine;
using UnityEngine.UI;

public class StringConnectionController : MonoBehaviour
{
    public static StringConnectionController Instance { get; private set; }

    [SerializeField] private bool stringPlaced;
    [SerializeField] private bool hangerAttached;
    private GameObject stringVisual;
    private GameObject hangerVisual;

    public bool StringPlaced => stringPlaced;
    public bool HangerAttached => hangerAttached;
    public bool Connected => stringPlaced && hangerAttached && PulleyController.Instance != null && PulleyController.Instance.Placed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(GameObject stringObj, GameObject hangerObj)
    {
        stringVisual = stringObj;
        hangerVisual = hangerObj;
        Apply();
    }

    public void PlaceString()
    {
        stringPlaced = true;
        Apply();
    }

    public void AttachHanger()
    {
        hangerAttached = true;
        Apply();
    }

    public void ResetConnection()
    {
        stringPlaced = false;
        hangerAttached = false;
        Apply();
    }

    private void Apply()
    {
        if (stringVisual != null)
        {
            stringVisual.SetActive(stringPlaced);
            var img = stringVisual.GetComponent<Image>();
            if (img != null) img.color = new Color(0.35f, 0.28f, 0.18f, 0.95f);
        }
        if (hangerVisual != null)
        {
            hangerVisual.SetActive(hangerAttached);
            var img = hangerVisual.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = NewtonsLawsIconFactory.GetNamed("hanger");
                img.preserveAspect = true;
            }
        }
    }
}
