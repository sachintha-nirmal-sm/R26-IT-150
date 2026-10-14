using UnityEngine;
using UnityEngine.EventSystems;

public class ElectronicsIntroClickToStart : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var step = ElectronicsPracticalManager.Instance != null
            ? ElectronicsPracticalManager.Instance.CurrentStep
            : ElectronicsPracticalStep.Introduction;
        if (step == ElectronicsPracticalStep.Introduction)
            ElectronicsPracticalManager.Instance?.StartPractical();
    }
}
