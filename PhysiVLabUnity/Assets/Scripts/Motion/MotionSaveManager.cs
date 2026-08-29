using UnityEngine;

public class MotionSaveManager : MonoBehaviour
{
    public static MotionSaveManager Instance { get; private set; }

    private const string SaveKey = "Motion_DistanceDisplacementSpeedVelocity";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(MotionExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public MotionExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new MotionExperimentSaveData();
        var data = JsonUtility.FromJson<MotionExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new MotionExperimentSaveData();
    }
}
