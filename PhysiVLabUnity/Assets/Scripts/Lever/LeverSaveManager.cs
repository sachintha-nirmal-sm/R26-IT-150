using UnityEngine;

public class LeverSaveManager : MonoBehaviour
{
    public static LeverSaveManager Instance { get; private set; }

    private const string SaveKey = "LeverActivity151Save";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(LeverExperimentSaveData data)
    {
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public LeverExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new LeverExperimentSaveData();
        return JsonUtility.FromJson<LeverExperimentSaveData>(PlayerPrefs.GetString(SaveKey)) ?? new LeverExperimentSaveData();
    }
}
