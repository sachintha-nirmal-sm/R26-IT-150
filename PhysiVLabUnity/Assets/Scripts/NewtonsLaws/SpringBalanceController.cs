using UnityEngine;
using UnityEngine.UI;

public class SpringBalanceController : MonoBehaviour
{
    public static SpringBalanceController Instance { get; private set; }

    [SerializeField] private float reading;
    [SerializeField] private bool objectAttached;
    private GameObject springVisual;
    private GameObject pointer;
    private Image springImage;
    private float restHeight = 90f;

    public float Reading => reading;
    public bool ObjectAttached => objectAttached;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(GameObject spring, GameObject pointerObj)
    {
        springVisual = spring;
        pointer = pointerObj;
        if (springVisual != null) springImage = springVisual.GetComponent<Image>();
        ResetBalance();
    }

    public void AttachObject(float massKg)
    {
        objectAttached = true;
        reading = NewtonForceCalculator.Instance != null
            ? NewtonForceCalculator.Instance.CalculateWeight(massKg)
            : massKg * 9.8f;
        ApplyVisual(reading);
        NewtonUIManager.Instance?.UpdateSpringReading(reading);
    }

    public void SetAppliedForce(float force)
    {
        reading = force;
        ApplyVisual(force);
        NewtonUIManager.Instance?.UpdateSpringReading(force);
    }

    public void ResetBalance()
    {
        objectAttached = false;
        reading = 0f;
        ApplyVisual(0f);
        NewtonUIManager.Instance?.UpdateSpringReading(0f);
    }

    private void ApplyVisual(float force)
    {
        if (springVisual != null) springVisual.SetActive(true);
        if (springImage != null)
        {
            springImage.sprite = NewtonsLawsIconFactory.GetNamed("spring");
            springImage.preserveAspect = true;
            var rt = springImage.rectTransform;
            float stretch = restHeight + Mathf.Clamp(force, 0f, 20f) * 6f;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x > 8f ? rt.sizeDelta.x : 36f, stretch);
        }
        if (pointer != null)
        {
            var rt = pointer.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -20f - force * 8f);
        }
    }
}
