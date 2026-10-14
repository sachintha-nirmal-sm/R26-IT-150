using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeverEquipmentCardUI : MonoBehaviour
{
    private TextMeshProUGUI label;
    private Image iconImage;
    private Button button;
    public LeverEquipmentDefinition Definition { get; private set; }
    private LeverEquipmentSelectionManager manager;

    public void Initialize(LeverEquipmentDefinition def, LeverEquipmentSelectionManager mgr)
    {
        Definition = def;
        manager = mgr;
        EnsureLayout();
        if (iconImage != null)
        {
            iconImage.sprite = LeverIconFactory.GetSprite(def.type);
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
        }
        if (label != null)
        {
            label.text = def.displayName;
            label.fontSize = 22;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
        }
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => manager?.SelectEquipment(Definition));
        }
        gameObject.SetActive(true);
    }

    public void SetCompactMode()
    {
        if (label != null) label.fontSize = 14;
        var le = GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 170f;
        le.preferredHeight = 100f;
    }

    private void EnsureLayout()
    {
        button = GetComponent<Button>() ?? gameObject.AddComponent<Button>();
        iconImage = transform.Find("IconImage")?.GetComponent<Image>();
        label = transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
        if (iconImage == null)
        {
            var iconObj = new GameObject("IconImage");
            iconObj.transform.SetParent(transform, false);
            var rt = iconObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.08f, 0.34f);
            rt.anchorMax = new Vector2(0.92f, 0.94f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            iconImage = iconObj.AddComponent<Image>();
            iconImage.raycastTarget = false;
        }
        if (label == null)
        {
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(transform, false);
            var rt = labelObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.04f, 0.02f);
            rt.anchorMax = new Vector2(0.96f, 0.32f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            label = labelObj.AddComponent<TextMeshProUGUI>();
            label.raycastTarget = false;
        }
        var bg = GetComponent<Image>();
        if (bg != null) bg.color = Color.Lerp(LeverIconFactory.GetColor(Definition?.type ?? LeverEquipmentType.Book), Color.white, 0.35f);
    }
}
