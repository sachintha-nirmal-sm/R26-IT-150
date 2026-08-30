using System.Collections.Generic;
using UnityEngine;

public class WavesEquipmentSelectionManager : MonoBehaviour
{
    public static WavesEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<WavesEquipmentDefinition> allEquipment = new List<WavesEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<WavesEquipmentType> selectedRequired = new HashSet<WavesEquipmentType>();
    private readonly List<WavesEquipmentCardUI> spawnedCards = new List<WavesEquipmentCardUI>();
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
        allEquipment = new List<WavesEquipmentDefinition>
        {
            Def(WavesEquipmentType.Slinky, "Slinky", true, "A long coiled spring (slinky) is the medium through which the transverse wave travels."),
            Def(WavesEquipmentType.Ribbons, "Pieces of ribbon", true, "Ribbons tied along the slinky mark particles of the medium so you can see how they move."),
            Def(WavesEquipmentType.Table, "Table", true, "The slinky must lie flat on a table so the pulse travels along its length."),
            Wrong(WavesEquipmentType.NewtonBalance, "Newton balance", "A newton balance measures force. This practical demonstrates a wave, not a force reading."),
            Wrong(WavesEquipmentType.WoodenBlock, "Wooden block", "A wooden block is used in friction or density experiments, not to form a slinky wave."),
            Wrong(WavesEquipmentType.MeterRuler, "Meter ruler", "Length is not measured here. You observe the motion of the slinky and ribbons."),
            Wrong(WavesEquipmentType.LooseSpring, "Loose spring without a slinky", "You need a long slinky laid on the table, not a short hanging spring."),
            Wrong(WavesEquipmentType.Ammeter, "Ammeter", "An ammeter measures electric current, not wave motion."),
            Wrong(WavesEquipmentType.Voltmeter, "Voltmeter", "A voltmeter measures potential difference, not waves."),
            Wrong(WavesEquipmentType.DryCell, "Dry cells", "No electrical circuit is used in this demonstration."),
            Wrong(WavesEquipmentType.Bulb, "Bulb", "A bulb is an electrical component and is not required."),
            Wrong(WavesEquipmentType.Beaker, "Beaker", "A beaker is used for liquids, not for a slinky wave."),
            Wrong(WavesEquipmentType.MeasuringCylinder, "Measuring cylinder", "Volume is not measured in this practical."),
            Wrong(WavesEquipmentType.Thermometer, "Thermometer", "Temperature is not the observed quantity."),
            Wrong(WavesEquipmentType.Magnet, "Magnet", "A magnet is not required to form a mechanical wave on a slinky."),
            Wrong(WavesEquipmentType.Stopwatch, "Stopwatch", "Time is not recorded. You observe the direction of particle motion."),
            Wrong(WavesEquipmentType.MassHanger, "Mass hanger", "No hanging masses are needed. The slinky is shaken on the table."),
            Wrong(WavesEquipmentType.Compass, "Compass", "A compass is not required for this experiment."),
            Wrong(WavesEquipmentType.BunsenBurner, "Bunsen burner", "Heating is not part of this demonstration."),
            Wrong(WavesEquipmentType.Pulley, "Pulley", "A pulley is not used. One end of the slinky is held and shaken by hand."),
            Wrong(WavesEquipmentType.Trolley, "Trolley", "A trolley is used in motion and force practicals, not here."),
            Wrong(WavesEquipmentType.Sandpaper, "Sandpaper", "Sandpaper is not required for a slinky wave.")
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
            var refs = Object.FindAnyObjectByType<WavesUIRefs>();
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
            var card = obj.GetComponent<WavesEquipmentCardUI>() ?? obj.AddComponent<WavesEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(WavesEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired || def.isOptional)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            WavesScoreManager.Instance?.AddScore(4, false);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is required for this practical."
                : def.correctReason;
            WavesFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, "+4 MARKS", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                WavesFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            WavesScoreManager.Instance?.SubtractScore(4);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            WavesFeedbackManager.Instance?.ShowMessage("✗ WRONG EQUIPMENT\n" + reason, "-4 MARKS", new Color(0.75f, 0.12f, 0.12f));
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

    private void MoveToRequired(WavesEquipmentDefinition def)
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

    private static WavesEquipmentDefinition Def(WavesEquipmentType type, string name, bool required, string reason)
    {
        return new WavesEquipmentDefinition { type = type, displayName = name, isRequired = required, correctReason = reason };
    }

    private static WavesEquipmentDefinition Wrong(WavesEquipmentType type, string name, string reason)
    {
        return new WavesEquipmentDefinition { type = type, displayName = name, isRequired = false, incorrectReason = reason };
    }
}
