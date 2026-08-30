using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ElectronicsTerminalTap : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string terminalId;

    public void Configure(string id) => terminalId = id;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(terminalId))
            ElectronicsWireController.Instance?.TapTerminal(terminalId);
    }
}
