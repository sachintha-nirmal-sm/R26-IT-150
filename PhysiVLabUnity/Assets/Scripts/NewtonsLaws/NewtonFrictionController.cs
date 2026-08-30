using UnityEngine;

public class NewtonFrictionController : MonoBehaviour
{
    public static NewtonFrictionController Instance { get; private set; }

    [SerializeField] private bool lowFriction = true;
    [SerializeField] private float lowFrictionAccel = 0.05f;
    [SerializeField] private float highFrictionAccel = 2.4f;

    public bool IsLowFriction => lowFriction;
    public float FrictionAcceleration => lowFriction ? lowFrictionAccel : highFrictionAccel;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetFriction(bool low)
    {
        lowFriction = low;
        NewtonUIManager.Instance?.HighlightFriction(low);
    }

    public void ResetFriction()
    {
        lowFriction = true;
        NewtonUIManager.Instance?.HighlightFriction(true);
    }
}
