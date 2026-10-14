using UnityEngine;

/// <summary>
/// Controls the book (load) visual: snap position and lift when effort is sufficient.
/// </summary>
public class BookLiftController : MonoBehaviour
{
    public static BookLiftController Instance { get; private set; }

    [SerializeField] private RectTransform book;
    [SerializeField] private float liftOffsetY = 28f;
    [SerializeField] private float liftLerpSpeed = 6f;

    private Vector2 restPosition;
    private Vector2 targetPosition;
    private bool lifted;
    private bool animating;

    public bool IsLifted => lifted;

    private void Awake()
    {
        Instance = this;
        if (book == null) book = GetComponent<RectTransform>();
        CaptureRest();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!animating || book == null) return;
        book.anchoredPosition = Vector2.Lerp(book.anchoredPosition, targetPosition, Time.deltaTime * liftLerpSpeed);
        if (Vector2.Distance(book.anchoredPosition, targetPosition) < 0.5f)
        {
            book.anchoredPosition = targetPosition;
            animating = false;
        }
    }

    public void Bind(RectTransform bookRect)
    {
        book = bookRect;
        CaptureRest();
        ResetPosition();
    }

    public void CaptureRest()
    {
        if (book != null)
        {
            restPosition = book.anchoredPosition;
            targetPosition = restPosition;
        }
    }

    public bool TryLift(float currentEffort, float requiredEffort)
    {
        bool shouldLift = LeverPhysicsController.Instance != null
            ? LeverPhysicsController.Instance.ShouldLift(currentEffort, requiredEffort)
            : currentEffort >= requiredEffort * 0.98f;

        if (shouldLift)
        {
            Lift();
            return true;
        }

        if (lifted && currentEffort < requiredEffort * 0.9f)
            Lower();
        return false;
    }

    public void Lift()
    {
        if (book == null) return;
        lifted = true;
        targetPosition = restPosition + new Vector2(0f, liftOffsetY);
        animating = true;
    }

    public void Lower()
    {
        if (book == null) return;
        lifted = false;
        targetPosition = restPosition;
        animating = true;
    }

    public void ResetPosition()
    {
        lifted = false;
        animating = false;
        if (book != null)
        {
            book.anchoredPosition = restPosition;
            targetPosition = restPosition;
        }
    }
}
