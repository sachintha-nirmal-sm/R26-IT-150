using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquilibriumConclusionManager : MonoBehaviour
{
    public static EquilibriumConclusionManager Instance { get; private set; }

    private readonly string[] correctOrder =
    {
        "For a meter ruler in equilibrium",
        "under three coplanar parallel forces,",
        "the two upward forces F1 and F2",
        "add up to the weight W of the ruler."
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
            EquilibriumScoreManager.Instance?.AddScore(10, false);
            EquilibriumFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT CONCLUSION\nFor a meter ruler in equilibrium under three coplanar parallel forces, F1 + F2 = W.",
                "+10 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            EquilibriumUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            EquilibriumScoreManager.Instance?.SubtractScore(5);
            EquilibriumFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nRebuild the conclusion: F1 + F2 = W for a horizontal ruler under three coplanar parallel forces.",
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
        EquilibriumUIManager.Instance?.SetConclusionPreview(CurrentSentence());
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
            var choice = buttons[i].GetComponent<EquilibriumPhraseChoiceButton>() ?? buttons[i].gameObject.AddComponent<EquilibriumPhraseChoiceButton>();
            choice.Configure(phrase);
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(choice.Activate);
        }
    }
}
