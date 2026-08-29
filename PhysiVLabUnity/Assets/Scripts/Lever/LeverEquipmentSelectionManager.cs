using System.Collections.Generic;
using UnityEngine;

public class LeverEquipmentSelectionManager : MonoBehaviour
{
    public static LeverEquipmentSelectionManager Instance { get; private set; }

    [SerializeField] private List<LeverEquipmentDefinition> allEquipment = new List<LeverEquipmentDefinition>();
    [SerializeField] private Transform equipmentCardContainer;
    [SerializeField] private Transform requiredEquipmentArea;
    [SerializeField] private GameObject equipmentCardPrefab;

    private readonly HashSet<LeverEquipmentType> selectedRequired = new HashSet<LeverEquipmentType>();
    private readonly List<LeverEquipmentCardUI> spawnedCards = new List<LeverEquipmentCardUI>();
    private bool selectionComplete;

    public bool IsEquipmentComplete => selectionComplete;

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
        allEquipment = new List<LeverEquipmentDefinition>
        {
            new LeverEquipmentDefinition { type = LeverEquipmentType.Book, displayName = "Book", isRequired = true },
            new LeverEquipmentDefinition { type = LeverEquipmentType.NewtonSpringBalance, displayName = "Newton Spring Balance", isRequired = true },
            new LeverEquipmentDefinition { type = LeverEquipmentType.WoodenStrip, displayName = "Wooden Strip", isRequired = true },
            new LeverEquipmentDefinition { type = LeverEquipmentType.SupportPivot, displayName = "Support (Pivot)", isRequired = true },
            new LeverEquipmentDefinition { type = LeverEquipmentType.Ruler, displayName = "Ruler", isRequired = true },
            new LeverEquipmentDefinition { type = LeverEquipmentType.Beaker, displayName = "Beaker", isRequired = false },
            new LeverEquipmentDefinition { type = LeverEquipmentType.MeasuringCylinder, displayName = "Measuring Cylinder", isRequired = false },
            new LeverEquipmentDefinition { type = LeverEquipmentType.Thermometer, displayName = "Thermometer", isRequired = false },
            new LeverEquipmentDefinition { type = LeverEquipmentType.Stopwatch, displayName = "Stopwatch", isRequired = false },
            new LeverEquipmentDefinition { type = LeverEquipmentType.Ammeter, displayName = "Ammeter", isRequired = false },
            new LeverEquipmentDefinition { type = LeverEquipmentType.Voltmeter, displayName = "Voltmeter", isRequired = false },
            new LeverEquipmentDefinition { type = LeverEquipmentType.Magnet, displayName = "Magnet", isRequired = false },
            new LeverEquipmentDefinition { type = LeverEquipmentType.BunsenBurner, displayName = "Bunsen Burner", isRequired = false },
            new LeverEquipmentDefinition { type = LeverEquipmentType.SandBag, displayName = "Sand Bag", isRequired = false },
            new LeverEquipmentDefinition { type = LeverEquipmentType.Pulley, displayName = "Pulley", isRequired = false }
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
            var refs = Object.FindAnyObjectByType<LeverUIRefsHolder>();
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
            var card = obj.GetComponent<LeverEquipmentCardUI>() ?? obj.AddComponent<LeverEquipmentCardUI>();
            card.Initialize(def, this);
            spawnedCards.Add(card);
        }
    }

    public void SelectEquipment(LeverEquipmentDefinition def)
    {
        if (selectionComplete) return;
        if (def.isRequired)
        {
            if (selectedRequired.Contains(def.type)) return;
            selectedRequired.Add(def.type);
            LeverScoreManager.Instance?.AddScore(5);
            LeverFeedbackManager.Instance?.ShowInstruction(def.displayName + " is required.");
            MoveToRequired(def);
        }
        else
        {
            LeverScoreManager.Instance?.SubtractScore(5);
            LeverFeedbackManager.Instance?.ShowInstruction(def.displayName + " is not required for this experiment.");
            LeverGameManager.Instance?.RegisterMistake();
        }
        ValidateSelection();
    }

    private void ValidateSelection()
    {
        int requiredCount = 0;
        foreach (var d in allEquipment) if (d.isRequired) requiredCount++;
        if (selectedRequired.Count >= requiredCount)
        {
            selectionComplete = true;
            LeverFeedbackManager.Instance?.ShowInstruction("All required equipment selected! Press NEXT STEP.");
            LeverUIManager.Instance?.SetEquipContinueVisible(true);
            LeverUIManager.Instance?.UpdateInstruction($"Required equipment selected ({selectedRequired.Count}/5). Press NEXT STEP.");
        }
        else
        {
            LeverUIManager.Instance?.UpdateInstruction($"Selected {selectedRequired.Count}/5 required items. Keep going!");
        }
    }

    public bool IsCompleteCheck() => selectionComplete;

    private void MoveToRequired(LeverEquipmentDefinition def)
    {
        foreach (var card in spawnedCards)
        {
            if (card.Definition.type == def.type && requiredEquipmentArea != null)
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
        LeverUIManager.Instance?.SetEquipContinueVisible(false);
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
