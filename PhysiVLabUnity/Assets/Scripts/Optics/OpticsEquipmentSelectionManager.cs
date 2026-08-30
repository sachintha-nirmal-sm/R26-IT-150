using System.Collections.Generic;
using UnityEngine;

public class OpticsEquipmentSelectionManager : MonoBehaviour
{
    public static OpticsEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<OpticsEquipmentDefinition> allEquipment = new List<OpticsEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<OpticsEquipmentType> selectedRequired = new HashSet<OpticsEquipmentType>();
    private readonly List<OpticsEquipmentCardUI> spawnedCards = new List<OpticsEquipmentCardUI>();
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
        allEquipment = new List<OpticsEquipmentDefinition>
        {
            Def(OpticsEquipmentType.ConcaveMirror, "Concave mirror", true, "A concave mirror converges parallel rays from a distant object at its focus."),
            Def(OpticsEquipmentType.WhiteScreen, "White screen", true, "A white screen (or white paper) is needed so the real inverted image can be seen."),
            Def(OpticsEquipmentType.MeterRuler, "Meter ruler", true, "You must measure the distance from the mirror to the screen. That distance is approximately the focal length."),
            Wrong(OpticsEquipmentType.ConvexMirror, "Convex mirror", "A convex mirror diverges rays. It cannot form a real image of a distant scene on a screen."),
            Wrong(OpticsEquipmentType.PlaneMirror, "Plane mirror", "A plane mirror forms a virtual image behind the mirror, not a real image on a screen."),
            Wrong(OpticsEquipmentType.ConvexLens, "Convex lens", "A convex lens can form a real image, but this practical uses a concave mirror."),
            Wrong(OpticsEquipmentType.ConcaveLens, "Concave lens", "A concave lens diverges light and does not form a real image of a distant object on a screen."),
            Wrong(OpticsEquipmentType.GlassPrism, "Glass prism", "A prism is used for refraction and dispersion, not to find the focal length of a mirror."),
            Wrong(OpticsEquipmentType.GlassSlab, "Glass slab", "A glass slab is used in refraction experiments, not here."),
            Wrong(OpticsEquipmentType.Thermometer, "Thermometer", "Temperature is not measured in this practical."),
            Wrong(OpticsEquipmentType.NewtonBalance, "Newton balance", "A newton balance measures force, not focal length."),
            Wrong(OpticsEquipmentType.WoodenBlock, "Wooden block", "A wooden block is not required. Hold the mirror and the screen."),
            Wrong(OpticsEquipmentType.Slinky, "Slinky", "A slinky is used in wave practicals, not geometrical optics."),
            Wrong(OpticsEquipmentType.Ammeter, "Ammeter", "An ammeter measures electric current, not image distance."),
            Wrong(OpticsEquipmentType.Voltmeter, "Voltmeter", "A voltmeter is not used in this optics practical."),
            Wrong(OpticsEquipmentType.DryCell, "Dry cells", "No electrical circuit is needed."),
            Wrong(OpticsEquipmentType.Bulb, "Bulb", "A nearby bulb is not a distant object. Use the scene outside the window."),
            Wrong(OpticsEquipmentType.Beaker, "Beaker", "A beaker is used for liquids, not for this experiment."),
            Wrong(OpticsEquipmentType.MeasuringCylinder, "Measuring cylinder", "Volume is not measured. You measure length with a meter ruler."),
            Wrong(OpticsEquipmentType.Magnet, "Magnet", "A magnet is not required."),
            Wrong(OpticsEquipmentType.Stopwatch, "Stopwatch", "Time is not recorded. You wait until the image is sharp, then measure distance."),
            Wrong(OpticsEquipmentType.MassHanger, "Mass hanger", "No hanging masses are used."),
            Wrong(OpticsEquipmentType.Compass, "Compass", "A compass is not required."),
            Wrong(OpticsEquipmentType.BunsenBurner, "Bunsen burner", "Heating is not part of this practical."),
            Wrong(OpticsEquipmentType.Pulley, "Pulley", "A pulley is not used."),
            Wrong(OpticsEquipmentType.Trolley, "Trolley", "A trolley is used in motion practicals, not here.")
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
            var refs = Object.FindAnyObjectByType<OpticsUIRefs>();
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
            var card = obj.GetComponent<OpticsEquipmentCardUI>() ?? obj.AddComponent<OpticsEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(OpticsEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired || def.isOptional)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            OpticsScoreManager.Instance?.AddScore(4, false);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is required for this practical."
                : def.correctReason;
            OpticsFeedbackManager.Instance?.ShowMessage("✓ CORRECT EQUIPMENT\n" + reason, "+4 MARKS", new Color(0.08f, 0.52f, 0.22f));
            MoveToRequired(def);
            if (AllRequiredSelected())
            {
                selectionComplete = true;
                OpticsFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP to continue.");
            }
        }
        else
        {
            OpticsScoreManager.Instance?.SubtractScore(4);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            OpticsFeedbackManager.Instance?.ShowMessage("✗ WRONG EQUIPMENT\n" + reason, "-4 MARKS", new Color(0.75f, 0.12f, 0.12f));
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

    private void MoveToRequired(OpticsEquipmentDefinition def)
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

    private static OpticsEquipmentDefinition Def(OpticsEquipmentType type, string name, bool required, string reason)
    {
        return new OpticsEquipmentDefinition { type = type, displayName = name, isRequired = required, correctReason = reason };
    }

    private static OpticsEquipmentDefinition Wrong(OpticsEquipmentType type, string name, string reason)
    {
        return new OpticsEquipmentDefinition { type = type, displayName = name, isRequired = false, incorrectReason = reason };
    }
}
