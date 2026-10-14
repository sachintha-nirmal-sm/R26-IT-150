using UnityEngine;

public class ResultantSaveManager : MonoBehaviour
{
    public static ResultantSaveManager Instance { get; private set; }

    private const string SaveKey = "ResultantForce_SameDirection";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(ResultantExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public ResultantExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new ResultantExperimentSaveData();
        var data = JsonUtility.FromJson<ResultantExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new ResultantExperimentSaveData();
    }
}
