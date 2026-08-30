using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultantVisualController : MonoBehaviour
{
    public static ResultantVisualController Instance { get; private set; }

    private GameObject trolley, ring, strings, pulley1, pulley2, balanceA, balanceB, balanceC, wall;
    private TextMeshProUGUI readingA, readingB, readingC;
    private RectTransform arrowA, arrowB, arrowC;
    private Image fillA, fillB, fillC;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(
        GameObject trolleyObj, GameObject ringObj, GameObject stringsObj,
        GameObject pulley1Obj, GameObject pulley2Obj,
        GameObject balanceAObj, GameObject balanceBObj, GameObject balanceCObj, GameObject wallObj,
        TextMeshProUGUI a, TextMeshProUGUI b, TextMeshProUGUI c,
        RectTransform arrA, RectTransform arrB, RectTransform arrC,
        Image fA, Image fB, Image fC)
    {
        trolley = trolleyObj; ring = ringObj; strings = stringsObj;
        pulley1 = pulley1Obj; pulley2 = pulley2Obj;
        balanceA = balanceAObj; balanceB = balanceBObj; balanceC = balanceCObj; wall = wallObj;
        readingA = a; readingB = b; readingC = c;
        arrowA = arrA; arrowB = arrB; arrowC = arrC;
        fillA = fA; fillB = fB; fillC = fC;
        RefreshVisuals();
        RefreshReadings();
    }

    public void RefreshVisuals()
    {
        var asm = ResultantAssemblyManager.Instance;
        SetActive(trolley, asm != null && asm.TrolleyPlaced);
        SetActive(ring, asm != null && asm.RingPlaced);
        SetActive(strings, asm != null && asm.StringsPlaced);
        SetActive(pulley1, asm != null && asm.Pulley1Placed);
        SetActive(pulley2, asm != null && asm.Pulley2Placed);
        SetActive(balanceA, asm != null && asm.BalanceAPlaced);
        SetActive(balanceB, asm != null && asm.BalanceBPlaced);
        SetActive(balanceC, asm != null && asm.BalanceCPlaced);
        SetActive(wall, true);
    }

    public void RefreshReadings()
    {
        var force = ResultantForceController.Instance;
        float a = force != null ? force.ForceA : 0f;
        float b = force != null ? force.ForceB : 0f;
        float c = force != null ? force.ForceC : 0f;
        if (readingA != null) readingA.text = $"{a:0.0} N";
        if (readingB != null) readingB.text = $"{b:0.0} N";
        if (readingC != null) readingC.text = $"{c:0.0} N";
        if (fillA != null) fillA.fillAmount = Mathf.Clamp01(a / 20f);
        if (fillB != null) fillB.fillAmount = Mathf.Clamp01(b / 10f);
        if (fillC != null) fillC.fillAmount = Mathf.Clamp01(c / 10f);
        if (arrowA != null) arrowA.sizeDelta = new Vector2(Mathf.Lerp(40f, 180f, Mathf.Clamp01(a / 16f)), arrowA.sizeDelta.y);
        if (arrowB != null) arrowB.sizeDelta = new Vector2(Mathf.Lerp(40f, 140f, Mathf.Clamp01(b / 10f)), arrowB.sizeDelta.y);
        if (arrowC != null) arrowC.sizeDelta = new Vector2(Mathf.Lerp(40f, 140f, Mathf.Clamp01(c / 10f)), arrowC.sizeDelta.y);
    }

    private static void SetActive(GameObject go, bool on)
    {
        if (go != null) go.SetActive(on);
    }
}
