using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConclusionManager : MonoBehaviour
{
    public static ConclusionManager Instance { get; private set; }

    private readonly string[] correctOrder =
    {
        "The limiting frictional force",
        "does not significantly depend on",
        "the area of contact",
        "when the weight and surface roughness remain constant."
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

    public void Undo()
    {
        if (scored || built.Count == 0) return;
        built.RemoveAt(built.Count - 1);
        Refresh();
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
            FrictionScoreManager.Instance?.AddScore(10, false);
            FrictionFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT CONCLUSION\nThe limiting frictional force does not depend significantly on the area of contact when the weight of the block and the roughness of the surfaces are kept constant.",
                "+10 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            FrictionUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            FrictionScoreManager.Instance?.SubtractScore(5);
            FrictionFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nRebuild the conclusion: limiting friction does not significantly depend on contact area when weight and roughness stay constant.",
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
        FrictionUIManager.Instance?.SetConclusionPreview(CurrentSentence());
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
            var choice = buttons[i].GetComponent<PhraseChoiceButton>() ?? buttons[i].gameObject.AddComponent<PhraseChoiceButton>();
            choice.Configure(phrase);
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(choice.Activate);
        }
    }
}
