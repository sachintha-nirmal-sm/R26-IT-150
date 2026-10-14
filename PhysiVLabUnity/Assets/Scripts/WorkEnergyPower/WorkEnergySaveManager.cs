using UnityEngine;

public class WorkEnergySaveManager : MonoBehaviour
{
    public static WorkEnergySaveManager Instance { get; private set; }

    private const string SaveKey = "WorkEnergyPower_PotentialEnergyHeight";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(WorkEnergyExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public WorkEnergyExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new WorkEnergyExperimentSaveData();
        var data = JsonUtility.FromJson<WorkEnergyExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new WorkEnergyExperimentSaveData();
    }
}
