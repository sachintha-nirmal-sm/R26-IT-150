using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(250)]
public class ElectronicsRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 2;

    private IEnumerator Start()
    {
        yield return null;

        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<ElectronicsSceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Electronics: builder missing on this scene.");
            yield break;
        }

        ElectronicsIconFactory.ClearCache();
        var refs = Object.FindAnyObjectByType<ElectronicsUIRefs>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.IntroPanel == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<ElectronicsUIRefs>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
        }

        builder.WireReferencesOnPlay(true, true);
        ElectronicsEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (ElectronicsUIManager.Instance == null)
            Debug.LogError("Electronics: ElectronicsUIManager missing after bootstrap.");
        else
            Debug.Log("Electronics: UI ready. Click START PRACTICAL.");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        ElectronicsPracticalManager.Instance?.CompleteExperiment();
    }
}
