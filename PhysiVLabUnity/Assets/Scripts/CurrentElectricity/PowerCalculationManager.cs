using UnityEngine;

public class PowerCalculationManager : MonoBehaviour
{
    public static PowerCalculationManager Instance { get; private set; }

    public float LastPower { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float Calculate(float voltage, float current)
    {
        LastPower = voltage * current;
        return LastPower;
    }

    public float PowerForCurrentConnection()
    {
        if (CircuitCalculationManager.Instance == null) return 0f;
        LastPower = CircuitCalculationManager.Instance.LastPower;
        return LastPower;
    }
}
