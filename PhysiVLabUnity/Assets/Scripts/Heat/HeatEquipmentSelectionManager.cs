using System.Collections.Generic;
using UnityEngine;

public class HeatEquipmentSelectionManager : MonoBehaviour
{
    public static HeatEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<HeatEquipmentDefinition> allEquipment = new List<HeatEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<HeatEquipmentType> selectedRequired = new HashSet<HeatEquipmentType>();
    private readonly List<HeatEquipmentCardUI> spawnedCards = new List<HeatEquipmentCardUI>();
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
        allEquipment = new List<HeatEquipmentDefinition>
        {
            Def(HeatEquipmentType.TestTube, "Test tube", true, "The test tube holds the coloured water. It is the glass container whose expansion you will also observe."),
            Def(HeatEquipmentType.ColoredWater, "Coloured water", true, "Coloured water is the liquid whose expansion is shown in the thin glass tube."),
            Def(HeatEquipmentType.RubberStopper, "Rubber stopper", true, "The stopper seals the test tube so the liquid can only rise through the thin tube."),
            Def(HeatEquipmentType.ThinGlassTube, "Thin glass tube", true, "The narrow tube magnifies the small change in liquid volume so you can see levels A, B and C."),
            Def(HeatEquipmentType.Beaker, "Beaker", true, "The beaker holds the warm-water bath that heats the test tube."),
            Def(HeatEquipmentType.BunsenBurner, "Bunsen burner", true, "The burner heats the water in the beaker, which then heats the test tube."),
            Def(HeatEquipmentType.RetortStand, "Retort stand and clamp", true, "The clamp holds the test tube so its lower part sits in the water bath."),
            Def(HeatEquipmentType.TripodStand, "Tripod stand", true, "The tripod supports the beaker over the Bunsen burner."),
            Wrong(HeatEquipmentType.Thermometer, "Thermometer", "Temperature is not recorded as a number in this illustration. You observe the liquid level."),
            Wrong(HeatEquipmentType.MeasuringCylinder, "Measuring cylinder", "Volume is not measured with a scale. The thin tube shows the expansion."),
            Wrong(HeatEquipmentType.NewtonBalance, "Newton balance", "A newton balance measures force, not thermal expansion."),
            Wrong(HeatEquipmentType.WoodenBlock, "Wooden block", "A wooden block is not part of this heating arrangement."),
            Wrong(HeatEquipmentType.Slinky, "Slinky", "A slinky is used in wave practicals, not heat."),
            Wrong(HeatEquipmentType.Ammeter, "Ammeter", "No electric current is measured."),
            Wrong(HeatEquipmentType.Voltmeter, "Voltmeter", "No voltage is measured."),
            Wrong(HeatEquipmentType.DryCell, "Dry cells", "No electrical circuit is needed."),
            Wrong(HeatEquipmentType.Bulb, "Bulb", "A bulb is not the heat source. Use a Bunsen burner."),
            Wrong(HeatEquipmentType.Magnet, "Magnet", "A magnet is not required."),
            Wrong(HeatEquipmentType.Stopwatch, "Stopwatch", "Time is not recorded. You watch the liquid level fall then rise."),
            Wrong(HeatEquipmentType.MassHanger, "Mass hanger", "No hanging masses are used."),
            Wrong(HeatEquipmentType.Compass, "Compass", "A compass is not required."),
            Wrong(HeatEquipmentType.Pulley, "Pulley", "A pulley is not used."),
            Wrong(HeatEquipmentType.Trolley, "Trolley", "A trolley is used in motion practicals, not here."),
            Wrong(HeatEquipmentType.ConcaveMirror, "Concave mirror", "A mirror is used in optics, not to show expansion of a liquid."),
            Wrong(HeatEquipmentType.ConvexLens, "Convex lens", "A lens is not required."),
            Wrong(HeatEquipmentType.GlassPrism, "Glass prism", "A prism is used for refraction and dispersion, not here."),
            Wrong(HeatEquipmentType.MeterRuler, "Meter ruler", "You mark levels A, B and C on the thin tube. You do not measure a length with a ruler."),
            Wrong(HeatEquipmentType.WhiteScreen, "White screen", "No image is formed on a screen."),
            Wrong(HeatEquipmentType.GlassSlab, "Glass slab", "A glass slab is used in refraction experiments."),
            Wrong(HeatEquipmentType.ConcaveLens, "Concave lens", "A lens is not part of this practical.")
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
            var refs = Object.FindAnyObjectByType<HeatUIRefs>();
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
            var card = obj.GetComponent<HeatEquipmentCardUI>() ?? obj.AddComponent<HeatEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(HeatEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired || def.isOptional)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            HeatScoreManager.Instance?.AddScore(4, false);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is required for this practical."
                : def.correctReason;
            HeatFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, "+4 MARKS", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                HeatFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            HeatScoreManager.Instance?.SubtractScore(4);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            HeatFeedbackManager.Instance?.ShowMessage("✗ WRONG EQUIPMENT\n" + reason, "-4 MARKS", new Color(0.75f, 0.12f, 0.12f));
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

    private void MoveToRequired(HeatEquipmentDefinition def)
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

    private static HeatEquipmentDefinition Def(HeatEquipmentType type, string name, bool required, string reason)
    {
        return new HeatEquipmentDefinition { type = type, displayName = name, isRequired = required, correctReason = reason };
    }

    private static HeatEquipmentDefinition Wrong(HeatEquipmentType type, string name, string reason)
    {
        return new HeatEquipmentDefinition { type = type, displayName = name, isRequired = false, incorrectReason = reason };
    }
}
