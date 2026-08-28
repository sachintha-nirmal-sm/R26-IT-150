using System;
using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Flutter ↔ Unity bridge for Unity as a Library.
/// Flutter writes a session JSON; Unity loads the mapped scene and returns a result JSON.
/// Unity never talks to Firebase directly.
/// </summary>
[DefaultExecutionOrder(-800)]
public class FlutterBridge : MonoBehaviour
{
    public static FlutterBridge Instance { get; private set; }

    public static FlutterBridge EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        Instance = FindAnyObjectByType<FlutterBridge>();
        if (Instance != null)
        {
            return Instance;
        }

        if (!Application.isPlaying)
        {
            return null;
        }

        var go = new GameObject("FlutterBridge");
        Instance = go.AddComponent<FlutterBridge>();
        DontDestroyOnLoad(go);
        return Instance;
    }

    public string StudentId { get; private set; } = "";
    public string PracticalId { get; private set; } = "grade9_density_water";
    public string LessonId { get; private set; } = "phy-g9-density-doc";
    public string Mode { get; private set; } = "trial";
    public string ResultId { get; private set; } = "";
    public int AttemptNumber { get; private set; } = 1;
    public int DurationSeconds { get; private set; } = 600;
    public string UnitySceneId { get; private set; } = "DensityWaterExperiment";

    public bool HasSession { get; private set; }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SendResultToFlutter(string json);
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ParseLaunchUrl(Application.absoluteURL);
        TryReadAndroidSession();
        ApplyAttemptLimit();
        PracticalManager.EnsureLoaded();
    }

    /// <summary>Called from Android via UnitySendMessage("FlutterBridge", "ReceiveSession", json).</summary>
    public void ReceiveSession(string json)
    {
        ApplySessionJson(json);
        PracticalManager.EnsureLoaded();
        PracticalManager.Instance?.OpenPractical(PracticalId, UnitySceneId);
    }

    public void ParseLaunchUrl(string url)
    {
        if (string.IsNullOrEmpty(url) || !url.Contains("?"))
        {
            return;
        }

        string query = url.Substring(url.IndexOf("?", StringComparison.Ordinal) + 1);
        int hash = query.IndexOf('#');
        if (hash >= 0)
        {
            query = query.Substring(0, hash);
        }

        foreach (string pair in query.Split('&'))
        {
            string[] parts = pair.Split('=');
            if (parts.Length != 2)
            {
                continue;
            }

            ApplyField(Uri.UnescapeDataString(parts[0]), Uri.UnescapeDataString(parts[1]));
        }

        HasSession = true;
    }

    public void ApplySessionJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        ApplyField("studentId", ReadJsonString(json, "studentId"));
        ApplyField("practicalId", ReadJsonString(json, "practicalId"));
        ApplyField("lessonId", ReadJsonString(json, "lessonId"));
        ApplyField("mode", ReadJsonString(json, "mode"));
        ApplyField("resultId", ReadJsonString(json, "resultId"));
        ApplyField("unitySceneId", ReadJsonString(json, "unitySceneId"));
        ApplyField("unityScene", ReadJsonString(json, "unityScene"));

        int attempt = ReadJsonInt(json, "attempt", ReadJsonInt(json, "attemptNumber", AttemptNumber));
        AttemptNumber = Mathf.Max(1, attempt);

        int limit = ReadJsonInt(json, "timeLimitSeconds", ReadJsonInt(json, "durationSeconds", DurationSeconds));
        DurationSeconds = Mathf.Max(0, limit);

        HasSession = true;
        ApplyAttemptLimit();
        Debug.Log("[FlutterBridge] session " + PracticalId + " mode=" + Mode + " attempt=" + AttemptNumber);
    }

    private void TryReadAndroidSession()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var bridge = new AndroidJavaClass("com.example.mobile_app.UnityBridge"))
            {
                string json = bridge.CallStatic<string>("takePendingSession");
                if (!string.IsNullOrEmpty(json))
                {
                    ApplySessionJson(json);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[FlutterBridge] Android session read failed: " + ex.Message);
        }
#endif
    }

    private void ApplyField(string key, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        switch (key)
        {
            case "studentId":
                StudentId = value;
                break;
            case "practicalId":
                PracticalId = value;
                break;
            case "lessonId":
                LessonId = value;
                break;
            case "mode":
                Mode = NormalizeMode(value);
                break;
            case "resultId":
                ResultId = value;
                break;
            case "attemptNumber":
            case "attempt":
                int.TryParse(value, out int attempt);
                AttemptNumber = Mathf.Max(1, attempt);
                break;
            case "durationSeconds":
            case "timeLimitSeconds":
                int.TryParse(value, out int duration);
                DurationSeconds = Mathf.Max(0, duration);
                break;
            case "unitySceneId":
            case "unityScene":
                UnitySceneId = value;
                break;
        }
    }

    public static string NormalizeMode(string value)
    {
        string mode = (value ?? "").Trim().ToLowerInvariant();
        if (mode == "start" || mode == "practical" || mode == "official")
        {
            return "start";
        }

        return "trial";
    }

    public bool IsOfficial
    {
        get { return Mode == "start"; }
    }

    private void ApplyAttemptLimit()
    {
    }

    public void NotifyCompleted(int score, float mass, float volume, float density, bool passed, int mistakes, int timeUsed, bool completed = true)
    {
        string measurements =
            "{\"mass\":" + mass.ToString(CultureInfo.InvariantCulture)
            + ",\"volume\":" + volume.ToString(CultureInfo.InvariantCulture)
            + ",\"density\":" + density.ToString(CultureInfo.InvariantCulture)
            + "}";
        NotifyCompleted(score, passed, mistakes, timeUsed, completed, measurements);
    }

    public void NotifyCompleted(int score, bool passed, int mistakes, int timeUsed, bool completed, string measurementsJson)
    {
        if (string.IsNullOrWhiteSpace(measurementsJson))
        {
            measurementsJson = "{}";
        }

        string json =
            "{"
            + "\"type\":\"practical_completed\","
            + "\"source\":\"physiv-lab\","
            + "\"studentId\":\"" + Escape(StudentId) + "\","
            + "\"practicalId\":\"" + Escape(PracticalId) + "\","
            + "\"lessonId\":\"" + Escape(LessonId) + "\","
            + "\"resultId\":\"" + Escape(ResultId) + "\","
            + "\"mode\":\"" + Escape(Mode) + "\","
            + "\"attempt\":" + AttemptNumber + ","
            + "\"attemptNumber\":" + AttemptNumber + ","
            + "\"score\":" + Mathf.Clamp(score, 0, 100) + ","
            + "\"unityScore\":" + Mathf.Clamp(score, 0, 100) + ","
            + "\"timeUsed\":" + Mathf.Max(0, timeUsed) + ","
            + "\"completed\":" + (completed ? "true" : "false") + ","
            + "\"passed\":" + (passed ? "true" : "false") + ","
            + "\"mistakes\":" + mistakes + ","
            + "\"measurements\":" + measurementsJson
            + "}";

        Debug.Log("[FlutterBridge] " + json);
        SendToHost(json);
    }

    private static void SendToHost(string json)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SendResultToFlutter(json);
