using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurningVisualController : MonoBehaviour
{
    public static TurningVisualController Instance { get; private set; }

    private GameObject table, stick, pivot, washer, screw, holeA, holeB, holeC, holeD;
    private GameObject loopA, loopB, loopC, loopD, balance, arrow;
    private TextMeshProUGUI reading, pointLabel;
    private Image forceFill;
    private RectTransform arrowRt;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(
        GameObject tableObj, GameObject stickObj, GameObject pivotObj, GameObject washerObj, GameObject screwObj,
        GameObject holeAObj, GameObject holeBObj, GameObject holeCObj, GameObject holeDObj,
        GameObject loopAObj, GameObject loopBObj, GameObject loopCObj, GameObject loopDObj,
        GameObject balanceObj, GameObject arrowObj,
        TextMeshProUGUI readingText, TextMeshProUGUI pointText, Image fill, RectTransform arrowRect)
    {
        table = tableObj; stick = stickObj; pivot = pivotObj; washer = washerObj; screw = screwObj;
        holeA = holeAObj; holeB = holeBObj; holeC = holeCObj; holeD = holeDObj;
        loopA = loopAObj; loopB = loopBObj; loopC = loopCObj; loopD = loopDObj;
        balance = balanceObj; arrow = arrowObj;
        reading = readingText; pointLabel = pointText; forceFill = fill; arrowRt = arrowRect;
        RefreshVisuals();
        RefreshReadings();
    }

    public void RefreshVisuals()
    {
        var asm = TurningAssemblyManager.Instance;
        SetActive(table, asm != null && asm.TablePlaced);
        SetActive(stick, asm != null && asm.StickPlaced);
        SetActive(pivot, asm != null && asm.HoleODrilled);
        SetActive(washer, asm != null && (asm.Washer1Placed || asm.Washer2Placed));
        SetActive(screw, asm != null && asm.ScrewPlaced);
        SetActive(holeA, asm != null && asm.HoleADrilled);
        SetActive(holeB, asm != null && asm.HoleBDrilled);
        SetActive(holeC, asm != null && asm.HoleCDrilled);
        SetActive(holeD, asm != null && asm.HoleDDrilled);
        SetActive(loopA, asm != null && asm.LoopsPlaced);
        SetActive(loopB, asm != null && asm.LoopsPlaced);
        SetActive(loopC, asm != null && asm.LoopsPlaced);
        SetActive(loopD, asm != null && asm.LoopsPlaced);
        var mom = TurningMomentController.Instance;
        SetActive(balance, mom != null && mom.BalanceAttached);
        SetActive(arrow, mom != null && mom.BalanceAttached);
    }

    public void RefreshReadings()
    {
        var mom = TurningMomentController.Instance;
        float f = mom != null ? mom.ForceN : 0f;
        float ang = mom != null ? mom.AngleDeg : 90f;
        if (reading != null) reading.text = $"{f:0.0} N";
        if (pointLabel != null)
        {
            string p = mom != null && mom.BalanceAttached ? mom.AttachedPoint : "—";
            pointLabel.text = $"Point {p}";
        }
        if (forceFill != null) forceFill.fillAmount = Mathf.Clamp01(f / 12f);
        if (arrowRt != null)
        {
            float len = Mathf.Lerp(40f, 220f, Mathf.Clamp01(f / 8f));
            arrowRt.sizeDelta = new Vector2(len, arrowRt.sizeDelta.y);
            arrowRt.localEulerAngles = new Vector3(0f, 0f, 90f - ang);
        }
        if (stick != null && mom != null && mom.StickJustMoves)
        {
            float wiggle = Mathf.Sin(Time.unscaledTime * 8f) * 1.4f;
            stick.transform.localEulerAngles = new Vector3(0f, 0f, wiggle);
        }
        else if (stick != null)
        {
            stick.transform.localEulerAngles = Vector3.zero;
        }
    }

    private static void SetActive(GameObject go, bool on)
    {
        if (go != null) go.SetActive(on);
    }
}
