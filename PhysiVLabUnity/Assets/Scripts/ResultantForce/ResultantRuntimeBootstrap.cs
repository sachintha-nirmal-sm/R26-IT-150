using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(250)]
public class ResultantRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 2;

    private IEnumerator Start()
    {
        yield return null;

        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<ResultantSceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Resultant Force: builder missing on this scene.");
            yield break;
        }

        ResultantIconFactory.ClearCache();
        var refs = Object.FindAnyObjectByType<ResultantUIRefs>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.IntroPanel == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<ResultantUIRefs>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
        }

        builder.WireReferencesOnPlay(true, true);
        ResultantEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (ResultantUIManager.Instance == null)
            Debug.LogError("Resultant Force: ResultantUIManager missing after bootstrap.");
        else
            Debug.Log("Resultant Force: UI ready. Click START PRACTICAL.");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        ResultantExperimentManager.Instance?.CompleteExperiment();
    }
}
