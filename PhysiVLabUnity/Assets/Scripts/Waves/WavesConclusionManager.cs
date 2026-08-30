using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WavesConclusionManager : MonoBehaviour
{
    public static WavesConclusionManager Instance { get; private set; }

    private readonly string[] correctOrder =
    {
        "When a slinky is shaken from side to side,",
        "a transverse wave travels along the slinky,",
        "while the ribbons (particles of the medium)",
        "move perpendicular to the direction of the wave."
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
            WavesScoreManager.Instance?.AddScore(10, false);
            WavesFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT CONCLUSION\nA transverse wave travels along the slinky while particles move perpendicular to the wave direction.",
                "+10 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            WavesUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            WavesScoreManager.Instance?.SubtractScore(5);
            WavesFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nRebuild: side-to-side shaking → transverse wave along the slinky; ribbons move perpendicular to the wave.",
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
        WavesUIManager.Instance?.SetConclusionPreview(CurrentSentence());
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
            var choice = buttons[i].GetComponent<WavesPhraseChoiceButton>() ?? buttons[i].gameObject.AddComponent<WavesPhraseChoiceButton>();
            choice.Configure(phrase);
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(choice.Activate);
        }
    }
}
