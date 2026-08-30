using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// PHASE 1 — Apparatus Identification.
/// Tray contains 5 correct items + distractors. Selecting all 5 unlocks Phase 2.
/// Scoring: correct +10, incorrect −5.
/// </summary>
public class UpthrustEquipmentSelector : MonoBehaviour
{
    public static UpthrustEquipmentSelector Instance { get; private set; }

    [Header("Selection Rules")]
    [SerializeField] private int requiredCorrectCount = UpthrustPracticalData.CorrectApparatusCount;

    [Header("UI References")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private RectTransform dropZone;
    [SerializeField] private Button confirmSelectionButton;
    [SerializeField] private Text confirmButtonLabel;
    [SerializeField] private GameObject phase2Root;

    [Header("Apparatus In Tray")]
    [SerializeField] private List<UpthrustApparatusItem> allApparatusItems = new List<UpthrustApparatusItem>();

    [Header("Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctClip;
    [SerializeField] private AudioClip wrongClip;

    private readonly HashSet<UpthrustApparatusType> selectedCorrectTypes = new HashSet<UpthrustApparatusType>();

    private int correctSelectedCount;
    private bool phase1Complete;

    public int CorrectSelectedCount => correctSelectedCount;
    public int RequiredCorrectCount => requiredCorrectCount;
    public bool Phase1Complete => phase1Complete;

    public event Action<int, int> OnSelectionProgressChanged;
    public event Action OnPhase1Completed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (confirmSelectionButton != null)
        {
            confirmSelectionButton.interactable = false;
            confirmSelectionButton.onClick.AddListener(UnlockPhase2);
        }

        if (phase2Root != null)
            phase2Root.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        if (allApparatusItems == null || allApparatusItems.Count == 0)
            allApparatusItems = new List<UpthrustApparatusItem>(GetComponentsInChildren<UpthrustApparatusItem>(true));

        NotifyProgress();
        RefreshConfirmButton();
    }

    public void Configure(
        GameObject panel,
        RectTransform zone,
        Button confirmButton,
        Text confirmLabel,
        GameObject phase2,
        List<UpthrustApparatusItem> items)
    {
        selectionPanel = panel;
        dropZone = zone;
        confirmSelectionButton = confirmButton;
        confirmButtonLabel = confirmLabel;
        phase2Root = phase2;
        allApparatusItems = items ?? new List<UpthrustApparatusItem>();

        if (confirmSelectionButton != null)
        {
            confirmSelectionButton.onClick.RemoveListener(UnlockPhase2);
            confirmSelectionButton.onClick.AddListener(UnlockPhase2);
        }

        RefreshConfirmButton();
        NotifyProgress();
    }

    /// <summary>
    /// Called by UpthrustApparatusItem when clicked or dropped on the zone.
    /// Returns true if the selection was a correct (and new) item.
    /// </summary>
    public bool ProcessSelection(UpthrustApparatusItem item)
    {
        if (phase1Complete || item == null) return false;

        if (item.IsCorrectApparatus)
        {
            if (selectedCorrectTypes.Contains(item.Type))
                return true;

            selectedCorrectTypes.Add(item.Type);
            correctSelectedCount++;
            UpthrustScoreManager.Instance?.RegisterCorrectApparatus();
            PlayFeedback(true);
            NotifyProgress();
            RefreshConfirmButton();

            UpthrustUIManager.Instance?.ShowFeedback(
                $"Correct! +10   ({correctSelectedCount}/{requiredCorrectCount})", true);

            if (correctSelectedCount >= requiredCorrectCount && confirmSelectionButton == null)
                UnlockPhase2();

            return true;
        }

        UpthrustScoreManager.Instance?.RegisterWrongApparatus();
        PlayFeedback(false);
        UpthrustUIManager.Instance?.ShowFeedback("Wrong apparatus! −5", false);
        return false;
    }

    public bool IsPointerOverDropZone(PointerEventData eventData)
    {
        if (dropZone == null) return true;

        return RectTransformUtility.RectangleContainsScreenPoint(
            dropZone,
            eventData.position,
            eventData.pressEventCamera);
    }

    public void UnlockPhase2()
    {
        if (correctSelectedCount < requiredCorrectCount) return;
        if (phase1Complete) return;

        phase1Complete = true;

        if (selectionPanel != null)
            selectionPanel.SetActive(false);

        if (phase2Root != null)
            phase2Root.SetActive(true);

        UpthrustPracticalManager.Instance?.BeginPractical();
        OnPhase1Completed?.Invoke();
        Debug.Log("[UpthrustEquipmentSelector] Phase 1 complete — Phase 2 unlocked.");
    }

    public void ResetSelection()
    {
        phase1Complete = false;
        correctSelectedCount = 0;
        selectedCorrectTypes.Clear();

        foreach (var item in allApparatusItems)
        {
            if (item != null) item.ResetItem();
        }

        if (selectionPanel != null)
            selectionPanel.SetActive(true);

        if (phase2Root != null)
            phase2Root.SetActive(false);

        RefreshConfirmButton();
        NotifyProgress();
    }

    private void RefreshConfirmButton()
    {
        bool ready = correctSelectedCount >= requiredCorrectCount;

        if (confirmSelectionButton != null)
            confirmSelectionButton.interactable = ready;

        if (confirmButtonLabel != null)
        {
            confirmButtonLabel.text = ready
                ? "START PRACTICAL ▶"
                : $"Select {Mathf.Max(0, requiredCorrectCount - correctSelectedCount)} more correct item(s)...";
        }

        if (confirmSelectionButton != null)
        {
            var img = confirmSelectionButton.GetComponent<Image>();
            if (img != null)
                img.color = ready
                    ? new Color(0.30f, 0.78f, 0.42f, 1f)
                    : new Color(0.35f, 0.38f, 0.42f, 1f);
        }
    }

    private void NotifyProgress()
    {
        OnSelectionProgressChanged?.Invoke(correctSelectedCount, requiredCorrectCount);
        UpthrustUIManager.Instance?.UpdateSelectionProgress(correctSelectedCount, requiredCorrectCount);
    }

    private void PlayFeedback(bool correct)
    {
        if (audioSource == null) return;
        AudioClip clip = correct ? correctClip : wrongClip;
        if (clip != null) audioSource.PlayOneShot(clip);
    }
}
