using System.Collections.Generic;
using UnityEngine;

public class FrictionEquipmentSelectionManager : MonoBehaviour
{
    public static FrictionEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<FrictionEquipmentDefinition> allEquipment = new List<FrictionEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<FrictionEquipmentType> selectedRequired = new HashSet<FrictionEquipmentType>();
    private readonly List<FrictionEquipmentCardUI> spawnedCards = new List<FrictionEquipmentCardUI>();
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
        allEquipment = new List<FrictionEquipmentDefinition>
        {
            Def(FrictionEquipmentType.WoodenBlock, "Wooden block", true, "A wooden block of weight 60 N is the object whose friction is investigated."),
            Def(FrictionEquipmentType.NewtonBalance, "Newton balance", true, "A Newton balance measures force, so it is required."),
            Def(FrictionEquipmentType.Sandpaper, "Sandpaper pieces", true, "Sandpaper of equal roughness is placed on each contact surface."),
            Def(FrictionEquipmentType.LabTable, "Flat laboratory table", true, "A flat table is the surface on which the block is pulled."),
            Def(FrictionEquipmentType.MeasuringRuler, "Measuring ruler", true, "A ruler is used to record the dimensions of each contact surface."),
            Def(FrictionEquipmentType.RecordingSheet, "Recording sheet", true, "A recording sheet is needed for Table 5.4 observations."),
            Def(FrictionEquipmentType.ForceDisplay, "Force reading display", true, "A force reading display shows the Newton-balance measurement."),
            Wrong(FrictionEquipmentType.Ammeter, "Ammeter", "An ammeter measures electric current and is not required for this friction experiment."),
            Wrong(FrictionEquipmentType.Voltmeter, "Voltmeter", "A voltmeter measures potential difference, not force."),
            Wrong(FrictionEquipmentType.DryCell, "Dry cells", "Dry cells supply electrical energy and are not required here."),
            Wrong(FrictionEquipmentType.Bulb, "Bulb", "A bulb is an electrical component and is not required."),
            Wrong(FrictionEquipmentType.Beaker, "Beaker", "A beaker is used for liquids, not friction."),
            Wrong(FrictionEquipmentType.MeasuringCylinder, "Measuring cylinder", "A measuring cylinder measures volume, not force."),
            Wrong(FrictionEquipmentType.Thermometer, "Thermometer", "A thermometer measures temperature, not friction."),
            Wrong(FrictionEquipmentType.Magnet, "Magnet", "A magnet is not required for this friction experiment."),
            Wrong(FrictionEquipmentType.Stopwatch, "Stopwatch", "Time is not the measured quantity in this limiting-friction experiment."),
            Wrong(FrictionEquipmentType.Pulley, "Pulley", "A pulley is not required; the block is pulled horizontally with a Newton balance."),
            Wrong(FrictionEquipmentType.Spring, "Spring", "A separate spring is not required; the Newton balance already contains a spring."),
            Wrong(FrictionEquipmentType.MassHanger, "Mass hanger", "A mass hanger is not required because the block already has a fixed weight of 60 N."),
            Wrong(FrictionEquipmentType.Compass, "Compass", "A compass is not required for this friction experiment."),
            Wrong(FrictionEquipmentType.BunsenBurner, "Bunsen burner", "A bunsen burner is not required for this friction experiment.")
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
            var refs = Object.FindAnyObjectByType<FrictionUIRefs>();
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
            var card = obj.GetComponent<FrictionEquipmentCardUI>() ?? obj.AddComponent<FrictionEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(FrictionEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired || def.isOptional)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            FrictionScoreManager.Instance?.AddScore(4, false);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is required for this practical."
                : def.correctReason;
            FrictionFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, "+4 MARKS", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                FrictionFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            FrictionScoreManager.Instance?.SubtractScore(4);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            FrictionFeedbackManager.Instance?.ShowMessage("✗ WRONG EQUIPMENT\n" + reason, "-4 MARKS", new Color(0.75f, 0.12f, 0.12f));
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

    private void MoveToRequired(FrictionEquipmentDefinition def)
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

    private static FrictionEquipmentDefinition Def(FrictionEquipmentType type, string name, bool required, string reason)
    {
        return new FrictionEquipmentDefinition { type = type, displayName = name, isRequired = required, correctReason = reason };
    }

    private static FrictionEquipmentDefinition Wrong(FrictionEquipmentType type, string name, string reason)
    {
        return new FrictionEquipmentDefinition { type = type, displayName = name, isRequired = false, incorrectReason = reason };
    }
}
