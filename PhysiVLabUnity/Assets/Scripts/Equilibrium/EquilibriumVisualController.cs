using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquilibriumVisualController : MonoBehaviour
{
    public static EquilibriumVisualController Instance { get; private set; }

    private GameObject stand, balance1, balance2, ruler, bandLeft, bandRight;
    private GameObject arrowF1, arrowF2, arrowW;
    private TextMeshProUGUI reading1, reading2;
    private Image fill1, fill2;
    private RectTransform rulerRt;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(
        GameObject standObj, GameObject b1, GameObject b2, GameObject rulerObj, GameObject leftBand, GameObject rightBand,
        GameObject f1Arrow, GameObject f2Arrow, GameObject wArrow,
        TextMeshProUGUI r1, TextMeshProUGUI r2, Image f1Fill, Image f2Fill, RectTransform rulerRect)
    {
        stand = standObj; balance1 = b1; balance2 = b2; ruler = rulerObj;
        bandLeft = leftBand; bandRight = rightBand;
        arrowF1 = f1Arrow; arrowF2 = f2Arrow; arrowW = wArrow;
        reading1 = r1; reading2 = r2; fill1 = f1Fill; fill2 = f2Fill;
        rulerRt = rulerRect != null ? rulerRect : (rulerObj != null ? rulerObj.GetComponent<RectTransform>() : null);
        RefreshVisuals();
        RefreshReadings();
    }

    public void RefreshVisuals()
    {
        var asm = EquilibriumAssemblyManager.Instance;
        var force = EquilibriumForceController.Instance;
        SetActive(stand, asm != null && asm.StandPlaced);
        SetActive(balance1, asm != null && asm.Balance1Placed);
        SetActive(balance2, asm != null && asm.Balance2Placed);
        bool showRuler = asm != null && asm.RulerPlaced;
        SetActive(ruler, showRuler);
        SetActive(bandLeft, asm != null && asm.BandLeft);
        SetActive(bandRight, asm != null && asm.BandRight);
        bool dual = force != null && force.DualHung;
        bool weigh = force != null && force.WeighingAttached;
        SetActive(arrowF1, dual || weigh);
        SetActive(arrowF2, dual);
        SetActive(arrowW, showRuler);
        RefreshReadings();
    }

    public void RefreshReadings()
    {
        var force = EquilibriumForceController.Instance;
        float f1 = force != null ? force.Force1N : 0f;
        float f2 = force != null ? force.Force2N : 0f;
        if (reading1 != null) reading1.text = $"{f1:0.00} N";
        if (reading2 != null) reading2.text = $"{f2:0.00} N";
        if (fill1 != null) fill1.fillAmount = Mathf.Clamp01(f1 / 2f);
        if (fill2 != null) fill2.fillAmount = Mathf.Clamp01(f2 / 2f);
        if (rulerRt != null && force != null && (force.DualHung || force.WeighingAttached))
            rulerRt.localEulerAngles = new Vector3(0f, 0f, -force.TiltDeg * 0.6f);
        else if (rulerRt != null)
            rulerRt.localEulerAngles = Vector3.zero;
    }

    private static void SetActive(GameObject go, bool on)
    {
        if (go != null && go.activeSelf != on) go.SetActive(on);
    }
}
