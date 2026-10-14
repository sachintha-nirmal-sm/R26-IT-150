#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// If EXPORT_NOW.txt exists in the project root, export unityLibrary into Flutter.
/// </summary>
[InitializeOnLoad]
public static class PhysiVLabExportOnce
{
    static PhysiVLabExportOnce()
    {
        EditorApplication.delayCall += TryExport;
    }

    private static void TryExport()
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        string flag = Path.Combine(root, "EXPORT_NOW.txt");
        if (!File.Exists(flag))
        {
            return;
        }

        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += TryExport;
            return;
        }

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.LogError("[PhysiVLab] EXPORT_NOW: switch to Android first (File -> Build Settings).");
            return;
        }

        File.Delete(flag);
        Debug.Log("[PhysiVLab] EXPORT_NOW - exporting all practicals to Flutter.");
        PhysiVLabExport.ExportAndroidLibrary(exitWhenDone: false);
    }
}
#endif
