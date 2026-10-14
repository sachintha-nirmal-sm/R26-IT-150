using System.Collections.Generic;
using UnityEngine;

public class MotionEquipmentSelectionManager : MonoBehaviour
{
    public static MotionEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<MotionEquipmentDefinition> allEquipment = new List<MotionEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<MotionEquipmentType> selectedRequired = new HashSet<MotionEquipmentType>();
    private readonly List<MotionEquipmentCardUI> spawnedCards = new List<MotionEquipmentCardUI>();
    private bool selectionComplete;

    public bool IsEquipmentComplete => selectionComplete;
    public int RequiredCount
    {
        get
        {
            int n = 0;
            foreach (var d in allEquipment) if (d.isRequired) n++;
            return n;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitEquipment();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void InitEquipment()
    {
        if (allEquipment.Count > 0) return;
        allEquipment = new List<MotionEquipmentDefinition>
        {
            Def(MotionEquipmentType.ToyCar, "Toy Car", true, "A toy car is the moving object whose motion will be investigated."),
            Def(MotionEquipmentType.StraightTrack, "Straight Track", true, "A straight track provides a path of known length for the car."),
            Def(MotionEquipmentType.MetreRuler, "Metre Ruler / Measuring Scale", true, "A metre ruler or measuring scale is required to measure distance along the track."),
            Def(MotionEquipmentType.Stopwatch, "Stopwatch", true, "A stopwatch is required to measure the time taken by the car."),
            Def(MotionEquipmentType.DistanceMarkers, "Distance Markers", true, "Distance markers identify the 1 m to 5 m positions on the track."),
            Def(MotionEquipmentType.StartingMarker, "Starting Marker", true, "A starting marker shows the 0 m position where the car begins."),
            Def(MotionEquipmentType.RecordingTable, "Recording Table / Notebook", true, "A recording table is needed to write down distances, times and calculated values."),
            Opt(MotionEquipmentType.Calculator, "Calculator", "A calculator can help you evaluate speed = distance / time."),
            Wrong(MotionEquipmentType.NewtonSpringBalance, "Newton Spring Balance", "A newton spring balance measures force, not distance or time."),
            Wrong(MotionEquipmentType.Ammeter, "Ammeter", "An ammeter measures electric current and is not required for this motion experiment."),
            Wrong(MotionEquipmentType.Voltmeter, "Voltmeter", "A voltmeter measures potential difference, not motion."),
            Wrong(MotionEquipmentType.Bulb, "Bulb", "A bulb is an electrical component and is not required for this motion experiment."),
            Wrong(MotionEquipmentType.DryCell, "Dry Cell", "A dry cell is a source of electrical energy, not a motion-measuring instrument."),
            Wrong(MotionEquipmentType.Beaker, "Beaker", "A beaker is used for liquids and is not required here."),
            Wrong(MotionEquipmentType.MeasuringCylinder, "Measuring Cylinder", "A measuring cylinder measures volume, not distance."),
            Wrong(MotionEquipmentType.Thermometer, "Thermometer", "A thermometer measures temperature, not motion."),
            Wrong(MotionEquipmentType.Magnet, "Magnet", "A magnet is not required for this motion experiment."),
            Wrong(MotionEquipmentType.Pulley, "Pulley", "A pulley is not required for motion along a straight track."),
            Wrong(MotionEquipmentType.Lever, "Lever", "A lever is not required for this motion experiment."),
            Wrong(MotionEquipmentType.Clay, "Clay", "Clay is not required for this motion experiment."),
            Wrong(MotionEquipmentType.BunsenBurner, "Bunsen Burner", "A bunsen burner is not required for this motion experiment."),
            Wrong(MotionEquipmentType.Compass, "Compass", "A compass is not required to measure distance, time or velocity on a marked track.")
        };
    }

    public void SetupUI(Transform container, Transform requiredArea, GameObject prefab)
    {
        equipmentCardContainer = container;
        requiredEquipmentArea = requiredArea;
        equipmentCardPrefab = prefab;
        if (spawnedCards.Count == 0) BuildCards();
    }

    public void EnsureCardsVisible()
    {
        if (equipmentCardContainer == null || equipmentCardPrefab == null)
        {
            var refs = Object.FindAnyObjectByType<MotionUIRefs>();
            if (refs != null) SetupUI(refs.CardContainer, refs.RequiredArea, refs.CardPrefab);
            return;
        }
        if (spawnedCards.Count == 0) BuildCards();
    }

    public void BuildCards()
    {
        ClearCards();
        if (equipmentCardContainer == null || equipmentCardPrefab == null) return;
        foreach (var def in allEquipment)
        {
            var obj = Object.Instantiate(equipmentCardPrefab, equipmentCardContainer);
            obj.SetActive(true);
            var card = obj.GetComponent<MotionEquipmentCardUI>() ?? obj.AddComponent<MotionEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(MotionEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired || def.isOptional)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            MotionScoreManager.Instance?.AddScore(5, false);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is useful for this practical."
                : def.correctReason;
            MotionFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                MotionFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            MotionScoreManager.Instance?.SubtractScore(5);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            MotionFeedbackManager.Instance?.ShowMessage("✗ INCORRECT EQUIPMENT\n" + reason, "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
        }
    }

    public bool IsCompleteCheck() => AllRequiredSelected();

    public void ResetSelection()
    {
        selectedRequired.Clear();
        selectionComplete = false;
        BuildCards();
    }

    private bool AllRequiredSelected()
    {
        foreach (var d in allEquipment)
            if (d.isRequired && !selectedRequired.Contains(d.type)) return false;
        return true;
    }

    private void MoveToRequired(MotionEquipmentDefinition def)
    {
        if (requiredEquipmentArea == null) return;
        foreach (var card in spawnedCards)
        {
            if (card == null || card.Definition == null || card.Definition.type != def.type) continue;
            card.transform.SetParent(requiredEquipmentArea, false);
            card.SetCompactMode();
            break;
        }
    }

    private void ClearCards()
    {
        foreach (var card in spawnedCards)
            if (card != null) Object.Destroy(card.gameObject);
        spawnedCards.Clear();
        if (equipmentCardContainer == null) return;
        for (int i = equipmentCardContainer.childCount - 1; i >= 0; i--)
            Object.Destroy(equipmentCardContainer.GetChild(i).gameObject);
    }

    private static MotionEquipmentDefinition Def(MotionEquipmentType type, string name, bool required, string reason)
    {
        return new MotionEquipmentDefinition { type = type, displayName = name, isRequired = required, correctReason = reason };
    }

    private static MotionEquipmentDefinition Opt(MotionEquipmentType type, string name, string reason)
    {
        return new MotionEquipmentDefinition { type = type, displayName = name, isRequired = false, isOptional = true, correctReason = reason };
    }

    private static MotionEquipmentDefinition Wrong(MotionEquipmentType type, string name, string reason)
    {
        return new MotionEquipmentDefinition { type = type, displayName = name, isRequired = false, incorrectReason = reason };
    }
}
