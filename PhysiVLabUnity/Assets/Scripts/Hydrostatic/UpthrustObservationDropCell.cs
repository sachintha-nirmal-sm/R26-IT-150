using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Drop target for one observation-table cell.
/// </summary>
public class UpthrustObservationDropCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private int stageIndex;
    [SerializeField] private UpthrustObservationTableUI.Column column;
    [SerializeField] private Text valueLabel;
    [SerializeField] private Image background;

    static readonly Color Hover = new Color(0.30f, 0.42f, 0.52f, 1f);
    static readonly Color Normal = new Color(0.22f, 0.30f, 0.38f, 1f);

    public int StageIndex => stageIndex;
    public UpthrustObservationTableUI.Column Column => column;
    public Text ValueLabel => valueLabel;
    public Image Background => background;

    public void Configure(int stage, UpthrustObservationTableUI.Column col, Text label, Image bg)
    {
        stageIndex = stage;
        column = col;
        valueLabel = label;
        background = bg;
        if (background != null)
            background.raycastTarget = true;
        SetDisplay(string.Empty);
    }

    public void ReceiveDrop(float valueN)
    {
        UpthrustObservationTableUI.Instance?.ApplyDroppedValue(this, valueN);
    }

    public void SetDisplay(string text)
    {
        if (valueLabel == null) return;

        bool empty = string.IsNullOrEmpty(text) || text == "drop here";
        valueLabel.text = empty ? "drop here" : text;
        valueLabel.color = empty ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
        valueLabel.fontStyle = empty ? FontStyle.Normal : FontStyle.Bold;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (background != null && UpthrustObservationTableUI.Instance != null && !UpthrustObservationTableUI.Instance.IsCellLocked(this))
            background.color = Hover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpthrustObservationTableUI.Instance?.RestoreCellColor(this);
    }
}