#elif UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var bridge = new AndroidJavaClass("com.example.mobile_app.UnityBridge"))
            {
                bridge.CallStatic("onResult", json);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[FlutterBridge] Android result send failed: " + ex.Message);
        }
#else
        Debug.Log("[FlutterBridge] host result (editor): " + json);
#endif
    }

    private static string ReadJsonString(string json, string key)
    {
        string needle = "\"" + key + "\"";
        int keyIndex = json.IndexOf(needle, StringComparison.Ordinal);
        if (keyIndex < 0)
        {
            return "";
        }

        int colon = json.IndexOf(':', keyIndex + needle.Length);
        if (colon < 0)
        {
            return "";
        }

        int start = colon + 1;
        while (start < json.Length && char.IsWhiteSpace(json[start]))
        {
            start++;
        }

        if (start >= json.Length || json[start] != '"')
        {
            return "";
        }

        start++;
        int end = json.IndexOf('"', start);
        if (end < 0)
        {
            return "";
        }

        return json.Substring(start, end - start);
    }

    private static int ReadJsonInt(string json, string key, int fallback)
    {
        string needle = "\"" + key + "\"";
        int keyIndex = json.IndexOf(needle, StringComparison.Ordinal);
        if (keyIndex < 0)
        {
            return fallback;
        }

        int colon = json.IndexOf(':', keyIndex + needle.Length);
        if (colon < 0)
        {
            return fallback;
        }

        int start = colon + 1;
        while (start < json.Length && (char.IsWhiteSpace(json[start]) || json[start] == '"'))
        {
            start++;
        }

        int end = start;
        while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-'))
        {
            end++;
        }

        int value;
        if (end > start && int.TryParse(json.Substring(start, end - start), out value))
        {
            return value;
        }

        return fallback;
    }

    private static string Escape(string value)
    {
        return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
