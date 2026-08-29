using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FrictionEquipmentCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private TextMeshProUGUI label;
    private Image iconImage;
    private Button button;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform homeParent;
    private int homeIndex;
    private Vector2 homePos;
    private Vector2 dragStartScreen;
    private bool dragging;

    public FrictionEquipmentDefinition Definition { get; private set; }
    private FrictionEquipmentSelectionManager manager;

    public void Initialize(FrictionEquipmentDefinition def, FrictionEquipmentSelectionManager mgr)
    {
        Definition = def;
        manager = mgr;
        EnsureLayout();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        homeParent = transform.parent;
        homeIndex = transform.GetSiblingIndex();
        if (iconImage != null)
        {
            iconImage.sprite = FrictionIconFactory.GetSprite(def.type);
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
        }
        if (label != null)
        {
            label.text = def.displayName;
            label.fontSize = 24;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.10f, 0.14f, 0.20f);
            label.enableAutoSizing = true;
            label.fontSizeMin = 16;
            label.fontSizeMax = 24;
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
        if (label != null)
        {
            label.fontSize = 13;
            label.enableAutoSizing = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
        }
        var le = GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 128f;
        le.preferredHeight = 88f;
        le.flexibleWidth = 1f;
        le.minWidth = 88f;
        le.minHeight = 72f;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Definition == null) return;
        dragging = true;
        dragStartScreen = eventData.position;
        homeParent = transform.parent;
        homeIndex = transform.GetSiblingIndex();
        var rt = transform as RectTransform;
        if (rt != null) homePos = rt.anchoredPosition;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        if (canvas != null) transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || canvas == null) return;
        var rt = transform as RectTransform;
        if (rt == null) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            rt.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        if (Vector2.Distance(eventData.position, dragStartScreen) < 28f)
        {
            manager?.SelectEquipment(Definition);
            ReturnHome();
            return;
        }

        FrictionUIDropTarget target = null;
        if (EventSystem.current != null)
        {
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (var hit in results)
            {
                target = hit.gameObject.GetComponentInParent<FrictionUIDropTarget>();
                if (target != null) break;
            }
        }

        if (target != null && target.ZoneId == "RequiredEquipment")
        {
            manager?.SelectEquipment(Definition);
            return;
        }

        ReturnHome();
    }

    private void ReturnHome()
    {
        if (homeParent != null) transform.SetParent(homeParent, false);
        transform.SetSiblingIndex(homeIndex);
        var rt = transform as RectTransform;
        if (rt != null) rt.anchoredPosition = homePos;
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
            rt.anchorMin = new Vector2(0.08f, 0.38f);
            rt.anchorMax = new Vector2(0.92f, 0.96f);
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
            rt.anchorMax = new Vector2(0.96f, 0.36f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            label = labelObj.AddComponent<TextMeshProUGUI>();
            label.raycastTarget = false;
            label.color = new Color(0.10f, 0.14f, 0.20f);
            if (label.font == null) label.font = TMP_Settings.defaultFontAsset;
        }
        var bg = GetComponent<Image>();
        if (bg != null)
            bg.color = Color.Lerp(FrictionIconFactory.GetColor(Definition != null ? Definition.type : FrictionEquipmentType.WoodenBlock), Color.white, 0.45f);
    }
}
