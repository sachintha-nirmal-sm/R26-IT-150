using UnityEngine;

public class AccelerationCalculator : MonoBehaviour
{
    public static AccelerationCalculator Instance { get; private set; }

    public float LastAcceleration { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float Calculate(float finalVelocity, float initialVelocity, float time)
    {
        if (time <= 0.0001f)
        {
            LastAcceleration = 0f;
            return 0f;
        }
        LastAcceleration = (finalVelocity - initialVelocity) / time;
        return LastAcceleration;
    }

    public bool ValidateStudentAnswer(float studentValue, float expected, float tolerance)
    {
        return Mathf.Abs(studentValue - expected) <= Mathf.Max(tolerance, 0.05f);
    }
}
