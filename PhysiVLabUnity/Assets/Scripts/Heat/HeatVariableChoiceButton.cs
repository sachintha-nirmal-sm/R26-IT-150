using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeatVariableChoiceButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string itemId;
    [SerializeField] private string zoneId;

    public void Configure(string item, string zone)
    {
        itemId = item;
        zoneId = zone;
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(Activate);
        }
    }

    public void OnPointerClick(PointerEventData eventData) => Activate();

    public void Activate()
    {
        if (string.IsNullOrEmpty(itemId) || string.IsNullOrEmpty(zoneId)) return;
        HeatVariableMatchingManager.Instance?.TapAssign(itemId, zoneId);
    }
}
