using System.Collections.Generic;
using UnityEngine;

public class ElecEquipmentSelectionManager : MonoBehaviour
{
    public static ElecEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<ElecEquipmentDefinition> allEquipment = new List<ElecEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<ElecEquipmentType> selectedRequired = new HashSet<ElecEquipmentType>();
    private readonly List<ElecEquipmentCardUI> spawnedCards = new List<ElecEquipmentCardUI>();
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
        allEquipment = new List<ElecEquipmentDefinition>
        {
            new ElecEquipmentDefinition { type = ElecEquipmentType.DryCell1, displayName = "Dry Cell 1", isRequired = true, correctReason = "A dry cell provides the potential difference for the circuit." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.DryCell2, displayName = "Dry Cell 2", isRequired = true, correctReason = "A second dry cell is required so different cell connections can be compared." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.ConductingWires, displayName = "Conducting Wires", isRequired = true, correctReason = "Conducting wires connect the terminals of the components." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Bulb, displayName = "Bulb", isRequired = true, correctReason = "The bulb is the load. Its brightness shows the effect of each cell arrangement." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Ammeter, displayName = "Ammeter", isRequired = true, correctReason = "An ammeter is required to measure the current through the bulb. An ammeter must be connected in series." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Voltmeter, displayName = "Voltmeter", isRequired = true, correctReason = "A voltmeter is required to measure the potential difference across the bulb. A voltmeter must be connected in parallel across the bulb." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.CircuitBoard, displayName = "Circuit Board", isRequired = false, isOptional = true, correctReason = "A circuit board or laboratory base is a useful surface for building the circuit." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.NewtonSpringBalance, displayName = "Newton Spring Balance", isRequired = false, incorrectReason = "A newton spring balance measures force, not current or potential difference." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Ruler, displayName = "Ruler", isRequired = false, incorrectReason = "A ruler is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Thermometer, displayName = "Thermometer", isRequired = false, incorrectReason = "A thermometer is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.MeasuringCylinder, displayName = "Measuring Cylinder", isRequired = false, incorrectReason = "A measuring cylinder is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Beaker, displayName = "Beaker", isRequired = false, incorrectReason = "A beaker is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Magnet, displayName = "Magnet", isRequired = false, incorrectReason = "A magnet is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Pulley, displayName = "Pulley", isRequired = false, incorrectReason = "A pulley is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Lever, displayName = "Lever", isRequired = false, incorrectReason = "A lever is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Stopwatch, displayName = "Stopwatch", isRequired = false, incorrectReason = "A stopwatch is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.BunsenBurner, displayName = "Bunsen Burner", isRequired = false, incorrectReason = "A bunsen burner is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Compass, displayName = "Compass", isRequired = false, incorrectReason = "A compass is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Clay, displayName = "Clay", isRequired = false, incorrectReason = "Clay is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.HeavyWeight, displayName = "Heavy Weight", isRequired = false, incorrectReason = "A heavy weight is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Spring, displayName = "Spring", isRequired = false, incorrectReason = "A spring is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.Barometer, displayName = "Barometer", isRequired = false, incorrectReason = "A barometer is not required for this electricity practical." },
            new ElecEquipmentDefinition { type = ElecEquipmentType.IncorrectAmmeter, displayName = "Ammeter (AC only)", isRequired = false, incorrectReason = "This practical uses a DC ammeter in series with the bulb, not an AC-only meter." }
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
            var refs = Object.FindAnyObjectByType<ElecUIRefsHolder>();
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
            var card = obj.GetComponent<ElecEquipmentCardUI>() ?? obj.AddComponent<ElecEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(ElecEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired || def.isOptional)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            ElecScoreManager.Instance?.AddScore(5);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is useful for this practical."
                : def.correctReason;
            ElecFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, "+5 Marks", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                ElecFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            ElecScoreManager.Instance?.SubtractScore(5);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            ElecFeedbackManager.Instance?.ShowMessage("✗ INCORRECT EQUIPMENT\n" + reason, "-5 Marks", new Color(0.75f, 0.12f, 0.12f));
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

    private void MoveToRequired(ElecEquipmentDefinition def)
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
}
