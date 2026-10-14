using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Phase 1 tray card. Uses a Button so Input System UI clicks always register.
/// </summary>
public class UpthrustApparatusItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Identity")]
    [SerializeField] private UpthrustApparatusType apparatusType;
    [SerializeField] private string displayName;
    [SerializeField] private bool isCorrectApparatus = true;

    [Header("Visual Feedback")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cardImage;
    [SerializeField] private Text checkLabel;
    [SerializeField] private Color normalColor = new Color(0.26f, 0.34f, 0.42f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.16f, 0.48f, 0.28f, 1f);
    [SerializeField] private Color hoverColor = new Color(0.32f, 0.42f, 0.52f, 1f);
    [SerializeField] private Color wrongFlashColor = new Color(0.90f, 0.32f, 0.32f, 1f);

    private bool hasBeenSelected;
    private Button button;

    public UpthrustApparatusType Type => apparatusType;
    public string DisplayName => string.IsNullOrEmpty(displayName) ? UpthrustApparatusTypeUtil.DisplayName(apparatusType) : displayName;
    public bool IsCorrectApparatus => isCorrectApparatus;
    public bool HasBeenSelected => hasBeenSelected;

    private void Awake()
    {
        if (cardImage == null)
            cardImage = GetComponent<Image>();

        if (checkLabel != null)
            checkLabel.text = string.Empty;

        EnsureClickable();
        ApplyCardColor(normalColor);
    }

    public void Configure(UpthrustApparatusType type, bool correct, Image card, Image icon, Text check, Color normal)
    {
        apparatusType = type;
        isCorrectApparatus = correct;
        displayName = UpthrustApparatusTypeUtil.DisplayName(type);
        cardImage = card;
        iconImage = icon;
        checkLabel = check;
        normalColor = normal;
        EnsureClickable();
        ApplyCardColor(normalColor);
    }

    public void EnsureClickable()
    {
        if (cardImage == null)
            cardImage = GetComponent<Image>();

        if (cardImage != null)
            cardImage.raycastTarget = true;

        foreach (var graphic in GetComponentsInChildren<Graphic>(true))
        {
            if (graphic.gameObject == gameObject) continue;
            graphic.raycastTarget = false;
        }

        button = GetComponent<Button>();
        if (button == null)
            button = gameObject.AddComponent<Button>();

        button.transition = Selectable.Transition.None;
        button.interactable = true;
        button.targetGraphic = cardImage;
        button.onClick.RemoveListener(TrySelect);
        button.onClick.AddListener(TrySelect);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!hasBeenSelected)
            ApplyCardColor(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!hasBeenSelected)
            ApplyCardColor(normalColor);
    }

    public void TrySelect()
    {
        if (hasBeenSelected) return;

        UpthrustEquipmentSelector selector = UpthrustEquipmentSelector.Instance;
        if (selector == null)
            selector = FindAnyObjectByType<UpthrustEquipmentSelector>();

        if (selector == null)
        {
            Debug.LogWarning("[UpthrustApparatusItem] No UpthrustEquipmentSelector in scene.");
            return;
        }

        bool accepted = selector.ProcessSelection(this);
        if (accepted)
            MarkSelected(true);
        else
            FlashWrong();
    }

    public void MarkSelected(bool selected)
    {
        hasBeenSelected = selected;
        ApplyCardColor(selected ? selectedColor : normalColor);
        if (checkLabel != null)
            checkLabel.text = selected ? "✓" : string.Empty;
    }

    public void ResetItem()
    {
        hasBeenSelected = false;
        ApplyCardColor(normalColor);
        if (checkLabel != null)
            checkLabel.text = string.Empty;
    }

    private void FlashWrong()
    {
        ApplyCardColor(wrongFlashColor);
        CancelInvoke(nameof(RestoreNormalColor));
        Invoke(nameof(RestoreNormalColor), 0.45f);
    }

    private void RestoreNormalColor()
    {
        if (!hasBeenSelected)
            ApplyCardColor(normalColor);
    }

    private void ApplyCardColor(Color c)
    {
        if (cardImage != null)
            cardImage.color = c;
    }
}
