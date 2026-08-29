#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Creates Assets/Scenes/WorkEnergyPowerExperiment.unity.
/// Menu: Tools → PhysiVLab → Create Work Energy Scene
/// </summary>
[InitializeOnLoad]
public static class WorkEnergySceneSetup
{
    private const string ScenePath = "Assets/Scenes/WorkEnergyPowerExperiment.unity";
    private const string SamplePath = "Assets/Scenes/SampleScene.unity";
    private const string ForcePath = "Assets/Scenes/ForceBasicConcepts.unity";

    static WorkEnergySceneSetup()
    {
        EditorApplication.delayCall += () => EnsureBuildSettings(false);
    }

    [MenuItem("Tools/PhysiVLab/Create Work Energy Scene")]
    public static void CreateWorkEnergySceneMenu()
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
            var root = Object.FindAnyObjectByType<WorkEnergyPowerSceneRuntimeBuilder>();
            if (root == null)
            {
                var lab = new GameObject("WorkEnergyLab");
                lab.AddComponent<WorkEnergyPowerSceneRuntimeBuilder>();
                lab.AddComponent<WorkEnergyPowerRuntimeBootstrap>();
                EditorSceneManager.MarkSceneDirty(opened);
                EditorSceneManager.SaveScene(opened);
            }
            else if (root.GetComponent<WorkEnergyPowerRuntimeBootstrap>() == null)
            {
                root.gameObject.AddComponent<WorkEnergyPowerRuntimeBootstrap>();
                EditorSceneManager.MarkSceneDirty(opened);
                EditorSceneManager.SaveScene(opened);
            }
        }

        EnsureBuildSettings(interactive);
        if (interactive)
        {
            EditorUtility.DisplayDialog(
                "PhysiVLab",
                "Work, energy and power scene is ready:\n" + ScenePath + "\n\nPress Play in Unity to try the lab.",
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

        var elec = Object.FindAnyObjectByType<CurrentElectricitySceneRuntimeBuilder>();
        if (elec != null) Object.DestroyImmediate(elec.gameObject);

        var motion = Object.FindAnyObjectByType<MotionSceneRuntimeBuilder>();
        if (motion != null) Object.DestroyImmediate(motion.gameObject);

        var newton = Object.FindAnyObjectByType<NewtonsLawsSceneRuntimeBuilder>();
        if (newton != null) Object.DestroyImmediate(newton.gameObject);
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
