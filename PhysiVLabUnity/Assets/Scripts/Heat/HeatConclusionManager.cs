using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeatConclusionManager : MonoBehaviour
{
    public static HeatConclusionManager Instance { get; private set; }

    private readonly string[] correctOrder =
    {
        "When heat is applied, the glass container expands first,",
        "so the liquid level falls slightly from A to B.",
        "Then the liquid expands more than the glass,",
        "so the level rises from B, past A, to C."
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
            HeatScoreManager.Instance?.AddScore(10, false);
            HeatFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT CONCLUSION\nBoth glass and liquid expand. The liquid expands more, so the final level is C.",
                "+10 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            HeatUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            HeatScoreManager.Instance?.SubtractScore(5);
            HeatFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nRebuild: glass expands first → level falls A to B → liquid expands more → level rises to C.",
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
        HeatUIManager.Instance?.SetConclusionPreview(CurrentSentence());
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
            var choice = buttons[i].GetComponent<HeatPhraseChoiceButton>() ?? buttons[i].gameObject.AddComponent<HeatPhraseChoiceButton>();
            choice.Configure(phrase);
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(choice.Activate);
        }
    }
}
