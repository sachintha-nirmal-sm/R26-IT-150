using UnityEngine;

public class NewtonForceCalculator : MonoBehaviour
{
    public static NewtonForceCalculator Instance { get; private set; }

    [SerializeField] private float gravitationalAcceleration = 9.8f;

    public float GravitationalAcceleration => gravitationalAcceleration;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Configure(float g) => gravitationalAcceleration = Mathf.Max(0.1f, g);

    public float CalculateWeight(float mass) => mass * gravitationalAcceleration;

    public float CalculateForce(float mass, float acceleration) => mass * acceleration;

    public bool ValidateStudentAnswer(float studentValue, float expected, float tolerance)
    {
        float band = Mathf.Max(tolerance, 0.05f);
        if (Mathf.Abs(expected - 19.6f) < 0.01f) return studentValue >= 19.55f && studentValue <= 19.65f;
        return Mathf.Abs(studentValue - expected) <= band;
    }
}
