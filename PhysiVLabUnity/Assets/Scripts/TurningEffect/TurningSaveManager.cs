using UnityEngine;

public class TurningSaveManager : MonoBehaviour
{
    public static TurningSaveManager Instance { get; private set; }

    private const string SaveKey = "TurningEffect_Activity2";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(TurningExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public TurningExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new TurningExperimentSaveData();
        var data = JsonUtility.FromJson<TurningExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new TurningExperimentSaveData();
    }
}
