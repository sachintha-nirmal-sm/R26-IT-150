using UnityEngine;

public class NewtonBalancePointer : MonoBehaviour
{
    public static NewtonBalancePointer Instance { get; private set; }

    [SerializeField] private RectTransform pointer;
    [SerializeField] private float minAngle = 40f;
    [SerializeField] private float maxAngle = -40f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(RectTransform pointerRect)
    {
        pointer = pointerRect;
        SetForce(0f);
    }

    public void SetForce(float newtons)
    {
        if (pointer == null) return;
        float t = Mathf.Clamp01(newtons / 70f);
        float angle = Mathf.Lerp(minAngle, maxAngle, t);
        pointer.localEulerAngles = new Vector3(0f, 0f, angle);
        pointer.anchoredPosition = new Vector2(Mathf.Lerp(-70f, 70f, t), pointer.anchoredPosition.y);
    }

    public void ResetPointer() => SetForce(0f);
}
