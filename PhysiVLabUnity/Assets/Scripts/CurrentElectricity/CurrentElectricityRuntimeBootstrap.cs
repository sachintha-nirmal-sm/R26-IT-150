using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(250)]
public class CurrentElectricityRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 6;

    private IEnumerator Start()
    {
        yield return null;

        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<CurrentElectricitySceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Current Electricity: builder missing on this scene.");
            yield break;
        }

        ElecIconFactory.ClearCache();
        var refs = Object.FindAnyObjectByType<ElecUIRefsHolder>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.IntroPanel == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<ElecUIRefsHolder>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
        }

        builder.WireReferencesOnPlay(true, true);
        ElecEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (ElecUIManager.Instance == null)
            Debug.LogError("Current Electricity: ElecUIManager missing after bootstrap.");
        else
            Debug.Log("Current Electricity: UI ready. Click START PRACTICAL.");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        CurrentElectricityExperimentManager.Instance?.CompleteExperiment();
    }
}
