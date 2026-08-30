using System.Collections;
using TMPro;
using UnityEngine;

public class NewtonFeedbackManager : MonoBehaviour
{
    public static NewtonFeedbackManager Instance { get; private set; }

    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TextMeshProUGUI scoreChangeText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float displayDuration = 2.4f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        HideImmediate();
    }

    public void BindUI(GameObject panel, TextMeshProUGUI message, TextMeshProUGUI scoreChange, CanvasGroup group)
    {
        feedbackPanel = panel;
        feedbackText = message;
        scoreChangeText = scoreChange;
        canvasGroup = group;
        HideImmediate();
    }

    public void ShowCorrect(string scoreChange = "+5 Marks") =>
        ShowMessage("✓ CORRECT", scoreChange, new Color(0.08f, 0.52f, 0.22f));

    public void ShowIncorrect(string scoreChange = "-5 Marks") =>
        ShowMessage("✗ INCORRECT", scoreChange, new Color(0.75f, 0.12f, 0.12f));

    public void ShowInstruction(string message) =>
        ShowMessage(message, "", new Color(0.12f, 0.32f, 0.62f));

    public void ShowMessage(string message, string scoreChange, Color color)
    {
        if (feedbackPanel == null || feedbackText == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        feedbackPanel.SetActive(true);
        feedbackText.text = message;
        feedbackText.color = color;
        if (scoreChangeText != null)
        {
            scoreChangeText.text = scoreChange;
            scoreChangeText.gameObject.SetActive(!string.IsNullOrEmpty(scoreChange));
        }
        float hold = message != null && message.Length > 90 ? displayDuration + 2.6f : displayDuration;
        fadeRoutine = StartCoroutine(FadeRoutine(hold));
    }

    private IEnumerator FadeRoutine(float holdSeconds)
    {
        if (canvasGroup == null)
        {
            yield return new WaitForSeconds(holdSeconds);
            HideImmediate();
            yield break;
        }

        canvasGroup.alpha = 0f;
        float t = 0f;
        while (t < 0.2f) { t += Time.deltaTime; canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.2f); yield return null; }
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(holdSeconds);
        t = 0f;
        while (t < 0.3f) { t += Time.deltaTime; canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.3f); yield return null; }
        HideImmediate();
    }

    public void HideImmediate()
    {
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
}
