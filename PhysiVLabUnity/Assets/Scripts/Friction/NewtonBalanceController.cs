using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewtonBalanceController : MonoBehaviour
{
    public static NewtonBalanceController Instance { get; private set; }

    [SerializeField] private float reading;
    [SerializeField] private bool attached;
    [SerializeField] private bool pulling;
    [SerializeField] private float pullRate = 8f;
    [SerializeField] private RectTransform visual;
    [SerializeField] private TextMeshProUGUI readingLabel;
    [SerializeField] private Image scaleFill;

    public float Reading => reading;
    public bool IsAttached => attached;
    public bool IsPulling => pulling;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform balanceVisual, TextMeshProUGUI label, Image fill)
    {
        visual = balanceVisual;
        readingLabel = label;
        scaleFill = fill;
        ResetBalance();
    }

    public void Attach()
    {
        attached = true;
        if (visual != null) visual.gameObject.SetActive(true);
        UpdateDisplay();
    }

    public void StartPull()
    {
        if (!attached)
        {
            FrictionFeedbackManager.Instance?.ShowInstruction("Attach the Newton balance to the block first.");
            return;
        }
        pulling = true;
        FrictionAppliedForceController.Instance?.StartApplying();
        PullController.Instance?.StartPull();
    }

    public void StopPull()
    {
        pulling = false;
        FrictionAppliedForceController.Instance?.StopApplying();
        PullController.Instance?.StopPull();
    }

    public void IncreaseForce()
    {
        if (!attached) return;
        FrictionAppliedForceController.Instance?.Increase(pullRate * Time.deltaTime);
        reading = FrictionAppliedForceController.Instance != null ? FrictionAppliedForceController.Instance.AppliedForce : reading;
        NewtonBalancePointer.Instance?.SetForce(reading);
        UpdateDisplay();
    }

    public void DecreaseForce()
    {
        FrictionAppliedForceController.Instance?.Decrease(pullRate * Time.deltaTime);
        reading = FrictionAppliedForceController.Instance != null ? FrictionAppliedForceController.Instance.AppliedForce : 0f;
        NewtonBalancePointer.Instance?.SetForce(reading);
        UpdateDisplay();
    }

    public float GetReading() => reading;

    public void SetReading(float value)
    {
        reading = Mathf.Clamp(value, 0f, 70f);
        NewtonBalancePointer.Instance?.SetForce(reading);
        UpdateDisplay();
    }

    public void ResetBalance()
    {
        pulling = false;
        reading = 0f;
        NewtonBalancePointer.Instance?.SetForce(0f);
        UpdateDisplay();
    }

    public void Detach()
    {
        attached = false;
        ResetBalance();
        if (visual != null) visual.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!pulling) return;
        IncreaseForce();
    }

    private void UpdateDisplay()
    {
        if (readingLabel != null)
            readingLabel.text = $"{reading:0.0} N";
        if (scaleFill != null)
            scaleFill.fillAmount = Mathf.Clamp01(reading / 70f);
    }
}
