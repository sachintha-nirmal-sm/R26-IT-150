using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResultantPhraseChoiceButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string phrase;

    public void Configure(string text)
    {
        phrase = text;
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
        if (string.IsNullOrEmpty(phrase))
        {
            var tmp = GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) phrase = tmp.text;
        }
        if (!string.IsNullOrEmpty(phrase))
            ResultantConclusionManager.Instance?.AddPhrase(phrase);
    }
}
