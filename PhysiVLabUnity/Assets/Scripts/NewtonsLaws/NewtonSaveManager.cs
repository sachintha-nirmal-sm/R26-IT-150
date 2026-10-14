using UnityEngine;

public class NewtonSaveManager : MonoBehaviour
{
    public static NewtonSaveManager Instance { get; private set; }

    private const string SaveKey = "NewtonsLaws_InvestigatingNewtonsLaws";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(NewtonExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public NewtonExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new NewtonExperimentSaveData();
        var data = JsonUtility.FromJson<NewtonExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new NewtonExperimentSaveData();
    }
}
