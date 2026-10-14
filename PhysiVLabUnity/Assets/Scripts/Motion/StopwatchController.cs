using UnityEngine;

public class StopwatchController : MonoBehaviour
{
    public static StopwatchController Instance { get; private set; }

    [SerializeField] private float elapsed;
    [SerializeField] private bool running;

    public bool IsRunning => running;
    public float Elapsed => elapsed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (running) elapsed += Time.deltaTime;
    }

    public void StartTimer()
    {
        running = true;
    }

    public void StopTimer()
    {
        running = false;
    }

    public void ResetTimer()
    {
        running = false;
        elapsed = 0f;
        MotionUIManager.Instance?.UpdateStopwatchDisplay(0f);
    }

    public float GetElapsedTime() => elapsed;

    public void SetElapsed(float value)
    {
        elapsed = Mathf.Max(0f, value);
    }
}
