using System;
using UnityEngine;

/// <summary>
/// Countdown timer for Trial and Start. When it hits zero the practical finishes.
/// </summary>
[DefaultExecutionOrder(-600)]
public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    public static TimerManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        Instance = FindAnyObjectByType<TimerManager>();
        if (Instance != null)
        {
            return Instance;
        }

        if (!Application.isPlaying)
        {
            return null;
        }

        var go = new GameObject("TimerManager");
        Instance = go.AddComponent<TimerManager>();
        DontDestroyOnLoad(go);
        return Instance;
    }

    public int TimeLimitSeconds { get; private set; } = 600;
    public int RemainingSeconds { get; private set; }
    public int TimeUsedSeconds { get; private set; }
    public bool Running { get; private set; }
    public bool Expired { get; private set; }
    public static Action OnExpired;
    public static Action<int> OnTick;
    public static bool HideOnGui;

    private float elapsed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartTimer(int timeLimitSeconds)
    {
        TimeLimitSeconds = Mathf.Max(1, timeLimitSeconds);
        RemainingSeconds = TimeLimitSeconds;
        TimeUsedSeconds = 0;
        elapsed = 0f;
        Expired = false;
        Running = true;
        UpdateHud();
    }

    public void Stop()
    {
        Running = false;
        TimeUsedSeconds = Mathf.Clamp(Mathf.RoundToInt(elapsed), 0, TimeLimitSeconds);
    }

    private void Update()
    {
        if (!Running || Expired)
        {
            return;
        }

        elapsed += Time.unscaledDeltaTime;
        TimeUsedSeconds = Mathf.Clamp(Mathf.FloorToInt(elapsed), 0, TimeLimitSeconds);
        RemainingSeconds = Mathf.Max(0, TimeLimitSeconds - TimeUsedSeconds);
        UpdateHud();

        if (RemainingSeconds <= 0)
        {
            Expired = true;
            Running = false;
            OnExpired?.Invoke();
        }
    }

    private void UpdateHud()
    {
        OnTick?.Invoke(RemainingSeconds);
    }

    private void OnGUI()
    {
        if (HideOnGui || (!Running && !Expired))
        {
            return;
        }

        int minutes = RemainingSeconds / 60;
        int seconds = RemainingSeconds % 60;
        string label = string.Format("{0:00}:{1:00}", minutes, seconds);
        var rect = new Rect(12f, 12f, 160f, 40f);
        GUI.color = RemainingSeconds <= 30 ? Color.red : Color.white;
        GUI.Box(rect, "Time  " + label);
        GUI.color = Color.white;
    }
}
