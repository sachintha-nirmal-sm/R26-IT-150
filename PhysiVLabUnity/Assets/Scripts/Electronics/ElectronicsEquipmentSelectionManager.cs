using System.Collections.Generic;
using UnityEngine;

public class ElectronicsEquipmentSelectionManager : MonoBehaviour
{
    public static ElectronicsEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<ElectronicsEquipmentDefinition> allEquipment = new List<ElectronicsEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<ElectronicsEquipmentType> selectedRequired = new HashSet<ElectronicsEquipmentType>();
    private readonly List<ElectronicsEquipmentCardUI> spawnedCards = new List<ElectronicsEquipmentCardUI>();
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
        allEquipment = new List<ElectronicsEquipmentDefinition>
        {
            Def(ElectronicsEquipmentType.Diode, "IN4001 diode", "An IN4001 diode is required for this practical."),
            Def(ElectronicsEquipmentType.Bulb, "2.5 V torch bulb", "A 2.5 V torch bulb is required to observe current flow."),
            Def(ElectronicsEquipmentType.DryCells, "Two 1.5 V dry cells", "Two 1.5 V dry cells provide 3 V for the circuit."),
            Def(ElectronicsEquipmentType.Switch, "Switch", "A switch is required to open and close the circuit."),
            Def(ElectronicsEquipmentType.Breadboard, "Circuit board", "A circuit board / breadboard is needed to assemble the circuit."),
            Def(ElectronicsEquipmentType.Wires, "Connecting wires", "Connecting wires complete the closed loop."),
            Wrong(ElectronicsEquipmentType.NewtonBalance, "Newton balance", "A Newton balance measures force and is not required."),
            Wrong(ElectronicsEquipmentType.Spring, "Spring", "A spring is used in mechanics, not in this diode practical."),
            Wrong(ElectronicsEquipmentType.WoodenBlock, "Wooden block", "A wooden block is not required for this practical."),
            Wrong(ElectronicsEquipmentType.Beaker, "Beaker", "A beaker is used in chemistry or heat practicals."),
            Wrong(ElectronicsEquipmentType.MeasuringCylinder, "Measuring cylinder", "Volume is not measured in this practical."),
            Wrong(ElectronicsEquipmentType.Thermometer, "Thermometer", "Temperature is not measured here."),
            Wrong(ElectronicsEquipmentType.Ruler, "Ruler", "Length is not measured in this practical."),
            Wrong(ElectronicsEquipmentType.Ammeter, "Ammeter", "An ammeter is not required for this practical."),
            Wrong(ElectronicsEquipmentType.Voltmeter, "Voltmeter", "A voltmeter is not required for this practical."),
            Wrong(ElectronicsEquipmentType.Magnet, "Magnet", "A magnet is not required to investigate a diode."),
            Wrong(ElectronicsEquipmentType.Stopwatch, "Stopwatch", "Time is not measured in this practical."),
            Wrong(ElectronicsEquipmentType.Pulley, "Pulley", "A pulley is used in mechanics, not electronics.")
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
            var refs = Object.FindAnyObjectByType<ElectronicsUIRefs>();
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
            var card = obj.GetComponent<ElectronicsEquipmentCardUI>() ?? obj.AddComponent<ElectronicsEquipmentCardUI>();
            card.Initialize(def, this);
            card.SetCompactMode();
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(ElectronicsEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            ElectronicsScoreManager.Instance?.AddToCategory(ElectronicsScoreCategory.Equipment, 3, false);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is required for this practical."
                : def.correctReason;
            ElectronicsFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, "+3 MARKS", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                ElectronicsFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            ElectronicsScoreManager.Instance?.SubtractScore(3);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            ElectronicsFeedbackManager.Instance?.ShowMessage("✗ WRONG EQUIPMENT\n" + reason, "-3 MARKS", new Color(0.75f, 0.12f, 0.12f));
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

    private void MoveToRequired(ElectronicsEquipmentDefinition def)
    {
        if (requiredEquipmentArea == null) return;
        ElectronicsEquipmentCardUI match = null;
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

    private static ElectronicsEquipmentDefinition Def(ElectronicsEquipmentType type, string name, string reason)
    {
        return new ElectronicsEquipmentDefinition { type = type, displayName = name, isRequired = true, correctReason = reason };
    }

    private static ElectronicsEquipmentDefinition Wrong(ElectronicsEquipmentType type, string name, string reason)
    {
        return new ElectronicsEquipmentDefinition { type = type, displayName = name, isRequired = false, incorrectReason = reason };
    }
}
