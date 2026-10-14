using UnityEngine;
using UnityEngine.UI;

public class BalloonController : MonoBehaviour
{
    public static BalloonController Instance { get; private set; }

    [SerializeField] private bool inflated;
    [SerializeField] private bool released;
    [SerializeField] private float position;
    [SerializeField] private float speed = 2.6f;
    [SerializeField] private bool flying;

    private RectTransform balloonRect;
    private Vector2 startPos;

    public bool Inflated => inflated;
    public bool Released => released;
    public bool Flying => flying;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform balloon)
    {
        balloonRect = balloon;
        if (balloonRect != null) startPos = balloonRect.anchoredPosition;
        ResetBalloon();
    }

    public void PrepareBalloon()
    {
        ResetBalloon();
        if (balloonRect != null)
        {
            balloonRect.gameObject.SetActive(true);
            ApplySprite(0.85f);
        }
    }

    public void InflateBalloon()
    {
        inflated = true;
        released = false;
        flying = false;
        ApplySprite(1.25f);
    }

    public void ReleaseBalloon()
    {
        if (!inflated)
        {
            NewtonScoreManager.Instance?.SubtractScore(5);
            NewtonFeedbackManager.Instance?.ShowMessage("✗ Inflate the balloon before releasing it.", "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
            return;
        }
        released = true;
        flying = true;
        position = 0f;
        ActionReactionController.Instance?.ShowForceArrows();
    }

    public void ResetBalloon()
    {
        inflated = false;
        released = false;
        flying = false;
        position = 0f;
        if (balloonRect != null)
        {
            balloonRect.anchoredPosition = startPos;
            balloonRect.localScale = Vector3.one;
            balloonRect.gameObject.SetActive(true);
            ApplySprite(0.85f);
        }
    }

    private void Update()
    {
        if (!flying || balloonRect == null) return;
        position += speed * Time.deltaTime;
        balloonRect.anchoredPosition = startPos + new Vector2(position * 180f, 0f);
        float shrink = Mathf.Lerp(1.25f, 0.7f, Mathf.Clamp01(position / 4f));
        balloonRect.localScale = Vector3.one * shrink;
        if (position >= 4.2f)
        {
            flying = false;
            ThirdLawExperimentManager.Instance?.NotifyFlightComplete();
        }
    }

    private void ApplySprite(float scale)
    {
        if (balloonRect == null) return;
        balloonRect.localScale = Vector3.one * scale;
        var img = balloonRect.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = NewtonsLawsIconFactory.GetNamed("balloon");
            img.preserveAspect = true;
        }
    }
}
