using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Holds all UI references for the Lever practical. Wired at runtime by LeverSceneRuntimeBuilder
/// or assigned in the Inspector and consumed by LeverUIManager.BindAll.
/// </summary>
public class LeverUIRefsHolder : MonoBehaviour
{
    public int UiVersion;

    [Header("Header")]
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Score;
    public TextMeshProUGUI Progress;
    public TextMeshProUGUI Attempts;
    public TextMeshProUGUI Instruction;
    public TextMeshProUGUI ObjectiveText;
    public TextMeshProUGUI InfoText;
    public TextMeshProUGUI DataTableText;
    public TextMeshProUGUI CompareText;
    public TextMeshProUGUI FinalScore;
    public TextMeshProUGUI ResultDetails;
    public TextMeshProUGUI StatusText;
    public TextMeshProUGUI FeedbackText;
    public TextMeshProUGUI ScoreChangeText;
    public TextMeshProUGUI ForceLabel;
    public TextMeshProUGUI MomentLabel;
    public TextMeshProUGUI HintText;
    public TextMeshProUGUI MeasureALabel;
    public TextMeshProUGUI MeasureXLabel;
    public TextMeshProUGUI PivotLabel;
    public Image ProgressBar;

    [Header("Panels")]
    public GameObject ObjectivePanel;
    public GameObject InstructionBar;
    public GameObject EquipmentPanel;
    public GameObject ExperimentPanel;
    public GameObject DataTablePanel;
    public GameObject ComparePanel;
    public GameObject ConclusionPanel;
    public GameObject ResultPanel;
    public GameObject ResetConfirm;
    public GameObject FeedbackPanel;
    public GameObject CardPrefab;
    public GameObject SetupTray;
    public GameObject ExperimentVisual;
    public GameObject DotPrefab;
    public Transform CardContainer;
    public Transform RequiredArea;
    public RectTransform GraphArea;
    public Image LineImage;
    public CanvasGroup FeedbackGroup;

    [Header("Buttons")]
    public Button StartBtn;
    public Button Next;
    public Button Reset;
    public Button Retry;
    public Button ResetYes;
    public Button ResetNo;
    public Button ViewProfileBtn;
    public Button EquipContinueBtn;
    public Button RecordBtn;
    public Button ConfirmDistanceABtn;

    [Header("Conclusion")]
    public Button ConclusionA;
    public Button ConclusionB;
    public Button ConclusionC;
    public Button ConclusionD;
    public GameObject ConclusionExplanationPanel;
    public TextMeshProUGUI ConclusionExplanationText;
    public TextMeshProUGUI ConclusionResultsReminder;
    public TextMeshProUGUI ConclusionQuestionText;
    public Button ConclusionContinueBtn;

    [Header("Drop Zones")]
    public LeverUIDropTarget PivotZone;
    public LeverUIDropTarget StripZone;
    public LeverUIDropTarget BookZone;
    public LeverUIDropTarget SpringBalanceZone;

    [Header("Draggable Items")]
    public LeverDraggableUIItem PivotItem;
    public LeverDraggableUIItem StripItem;
    public LeverDraggableUIItem BookItem;
    public LeverDraggableUIItem SpringBalanceItem;

    [Header("Lab Visuals")]
    public RectTransform PullHandle;
    public RectTransform SpringVisual;
    public RectTransform StripVisual;
    public RectTransform BookVisual;
    public RectTransform PivotVisual;
    public RectTransform SpringBalanceVisual;

    [Header("Measurement")]
    public TMP_InputField MeasureAInput;
    public Transform XSelectionButtonsContainer;
    public Button[] XSelectionButtons;
}
