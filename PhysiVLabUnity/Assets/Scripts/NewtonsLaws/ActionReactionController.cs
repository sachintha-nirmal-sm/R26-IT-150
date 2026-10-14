using UnityEngine;
using UnityEngine.UI;

public class ActionReactionController : MonoBehaviour
{
    public static ActionReactionController Instance { get; private set; }

    private GameObject actionArrow;
    private GameObject reactionArrow;
    private float pulse;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(GameObject action, GameObject reaction)
    {
        actionArrow = action;
        reactionArrow = reaction;
        Hide();
    }

    public void ShowForceArrows()
    {
        if (actionArrow != null) actionArrow.SetActive(true);
        if (reactionArrow != null) reactionArrow.SetActive(true);
        pulse = 0f;
    }

    public void Hide()
    {
        if (actionArrow != null) actionArrow.SetActive(false);
        if (reactionArrow != null) reactionArrow.SetActive(false);
    }

    public void CalculateActionReaction()
    {
        ShowForceArrows();
        NewtonFeedbackManager.Instance?.ShowInstruction("ACTION: air is pushed backward. REACTION: balloon moves forward. Equal magnitude, opposite direction.");
    }

    private void Update()
    {
        if (actionArrow == null || !actionArrow.activeSelf) return;
        pulse += Time.deltaTime * 4f;
        float s = 1f + 0.08f * Mathf.Sin(pulse);
        actionArrow.transform.localScale = new Vector3(s, 1f, 1f);
        if (reactionArrow != null) reactionArrow.transform.localScale = new Vector3(s, 1f, 1f);
        var aImg = actionArrow.GetComponent<Image>();
        var rImg = reactionArrow.GetComponent<Image>();
        if (aImg != null) aImg.sprite = NewtonsLawsIconFactory.GetNamed("arrowLeft");
        if (rImg != null) rImg.sprite = NewtonsLawsIconFactory.GetNamed("arrowRight");
    }
}
