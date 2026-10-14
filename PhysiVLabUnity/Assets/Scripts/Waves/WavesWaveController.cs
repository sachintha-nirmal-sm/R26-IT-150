using UnityEngine;

public class WavesWaveController : MonoBehaviour
{
    public static WavesWaveController Instance { get; private set; }

    [SerializeField] private bool shaking;
    [SerializeField] private bool transverse;
    [SerializeField] private float amplitude = 42f;
    [SerializeField] private float wavelength = 280f;
    [SerializeField] private float speed = 140f;
    [SerializeField] private float time;

    public bool IsShaking => shaking;
    public bool IsTransverse => transverse;
    public bool HasTransverseWave => shaking && transverse;
    public float Amplitude => transverse ? amplitude : 0f;
    public float Phase => time;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (!shaking) return;
        time += Time.deltaTime;
        WavesVisualController.Instance?.AnimateWave(time, transverse, amplitude, wavelength, speed);
        WavesUIManager.Instance?.UpdateLiveReadings();
    }

    public void ResetAll()
    {
        shaking = false;
        transverse = false;
        time = 0f;
        WavesVisualController.Instance?.AnimateWave(0f, false, 0f, wavelength, speed);
    }

    public bool TryShakeTransverse()
    {
        var step = WavesExperimentManager.Instance != null
            ? WavesExperimentManager.Instance.CurrentStep
            : WavesExperimentStep.Introduction;
        if (step != WavesExperimentStep.GenerateWave && step != WavesExperimentStep.ObserveRibbons)
        {
            WavesFeedbackManager.Instance?.ShowInstruction("Shake the slinky in the generate-wave step.");
            return false;
        }
        if (WavesAssemblyManager.Instance == null || !WavesAssemblyManager.Instance.SetupConfirmed)
        {
            WavesScoreManager.Instance?.SubtractScore(5);
            WavesFeedbackManager.Instance?.ShowMessage("✗ INCORRECT\nConfirm the setup before shaking the slinky.", "-5 MARKS", new Color(0.75f, 0.12f, 0.12f));
            return false;
        }

        shaking = true;
        transverse = true;
        time = 0f;
        return true;
    }

    public bool TryShakeLongitudinal()
    {
        var step = WavesExperimentManager.Instance != null
            ? WavesExperimentManager.Instance.CurrentStep
            : WavesExperimentStep.Introduction;
        if (step != WavesExperimentStep.GenerateWave)
        {
            WavesFeedbackManager.Instance?.ShowInstruction("Choose how to shake the slinky in the generate-wave step.");
            return false;
        }
        shaking = true;
        transverse = false;
        time = 0f;
        return false;
    }
}
