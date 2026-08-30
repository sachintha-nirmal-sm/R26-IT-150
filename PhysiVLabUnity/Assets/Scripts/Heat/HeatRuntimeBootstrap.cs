using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(250)]
public class HeatRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 2;

    private IEnumerator Start()
    {
        yield return null;

        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<HeatSceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Heat: builder missing on this scene.");
            yield break;
        }

        HeatIconFactory.ClearCache();
        var refs = Object.FindAnyObjectByType<HeatUIRefs>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.IntroPanel == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<HeatUIRefs>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
        }

        builder.WireReferencesOnPlay(true, true);
        HeatEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (HeatUIManager.Instance == null)
            Debug.LogError("Heat: HeatUIManager missing after bootstrap.");
        else
            Debug.Log("Heat: UI ready. Click START PRACTICAL.");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        HeatExperimentManager.Instance?.CompleteExperiment();
    }
}
