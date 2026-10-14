using System.Collections.Generic;
using UnityEngine;

public class TurningEquipmentSelectionManager : MonoBehaviour
{
    public static TurningEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<TurningEquipmentDefinition> allEquipment = new List<TurningEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<TurningEquipmentType> selectedRequired = new HashSet<TurningEquipmentType>();
    private readonly List<TurningEquipmentCardUI> spawnedCards = new List<TurningEquipmentCardUI>();
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
        allEquipment = new List<TurningEquipmentDefinition>
        {
            Def(TurningEquipmentType.WoodenStick, "Calibrated wooden stick", true, "A fairly long calibrated stick is the object that turns about the pivot."),
            Def(TurningEquipmentType.RubberWashers, "Two rubber washers", true, "Two rubber washers sit above and below the stick so the screw nail can clamp it without slipping."),
            Def(TurningEquipmentType.Drill, "Drill", true, "A drill is needed to make holes at O, A, B, C and D, 15 cm apart."),
            Def(TurningEquipmentType.NewtonBalance, "Newton balance", true, "A Newton balance measures the force that just starts the stick turning."),
            Def(TurningEquipmentType.LabTable, "Table or wooden plank", true, "The stick is clamped to a table (or plank) at the pivot O."),
            Def(TurningEquipmentType.ScrewNail, "Screw nail", true, "The screw nail through O is the pivot. Tightening it increases friction at the axis."),
            Def(TurningEquipmentType.Wire, "Piece of wire", true, "Wire is used to form loops at A, B, C and D so the Newton balance can be hooked on."),
            Wrong(TurningEquipmentType.Trolley, "Trolley", "A trolley is used in the resultant-force practical, not this turning-effect experiment."),
            Wrong(TurningEquipmentType.Pulley, "Pulleys", "Pulleys are not required. The Newton balance is hooked directly onto a wire loop."),
            Wrong(TurningEquipmentType.WoodenBlock, "Wooden block", "A wooden block is used in the friction practical, not here."),
            Wrong(TurningEquipmentType.Sandpaper, "Sandpaper", "Sandpaper is not required for investigating turning effect."),
            Wrong(TurningEquipmentType.Spring, "Spring without a scale", "A loose spring cannot measure force. You need a Newton balance."),
            Wrong(TurningEquipmentType.Ammeter, "Ammeter", "An ammeter measures electric current, not force."),
            Wrong(TurningEquipmentType.Voltmeter, "Voltmeter", "A voltmeter measures potential difference, not force."),
            Wrong(TurningEquipmentType.DryCell, "Dry cells", "Dry cells supply electrical energy and are not required here."),
            Wrong(TurningEquipmentType.Bulb, "Bulb", "A bulb is an electrical component and is not required."),
            Wrong(TurningEquipmentType.Beaker, "Beaker", "A beaker is used for liquids, not forces."),
            Wrong(TurningEquipmentType.MeasuringCylinder, "Measuring cylinder", "A measuring cylinder measures volume, not force."),
            Wrong(TurningEquipmentType.Thermometer, "Thermometer", "A thermometer measures temperature, not force."),
            Wrong(TurningEquipmentType.Magnet, "Magnet", "A magnet is not required for this experiment."),
            Wrong(TurningEquipmentType.Stopwatch, "Stopwatch", "Time is not the measured quantity. You record force in newtons."),
            Wrong(TurningEquipmentType.MassHanger, "Mass hanger", "Force is applied with a Newton balance, not hanging masses."),
            Wrong(TurningEquipmentType.Compass, "Compass", "A compass is not required for this experiment."),
            Wrong(TurningEquipmentType.BunsenBurner, "Bunsen burner", "A bunsen burner is not required for this experiment.")
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
            var refs = Object.FindAnyObjectByType<TurningUIRefs>();
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
            var card = obj.GetComponent<TurningEquipmentCardUI>() ?? obj.AddComponent<TurningEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(TurningEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired || def.isOptional)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            TurningScoreManager.Instance?.AddScore(4, false);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is required for this practical."
                : def.correctReason;
            TurningFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, "+4 MARKS", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                TurningFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            TurningScoreManager.Instance?.SubtractScore(4);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            TurningFeedbackManager.Instance?.ShowMessage("✗ WRONG EQUIPMENT\n" + reason, "-4 MARKS", new Color(0.75f, 0.12f, 0.12f));
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

    private void MoveToRequired(TurningEquipmentDefinition def)
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

    private static TurningEquipmentDefinition Def(TurningEquipmentType type, string name, bool required, string reason)
    {
        return new TurningEquipmentDefinition { type = type, displayName = name, isRequired = required, correctReason = reason };
    }

    private static TurningEquipmentDefinition Wrong(TurningEquipmentType type, string name, string reason)
    {
        return new TurningEquipmentDefinition { type = type, displayName = name, isRequired = false, incorrectReason = reason };
    }
}
