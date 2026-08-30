using UnityEngine;

/// <summary>
/// Book values for the Upthrust / Archimedes practical.
/// Single source of truth used by the gauge, lab visuals, and observation table.
/// </summary>
public static class UpthrustPracticalData
{
    public const float WeightInAir = 1.2f;
    public const float EmptyBeakerWeight = 1.3f;
    public const float SpringScaleMax = 5f;
    public const float ValueTolerance = 0.051f;

    public const int CorrectApparatusCount = 5;
    public const int PracticalStepCount = 6;
    public const int ObservationCellCount = 16; // 4 stages × 4 columns

    public enum ImmersionStage
    {
        AirHang = -1,
        NearSurface = 0,      // (a)
        HalfSubmerged = 1,    // (b)
        FullyNearSurface = 2, // (c)
        FullyDeep = 3         // (d)
    }

    [System.Serializable]
    public struct StageReading
    {
        public ImmersionStage stage;
        public string stageLabel;
        public string description;
        public float springBalanceN;
        public float beakerWithWaterN;
        public float upthrustN;
        public float displacedWaterN;
        public bool overflows;
        public float overflowAmountN;
    }

    public static readonly StageReading[] Stages =
    {
        new StageReading
        {
            stage = ImmersionStage.NearSurface,
            stageLabel = "(a)",
            description = "Metal cube near water surface (not submerged)",
            springBalanceN = 1.2f,
            beakerWithWaterN = 1.3f,
            upthrustN = 0f,
            displacedWaterN = 0f,
            overflows = false,
            overflowAmountN = 0f
        },
        new StageReading
        {
            stage = ImmersionStage.HalfSubmerged,
            stageLabel = "(b)",
            description = "Metal cube half submerged in water",
            springBalanceN = 0.9f,
            beakerWithWaterN = 1.6f,
            upthrustN = 0.3f,
            displacedWaterN = 0.3f,
            overflows = true,
            overflowAmountN = 0.3f
        },
        new StageReading
        {
            stage = ImmersionStage.FullyNearSurface,
            stageLabel = "(c)",
            description = "Metal cube fully immersed (near surface)",
            springBalanceN = 0.6f,
            beakerWithWaterN = 1.9f,
            upthrustN = 0.6f,
            displacedWaterN = 0.6f,
            overflows = true,
            overflowAmountN = 0.3f
        },
        new StageReading
        {
            stage = ImmersionStage.FullyDeep,
            stageLabel = "(d)",
            description = "Metal cube fully immersed (deeper / far from surface)",
            springBalanceN = 0.6f,
            beakerWithWaterN = 1.9f,
            upthrustN = 0.6f,
            displacedWaterN = 0.6f,
            overflows = false,
            overflowAmountN = 0f
        }
    };

    public static StageReading GetStage(ImmersionStage stage)
    {
        int index = (int)stage;
        if (index < 0 || index >= Stages.Length)
        {
            return new StageReading
            {
                stage = ImmersionStage.AirHang,
                stageLabel = "Air",
                description = "Metal cube hanging in air",
                springBalanceN = WeightInAir,
                beakerWithWaterN = EmptyBeakerWeight,
                upthrustN = 0f,
                displacedWaterN = 0f
            };
        }

        return Stages[index];
    }

    public static bool ValuesMatch(float entered, float expected)
    {
        return Mathf.Abs(entered - expected) <= ValueTolerance;
    }

    public static string FormatNewton(float value)
    {
        return value.ToString("0.0");
    }
}
