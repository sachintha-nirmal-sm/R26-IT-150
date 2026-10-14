using System.Collections;
using UnityEngine;

public class WorkEnergyFallingWeightController : MonoBehaviour
{
    public static WorkEnergyFallingWeightController Instance { get; private set; }

    [SerializeField] private bool isFalling;
    [SerializeField] private bool hasImpacted;
    [SerializeField] private float visualGravity = 9.8f;

    private RectTransform weightVisual;
    private Vector2 heldPosition;
    private Vector2 impactPosition;
    private Coroutine fallRoutine;

    public bool IsFalling => isFalling;
    public bool HasImpacted => hasImpacted;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform visual)
    {
        weightVisual = visual;
        if (weightVisual != null) heldPosition = weightVisual.anchoredPosition;
    }

    public void PrepareWeight()
    {
        isFalling = false;
        hasImpacted = false;
        if (fallRoutine != null) StopCoroutine(fallRoutine);
        if (weightVisual != null)
        {
            weightVisual.gameObject.SetActive(true);
            weightVisual.anchoredPosition = heldPosition;
        }
    }

    public void SetHeight(Vector2 holdPos, Vector2 impactPos)
    {
        heldPosition = holdPos;
        impactPosition = impactPos;
        impactPosition.x = holdPos.x;
        if (weightVisual != null && !isFalling)
            weightVisual.anchoredPosition = heldPosition;
    }

    public void ReleaseWeight()
    {
        if (isFalling || hasImpacted) return;
        StartFall();
    }

    public void StartFall()
    {
        if (weightVisual == null) return;
        isFalling = true;
        hasImpacted = false;
        if (fallRoutine != null) StopCoroutine(fallRoutine);
        fallRoutine = StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        Vector2 start = heldPosition;
        Vector2 end = impactPosition;
        end.x = start.x;
        float distance = Mathf.Abs(start.y - end.y);
        if (distance < 8f) distance = 8f;
        float g = Mathf.Max(2f, visualGravity);
        float t = 0f;
        float duration = Mathf.Sqrt(2f * (distance / 220f) / g) + 0.18f;
        duration = Mathf.Clamp(duration, 0.35f, 1.6f);

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float eased = k * k;
            Vector2 pos = Vector2.Lerp(start, end, eased);
            pos.x = start.x;
            if (weightVisual != null) weightVisual.anchoredPosition = pos;
            yield return null;
        }

        if (weightVisual != null) weightVisual.anchoredPosition = end;
        isFalling = false;
        HandleImpact();
    }

    public void HandleImpact()
    {
        hasImpacted = true;
        isFalling = false;
        WorkEnergyImpactController.Instance?.OnImpact();
    }

    public void ResetWeight()
    {
        isFalling = false;
        hasImpacted = false;
        if (fallRoutine != null) StopCoroutine(fallRoutine);
        if (weightVisual != null) weightVisual.anchoredPosition = heldPosition;
    }
}
