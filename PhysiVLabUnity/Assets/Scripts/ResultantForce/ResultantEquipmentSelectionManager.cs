using System.Collections.Generic;
using UnityEngine;

public class ResultantEquipmentSelectionManager : MonoBehaviour
{
    public static ResultantEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<ResultantEquipmentDefinition> allEquipment = new List<ResultantEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<ResultantEquipmentType> selectedRequired = new HashSet<ResultantEquipmentType>();
    private readonly List<ResultantEquipmentCardUI> spawnedCards = new List<ResultantEquipmentCardUI>();
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
        allEquipment = new List<ResultantEquipmentDefinition>
        {
            Def(ResultantEquipmentType.Trolley, "Trolley", true, "A trolley is the object on which the two forces act."),
            Def(ResultantEquipmentType.NewtonBalance, "3 Newton balances", true, "Three Newton balances are needed: A measures the resultant, while B and C apply the two forces."),
            Def(ResultantEquipmentType.Pulley, "2 Pulleys", true, "Two pulleys redirect the strings so that forces B and C pull the trolley in the same direction."),
            Def(ResultantEquipmentType.Ring, "Ring", true, "A ring is fixed to the front of the trolley so that two strings can be attached."),
            Def(ResultantEquipmentType.String, "Strings", true, "Strings connect the ring to balances B and C through the pulleys."),
            Def(ResultantEquipmentType.LabTable, "Laboratory table", true, "A horizontal table supports the trolley and pulleys."),
            Def(ResultantEquipmentType.RecordingSheet, "Recording sheet", true, "A recording sheet is needed to write the readings of balances A, B and C."),
            Wrong(ResultantEquipmentType.WoodenBlock, "Wooden block", "A wooden block is used in the friction practical, not this resultant-force experiment."),
            Wrong(ResultantEquipmentType.Sandpaper, "Sandpaper", "Sandpaper is not required. The trolley rolls on a smooth table."),
            Wrong(ResultantEquipmentType.Spring, "Spring without a scale", "A loose spring cannot measure force. You need Newton balances."),
            Wrong(ResultantEquipmentType.Ammeter, "Ammeter", "An ammeter measures electric current, not force."),
            Wrong(ResultantEquipmentType.Voltmeter, "Voltmeter", "A voltmeter measures potential difference, not force."),
            Wrong(ResultantEquipmentType.DryCell, "Dry cells", "Dry cells supply electrical energy and are not required here."),
            Wrong(ResultantEquipmentType.Bulb, "Bulb", "A bulb is an electrical component and is not required."),
            Wrong(ResultantEquipmentType.Beaker, "Beaker", "A beaker is used for liquids, not forces."),
            Wrong(ResultantEquipmentType.MeasuringCylinder, "Measuring cylinder", "A measuring cylinder measures volume, not force."),
            Wrong(ResultantEquipmentType.Thermometer, "Thermometer", "A thermometer measures temperature, not force."),
            Wrong(ResultantEquipmentType.Magnet, "Magnet", "A magnet is not required for this experiment."),
            Wrong(ResultantEquipmentType.Stopwatch, "Stopwatch", "Time is not the measured quantity. You record forces in newtons."),
            Wrong(ResultantEquipmentType.MassHanger, "Mass hanger", "Forces are applied with Newton balances B and C, not hanging masses."),
            Wrong(ResultantEquipmentType.Compass, "Compass", "A compass is not required for this experiment."),
            Wrong(ResultantEquipmentType.BunsenBurner, "Bunsen burner", "A bunsen burner is not required for this experiment.")
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
            var refs = Object.FindAnyObjectByType<ResultantUIRefs>();
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
            var card = obj.GetComponent<ResultantEquipmentCardUI>() ?? obj.AddComponent<ResultantEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(ResultantEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired || def.isOptional)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            ResultantScoreManager.Instance?.AddScore(4, false);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is required for this practical."
                : def.correctReason;
            ResultantFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, "+4 MARKS", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                ResultantFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            ResultantScoreManager.Instance?.SubtractScore(4);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            ResultantFeedbackManager.Instance?.ShowMessage("✗ WRONG EQUIPMENT\n" + reason, "-4 MARKS", new Color(0.75f, 0.12f, 0.12f));
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

    private void MoveToRequired(ResultantEquipmentDefinition def)
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

    private static ResultantEquipmentDefinition Def(ResultantEquipmentType type, string name, bool required, string reason)
    {
        return new ResultantEquipmentDefinition { type = type, displayName = name, isRequired = required, correctReason = reason };
    }

    private static ResultantEquipmentDefinition Wrong(ResultantEquipmentType type, string name, string reason)
    {
        return new ResultantEquipmentDefinition { type = type, displayName = name, isRequired = false, incorrectReason = reason };
    }
}
