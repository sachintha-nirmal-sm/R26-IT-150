#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Creates Assets/Scenes/PressureExertedBySolid.unity.
/// Menu: Tools → PhysiVLab → Create Pressure Scene
/// </summary>
[InitializeOnLoad]
public static class PressureSceneSetup
{
    private const string ScenePath = "Assets/Scenes/PressureExertedBySolid.unity";
    private const string SamplePath = "Assets/Scenes/SampleScene.unity";
    private const string ForcePath = "Assets/Scenes/ForceBasicConcepts.unity";

    static PressureSceneSetup()
    {
        EditorApplication.delayCall += () => EnsureBuildSettings(false);
    }

    [MenuItem("Tools/PhysiVLab/Create Pressure Scene")]
    public static void CreatePressureSceneMenu()
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
            var force = Object.FindAnyObjectByType<ForcePracticalController>();
            if (force != null)
            {
                Object.DestroyImmediate(force.gameObject);
            }

            var density = Object.FindAnyObjectByType<DensityWaterPracticalController>();
            if (density != null)
            {
                Object.DestroyImmediate(density.gameObject);
            }

            var reflection = Object.FindAnyObjectByType<PrismSceneRuntimeBuilder>();
            if (reflection != null)
            {
                Object.DestroyImmediate(reflection.gameObject);
            }

            var lever = Object.FindAnyObjectByType<LeverSceneRuntimeBuilder>();
            if (lever != null)
            {
                Object.DestroyImmediate(lever.gameObject);
            }

            var hydrostatic = Object.FindAnyObjectByType<UpthrustSceneRuntimeBuilder>();
            if (hydrostatic != null)
            {
                Object.DestroyImmediate(hydrostatic.gameObject);
            }

            var wep = Object.FindAnyObjectByType<WorkEnergyPowerSceneRuntimeBuilder>();
            if (wep != null)
            {
                Object.DestroyImmediate(wep.gameObject);
            }

            var elec = Object.FindAnyObjectByType<CurrentElectricitySceneRuntimeBuilder>();
            if (elec != null)
            {
                Object.DestroyImmediate(elec.gameObject);
            }

            var motion = Object.FindAnyObjectByType<MotionSceneRuntimeBuilder>();
            if (motion != null)
            {
                Object.DestroyImmediate(motion.gameObject);
            }

            var newton = Object.FindAnyObjectByType<NewtonsLawsSceneRuntimeBuilder>();
            if (newton != null)
            {
                Object.DestroyImmediate(newton.gameObject);
            }

            var friction = Object.FindAnyObjectByType<FrictionSceneRuntimeBuilder>();
            if (friction != null)
            {
                Object.DestroyImmediate(friction.gameObject);
            }

            if (Object.FindAnyObjectByType<PressureSolidPracticalController>() == null)
            {
                var lab = new GameObject("PressureLab");
                lab.AddComponent<PressureSolidPracticalController>();
                EditorSceneManager.MarkSceneDirty(opened);
                EditorSceneManager.SaveScene(opened);
            }
        }

        EnsureBuildSettings(interactive);
        if (interactive)
        {
            EditorUtility.DisplayDialog(
                "PhysiVLab",
                "Pressure scene is ready:\n" + ScenePath + "\n\nPress Play in Unity to try the lab.",
                "OK");
        }
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
