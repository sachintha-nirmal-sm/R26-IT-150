using UnityEngine;

public class ElecSaveManager : MonoBehaviour
{
    public static ElecSaveManager Instance { get; private set; }

    private const string SaveKey = "CurrentElectricity_TwoDryCells";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(ElecExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public ElecExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new ElecExperimentSaveData();
        var data = JsonUtility.FromJson<ElecExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new ElecExperimentSaveData();
    }
}
