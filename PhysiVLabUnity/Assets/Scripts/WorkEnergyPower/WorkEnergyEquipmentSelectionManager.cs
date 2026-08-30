using System.Collections.Generic;
using UnityEngine;

public class WorkEnergyEquipmentSelectionManager : MonoBehaviour
{
    public static WorkEnergyEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<EquipmentDefinition> allEquipment = new List<EquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<WorkEnergyEquipmentType> selectedRequired = new HashSet<WorkEnergyEquipmentType>();
    private readonly List<WorkEnergyEquipmentCardUI> spawnedCards = new List<WorkEnergyEquipmentCardUI>();
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
        allEquipment = new List<EquipmentDefinition>
        {
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Clay, displayName = "Clay", isRequired = true, correctReason = "Clay is required because the depression produced by the falling weight must be observed and measured." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.HeavyWeight, displayName = "Heavy Weight", isRequired = true, correctReason = "A fairly heavy weight is released from rest onto the clay surface." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.ReleaseStand, displayName = "Stand / Support", isRequired = true, correctReason = "A stand supports the weight at a chosen vertical height." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.ReleaseMechanism, displayName = "Release Mechanism", isRequired = true, correctReason = "The release mechanism holds the weight and lets it fall from rest." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Ruler, displayName = "Metre Ruler", isRequired = true, correctReason = "A ruler is used to measure the height above the clay surface." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.DepthRuler, displayName = "Depth Ruler", isRequired = true, correctReason = "A depth ruler is used to measure the depression in the clay." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.ClayTray, displayName = "Clay Tray", isRequired = true, correctReason = "The clay tray holds a flat clay surface of about 3 cm thickness." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Balance, displayName = "Balance", isRequired = false, isOptional = true, correctReason = "A balance can be used to measure the mass of the weight." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Beaker, displayName = "Beaker", isRequired = false, incorrectReason = "A beaker is not required for this experiment." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.MeasuringCylinder, displayName = "Measuring Cylinder", isRequired = false, incorrectReason = "A measuring cylinder is not required for this experiment." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Thermometer, displayName = "Thermometer", isRequired = false, incorrectReason = "A thermometer is not required for this experiment." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.NewtonSpringBalance, displayName = "Newton Spring Balance", isRequired = false, incorrectReason = "A newton spring balance is not required for this experiment." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Ammeter, displayName = "Ammeter", isRequired = false, incorrectReason = "An ammeter is not required for this experiment." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Voltmeter, displayName = "Voltmeter", isRequired = false, incorrectReason = "A voltmeter is not required for this experiment." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Magnet, displayName = "Magnet", isRequired = false, incorrectReason = "A magnet is not required for this experiment." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Pulley, displayName = "Pulley", isRequired = false, incorrectReason = "A pulley is not required for this experiment." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Lever, displayName = "Wooden Lever", isRequired = false, incorrectReason = "A wooden lever is not required for this experiment." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Stopwatch, displayName = "Stopwatch", isRequired = false, incorrectReason = "A stopwatch is not required for the main clay-depression experiment." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.BunsenBurner, displayName = "Bunsen Burner", isRequired = false, incorrectReason = "A bunsen burner is not required for this experiment." },
            new EquipmentDefinition { type = WorkEnergyEquipmentType.Compass, displayName = "Compass", isRequired = false, incorrectReason = "A compass is not required for this experiment." }
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
            var refs = Object.FindAnyObjectByType<WorkEnergyUIRefsHolder>();
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
            var obj = Instantiate(equipmentCardPrefab, equipmentCardContainer);
            obj.SetActive(true);
            var card = obj.GetComponent<WorkEnergyEquipmentCardUI>() ?? obj.AddComponent<WorkEnergyEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(EquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired || def.isOptional)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            WorkEnergyScoreManager.Instance?.AddScore(5);
            string reason = string.IsNullOrEmpty(def.correctReason)
                ? def.displayName + " is useful for this practical."
                : def.correctReason;
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("✓ Correct Equipment\n" + reason);
            MoveToRequired(def);
        }
        else
        {
            WorkEnergyScoreManager.Instance?.SubtractScore(5);
            string reason = string.IsNullOrEmpty(def.incorrectReason)
                ? def.displayName + " is not required for this experiment."
                : def.incorrectReason;
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("✗ Incorrect Equipment\n" + reason);
        }
        ValidateSelection();
    }

    private void ValidateSelection()
    {
        int requiredCount = RequiredCount;
        int selectedMain = 0;
        foreach (var d in allEquipment)
        {
            if (d.isRequired && selectedRequired.Contains(d.type)) selectedMain++;
        }

        if (selectedMain >= requiredCount)
        {
            selectionComplete = true;
            WorkEnergyFeedbackManager.Instance?.ShowInstruction("All required equipment selected. Press NEXT STEP.");
            WorkEnergyUIManager.Instance?.SetEquipContinueVisible(true);
            WorkEnergyUIManager.Instance?.UpdateInstruction($"Required equipment selected ({selectedMain}/{requiredCount}). Press NEXT STEP.");
        }
        else
        {
            WorkEnergyUIManager.Instance?.UpdateInstruction($"Selected {selectedMain}/{requiredCount} required items. Drag or tap the correct equipment.");
        }
    }

    public bool IsCompleteCheck() => selectionComplete;

    public void SelectRemainingRequired()
    {
        foreach (var def in allEquipment)
        {
            if (!def.isRequired) continue;
            if (selectedRequired.Contains(def.type)) continue;
            selectedRequired.Add(def.type);
            WorkEnergyScoreManager.Instance?.AddScore(5);
            MoveToRequired(def);
        }
        ValidateSelection();
        if (!selectionComplete)
        {
            selectionComplete = true;
            WorkEnergyUIManager.Instance?.SetEquipContinueVisible(true);
        }
    }

    private void MoveToRequired(EquipmentDefinition def)
    {
        foreach (var card in spawnedCards)
        {
            if (card != null && card.Definition.type == def.type && requiredEquipmentArea != null)
            {
                card.transform.SetParent(requiredEquipmentArea, false);
                card.SetCompactMode();
                break;
            }
        }
    }

    public void ResetSelection()
    {
        selectionComplete = false;
        selectedRequired.Clear();
        WorkEnergyUIManager.Instance?.SetEquipContinueVisible(false);
        BuildCards();
    }

    private void ClearCards()
    {
        foreach (var c in spawnedCards) if (c != null) Destroy(c.gameObject);
        spawnedCards.Clear();
        if (equipmentCardContainer == null) return;
        for (int i = equipmentCardContainer.childCount - 1; i >= 0; i--)
            Destroy(equipmentCardContainer.GetChild(i).gameObject);
    }
}
