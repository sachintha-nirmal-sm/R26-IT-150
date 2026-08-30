using UnityEngine;

public class CurrentMeasurementManager : MonoBehaviour
{
    public static CurrentMeasurementManager Instance { get; private set; }

    [SerializeField] private float currentTolerance = 0.03f;
    public bool HasMeasured { get; private set; }
    public float LastReading { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float GetBulbCurrent()
    {
        return CircuitCalculationManager.Instance != null ? CircuitCalculationManager.Instance.LastCurrent : 0f;
    }

    public float Measure()
    {
        LastReading = GetBulbCurrent();
        HasMeasured = true;
        AmmeterController.Instance?.Show(LastReading, true);
        return LastReading;
    }

    public bool IsWithinTolerance(float entered)
    {
        return Mathf.Abs(entered - GetBulbCurrent()) <= currentTolerance;
    }

    public void ResetMeasurement()
    {
        HasMeasured = false;
        LastReading = 0f;
        AmmeterController.Instance?.ResetMeter();
    }

    public float Tolerance => currentTolerance;
    public void SetTolerance(float value) => currentTolerance = Mathf.Max(0.001f, value);
}
