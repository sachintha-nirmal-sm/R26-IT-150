using UnityEngine;

public class ResultantForceController : MonoBehaviour
{
    public static ResultantForceController Instance { get; private set; }

    [SerializeField] private float forceB;
    [SerializeField] private float forceC;
    [SerializeField] private float maxForce = 10f;
    [SerializeField] private float step = 0.5f;

    public float ForceB => forceB;
    public float ForceC => forceC;
    public float ForceA => forceB + forceC;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddForceB(float delta)
    {
        forceB = Mathf.Clamp(forceB + Mathf.Sign(delta) * step, 0f, maxForce);
        ResultantVisualController.Instance?.RefreshReadings();
        ResultantUIManager.Instance?.UpdateLiveReadings();
    }

    public void AddForceC(float delta)
    {
        forceC = Mathf.Clamp(forceC + Mathf.Sign(delta) * step, 0f, maxForce);
        ResultantVisualController.Instance?.RefreshReadings();
        ResultantUIManager.Instance?.UpdateLiveReadings();
    }

    public void SetForces(float b, float c)
    {
        forceB = Mathf.Clamp(b, 0f, maxForce);
        forceC = Mathf.Clamp(c, 0f, maxForce);
        ResultantVisualController.Instance?.RefreshReadings();
        ResultantUIManager.Instance?.UpdateLiveReadings();
    }

    public void ResetForces()
    {
        forceB = 0f;
        forceC = 0f;
        ResultantVisualController.Instance?.RefreshReadings();
        ResultantUIManager.Instance?.UpdateLiveReadings();
    }
}
