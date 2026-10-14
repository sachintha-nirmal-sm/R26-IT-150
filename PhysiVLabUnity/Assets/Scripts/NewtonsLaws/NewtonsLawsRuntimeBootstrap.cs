using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(250)]
public class NewtonsLawsRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 11;

    private IEnumerator Start()
    {
        yield return null;

        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<NewtonsLawsSceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Newton's Laws: builder missing on this scene.");
            yield break;
        }

        NewtonsLawsIconFactory.ClearCache();
        var refs = Object.FindAnyObjectByType<NewtonUIRefs>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.IntroPanel == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<NewtonUIRefs>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
        }

        builder.WireReferencesOnPlay(true, true);
        NewtonEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (NewtonUIManager.Instance == null)
            Debug.LogError("Newton's Laws: NewtonUIManager missing after bootstrap.");
        else
            Debug.Log("Newton's Laws: UI ready. Click START PRACTICAL.");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        NewtonsLawsExperimentManager.Instance?.CompleteExperiment();
    }
}
