using UnityEngine;

public class VelocityCalculator : MonoBehaviour
{
    public static VelocityCalculator Instance { get; private set; }

    public float LastVelocity { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float Calculate(float displacement, float time)
    {
        if (time <= 0.0001f)
        {
            LastVelocity = 0f;
            return 0f;
        }
        LastVelocity = displacement / time;
        return LastVelocity;
    }

    public string DirectionArrow(float velocity)
    {
        if (velocity > 0.0001f) return "→";
        if (velocity < -0.0001f) return "←";
        return "•";
    }

    public bool ValidateStudentAnswer(float studentValue, float expected, float tolerance)
    {
        return Mathf.Abs(studentValue - expected) <= Mathf.Max(tolerance, 0.05f);
    }
}
