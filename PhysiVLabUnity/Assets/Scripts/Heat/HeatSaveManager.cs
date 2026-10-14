using UnityEngine;

public class HeatSaveManager : MonoBehaviour
{
    public static HeatSaveManager Instance { get; private set; }

    private const string SaveKey = "Heat_ExpansionOfLiquids";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(HeatExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public HeatExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new HeatExperimentSaveData();
        var data = JsonUtility.FromJson<HeatExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new HeatExperimentSaveData();
    }
}
