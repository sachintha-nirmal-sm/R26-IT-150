using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeverFeedbackManager : MonoBehaviour
{
    public static LeverFeedbackManager Instance { get; private set; }

    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TextMeshProUGUI scoreChangeText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float displayDuration = 2f;

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
        ShowMessage("✓ Correct!", scoreChange, new Color(0.1f, 0.55f, 0.2f));

    public void ShowIncorrect(string scoreChange = "-5 Marks") =>
        ShowMessage("✗ Incorrect!", scoreChange, new Color(0.75f, 0.15f, 0.15f));

    public void ShowInstruction(string message) =>
        ShowMessage(message, "", new Color(0.15f, 0.35f, 0.65f));

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
        float hold = message != null && message.Length > 80 ? displayDuration + 2.5f : displayDuration;
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
        while (t < 0.25f) { t += Time.deltaTime; canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.25f); yield return null; }
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(holdSeconds);
        t = 0f;
        while (t < 0.35f) { t += Time.deltaTime; canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.35f); yield return null; }
        HideImmediate();
    }

    public void HideImmediate()
    {
        if (feedbackPanel != null) feedbackPanel.SetActive(false);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
}
