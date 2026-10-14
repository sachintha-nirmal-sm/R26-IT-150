using UnityEngine;

public class OpticsSaveManager : MonoBehaviour
{
    public static OpticsSaveManager Instance { get; private set; }

    private const string SaveKey = "GeometricalOptics_ConcaveMirrorFocalLength";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(OpticsExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public OpticsExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new OpticsExperimentSaveData();
        var data = JsonUtility.FromJson<OpticsExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new OpticsExperimentSaveData();
    }
}
