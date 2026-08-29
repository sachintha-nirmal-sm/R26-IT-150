using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maps practicalId → Unity scene. Only the selected practical scene is loaded.
/// </summary>
[DefaultExecutionOrder(-700)]
public class PracticalManager : MonoBehaviour
{
    public static PracticalManager Instance { get; private set; }

    public static void EnsureLoaded()
    {
        if (Instance != null)
        {
            return;
        }

        var go = new GameObject("PracticalManager");
        go.AddComponent<PracticalManager>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public string SceneFor(string practicalId, string unitySceneId)
    {
        if (!string.IsNullOrEmpty(unitySceneId))
        {
            return unitySceneId;
        }

        string id = (practicalId ?? "").ToLowerInvariant();
        if (id.Contains("newton"))
        {
            return "NewtonsLawsExperiment";
        }

        if (id.Contains("friction"))
        {
            return "FrictionExperiment";
        }

        if (id.Contains("resultant"))
        {
            return "ResultantForceExperiment";
        }

        if (id.Contains("straight_line") || id.Contains("straightline") || id.Contains("motion_straight")
            || (id.Contains("motion") && id.Contains("line")))
        {
            return "MotionStraightLineExperiment";
        }

        if (id.Contains("current_electric") || id.Contains("currentelectricity") || id.Contains("circuit"))
        {
            return "CurrentElectricityExperiment";
        }

        if (id.Contains("work") || id.Contains("energy_power") || id.Contains("work_energy"))
        {
            return "WorkEnergyPowerExperiment";
        }

        if (id.Contains("hydrostatic") || id.Contains("upthrust") || id.Contains("archimedes"))
        {
            return "HydrostaticPressureExperiment";
        }

        if (id.Contains("lever"))
        {
            return "LeverActivity15_1";
        }

        if (id.Contains("reflection") || id.Contains("prism") || id.Contains("dispersion"))
        {
            return "ReflectionPrismExperiment";
        }

        if (id.Contains("density"))
        {
            return "DensityWaterExperiment";
        }

        if (id.Contains("pressure"))
        {
            return "PressureExertedBySolid";
        }

        if (id.Contains("force"))
        {
            return "ForceBasicConcepts";
        }

        return SceneManager.GetActiveScene().name;
    }

    public void OpenPractical(string practicalId, string unitySceneId)
    {
        string scene = SceneFor(practicalId, unitySceneId);
        if (string.IsNullOrEmpty(scene))
        {
            return;
        }

        if (SceneManager.GetActiveScene().name == scene)
        {
            return;
        }

        Debug.Log("[PracticalManager] loading scene " + scene + " for " + practicalId);
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    public void UnloadToIdle()
    {
        // Future practicals can return to a blank bootstrap scene here.
    }
}
