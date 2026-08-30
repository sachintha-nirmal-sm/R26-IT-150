using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(250)]
public class WavesRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 2;

    private IEnumerator Start()
    {
        yield return null;

        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<WavesSceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Waves: builder missing on this scene.");
            yield break;
        }

        WavesIconFactory.ClearCache();
        var refs = Object.FindAnyObjectByType<WavesUIRefs>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.IntroPanel == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<WavesUIRefs>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
        }

        builder.WireReferencesOnPlay(true, true);
        WavesEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (WavesUIManager.Instance == null)
            Debug.LogError("Waves: WavesUIManager missing after bootstrap.");
        else
            Debug.Log("Waves: UI ready. Click START PRACTICAL.");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        WavesExperimentManager.Instance?.CompleteExperiment();
    }
}
