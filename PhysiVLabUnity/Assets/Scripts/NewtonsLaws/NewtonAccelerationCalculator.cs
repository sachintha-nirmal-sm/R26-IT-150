using UnityEngine;

public class NewtonAccelerationCalculator : MonoBehaviour
{
    public static NewtonAccelerationCalculator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float Calculate(float force, float mass)
    {
        if (mass <= 0.0001f) return 0f;
        return force / mass;
    }

    public bool ValidateStudentAnswer(float studentValue, float expected, float tolerance)
    {
        return Mathf.Abs(studentValue - expected) <= Mathf.Max(tolerance, 0.05f);
    }
}
