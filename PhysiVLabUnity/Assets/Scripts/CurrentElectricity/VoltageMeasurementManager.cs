using UnityEngine;

public class VoltageMeasurementManager : MonoBehaviour
{
    public static VoltageMeasurementManager Instance { get; private set; }

    [SerializeField] private float voltageTolerance = 0.08f;
    public bool HasMeasured { get; private set; }
    public float LastReading { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float GetBulbVoltage()
    {
        return CircuitCalculationManager.Instance != null ? CircuitCalculationManager.Instance.LastVoltage : 0f;
    }

    public float Measure()
    {
        LastReading = GetBulbVoltage();
        HasMeasured = true;
        VoltmeterController.Instance?.Show(LastReading, true);
        return LastReading;
    }

    public bool IsWithinTolerance(float entered)
    {
        return Mathf.Abs(entered - GetBulbVoltage()) <= voltageTolerance;
    }

    public void ResetMeasurement()
    {
        HasMeasured = false;
        LastReading = 0f;
        VoltmeterController.Instance?.ResetMeter();
    }

    public float Tolerance => voltageTolerance;
    public void SetTolerance(float value) => voltageTolerance = Mathf.Max(0.001f, value);
}
