using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Apparatus icons built from UI shapes (no imported art).
/// Uses a 2×2 white sprite so Unity 6 uGUI always tints and displays them.
/// </summary>
public static class UpthrustIconFactory
{
    static Sprite whiteSprite;
    static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    public static void ClearCache() => Cache.Clear();

    public static Sprite WhiteSprite
    {
        get
        {
            if (whiteSprite != null) return whiteSprite;

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply(false, false);
            whiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f, 0, SpriteMeshType.FullRect);
            whiteSprite.hideFlags = HideFlags.HideAndDontSave;
            return whiteSprite;
        }
    }

    public static Sprite GetSprite(string id)
    {
        BuildIcon(null, id);
        return WhiteSprite;
    }

    /// <summary>Builds a visible icon into parent. Parent should be an empty or Image rect.</summary>
    public static void BuildIcon(RectTransform parent, string id)
    {
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i).gameObject;
            if (Application.isPlaying) Object.Destroy(child);
            else Object.DestroyImmediate(child);
        }

        var host = parent.GetComponent<Image>();
        if (host != null)
        {
            host.sprite = WhiteSprite;
            host.color = new Color(1f, 1f, 1f, 0.04f);
            host.raycastTarget = false;
            host.preserveAspect = false;
        }

        switch (id)
        {
            case "cube": IconCube(parent); break;
            case "spring": IconSpring(parent); break;
            case "eureka": IconEureka(parent); break;
            case "beaker": IconBeaker(parent); break;
            case "stand": IconStand(parent); break;
            case "thermometer": IconThermometer(parent); break;
            case "voltmeter": IconVoltmeter(parent); break;
            case "lens": IconLens(parent); break;
            case "block": IconBlock(parent); break;
            default: Box(parent, "Mark", new Color(0.7f, 0.7f, 0.7f), new Vector2(0.5f, 0.5f), new Vector2(70, 70)); break;
        }
    }

    static void IconCube(RectTransform p)
    {
        Box(p, "Body", new Color(0.70f, 0.74f, 0.80f), new Vector2(0.50f, 0.46f), new Vector2(78, 78));
        Box(p, "Top", new Color(0.82f, 0.85f, 0.90f), new Vector2(0.50f, 0.78f), new Vector2(78, 14));
        Box(p, "Line", new Color(0.12f, 0.13f, 0.16f), new Vector2(0.50f, 0.46f), new Vector2(78, 8));
    }

    static void IconSpring(RectTransform p)
    {
        Box(p, "Hook", new Color(0.35f, 0.38f, 0.42f), new Vector2(0.50f, 0.90f), new Vector2(22, 12));
        Box(p, "Body", new Color(0.95f, 0.80f, 0.22f), new Vector2(0.50f, 0.68f), new Vector2(46, 36));
        for (int i = 0; i < 4; i++)
            Box(p, "Coil" + i, new Color(0.78f, 0.80f, 0.86f), new Vector2(0.50f, 0.42f - i * 0.10f), new Vector2(34, 8));
        Box(p, "Hook2", new Color(0.45f, 0.48f, 0.52f), new Vector2(0.50f, 0.08f), new Vector2(10, 14));
    }

    static void IconEureka(RectTransform p)
    {
        Box(p, "Can", new Color(0.62f, 0.80f, 0.90f), new Vector2(0.42f, 0.48f), new Vector2(58, 78));
        Box(p, "Water", new Color(0.20f, 0.50f, 0.88f), new Vector2(0.42f, 0.38f), new Vector2(46, 48));
        Box(p, "Spout", new Color(0.55f, 0.72f, 0.80f), new Vector2(0.82f, 0.62f), new Vector2(28, 12));
        Box(p, "Lip", new Color(0.55f, 0.72f, 0.80f), new Vector2(0.88f, 0.48f), new Vector2(10, 28));
    }

    static void IconBeaker(RectTransform p)
    {
        Box(p, "Glass", new Color(0.78f, 0.92f, 0.96f), new Vector2(0.50f, 0.44f), new Vector2(56, 72));
        Box(p, "Water", new Color(0.22f, 0.55f, 0.90f), new Vector2(0.50f, 0.32f), new Vector2(44, 36));
        Box(p, "Rim", new Color(0.85f, 0.95f, 0.98f), new Vector2(0.50f, 0.82f), new Vector2(66, 10));
    }

    static void IconStand(RectTransform p)
    {
        Box(p, "Base", new Color(0.38f, 0.40f, 0.46f), new Vector2(0.50f, 0.12f), new Vector2(80, 12));
        Box(p, "Pole", new Color(0.42f, 0.44f, 0.50f), new Vector2(0.22f, 0.52f), new Vector2(12, 72));
        Box(p, "Arm", new Color(0.42f, 0.44f, 0.50f), new Vector2(0.52f, 0.84f), new Vector2(52, 10));
        Box(p, "Clamp", new Color(0.55f, 0.58f, 0.64f), new Vector2(0.78f, 0.72f), new Vector2(10, 24));
    }

    static void IconThermometer(RectTransform p)
    {
        Box(p, "Tube", new Color(0.90f, 0.94f, 0.98f), new Vector2(0.50f, 0.56f), new Vector2(16, 70));
        Box(p, "Mercury", new Color(0.90f, 0.18f, 0.18f), new Vector2(0.50f, 0.48f), new Vector2(8, 44));
        Box(p, "Bulb", new Color(0.90f, 0.18f, 0.18f), new Vector2(0.50f, 0.16f), new Vector2(26, 26));
    }

    static void IconVoltmeter(RectTransform p)
    {
        Box(p, "Ring", new Color(0.18f, 0.48f, 0.28f), new Vector2(0.50f, 0.50f), new Vector2(78, 78));
        Box(p, "Face", new Color(0.94f, 0.96f, 0.88f), new Vector2(0.50f, 0.50f), new Vector2(54, 54));
        Box(p, "Needle", new Color(0.12f, 0.12f, 0.12f), new Vector2(0.58f, 0.58f), new Vector2(8, 28));
    }

    static void IconLens(RectTransform p)
    {
        Box(p, "Glass", new Color(0.45f, 0.78f, 0.98f), new Vector2(0.50f, 0.50f), new Vector2(72, 72));
        Box(p, "Shine", new Color(0.80f, 0.94f, 1f), new Vector2(0.42f, 0.60f), new Vector2(28, 28));
    }

    static void IconBlock(RectTransform p)
    {
        Box(p, "Wood", new Color(0.70f, 0.46f, 0.22f), new Vector2(0.50f, 0.48f), new Vector2(80, 48));
        Box(p, "Grain", new Color(0.50f, 0.30f, 0.14f), new Vector2(0.50f, 0.48f), new Vector2(80, 6));
    }

    static Image Box(RectTransform parent, string name, Color color, Vector2 anchor, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;

        var img = go.GetComponent<Image>();
        img.sprite = WhiteSprite;
        img.type = Image.Type.Simple;
        img.color = color;
        img.raycastTarget = false;
        img.preserveAspect = false;
        return img;
    }
}
