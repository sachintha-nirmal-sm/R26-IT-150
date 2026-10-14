using UnityEngine;

public class ElectronicsSaveManager : MonoBehaviour
{
    public static ElectronicsSaveManager Instance { get; private set; }

    private const string SaveKey = "Electronics_DiodeForwardReverseBias";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(ElectronicsExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public ElectronicsExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new ElectronicsExperimentSaveData();
        var data = JsonUtility.FromJson<ElectronicsExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new ElectronicsExperimentSaveData();
    }
}
