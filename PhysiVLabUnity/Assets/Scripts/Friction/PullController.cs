using UnityEngine;

public class PullController : MonoBehaviour
{
    public static PullController Instance { get; private set; }

    [SerializeField] private bool pulling;
    [SerializeField] private float elapsed;
    [SerializeField] private float lastForce;
    [SerializeField] private float lastTime;

    public bool IsPulling => pulling;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartPull()
    {
        if (FrictionExperimentManager.Instance != null &&
            FrictionExperimentManager.Instance.CurrentStep != FrictionExperimentStep.Pulling)
        {
            FrictionFeedbackManager.Instance?.ShowInstruction("Confirm the setup before pulling the block.");
            FrictionScoreManager.Instance?.SubtractScore(5);
            return;
        }
        pulling = true;
        elapsed = 0f;
        lastForce = FrictionAppliedForceController.Instance != null ? FrictionAppliedForceController.Instance.AppliedForce : 0f;
        lastTime = Time.time;
    }

    public void StopPull()
    {
        pulling = false;
    }

    public void ResetPull()
    {
        pulling = false;
        elapsed = 0f;
        lastForce = 0f;
    }

    private void Update()
    {
        if (!pulling) return;
        elapsed += Time.deltaTime;
        float force = FrictionAppliedForceController.Instance != null ? FrictionAppliedForceController.Instance.AppliedForce : 0f;
        float dt = Time.time - lastTime;
        if (dt > 0.05f)
        {
            float rate = (force - lastForce) / dt;
            if (rate > 25f)
                FrictionFeedbackManager.Instance?.ShowInstruction("Pull slowly. Limiting friction is the force just as the block starts to move.");
            lastForce = force;
            lastTime = Time.time;
        }
    }
}
