using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(250)]
public class WorkEnergyPowerRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 6;

    private IEnumerator Start()
    {
        yield return null;

        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<WorkEnergyPowerSceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Work Energy Power: builder missing on this scene.");
            yield break;
        }

        WorkEnergyIconFactory.ClearCache();
        var refs = Object.FindAnyObjectByType<WorkEnergyUIRefsHolder>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.ObjectivePanel == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<WorkEnergyUIRefsHolder>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
        }

        builder.WireReferencesOnPlay(true, true);
        WorkEnergyEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (WorkEnergyUIManager.Instance == null)
            Debug.LogError("Work Energy Power: WorkEnergyUIManager missing after bootstrap.");
        else
            Debug.Log("Work Energy Power: UI ready. Click START PRACTICAL.");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        WorkEnergyPowerExperimentManager.Instance?.CompleteExperiment();
    }
}
