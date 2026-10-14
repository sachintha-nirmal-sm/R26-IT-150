using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultantConclusionManager : MonoBehaviour
{
    public static ResultantConclusionManager Instance { get; private set; }

    private readonly string[] correctOrder =
    {
        "The resultant force",
        "is equal to the sum of",
        "the two forces acting",
        "in the same direction."
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
            ResultantScoreManager.Instance?.AddScore(10, false);
            ResultantFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT CONCLUSION\nThe resultant force is equal to the sum of the two forces acting in the same direction. A = B + C.",
                "+10 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            ResultantUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            ResultantScoreManager.Instance?.SubtractScore(5);
            ResultantFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nRebuild the conclusion: the resultant force equals the sum of the two forces acting in the same direction.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            built.Clear();
            scored = false;
            Refresh();
        }
    }

    public string CurrentSentence()
    {
        if (built.Count == 0) return "Tap the phrases in the correct order to build the conclusion.";
        return string.Join(" ", built);
    }

    private void Refresh()
    {
        ResultantUIManager.Instance?.SetConclusionPreview(CurrentSentence());
    }

    public IReadOnlyList<string> Phrases => correctOrder;
    public bool IsCorrect => scored;

    public void BindPhrases(Button[] buttons)
    {
        if (buttons == null) return;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            var tmp = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            string phrase = tmp != null ? tmp.text : (i < correctOrder.Length ? correctOrder[i] : "");
            var choice = buttons[i].GetComponent<ResultantPhraseChoiceButton>() ?? buttons[i].gameObject.AddComponent<ResultantPhraseChoiceButton>();
            choice.Configure(phrase);
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(choice.Activate);
        }
    }
}
