using UnityEngine;

public class ConceptMatchingManager : MonoBehaviour
{
    public static ConceptMatchingManager Instance { get; private set; }

    private readonly bool[] matched = new bool[4];
    private int selectedConcept = -1;
    private bool complete;

    private static readonly string[] Concepts =
    {
        "Newton's First Law",
        "Newton's Second Law",
        "Newton's Third Law",
        "Weight"
    };

    private static readonly string[] Meanings =
    {
        "Inertia / uniform motion when resultant force is zero",
        "F = ma",
        "Equal and opposite action and reaction",
        "W = mg"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartMatching()
    {
        for (int i = 0; i < matched.Length; i++) matched[i] = false;
        selectedConcept = -1;
        complete = false;
        NewtonUIManager.Instance?.ShowMatching(Concepts, Meanings, matched);
    }

    public void SelectConcept(int index)
    {
        if (index < 0 || index >= Concepts.Length || matched[index]) return;
        selectedConcept = index;
        NewtonFeedbackManager.Instance?.ShowInstruction("Now choose the matching statement.");
    }

    public void SelectMeaning(int index)
    {
        if (selectedConcept < 0)
        {
            NewtonFeedbackManager.Instance?.ShowInstruction("Select a law or quantity first, then its meaning.");
            return;
        }
        if (index == selectedConcept)
        {
            matched[index] = true;
            NewtonScoreManager.Instance?.AddScore(5, false);
            NewtonFeedbackManager.Instance?.ShowMessage($"✓ {Concepts[index]} → {Meanings[index]}", "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
        }
        else
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage($"✗ {Concepts[selectedConcept]} does not match that statement.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
        selectedConcept = -1;
        NewtonUIManager.Instance?.ShowMatching(Concepts, Meanings, matched);
        complete = true;
        foreach (bool m in matched) if (!m) complete = false;
        if (complete) NewtonUIManager.Instance?.SetNextButtonVisible(true);
    }

    public bool IsComplete => complete;
}
