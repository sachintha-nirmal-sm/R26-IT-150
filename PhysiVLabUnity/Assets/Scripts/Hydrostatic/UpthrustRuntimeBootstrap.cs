using UnityEngine;

/// <summary>
/// Wires the Hydrostatic / Upthrust lab on Play. Does not auto-spawn into other practical scenes.
/// </summary>
[DefaultExecutionOrder(250)]
public class UpthrustRuntimeBootstrap : MonoBehaviour
{
    private void Start()
    {
        TimerManager.HideOnGui = true;
        FlutterBridge.EnsureInstance();
        TimerManager.EnsureInstance();
        UpthrustIconFactory.ClearCache();

        var builder = Object.FindAnyObjectByType<UpthrustSceneRuntimeBuilder>();
        if (builder == null)
        {
            Debug.LogError("Upthrust: builder missing on this scene.");
            return;
        }

        var holder = Object.FindAnyObjectByType<UpthrustUIRefsHolder>();
        bool needsRebuild = !builder.HasExistingBuild()
                            || holder == null
                            || holder.UiVersion < UpthrustSceneRuntimeBuilder.RequiredUiVersion;

        if (needsRebuild)
        {
            Debug.Log("Upthrust: rebuilding UI to version " + UpthrustSceneRuntimeBuilder.RequiredUiVersion);
            builder.BuildScenePersistent();
            holder = Object.FindAnyObjectByType<UpthrustUIRefsHolder>();
            if (holder != null) holder.UiVersion = UpthrustSceneRuntimeBuilder.RequiredUiVersion;
        }

        builder.WireOnPlay();

        TimerManager.OnExpired += CompleteOnTimeout;
        int limit = FlutterBridge.Instance != null ? FlutterBridge.Instance.DurationSeconds : 600;
        if (limit <= 0)
        {
            limit = 600;
        }

        TimerManager.EnsureInstance()?.StartTimer(limit);
    }

    private void OnDestroy()
    {
        TimerManager.OnExpired -= CompleteOnTimeout;
        TimerManager.HideOnGui = false;
    }

    private static void CompleteOnTimeout()
    {
        if (UpthrustPracticalManager.Instance != null)
        {
            UpthrustPracticalManager.Instance.CompleteObservationPhase();
            return;
        }

        UpthrustScoreManager.Instance?.FinalizeScore();
        UpthrustUIManager.Instance?.ShowEndScreen();
    }
}
