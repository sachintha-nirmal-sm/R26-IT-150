using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpticsConclusionManager : MonoBehaviour
{
    public static OpticsConclusionManager Instance { get; private set; }

    private readonly string[] correctOrder =
    {
        "Because light rays from a far-away object can be considered as parallel,",
        "they meet at the principal focus of the concave mirror,",
        "so the distance from the mirror to the sharp real image on the screen",
        "is approximately equal to the focal length of the mirror."
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
            OpticsScoreManager.Instance?.AddScore(10, false);
            OpticsFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT CONCLUSION\nThe mirror–screen distance for a sharp image of a distant object is approximately the focal length.",
                "+10 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            OpticsUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            OpticsScoreManager.Instance?.SubtractScore(5);
            OpticsFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nRebuild: distant object → parallel rays → meet at F → mirror–screen distance ≈ focal length.",
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
        OpticsUIManager.Instance?.SetConclusionPreview(CurrentSentence());
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
            var choice = buttons[i].GetComponent<OpticsPhraseChoiceButton>() ?? buttons[i].gameObject.AddComponent<OpticsPhraseChoiceButton>();
            choice.Configure(phrase);
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(choice.Activate);
        }
    }
}
