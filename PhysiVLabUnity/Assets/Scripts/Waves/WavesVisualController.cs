using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WavesVisualController : MonoBehaviour
{
    public static WavesVisualController Instance { get; private set; }

    private GameObject tableVisual;
    private GameObject slinkyHost;
    private GameObject handVisual;
    private GameObject waveArrow;
    private readonly List<RectTransform> coils = new List<RectTransform>();
    private readonly List<RectTransform> ribbons = new List<RectTransform>();
    private readonly List<Vector2> restPositions = new List<Vector2>();
    private TextMeshProUGUI directionLabel;
    private TextMeshProUGUI particleLabel;

    public const int CoilCount = 22;
    public static readonly int[] RibbonCoilIndex = { 4, 8, 12, 16, 20 };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Bind(GameObject table, GameObject slinky, GameObject hand, GameObject arrow, TextMeshProUGUI dirLabel, TextMeshProUGUI partLabel)
    {
        tableVisual = table;
        slinkyHost = slinky;
        handVisual = hand;
        waveArrow = arrow;
        directionLabel = dirLabel;
        particleLabel = partLabel;
        BuildCoils();
        ResetVisuals();
    }

    public void ResetVisuals()
    {
        if (tableVisual != null) tableVisual.SetActive(false);
        if (slinkyHost != null) slinkyHost.SetActive(false);
        if (handVisual != null) handVisual.SetActive(false);
        if (waveArrow != null) waveArrow.SetActive(false);
        foreach (var r in ribbons)
            if (r != null) r.gameObject.SetActive(false);
        RestoreDropSlots();
        AnimateWave(0f, false, 0f, 280f, 140f);
        if (directionLabel != null) directionLabel.text = "Direction of wave: —";
        if (particleLabel != null) particleLabel.text = "Ribbon motion: —";
    }

    public void ShowTable(bool show)
    {
        if (tableVisual != null) tableVisual.SetActive(show);
    }

    public void ShowSlinky(bool show)
    {
        if (slinkyHost != null)
        {
            slinkyHost.SetActive(show);
            slinkyHost.transform.SetAsLastSibling();
        }
        if (handVisual != null)
        {
            handVisual.SetActive(show);
            handVisual.transform.SetAsLastSibling();
        }
        foreach (var coil in coils)
            if (coil != null) coil.gameObject.SetActive(show);
    }

    public void ShowRibbon(int index, bool show)
    {
        if (index < 0 || index >= ribbons.Count) return;
        if (ribbons[index] != null)
        {
            ribbons[index].gameObject.SetActive(show);
            ribbons[index].SetAsLastSibling();
        }
        HideDropSlot(index);
    }

    public void HideDropSlot(int index)
    {
        var slot = FindDropSlot(index);
        if (slot != null) slot.gameObject.SetActive(false);
    }

    public void RestoreDropSlots()
    {
        for (int i = 0; i < 5; i++)
        {
            var slot = FindDropSlot(i);
            if (slot != null) slot.gameObject.SetActive(true);
        }
    }

    private Transform FindDropSlot(int index)
    {
        if (slinkyHost == null) return null;
        Transform zone = slinkyHost.transform.parent;
        return zone != null ? zone.Find("RibbonSlot" + index) : null;
    }

    public void ShowWaveTravel(bool show)
    {
        if (waveArrow != null) waveArrow.SetActive(show);
        if (handVisual != null) handVisual.SetActive(true);
        if (show)
        {
            for (int i = 0; i < 5; i++) HideDropSlot(i);
            if (slinkyHost != null) slinkyHost.transform.SetAsLastSibling();
        }
    }

    public void AnimateWave(float time, bool transverse, float amplitude, float wavelength, float speed)
    {
        if (coils.Count == 0) return;
        float k = wavelength > 1f ? (2f * Mathf.PI) / wavelength : 0.02f;
        float omega = speed * k;

        for (int i = 0; i < coils.Count; i++)
        {
            var coil = coils[i];
            if (coil == null) continue;
            Vector2 rest = i < restPositions.Count ? restPositions[i] : coil.anchoredPosition;
            float x = rest.x;
            Vector2 pos = rest;
            if (transverse)
                pos.y = rest.y + amplitude * Mathf.Sin(k * x - omega * time);
            else if (amplitude > 0f || time > 0f)
                pos.x = rest.x + 10f * Mathf.Sin(k * x - omega * time);
            coil.anchoredPosition = pos;
        }

        for (int r = 0; r < ribbons.Count && r < RibbonCoilIndex.Length; r++)
        {
            var ribbon = ribbons[r];
            int ci = RibbonCoilIndex[r];
            if (ribbon == null || ci < 0 || ci >= coils.Count || coils[ci] == null) continue;
            ribbon.anchoredPosition = coils[ci].anchoredPosition + new Vector2(0f, 16f);
        }

        if (directionLabel != null)
            directionLabel.text = transverse ? "Direction of wave  →  along the slinky" : "Direction of wave: —";
        if (particleLabel != null)
            particleLabel.text = transverse
                ? "Ribbons move  ↕  perpendicular to the wave"
                : "Ribbon motion: —";
    }

    private void BuildCoils()
    {
        if (slinkyHost == null) return;
        var existing = slinkyHost.transform.Find("Coils");
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        coils.Clear();
        ribbons.Clear();
        restPositions.Clear();

        var coilsRoot = new GameObject("Coils");
        coilsRoot.transform.SetParent(slinkyHost.transform, false);
        var rootRt = coilsRoot.AddComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = rootRt.offsetMax = Vector2.zero;

        float left = 70f;
        float spacing = 28f;
        Sprite coilSprite = WavesIconFactory.GetNamed("coil");
        Sprite ribbonSprite = WavesIconFactory.GetSprite(WavesEquipmentType.Ribbons);

        for (int i = 0; i < CoilCount; i++)
        {
            var coilObj = new GameObject("Coil" + i);
            coilObj.transform.SetParent(coilsRoot.transform, false);
            var rt = coilObj.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(34f, 54f);
            rt.anchoredPosition = new Vector2(left + i * spacing, 0f);
            var img = coilObj.AddComponent<Image>();
            img.sprite = coilSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.color = Color.Lerp(new Color(0.55f, 0.58f, 0.64f), new Color(0.78f, 0.80f, 0.86f), (i % 2) * 0.5f);
            coils.Add(rt);
            restPositions.Add(rt.anchoredPosition);
        }

        for (int r = 0; r < RibbonCoilIndex.Length; r++)
        {
            var ribbonObj = new GameObject("RibbonMark" + r);
            ribbonObj.transform.SetParent(coilsRoot.transform, false);
            var rt = ribbonObj.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(10f, 18f);
            int ci = RibbonCoilIndex[r];
            rt.anchoredPosition = (ci < restPositions.Count ? restPositions[ci] : Vector2.zero) + new Vector2(0f, 16f);
            var img = ribbonObj.AddComponent<Image>();
            img.sprite = ribbonSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.color = new Color(0.92f, 0.32f, 0.55f);
            ribbonObj.SetActive(false);
            ribbons.Add(rt);
        }
    }
}
