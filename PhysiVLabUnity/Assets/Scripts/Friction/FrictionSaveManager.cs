using UnityEngine;

public class FrictionSaveManager : MonoBehaviour
{
    public static FrictionSaveManager Instance { get; private set; }

    private const string SaveKey = "Friction_SurfaceAreaInvestigation";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(FrictionExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public FrictionExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new FrictionExperimentSaveData();
        var data = JsonUtility.FromJson<FrictionExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new FrictionExperimentSaveData();
    }
}
