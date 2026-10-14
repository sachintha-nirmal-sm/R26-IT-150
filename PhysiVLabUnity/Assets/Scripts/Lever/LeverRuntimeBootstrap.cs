using UnityEngine;

[DefaultExecutionOrder(250)]
public class LeverRuntimeBootstrap : MonoBehaviour
{
    private const int RequiredUiVersion = 2;

    private void Start()
    {
        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();

        var builder = Object.FindAnyObjectByType<LeverSceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Lever: LeverSceneRuntimeBuilder not found on scene root.");
            return;
        }

        LeverIconFactory.ClearCache();

        var refs = Object.FindAnyObjectByType<LeverUIRefsHolder>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || refs == null
                            || refs.UiVersion < RequiredUiVersion
                            || refs.StartBtn == null
                            || refs.EquipContinueBtn == null;

        if (needsRebuild)
        {
            builder.BuildScenePersistent();
            refs = Object.FindAnyObjectByType<LeverUIRefsHolder>();
            if (refs != null) refs.UiVersion = RequiredUiVersion;
            Debug.Log("Lever: UI rebuilt (version " + RequiredUiVersion + ")");
        }

        builder.WireReferencesOnPlay(true, true);
        LeverEquipmentSelectionManager.Instance?.EnsureCardsVisible();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);

        if (LeverUIManager.Instance == null)
            Debug.LogError("LeverUIManager missing after bootstrap");
        if (LeverGameManager.Instance == null)
            Debug.LogError("LeverGameManager missing after bootstrap");
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        LeverGameManager.Instance?.CompleteExperiment();
    }
}
