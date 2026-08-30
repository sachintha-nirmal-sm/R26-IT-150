using System.Collections.Generic;
using UnityEngine;

public class NewtonEquipmentSelectionManager : MonoBehaviour
{
    public static NewtonEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<NewtonEquipmentDefinition> allEquipment = new List<NewtonEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<NewtonEquipmentType> selectedRequired = new HashSet<NewtonEquipmentType>();
    private readonly List<NewtonEquipmentCardUI> spawnedCards = new List<NewtonEquipmentCardUI>();
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
        allEquipment = new List<NewtonEquipmentDefinition>
        {
            Def(NewtonEquipmentType.DynamicsTrolley, "Dynamics trolley / toy cart", "A dynamics trolley is the moving body used to investigate Newton's laws."),
            Def(NewtonEquipmentType.StraightTrack, "Straight track", "A straight track provides a low-friction path for the trolley."),
            Def(NewtonEquipmentType.NewtonSpringBalance, "Newton spring balance", "A newton spring balance can be used to measure force and weight."),
            Def(NewtonEquipmentType.MassBlocks, "Mass blocks", "Mass blocks let you change the mass of the trolley."),
            Def(NewtonEquipmentType.WeightHanger, "Weight hanger", "A weight hanger applies a known pulling force to the trolley."),
            Def(NewtonEquipmentType.Stopwatch, "Stopwatch", "A stopwatch is used to measure time of motion."),
            Def(NewtonEquipmentType.Ruler, "Ruler / measuring scale", "A ruler is used to measure distance along the track."),
            Def(NewtonEquipmentType.Balloon, "Balloon", "A balloon is used for the action-reaction (balloon rocket) experiment."),
            Def(NewtonEquipmentType.String, "String", "String connects the trolley to the hanger and guides the balloon rocket."),
            Def(NewtonEquipmentType.Pulley, "Pulley", "A pulley changes the direction of the string so a hanging mass can pull the trolley."),
            Def(NewtonEquipmentType.RecordingTable, "Recording table", "A recording table is needed to write down force, mass, time and acceleration."),
            Def(NewtonEquipmentType.Calculator, "Calculator", "A calculator helps you evaluate a = F/m and W = mg."),
            Wrong(NewtonEquipmentType.Ammeter, "Ammeter", "An ammeter measures electric current and is not required here."),
            Wrong(NewtonEquipmentType.Voltmeter, "Voltmeter", "A voltmeter measures potential difference, not force or motion."),
            Wrong(NewtonEquipmentType.Bulb, "Bulb", "A bulb is an electrical component and is not required for Newton's laws."),
            Wrong(NewtonEquipmentType.DryCell, "Dry cell", "A dry cell is a source of electrical energy, not a mechanics instrument."),
            Wrong(NewtonEquipmentType.Beaker, "Beaker", "A beaker is used for liquids and is not required here."),
            Wrong(NewtonEquipmentType.MeasuringCylinder, "Measuring cylinder", "A measuring cylinder measures volume, not force or mass."),
            Wrong(NewtonEquipmentType.Thermometer, "Thermometer", "A thermometer measures temperature, not motion."),
            Wrong(NewtonEquipmentType.Magnet, "Magnet", "A magnet is not required for this Newton's laws practical."),
            Wrong(NewtonEquipmentType.BunsenBurner, "Bunsen burner", "A bunsen burner is not required for this practical."),
            Wrong(NewtonEquipmentType.Compass, "Compass", "A compass is not required to investigate force, mass and acceleration."),
            Wrong(NewtonEquipmentType.Microscope, "Microscope", "A microscope is used to view small objects, not to measure force."),
            Wrong(NewtonEquipmentType.Pipette, "Pipette", "A pipette is used for liquids and is not required here.")
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
            var refs = Object.FindAnyObjectByType<NewtonUIRefs>();
            if (refs != null) SetupUI(refs.CardContainer, refs.RequiredArea, refs.CardPrefab);
        }
        if (spawnedCards.Count == 0) BuildCards();
        foreach (var card in spawnedCards)
            if (card != null) card.gameObject.SetActive(true);
    }

    public void BuildCards()
    {
        ClearCards();
        if (equipmentCardContainer == null || equipmentCardPrefab == null) return;
        foreach (var def in allEquipment)
        {
            var obj = Object.Instantiate(equipmentCardPrefab, equipmentCardContainer);
            obj.SetActive(true);
            var card = obj.GetComponent<NewtonEquipmentCardUI>() ?? obj.AddComponent<NewtonEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(NewtonEquipmentDefinition def)
    {
        if (selectionComplete) return;
        int marks = NewtonScoreManager.Instance != null ? NewtonScoreManager.Instance.EquipmentScore : 4;
        if (def.isRequired || def.isOptional)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            NewtonScoreManager.Instance?.AddScore(marks, false);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is useful for this practical."
                : def.correctReason;
            NewtonFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, $"+{marks} Marks", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                NewtonFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            NewtonScoreManager.Instance?.SubtractScore(marks);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            NewtonFeedbackManager.Instance?.ShowMessage("✗ WRONG EQUIPMENT\n" + reason, $"-{marks} Marks", new Color(0.75f, 0.12f, 0.12f));
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

    private void MoveToRequired(NewtonEquipmentDefinition def)
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

    private static NewtonEquipmentDefinition Def(NewtonEquipmentType type, string name, string reason)
    {
        return new NewtonEquipmentDefinition { type = type, displayName = name, isRequired = true, correctReason = reason };
    }

    private static NewtonEquipmentDefinition Wrong(NewtonEquipmentType type, string name, string reason)
    {
        return new NewtonEquipmentDefinition { type = type, displayName = name, isRequired = false, incorrectReason = reason };
    }
}
