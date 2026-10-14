using UnityEngine;

public class EquilibriumSaveManager : MonoBehaviour
{
    public static EquilibriumSaveManager Instance { get; private set; }

    private const string SaveKey = "EquilibriumOfForces_Activity1";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Save(EquilibriumExperimentSaveData data)
    {
        if (data == null) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public EquilibriumExperimentSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return new EquilibriumExperimentSaveData();
        var data = JsonUtility.FromJson<EquilibriumExperimentSaveData>(PlayerPrefs.GetString(SaveKey));
        return data ?? new EquilibriumExperimentSaveData();
    }
}
