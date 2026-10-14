using UnityEngine;

public class WorkEnergyPotentialEnergyCalculator : MonoBehaviour
{
    public static WorkEnergyPotentialEnergyCalculator Instance { get; private set; }

    [SerializeField] private float gravity = 9.8f;

    public float Gravity
    {
        get => gravity;
        set => gravity = value;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public float CalculatePotentialEnergy(float mass, float height)
    {
        return mass * gravity * height;
    }

    public string FormatCalculation(float mass, float height)
    {
        float pe = CalculatePotentialEnergy(mass, height);
        return $"PE = m × g × h\nPE = {mass:0.##} × {gravity:0.##} × {height:0.00}\nPE = {pe:0.00} J";
    }
}
