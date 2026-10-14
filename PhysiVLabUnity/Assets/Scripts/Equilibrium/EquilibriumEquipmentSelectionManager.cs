using System.Collections.Generic;
using UnityEngine;

public class EquilibriumEquipmentSelectionManager : MonoBehaviour
{
    public static EquilibriumEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<EquilibriumEquipmentDefinition> allEquipment = new List<EquilibriumEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<EquilibriumEquipmentType> selectedRequired = new HashSet<EquilibriumEquipmentType>();
    private readonly List<EquilibriumEquipmentCardUI> spawnedCards = new List<EquilibriumEquipmentCardUI>();
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
        allEquipment = new List<EquilibriumEquipmentDefinition>
        {
            Def(EquilibriumEquipmentType.TwoSpringBalances, "Two spring balances", true, "F1 and F2 are the two upward forces. Their readings are compared with the weight of the ruler."),
            Def(EquilibriumEquipmentType.MeterRuler, "Meter ruler", true, "The meter ruler is the horizontal object whose weight W acts at its centre of gravity."),
            Def(EquilibriumEquipmentType.TwoRubberBands, "Two rubber bands", true, "Rubber bands loop around the ends of the ruler and hook onto the spring balances."),
            Def(EquilibriumEquipmentType.RetortStand, "Retort stand / support", true, "A stand (or overhead support) is needed to hang the two spring balances vertically."),
            Wrong(EquilibriumEquipmentType.Trolley, "Trolley", "A trolley is used in the resultant-force practical, not for a hanging meter ruler."),
            Wrong(EquilibriumEquipmentType.Pulley, "Pulleys", "Pulleys are not required. The spring balances hang vertically from a stand."),
            Wrong(EquilibriumEquipmentType.WoodenBlock, "Wooden block", "A wooden block is used in the friction practical, not here."),
            Wrong(EquilibriumEquipmentType.Sandpaper, "Sandpaper", "Sandpaper is not required for investigating equilibrium of forces."),
            Wrong(EquilibriumEquipmentType.Drill, "Drill", "No holes need to be drilled. The rubber bands loop around the ruler."),
            Wrong(EquilibriumEquipmentType.ScrewNail, "Screw nail", "There is no pivot to clamp. The ruler hangs freely from two spring balances."),
            Wrong(EquilibriumEquipmentType.WoodenStick, "Calibrated wooden stick", "This practical uses a meter ruler, not the turning-effect stick."),
            Wrong(EquilibriumEquipmentType.LooseSpring, "Spring without a scale", "A loose spring cannot measure force. You need spring balances with newton scales."),
            Wrong(EquilibriumEquipmentType.Ammeter, "Ammeter", "An ammeter measures electric current, not force."),
            Wrong(EquilibriumEquipmentType.Voltmeter, "Voltmeter", "A voltmeter measures potential difference, not force."),
            Wrong(EquilibriumEquipmentType.DryCell, "Dry cells", "Dry cells supply electrical energy and are not required here."),
            Wrong(EquilibriumEquipmentType.Bulb, "Bulb", "A bulb is an electrical component and is not required."),
            Wrong(EquilibriumEquipmentType.Beaker, "Beaker", "A beaker is used for liquids, not forces."),
            Wrong(EquilibriumEquipmentType.MeasuringCylinder, "Measuring cylinder", "A measuring cylinder measures volume, not force."),
            Wrong(EquilibriumEquipmentType.Thermometer, "Thermometer", "A thermometer measures temperature, not force."),
            Wrong(EquilibriumEquipmentType.Magnet, "Magnet", "A magnet is not required for this experiment."),
            Wrong(EquilibriumEquipmentType.Stopwatch, "Stopwatch", "Time is not the measured quantity. You record force in newtons."),
            Wrong(EquilibriumEquipmentType.MassHanger, "Mass hanger", "The weight of the ruler itself is W. No extra hanging masses are needed."),
            Wrong(EquilibriumEquipmentType.Compass, "Compass", "A compass is not required for this experiment."),
            Wrong(EquilibriumEquipmentType.BunsenBurner, "Bunsen burner", "A bunsen burner is not required for this experiment."),
            Wrong(EquilibriumEquipmentType.Wire, "Piece of wire", "Rubber bands, not wire loops, connect the ruler to the spring balances.")
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
            var refs = Object.FindAnyObjectByType<EquilibriumUIRefs>();
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
            var card = obj.GetComponent<EquilibriumEquipmentCardUI>() ?? obj.AddComponent<EquilibriumEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(EquilibriumEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired || def.isOptional)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            EquilibriumScoreManager.Instance?.AddScore(4, false);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is required for this practical."
                : def.correctReason;
            EquilibriumFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, "+4 MARKS", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                EquilibriumFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            EquilibriumScoreManager.Instance?.SubtractScore(4);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            EquilibriumFeedbackManager.Instance?.ShowMessage("✗ WRONG EQUIPMENT\n" + reason, "-4 MARKS", new Color(0.75f, 0.12f, 0.12f));
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

    private void MoveToRequired(EquilibriumEquipmentDefinition def)
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

    private static EquilibriumEquipmentDefinition Def(EquilibriumEquipmentType type, string name, bool required, string reason)
    {
        return new EquilibriumEquipmentDefinition { type = type, displayName = name, isRequired = required, correctReason = reason };
    }

    private static EquilibriumEquipmentDefinition Wrong(EquilibriumEquipmentType type, string name, string reason)
    {
        return new EquilibriumEquipmentDefinition { type = type, displayName = name, isRequired = false, incorrectReason = reason };
    }
}
