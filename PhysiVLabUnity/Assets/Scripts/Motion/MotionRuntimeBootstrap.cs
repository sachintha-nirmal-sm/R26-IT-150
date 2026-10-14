using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(250)]
public class MotionRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 6;

    private IEnumerator Start()
    {
        yield return null;

        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<MotionSceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Motion: builder missing on this scene.");
            yield break;
        }

        MotionIconFactory.ClearCache();
        var refs = Object.FindAnyObjectByType<MotionUIRefs>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.IntroPanel == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<MotionUIRefs>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
        }

        builder.WireReferencesOnPlay(true, true);
        MotionEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (MotionUIManager.Instance == null)
            Debug.LogError("Motion: MotionUIManager missing after bootstrap.");
        else
            Debug.Log("Motion: UI ready. Click START PRACTICAL.");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        MotionExperimentManager.Instance?.CompleteExperiment();
    }
}
