using UnityEngine;

public class LeverPivotController : MonoBehaviour
{
    public static LeverPivotController Instance { get; private set; }

    [SerializeField] private bool isPlaced;
    [SerializeField] private bool isSupported;
    [SerializeField] private RectTransform pivotVisual;

    public bool IsPlaced => isPlaced;
    public bool IsSupported => isSupported;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Bind(RectTransform visual)
    {
        pivotVisual = visual;
    }

    public void MarkPlaced(bool placed)
    {
        isPlaced = placed;
        if (!placed) isSupported = false;
    }

    /// <summary>
    /// Confirms the wooden strip is resting on the pivot/support.
    /// </summary>
    public bool ConfirmSupported()
    {
        if (!isPlaced)
        {
            LeverFeedbackManager.Instance?.ShowInstruction("Place the support (pivot) first.");
            return false;
        }

        isSupported = true;
        return true;
    }

    public void SetSupported(bool supported) => isSupported = supported;

    public void ResetPivot()
    {
        isPlaced = false;
        isSupported = false;
    }
}
