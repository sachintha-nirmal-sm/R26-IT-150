using UnityEngine;

public class LeverPhysicsController : MonoBehaviour
{
    public static LeverPhysicsController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Principle of moments: Effort = (Load × A) / X
    /// </summary>
    public float CalculateRequiredEffort(float load, float a, float x) =>
        (load * a) / Mathf.Max(0.01f, x);

    public bool ShouldLift(float currentEffort, float required) =>
        currentEffort >= required * 0.98f;
}
