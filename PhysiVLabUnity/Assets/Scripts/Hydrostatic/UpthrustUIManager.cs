using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// HUD, step instructions, feedback toasts, and Phase 4 summary popup.
/// </summary>
public class UpthrustUIManager : MonoBehaviour
{
    public static UpthrustUIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text stepInstructionText;
    [SerializeField] private Text selectionProgressText;
    [SerializeField] private Text feedbackText;
    [SerializeField] private Slider scoreSlider;

    [Header("End Screen Popup")]
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text correctCountText;
    [SerializeField] private Text mistakeCountText;
    [SerializeField] private Text gradeText;
    [SerializeField] private Text starsText;
    [SerializeField] private Image[] resultStars;
    [SerializeField] private Button restartButton;

    [Header("Feedback Timing")]
    [SerializeField] private float feedbackDuration = 2.2f;
    [SerializeField] private Color feedbackGood = new Color(0.3f, 0.9f, 0.4f);
    [SerializeField] private Color feedbackBad = new Color(1f, 0.35f, 0.35f);

    private Coroutine feedbackRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        ClearFeedback();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        BindScoreEvents();
    }

    public void Configure(
        Text score,
        Text instruction,
        Text progress,
        Text feedback,
        GameObject endPanel,
        Text finalScore,
        Text correct,
        Text mistakes,
        Text grade,
        Text stars,
        Button restart)
    {
        scoreText = score;
        stepInstructionText = instruction;
        selectionProgressText = progress;
        feedbackText = feedback;
        endScreenPanel = endPanel;
        finalScoreText = finalScore;
        correctCountText = correct;
        mistakeCountText = mistakes;
        gradeText = grade;
        starsText = stars;
        restartButton = restart;

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClicked);
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        BindScoreEvents();
    }

    private void BindScoreEvents()
    {
        if (UpthrustScoreManager.Instance == null) return;

        UpthrustScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
        UpthrustScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
        HandleScoreChanged(UpthrustScoreManager.Instance.RawScore, UpthrustScoreManager.Instance.PercentageScore);
    }

    private void OnDisable()
    {
        if (UpthrustScoreManager.Instance != null)
            UpthrustScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
    }

    private void HandleScoreChanged(int raw, float percent)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {percent:F0} / 100";

        if (scoreSlider != null)
        {
            scoreSlider.minValue = 0f;
            scoreSlider.maxValue = 100f;
            scoreSlider.value = percent;
        }
    }

    public void UpdateSelectionProgress(int current, int required)
    {
        if (selectionProgressText != null)
            selectionProgressText.text = $"Apparatus selected: {current} / {required}";
    }

    public void ShowProgress(string text)
    {
        if (selectionProgressText != null)
            selectionProgressText.text = text;
    }

    public void ShowStepInstruction(int stepNumber, int totalSteps, string instruction)
    {
        if (stepInstructionText == null) return;

        if (stepNumber <= 0)
            stepInstructionText.text = instruction;
        else
            stepInstructionText.text = $"[{stepNumber}/{totalSteps}] {instruction}";
    }

    public void ShowFeedback(string message, bool positive)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = positive ? feedbackGood : feedbackBad;
        }

        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(ClearFeedbackAfterDelay());
    }

    private IEnumerator ClearFeedbackAfterDelay()
    {
        yield return new WaitForSeconds(feedbackDuration);
        ClearFeedback();
    }

    private void ClearFeedback()
    {
        if (feedbackText != null) feedbackText.text = string.Empty;
    }

    public void ShowEndScreen()
    {
        if (endScreenPanel != null)
            endScreenPanel.SetActive(true);

        UpthrustScoreManager score = UpthrustScoreManager.Instance;
        if (score == null) return;

        float percent = score.PercentageScore;
        int stars = score.GetStarRating();

        if (finalScoreText != null)
            finalScoreText.text = $"Total Score: {percent:F0} / 100";

        if (correctCountText != null)
            correctCountText.text = $"Correct Choices: {score.CorrectChoicesCount}";

        if (mistakeCountText != null)
            mistakeCountText.text = $"Incorrect Mistakes: {score.IncorrectMistakesCount}";

        if (gradeText != null)
            gradeText.text = $"Grade: {score.GetPerformanceGrade()}  ({stars} Star{(stars > 1 ? "s" : "")})";

        if (starsText != null)
        {
            string filled = new string('★', stars);
            string empty = new string('☆', 3 - stars);
            starsText.text = (filled + " " + empty).Trim();
        }

        if (resultStars != null)
        {
            for (int i = 0; i < resultStars.Length; i++)
            {
                if (resultStars[i] != null)
                    resultStars[i].enabled = i < stars;
            }
        }

        UpthrustObservationTableUI.Instance?.PopulateReview(endScreenPanel != null ? endScreenPanel.transform : null);
        UpthrustProfileManager.Instance?.RefreshProfileUI();
    }

    public void HideEndScreen()
    {
        if (endScreenPanel != null)
            endScreenPanel.SetActive(false);
    }

    private void OnRestartClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
