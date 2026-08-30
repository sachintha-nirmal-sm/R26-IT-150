using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(250)]
public class EquilibriumRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 2;

    private IEnumerator Start()
    {
        yield return null;

        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<EquilibriumSceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Equilibrium of Forces: builder missing on this scene.");
            yield break;
        }

        EquilibriumIconFactory.ClearCache();
        var refs = Object.FindAnyObjectByType<EquilibriumUIRefs>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.IntroPanel == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<EquilibriumUIRefs>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
        }

        builder.WireReferencesOnPlay(true, true);
        EquilibriumEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (EquilibriumUIManager.Instance == null)
            Debug.LogError("Equilibrium of Forces: EquilibriumUIManager missing after bootstrap.");
        else
            Debug.Log("Equilibrium of Forces: UI ready. Click START PRACTICAL.");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        EquilibriumExperimentManager.Instance?.CompleteExperiment();
    }
}
