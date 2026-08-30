#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Creates Assets/Scenes/WavesApplicationsExperiment.unity.
/// Menu: Tools → PhysiVLab → Create Waves Scene
/// </summary>
[InitializeOnLoad]
public static class WavesApplicationsSceneSetup
{
    private const string ScenePath = "Assets/Scenes/WavesApplicationsExperiment.unity";
    private const string SamplePath = "Assets/Scenes/SampleScene.unity";
    private const string ForcePath = "Assets/Scenes/ForceBasicConcepts.unity";

    static WavesApplicationsSceneSetup()
    {
        EditorApplication.delayCall += () => EnsureBuildSettings(false);
    }

    [MenuItem("Tools/PhysiVLab/Create Waves Scene")]
    public static void CreateWavesSceneMenu()
    {
        CreateOrRepair(true);
    }

    public static void CreateOrRepair(bool interactive)
    {
        Directory.CreateDirectory("Assets/Scenes");

        if (!File.Exists(ScenePath))
        {
            string source = File.Exists(ForcePath) ? ForcePath : SamplePath;
            if (File.Exists(source))
            {
                AssetDatabase.CopyAsset(source, ScenePath);
            }
            else
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(scene, ScenePath);
                EditorSceneManager.CloseScene(scene, true);
            }

            AssetDatabase.Refresh();
        }

        if (interactive)
        {
            var opened = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            StripOtherLabs();
            var root = Object.FindAnyObjectByType<WavesSceneRuntimeBuilder>();
            if (root == null)
            {
                var lab = new GameObject("WavesLab");
                lab.AddComponent<WavesSceneRuntimeBuilder>();
                lab.AddComponent<WavesRuntimeBootstrap>();
                EditorSceneManager.MarkSceneDirty(opened);
                EditorSceneManager.SaveScene(opened);
            }
            else if (root.GetComponent<WavesRuntimeBootstrap>() == null)
            {
                root.gameObject.AddComponent<WavesRuntimeBootstrap>();
                EditorSceneManager.MarkSceneDirty(opened);
                EditorSceneManager.SaveScene(opened);
            }
        }

        EnsureBuildSettings(interactive);
        if (interactive)
        {
            EditorUtility.DisplayDialog(
                "PhysiVLab",
                "Waves scene is ready:\n" + ScenePath + "\n\nPress Play in Unity to try the lab.",
                "OK");
        }
    }

    private static void StripOtherLabs()
    {
        var force = Object.FindAnyObjectByType<ForcePracticalController>();
        if (force != null) Object.DestroyImmediate(force.gameObject);

        var pressure = Object.FindAnyObjectByType<PressureSolidPracticalController>();
        if (pressure != null) Object.DestroyImmediate(pressure.gameObject);

        var density = Object.FindAnyObjectByType<DensityWaterPracticalController>();
        if (density != null) Object.DestroyImmediate(density.gameObject);

        var reflection = Object.FindAnyObjectByType<PrismSceneRuntimeBuilder>();
        if (reflection != null) Object.DestroyImmediate(reflection.gameObject);

        var lever = Object.FindAnyObjectByType<LeverSceneRuntimeBuilder>();
        if (lever != null) Object.DestroyImmediate(lever.gameObject);

        var hydrostatic = Object.FindAnyObjectByType<UpthrustSceneRuntimeBuilder>();
        if (hydrostatic != null) Object.DestroyImmediate(hydrostatic.gameObject);

        var wep = Object.FindAnyObjectByType<WorkEnergyPowerSceneRuntimeBuilder>();
        if (wep != null) Object.DestroyImmediate(wep.gameObject);

        var elec = Object.FindAnyObjectByType<CurrentElectricitySceneRuntimeBuilder>();
        if (elec != null) Object.DestroyImmediate(elec.gameObject);

        var motion = Object.FindAnyObjectByType<MotionSceneRuntimeBuilder>();
        if (motion != null) Object.DestroyImmediate(motion.gameObject);

        var newton = Object.FindAnyObjectByType<NewtonsLawsSceneRuntimeBuilder>();
        if (newton != null) Object.DestroyImmediate(newton.gameObject);

        var friction = Object.FindAnyObjectByType<FrictionSceneRuntimeBuilder>();
        if (friction != null) Object.DestroyImmediate(friction.gameObject);

        var resultant = Object.FindAnyObjectByType<ResultantSceneRuntimeBuilder>();
        if (resultant != null) Object.DestroyImmediate(resultant.gameObject);

        var turning = Object.FindAnyObjectByType<TurningSceneRuntimeBuilder>();
        if (turning != null) Object.DestroyImmediate(turning.gameObject);

        var equilibrium = Object.FindAnyObjectByType<EquilibriumSceneRuntimeBuilder>();
        if (equilibrium != null) Object.DestroyImmediate(equilibrium.gameObject);

        var optics = Object.FindAnyObjectByType<OpticsSceneRuntimeBuilder>();
        if (optics != null) Object.DestroyImmediate(optics.gameObject);

        var heat = Object.FindAnyObjectByType<HeatSceneRuntimeBuilder>();
        if (heat != null) Object.DestroyImmediate(heat.gameObject);

        var powerEnergy = Object.FindAnyObjectByType<PowerEnergySceneRuntimeBuilder>();
        if (powerEnergy != null) Object.DestroyImmediate(powerEnergy.gameObject);

        var electronics = Object.FindAnyObjectByType<ElectronicsSceneRuntimeBuilder>();
        if (electronics != null) Object.DestroyImmediate(electronics.gameObject);
    }

    private static void EnsureBuildSettings(bool log)
    {
        if (!File.Exists(ScenePath))
        {
            return;
        }

        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Any(item => item.path == ScenePath))
        {
            return;
        }

        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        if (log)
        {
            Debug.Log("[PhysiVLab] Added " + ScenePath + " to Build Settings.");
        }
    }
}
#endif
