using UnityEngine;

public class DisplacementCalculator : MonoBehaviour
{
    public static DisplacementCalculator Instance { get; private set; }

    [SerializeField] private float initialPosition;
    [SerializeField] private float currentDisplacement;

    public float Displacement => currentDisplacement;
    public int DirectionSign => currentDisplacement > 0.0001f ? 1 : currentDisplacement < -0.0001f ? -1 : 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetInitialPosition(float positionMeters)
    {
        initialPosition = positionMeters;
        currentDisplacement = 0f;
    }

    public float Calculate(float finalPosition)
    {
        currentDisplacement = finalPosition - initialPosition;
        return currentDisplacement;
    }

    public float Calculate(float startPosition, float finalPosition)
    {
        initialPosition = startPosition;
        currentDisplacement = finalPosition - startPosition;
        return currentDisplacement;
    }

    public string DirectionArrow()
    {
        if (currentDisplacement > 0.0001f) return "→";
        if (currentDisplacement < -0.0001f) return "←";
        return "•";
    }

    public void ResetDisplacement()
    {
        initialPosition = 0f;
        currentDisplacement = 0f;
    }
}
