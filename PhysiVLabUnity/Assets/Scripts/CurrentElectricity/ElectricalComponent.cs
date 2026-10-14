using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElectricalComponent : MonoBehaviour
{
    [SerializeField] private string componentId;
    [SerializeField] private ElectricalComponentType componentType;
    [SerializeField] private List<ElectricalTerminal> terminals = new List<ElectricalTerminal>();
    [SerializeField] private bool isPlaced;
    [SerializeField] private bool flipped;

    private ElecDragDrop2D drag;
    private RectTransform rectTransform;
    private Transform homeParent;
    private Vector2 homePos;
    private Vector2 termAHome;
    private Vector2 termBHome;
    private bool termHomesStored;

    public string ComponentId => componentId;
    public ElectricalComponentType ComponentType => componentType;
    public IReadOnlyList<ElectricalTerminal> Terminals => terminals;
    public bool IsPlaced => isPlaced;
    public bool Flipped => flipped;
    public RectTransform Rect => rectTransform;

    public ElectricalTerminal TerminalA => terminals.Count > 0 ? terminals[0] : null;
    public ElectricalTerminal TerminalB => terminals.Count > 1 ? terminals[1] : null;

    public void Configure(string id, ElectricalComponentType type)
    {
        componentId = id;
        componentType = type;
        rectTransform = GetComponent<RectTransform>();
        drag = GetComponent<ElecDragDrop2D>() ?? gameObject.AddComponent<ElecDragDrop2D>();
        drag.Configure(id);
        if (rectTransform != null && transform.parent != null)
        {
            homeParent = transform.parent;
            homePos = rectTransform.anchoredPosition;
            drag.StoreHome(homeParent, homePos);
        }
        EnsureTerminals();
        StoreTerminalHomes();
    }

    public void StoreHome()
    {
        rectTransform = GetComponent<RectTransform>();
        homeParent = transform.parent;
        homePos = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        if (drag != null) drag.StoreHome(homeParent, homePos);
    }

    public void MarkPlaced(bool placed)
    {
        isPlaced = placed;
    }

    public void ReturnHome()
    {
        isPlaced = false;
        flipped = false;
        ApplyFlipVisual();
        if (drag != null) drag.ResetItem();
        else if (homeParent != null && rectTransform != null)
        {
            transform.SetParent(homeParent, false);
            rectTransform.anchoredPosition = homePos;
        }
    }

    public void ToggleFlip()
    {
        flipped = !flipped;
        ApplyFlipVisual();
        CircuitBuilder.Instance?.RefreshWires();
        ElecFeedbackManager.Instance?.ShowInstruction(flipped
            ? $"{DisplayName()} polarity reversed. Check + and − before connecting."
            : $"{DisplayName()} polarity restored.");
    }

    public void SetFlipped(bool value)
    {
        flipped = value;
        ApplyFlipVisual();
    }

    private void StoreTerminalHomes()
    {
        if (terminals.Count < 2) return;
        var a = terminals[0].GetComponent<RectTransform>();
        var b = terminals[1].GetComponent<RectTransform>();
        if (a == null || b == null) return;
        termAHome = a.anchoredPosition;
        termBHome = b.anchoredPosition;
        termHomesStored = true;
    }

    private void ApplyFlipVisual()
    {
        var body = transform.Find("Body");
        if (body != null)
            body.localRotation = Quaternion.Euler(0f, 0f, flipped ? 180f : 0f);

        if (!termHomesStored || terminals.Count < 2) return;
        var a = terminals[0].GetComponent<RectTransform>();
        var b = terminals[1].GetComponent<RectTransform>();
        if (a == null || b == null) return;
        a.anchoredPosition = flipped ? termBHome : termAHome;
        b.anchoredPosition = flipped ? termAHome : termBHome;
    }

    public ElectricalTerminal GetTerminal(string polarity)
    {
        foreach (var t in terminals)
            if (t != null && t.Polarity == polarity) return t;
        return null;
    }

    public string DisplayName()
    {
        switch (componentType)
        {
            case ElectricalComponentType.DryCell: return componentId == "Cell2" ? "Dry Cell 2" : "Dry Cell 1";
            case ElectricalComponentType.Bulb: return "Bulb";
            case ElectricalComponentType.Ammeter: return "Ammeter";
            case ElectricalComponentType.Voltmeter: return "Voltmeter";
            default: return componentId;
        }
    }

    private void EnsureTerminals()
    {
        terminals.Clear();
        foreach (var t in GetComponentsInChildren<ElectricalTerminal>(true))
            terminals.Add(t);

        if (terminals.Count >= 2) return;

        string poleA = "+";
        string poleB = "-";
        Vector2 anchorA = new Vector2(-0.02f, 0.5f);
        Vector2 anchorB = new Vector2(1.02f, 0.5f);
        if (componentType == ElectricalComponentType.Bulb)
        {
            poleA = "A";
            poleB = "B";
        }
        else if (componentType == ElectricalComponentType.DryCell)
        {
            poleA = "-";
            poleB = "+";
        }

        terminals.Add(CreateTerminal("TermA", poleA, anchorA));
        terminals.Add(CreateTerminal("TermB", poleB, anchorB));
    }

    private ElectricalTerminal CreateTerminal(string name, string pole, Vector2 anchor)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(transform, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(42f, 42f);
        rt.anchoredPosition = Vector2.zero;
        var img = obj.AddComponent<Image>();
        img.sprite = ElecIconFactory.White();
        var term = obj.AddComponent<ElectricalTerminal>();
        term.Configure(componentId + "_" + pole, pole, this);
        var labelObj = new GameObject("P");
        labelObj.transform.SetParent(obj.transform, false);
        var lrt = labelObj.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero;
        lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = pole;
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null) tmp.font = TMP_Settings.defaultFontAsset;
        return term;
    }
}
