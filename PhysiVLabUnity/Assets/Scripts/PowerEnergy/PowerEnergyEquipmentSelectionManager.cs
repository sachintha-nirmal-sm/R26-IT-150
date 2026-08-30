using System.Collections.Generic;
using UnityEngine;

public class PowerEnergyEquipmentSelectionManager : MonoBehaviour
{
    public static PowerEnergyEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<PowerEnergyEquipmentDefinition> allEquipment = new List<PowerEnergyEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<PowerEnergyEquipmentType> selectedRequired = new HashSet<PowerEnergyEquipmentType>();
    private readonly List<PowerEnergyEquipmentCardUI> spawnedCards = new List<PowerEnergyEquipmentCardUI>();
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
        allEquipment = new List<PowerEnergyEquipmentDefinition>
        {
            Def(PowerEnergyEquipmentType.ElectricalAppliance, "Electrical appliance", "An appliance is needed so you can measure its voltage, current, power and energy."),
            Def(PowerEnergyEquipmentType.PowerSupply, "Power supply", "A power supply provides the potential difference that drives current through the appliance."),
            Def(PowerEnergyEquipmentType.Voltmeter, "Voltmeter", "A voltmeter is required to measure potential difference."),
            Def(PowerEnergyEquipmentType.Ammeter, "Ammeter", "An ammeter is required to measure current."),
            Def(PowerEnergyEquipmentType.Timer, "Timer", "A timer is required to measure the operating time used in E = Pt."),
            Def(PowerEnergyEquipmentType.Calculator, "Calculator", "A calculator helps you work out P = VI and E = Pt."),
            Def(PowerEnergyEquipmentType.ObservationSheet, "Observation sheet", "An observation sheet is used to record voltage, current, power, time and energy."),
            Wrong(PowerEnergyEquipmentType.NewtonBalance, "Newton balance", "A Newton balance measures force and is not required in this electricity practical."),
            Wrong(PowerEnergyEquipmentType.Spring, "Spring", "A spring is used in force and Hooke's law practicals, not here."),
            Wrong(PowerEnergyEquipmentType.WoodenBlock, "Wooden block", "A wooden block is not part of an electrical energy investigation."),
            Wrong(PowerEnergyEquipmentType.Thermometer, "Thermometer", "Temperature is not measured in this practical."),
            Wrong(PowerEnergyEquipmentType.MeasuringCylinder, "Measuring cylinder", "Volume is not measured here."),
            Wrong(PowerEnergyEquipmentType.Beaker, "Beaker", "A beaker is used in heat or chemistry practicals, not this circuit."),
            Wrong(PowerEnergyEquipmentType.Magnet, "Magnet", "A magnet is not required to measure power and energy."),
            Wrong(PowerEnergyEquipmentType.Ruler, "Ruler", "Length is not measured in this practical."),
            Wrong(PowerEnergyEquipmentType.Stopwatch, "Stopwatch", "Use the virtual timer provided. A separate stopwatch is not required."),
            Wrong(PowerEnergyEquipmentType.Pulley, "Pulley", "A pulley is used in mechanics, not in this electricity practical.")
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
            var refs = Object.FindAnyObjectByType<PowerEnergyUIRefs>();
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
            var card = obj.GetComponent<PowerEnergyEquipmentCardUI>() ?? obj.AddComponent<PowerEnergyEquipmentCardUI>();
            card.Initialize(def, this);
            card.SetCompactMode();
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(PowerEnergyEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            PowerEnergyScoreManager.Instance?.AddToCategory(PowerEnergyScoreCategory.Equipment, 4, false);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is required for this practical."
                : def.correctReason;
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, "+4 MARKS", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                PowerEnergyFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            PowerEnergyScoreManager.Instance?.SubtractScore(4);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            PowerEnergyFeedbackManager.Instance?.ShowMessage("✗ WRONG EQUIPMENT\n" + reason, "-4 MARKS", new Color(0.75f, 0.12f, 0.12f));
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

    private void MoveToRequired(PowerEnergyEquipmentDefinition def)
    {
        if (requiredEquipmentArea == null) return;
        PowerEnergyEquipmentCardUI match = null;
        foreach (var card in spawnedCards)
        {
            if (card != null && card.Definition != null && card.Definition.type == def.type)
            {
                match = card;
                break;
            }
        }
        if (match == null) return;
        match.transform.SetParent(requiredEquipmentArea, false);
        match.SetCompactMode();
    }

    private void ClearCards()
    {
        foreach (var card in spawnedCards)
            if (card != null) Destroy(card.gameObject);
        spawnedCards.Clear();
    }

    private static PowerEnergyEquipmentDefinition Def(PowerEnergyEquipmentType type, string name, string reason)
    {
        return new PowerEnergyEquipmentDefinition { type = type, displayName = name, isRequired = true, correctReason = reason };
    }

    private static PowerEnergyEquipmentDefinition Wrong(PowerEnergyEquipmentType type, string name, string reason)
    {
        return new PowerEnergyEquipmentDefinition { type = type, displayName = name, isRequired = false, incorrectReason = reason };
    }
}
