using UnityEngine;

public class PowerEnergySaveManager : MonoBehaviour
{
    public static PowerEnergySaveManager Instance { get; private set; }

    private const string SaveKey = "PowerEnergy_ElectricAppliances";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(PowerEnergyExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public PowerEnergyExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new PowerEnergyExperimentSaveData();
        var data = JsonUtility.FromJson<PowerEnergyExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new PowerEnergyExperimentSaveData();
    }
}
