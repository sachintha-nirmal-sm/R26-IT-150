using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Maps practicalId → Unity scene. Only the selected practical scene is loaded.
/// Exact IDs always win so a stale or generic unitySceneId cannot open the wrong lab.
/// </summary>
[DefaultExecutionOrder(-700)]
public class PracticalManager : MonoBehaviour
{
    public static PracticalManager Instance { get; private set; }

    static readonly Dictionary<string, string> ExactScenes = new Dictionary<string, string>
    {
        { "grade9_force_basic", "ForceBasicConcepts" },
        { "grade9_density_water", "DensityWaterExperiment" },
        { "grade9_pressure_solid", "PressureExertedBySolid" },
        { "grade9_reflection_prism", "ReflectionPrismExperiment" },
        { "grade9_lever_15_1", "LeverActivity15_1" },
        { "grade10_hydrostatic_pressure", "HydrostaticPressureExperiment" },
        { "grade10_work_energy_power", "WorkEnergyPowerExperiment" },
        { "grade10_current_electricity", "CurrentElectricityExperiment" },
        { "grade10_motion_straight_line", "MotionStraightLineExperiment" },
        { "grade10_newtons_laws", "NewtonsLawsExperiment" },
        { "grade10_friction", "FrictionExperiment" },
        { "grade10_resultant_force", "ResultantForceExperiment" },
        { "grade10_turning_effect", "TurningEffectExperiment" },
        { "grade10_equilibrium", "EquilibriumOfForcesExperiment" },
        { "grade11_waves", "WavesApplicationsExperiment" },
        { "grade11_geometrical_optics", "GeometricalOpticsExperiment" },
        { "grade11_heat", "HeatExpansionExperiment" },
        { "grade11_power_appliances", "PowerEnergyAppliancesExperiment" },
        { "grade11_electronics", "ElectronicsDiodeExperiment" },
    };

    static readonly HashSet<string> KnownScenes = new HashSet<string>(ExactScenes.Values);

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
        string id = (practicalId ?? "").Trim().ToLowerInvariant();
        if (ExactScenes.TryGetValue(id, out string fromId))
        {
            return fromId;
        }

        if (IsKnownScene(unitySceneId))
        {
            return unitySceneId;
        }

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

        if (id.Contains("turning") || id.Contains("moment"))
        {
            return "TurningEffectExperiment";
        }

        if (id.Contains("equilibrium"))
        {
            return "EquilibriumOfForcesExperiment";
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

        if (id.Contains("electronics") || id.Contains("diode"))
        {
            return "ElectronicsDiodeExperiment";
        }

        if (id.Contains("appliance") || id.Contains("power_appliances"))
        {
            return "PowerEnergyAppliancesExperiment";
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

        if (id.Contains("optics") || id.Contains("geometrical"))
        {
            return "GeometricalOpticsExperiment";
        }

        if (id.Contains("heat"))
        {
            return "HeatExpansionExperiment";
        }

        if (id.Contains("waves"))
        {
            return "WavesApplicationsExperiment";
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

    static bool IsKnownScene(string scene)
    {
        return !string.IsNullOrEmpty(scene) && KnownScenes.Contains(scene);
    }

    public void OpenPractical(string practicalId, string unitySceneId)
    {
        string scene = SceneFor(practicalId, unitySceneId);
        if (string.IsNullOrEmpty(scene))
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError("[PracticalManager] scene not in build: " + scene + " for " + practicalId);
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
