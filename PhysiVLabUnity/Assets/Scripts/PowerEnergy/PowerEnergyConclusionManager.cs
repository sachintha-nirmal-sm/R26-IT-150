using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerEnergyConclusionManager : MonoBehaviour
{
    public static PowerEnergyConclusionManager Instance { get; private set; }

    private readonly string[] correctOrder =
    {
        "The electrical power of an appliance tells us the rate at which it consumes electrical energy.",
        "Electrical energy consumed increases when the power or operating time increases.",
        "Power can be calculated using P = VI and energy can be calculated using E = Pt."
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
            PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Conclusion, 10, false);
            PowerEnergyFeedbackManager.Instance?.ShowMessage(
                "✓ CORRECT CONCLUSION\nPower is the rate of energy use. Energy increases with power or time. P = VI and E = Pt.",
                "+10 MARKS",
                new Color(0.08f, 0.52f, 0.22f));
            PowerEnergyUIManager.Instance?.SetNextButtonVisible(true);
        }
        else
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(5);
            PowerEnergyFeedbackManager.Instance?.ShowMessage(
                "✗ INCORRECT\nRebuild the three conclusion sentences in the correct order.",
                "-5 MARKS",
                new Color(0.75f, 0.12f, 0.12f));
            built.Clear();
            scored = false;
            Refresh();
        }
    }

    public string CurrentSentence()
    {
        if (built.Count == 0) return "Tap the sentences in the correct order to build the conclusion.";
        return string.Join("\n\n", built);
    }

    private void Refresh()
    {
        PowerEnergyUIManager.Instance?.SetConclusionPreview(CurrentSentence());
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
            var choice = buttons[i].GetComponent<PowerEnergyPhraseChoiceButton>() ?? buttons[i].gameObject.AddComponent<PowerEnergyPhraseChoiceButton>();
            choice.Configure(phrase);
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(choice.Activate);
        }
    }
}
