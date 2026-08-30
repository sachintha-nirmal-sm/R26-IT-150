using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(250)]
public class PowerEnergyRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 2;

    private IEnumerator Start()
    {
        yield return null;

        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<PowerEnergySceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("PowerEnergy: builder missing on this scene.");
            yield break;
        }

        PowerEnergyIconFactory.ClearCache();
        var refs = Object.FindAnyObjectByType<PowerEnergyUIRefs>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.IntroPanel == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<PowerEnergyUIRefs>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
        }

        builder.WireReferencesOnPlay(true, true);
        PowerEnergyEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (PowerEnergyUIManager.Instance == null)
            Debug.LogError("PowerEnergy: PowerEnergyUIManager missing after bootstrap.");
        else
            Debug.Log("PowerEnergy: UI ready. Click START PRACTICAL.");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        PowerEnergyExperimentManager.Instance?.CompleteExperiment();
    }
}
