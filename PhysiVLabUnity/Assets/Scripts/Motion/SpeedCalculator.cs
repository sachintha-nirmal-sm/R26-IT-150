using UnityEngine;

public class SpeedCalculator : MonoBehaviour
{
    public static SpeedCalculator Instance { get; private set; }

    public float LastSpeed { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float Calculate(float distance, float time)
    {
        if (time <= 0.0001f)
        {
            LastSpeed = 0f;
            return 0f;
        }
        LastSpeed = distance / time;
        return LastSpeed;
    }

    public bool ValidateStudentAnswer(float studentValue, float expected, float tolerance)
    {
        return Mathf.Abs(studentValue - expected) <= Mathf.Max(tolerance, 0.05f);
    }
}
