using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Saves / loads Upthrust practical profile data via PlayerPrefs.
/// Keys: UpthrustPracticalScore, UpthrustPracticalCompleted, UpthrustPracticalDate.
/// </summary>
public class UpthrustProfileManager : MonoBehaviour
{
    public static UpthrustProfileManager Instance { get; private set; }

    public const string KEY_SCORE = "UpthrustPracticalScore";
    public const string KEY_COMPLETED = "UpthrustPracticalCompleted";
    public const string KEY_DATE = "UpthrustPracticalDate";
    public const string KEY_STUDENT_NAME = "UpthrustStudentName";

    [Header("Profile UI (optional — assign on a profile / menu screen)")]
    [SerializeField] private Text highestScoreText;
    [SerializeField] private Text completionStatusText;
    [SerializeField] private Text lastCompletedDateText;
    [SerializeField] private Text studentNameText;
    [SerializeField] private Image[] starImages;

    public float HighestScore => PlayerPrefs.GetFloat(KEY_SCORE, 0f);
    public bool IsCompleted => PlayerPrefs.GetInt(KEY_COMPLETED, 0) == 1;
    public string CompletionDate => PlayerPrefs.GetString(KEY_DATE, "—");
    public string StudentName => PlayerPrefs.GetString(KEY_STUDENT_NAME, "Student");

    public event Action OnProfileUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        RefreshProfileUI();
    }

    /// <summary>
    /// Saves the result. Only overwrites the stored high score if the new % is higher.
    /// Always updates completion flag and date when completed.
    /// </summary>
    public void SavePracticalResult(float finalPercentage, bool completed)
    {
        float clamped = Mathf.Clamp(finalPercentage, 0f, 100f);

        if (clamped > HighestScore)
            PlayerPrefs.SetFloat(KEY_SCORE, clamped);

        if (completed)
        {
            PlayerPrefs.SetInt(KEY_COMPLETED, 1);
            PlayerPrefs.SetString(KEY_DATE, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        }

        PlayerPrefs.Save();
        RefreshProfileUI();
        OnProfileUpdated?.Invoke();

        Debug.Log($"[UpthrustProfileManager] Saved — Score: {clamped:F1}% (Best: {HighestScore:F1}%)");
    }

    public void SetStudentName(string name)
    {
        PlayerPrefs.SetString(KEY_STUDENT_NAME, string.IsNullOrWhiteSpace(name) ? "Student" : name.Trim());
        PlayerPrefs.Save();
        RefreshProfileUI();
    }

    public void RefreshProfileUI()
    {
        if (highestScoreText != null) highestScoreText.text = $"{HighestScore:F1}%";
        if (completionStatusText != null) completionStatusText.text = IsCompleted ? "Completed" : "Not Completed";
        if (lastCompletedDateText != null) lastCompletedDateText.text = CompletionDate;
        if (studentNameText != null) studentNameText.text = StudentName;
        UpdateStars(HighestScore);
    }

    public void BindProfileTexts(Text score, Text status, Text date, Text name, Image[] stars)
    {
        highestScoreText = score;
        completionStatusText = status;
        lastCompletedDateText = date;
        studentNameText = name;
        starImages = stars;
        RefreshProfileUI();
    }

    private void UpdateStars(float percent)
    {
        if (starImages == null || starImages.Length == 0) return;

        int stars = 1;
        if (percent >= 80f) stars = 3;
        else if (percent >= 50f) stars = 2;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
                starImages[i].enabled = i < stars;
        }
    }

    [ContextMenu("Clear Upthrust Profile Data")]
    public void ClearProfileData()
    {
        PlayerPrefs.DeleteKey(KEY_SCORE);
        PlayerPrefs.DeleteKey(KEY_COMPLETED);
        PlayerPrefs.DeleteKey(KEY_DATE);
        PlayerPrefs.Save();
        RefreshProfileUI();
    }
}
