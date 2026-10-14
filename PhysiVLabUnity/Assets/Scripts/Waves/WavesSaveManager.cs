using UnityEngine;

public class WavesSaveManager : MonoBehaviour
{
    public static WavesSaveManager Instance { get; private set; }

    private const string SaveKey = "Waves_TransverseSlinky_4_5";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(WavesExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public WavesExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new WavesExperimentSaveData();
        var data = JsonUtility.FromJson<WavesExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new WavesExperimentSaveData();
    }
}
