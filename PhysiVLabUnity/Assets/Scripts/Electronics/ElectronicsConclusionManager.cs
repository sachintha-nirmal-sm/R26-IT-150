using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElectronicsConclusionManager : MonoBehaviour
{
    public static ElectronicsConclusionManager Instance { get; private set; }

    private readonly string[] correctOrder =
    {
        "A diode allows current to flow mainly in one direction.",
        "In forward bias, current can flow and the bulb glows.",
        "In reverse bias, the diode blocks current and the bulb does not glow."
    };

    private readonly List<string> built = new List<string>();
    private bool scored;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetBuilder()
    {
        built.Clear();
        scored = false;
        Refresh();
    }

    public void AddPhrase(string phrase)
    {
        if (scored) return;
        if (built.Count >= correctOrder.Length) return;
        built.Add(phrase);
        Refresh();
        if (built.Count == correctOrder.Length) Evaluate();
    }

    public void Evaluate()
    {
        if (scored) return;
        bool ok = built.Count == correctOrder.Length;
        for (int i = 0; i < correctOrder.Length && ok; i++)
            if (built[i] != correctOrder[i]) ok = false;

        scored = true;
        if (ok)
        {
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.Conclusion, 5, false);
            ElectronicsFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT CONCLUSION\nA diode allows current mainly in one direction. In forward bias the bulb glows; in reverse bias it does not.",
                "+5 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            ElectronicsUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            ElectronicsFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nArrange the three conclusion sentences in the correct order.",
                "-3 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            built.Clear();
            scored = false;
            Refresh();
        }
    }

    public string CurrentSentence()
    {
        if (built.Count == 0) return "Tap the three statements below, in the correct order.";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < built.Count; i++)
            sb.Append(i + 1).Append(".  ").Append(built[i]).Append("\n\n");
        return sb.ToString().TrimEnd();
    }

    private void Refresh()
    {
        ElectronicsUIManager.Instance?.SetConclusionPreview(CurrentSentence());
    }

    public IReadOnlyList<string> Phrases => correctOrder;
    public bool IsCorrect => scored;

    public void BindPhrases(Button[] buttons)
    {
        if (buttons == null) return;
        string[] shuffled = { correctOrder[1], correctOrder[2], correctOrder[0] };
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            var tmp = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            string phrase = i < shuffled.Length ? shuffled[i] : "";
            if (tmp != null) tmp.text = phrase;
            var choice = buttons[i].GetComponent<ElectronicsPhraseChoiceButton>() ?? buttons[i].gameObject.AddComponent<ElectronicsPhraseChoiceButton>();
            choice.Configure(phrase);
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(choice.Activate);
        }
    }
}
