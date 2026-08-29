#if UNITY_EDITOR
using System;
using System.IO;
using Unity.Burst;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public static class PhysiVLabBurstGuard
{
    static PhysiVLabBurstGuard()
    {
        EditorPrefs.SetBool("BurstCompilation", false);
        BurstCompiler.Options.EnableBurstCompilation = false;
    }
}

/// <summary>
/// Burst ILPP makes Unity 6 Android export hang on RuntimeInitializeOnLoads.json.
/// </summary>
public class PhysiVLabDisableBurstOnBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.Android)
        {
            return;
        }

        EditorPrefs.SetBool("BurstCompilation", false);
        BurstCompiler.Options.EnableBurstCompilation = false;
    }
}

/// <summary>
/// Exports this project as an Android Gradle library for the Flutter host app.
/// Menu: Tools → PhysiVLab → Export Android Library
///
/// Do not switch build target inside this method. Switching target starts a
/// domain reload, which hangs Unity on "Extracting script serialization layouts".
/// </summary>
public static class PhysiVLabExport
{
    private const string FlutterAndroid = @"E:\R26-IT-150\mobile_app\android";
    private static readonly string[] KnownScenes =
    {
        "Assets/Scenes/ForceBasicConcepts.unity",
        "Assets/Scenes/PressureExertedBySolid.unity",
        "Assets/Scenes/DensityWaterExperiment.unity",
        "Assets/Scenes/ReflectionPrismExperiment.unity",
        "Assets/Scenes/LeverActivity15_1.unity",
        "Assets/Scenes/HydrostaticPressureExperiment.unity",
        "Assets/Scenes/WorkEnergyPowerExperiment.unity",
        "Assets/Scenes/CurrentElectricityExperiment.unity",
        "Assets/Scenes/MotionStraightLineExperiment.unity",
        "Assets/Scenes/NewtonsLawsExperiment.unity",
        "Assets/Scenes/FrictionExperiment.unity",
        "Assets/Scenes/ResultantForceExperiment.unity",
    };

    private static string ExportFolder =>
        Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds", "androidExport");

    [MenuItem("Tools/PhysiVLab/Export Android Library")]
    public static void ExportAndroidLibraryMenu()
    {
        ExportAndroidLibrary(exitWhenDone: false);
    }

    public static void ExportAndroidLibrary()
    {
        ExportAndroidLibrary(exitWhenDone: true);
    }

    public static void ExportAndroidLibrary(bool exitWhenDone)
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            if (exitWhenDone)
            {
                Debug.LogError("Unity is still compiling. Retry export after it finishes.");
                EditorApplication.Exit(1);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "PhysiVLab",
                    "Unity is still compiling. Wait until it finishes, then run this again.",
                    "OK");
            }
            return;
        }

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            if (exitWhenDone)
            {
                Debug.LogError("Switch to Android first: File → Build Settings → Android → Switch Platform.");
                EditorApplication.Exit(1);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Switch to Android first",
                    "File → Build Settings → Android → Switch Platform.\n\n"
                    + "Wait until compiling finishes. Then run:\n"
                    + "Tools → PhysiVLab → Export Android Library",
                    "OK");
            }
            return;
        }

        string[] scenes = ResolveScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("No practical scenes found under Assets/Scenes.");
            if (exitWhenDone) EditorApplication.Exit(1);
            return;
        }

        EditorPrefs.SetBool("BurstCompilation", false);
        BurstCompiler.Options.EnableBurstCompilation = false;

        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        string exportFolder = ExportFolder;
        if (Directory.Exists(exportFolder))
        {
            Directory.Delete(exportFolder, true);
        }

        Directory.CreateDirectory(exportFolder);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = exportFolder,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report == null || report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("PhysiVLab Android export failed.");
            if (exitWhenDone) EditorApplication.Exit(1);
            return;
        }

        string unityLibrary = Path.Combine(exportFolder, "unityLibrary");
        string dest = Path.Combine(FlutterAndroid, "unityLibrary");
        if (!Directory.Exists(unityLibrary))
        {
            Debug.LogError("Export succeeded but unityLibrary was not created at " + unityLibrary);
            if (exitWhenDone) EditorApplication.Exit(1);
            return;
        }

        if (Directory.Exists(dest))
        {
            Directory.Delete(dest, true);
        }

        CopyDirectory(unityLibrary, dest);
        Debug.Log("PhysiVLab unityLibrary copied to " + dest);
        if (!exitWhenDone)
        {
            EditorUtility.DisplayDialog("PhysiVLab", "unityLibrary exported to:\n" + dest, "OK");
        }
        if (exitWhenDone) EditorApplication.Exit(0);
    }

    private static string[] ResolveScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (string scene in KnownScenes)
        {
            if (File.Exists(scene))
            {
                scenes.Add(scene);
            }
        }

        if (scenes.Count > 0)
        {
            return scenes.ToArray();
        }

        string scenesDir = Path.Combine("Assets", "Scenes");
        if (!Directory.Exists(scenesDir))
        {
            return Array.Empty<string>();
        }

        foreach (string path in Directory.GetFiles(scenesDir, "*.unity"))
        {
            scenes.Add(path.Replace('\\', '/'));
        }

        return scenes.ToArray();
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dir.Replace(source, destination));
        }

        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            File.Copy(file, file.Replace(source, destination), true);
        }
    }
}
#endif
