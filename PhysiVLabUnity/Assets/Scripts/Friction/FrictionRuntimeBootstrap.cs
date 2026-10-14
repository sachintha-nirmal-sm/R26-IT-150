using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(250)]
public class FrictionRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 4;

    private IEnumerator Start()
    {
        yield return null;

        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<FrictionSceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Friction: builder missing on this scene.");
            yield break;
        }

        FrictionIconFactory.ClearCache();
        var refs = Object.FindAnyObjectByType<FrictionUIRefs>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.IntroPanel == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<FrictionUIRefs>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
        }

        builder.WireReferencesOnPlay(true, true);
        FrictionEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (FrictionUIManager.Instance == null)
            Debug.LogError("Friction: FrictionUIManager missing after bootstrap.");
        else
            Debug.Log("Friction: UI ready. Click START PRACTICAL.");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        FrictionExperimentManager.Instance?.CompleteExperiment();
    }
}
