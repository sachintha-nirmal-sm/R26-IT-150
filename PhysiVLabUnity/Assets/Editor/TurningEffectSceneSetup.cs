#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Creates Assets/Scenes/TurningEffectExperiment.unity.
/// Menu: Tools → PhysiVLab → Create Turning Effect Scene
/// </summary>
[InitializeOnLoad]
public static class TurningEffectSceneSetup
{
    private const string ScenePath = "Assets/Scenes/TurningEffectExperiment.unity";
    private const string SamplePath = "Assets/Scenes/SampleScene.unity";
    private const string ForcePath = "Assets/Scenes/ForceBasicConcepts.unity";

    static TurningEffectSceneSetup()
    {
        EditorApplication.delayCall += () => EnsureBuildSettings(false);
    }

    [MenuItem("Tools/PhysiVLab/Create Turning Effect Scene")]
    public static void CreateTurningEffectSceneMenu()
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
            var root = Object.FindAnyObjectByType<TurningSceneRuntimeBuilder>();
            if (root == null)
            {
                var lab = new GameObject("TurningEffectLab");
                lab.AddComponent<TurningSceneRuntimeBuilder>();
                lab.AddComponent<TurningRuntimeBootstrap>();
                EditorSceneManager.MarkSceneDirty(opened);
                EditorSceneManager.SaveScene(opened);
            }
            else if (root.GetComponent<TurningRuntimeBootstrap>() == null)
            {
                root.gameObject.AddComponent<TurningRuntimeBootstrap>();
                EditorSceneManager.MarkSceneDirty(opened);
                EditorSceneManager.SaveScene(opened);
            }
        }

        EnsureBuildSettings(interactive);
        if (interactive)
        {
            EditorUtility.DisplayDialog(
                "PhysiVLab",
                "Turning effect scene is ready:\n" + ScenePath + "\n\nPress Play in Unity to try the lab.",
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

        var equilibrium = Object.FindAnyObjectByType<EquilibriumSceneRuntimeBuilder>();
        if (equilibrium != null) Object.DestroyImmediate(equilibrium.gameObject);

        var waves = Object.FindAnyObjectByType<WavesSceneRuntimeBuilder>();
        if (waves != null) Object.DestroyImmediate(waves.gameObject);

        var optics = Object.FindAnyObjectByType<OpticsSceneRuntimeBuilder>();
        if (optics != null) Object.DestroyImmediate(optics.gameObject);
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
