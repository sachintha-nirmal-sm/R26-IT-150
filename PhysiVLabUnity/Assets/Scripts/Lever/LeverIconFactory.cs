using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LeverIconFactory
{
    private static readonly Dictionary<LeverEquipmentType, Sprite> Cache = new Dictionary<LeverEquipmentType, Sprite>();

    private static readonly Dictionary<LeverEquipmentType, string[]> FileNames =
        new Dictionary<LeverEquipmentType, string[]>
        {
            { LeverEquipmentType.Book, new[] { "book.png" } },
            { LeverEquipmentType.NewtonSpringBalance, new[] { "newton_balance.png", "balance.jfif" } },
            { LeverEquipmentType.WoodenStrip, new[] { "wooden_strip.png", "plank.png" } },
            { LeverEquipmentType.SupportPivot, new[] { "support_pivot.png" } },
            { LeverEquipmentType.Ruler, new[] { "ruler.jpg" } },
            { LeverEquipmentType.Beaker, new[] { "beaker.jfif" } },
            { LeverEquipmentType.MeasuringCylinder, new[] { "measuring_cylinder.jpg" } },
            { LeverEquipmentType.Thermometer, new[] { "thermometer.jpg" } },
            { LeverEquipmentType.Stopwatch, new[] { "stopwatch.jfif" } },
            { LeverEquipmentType.Ammeter, new[] { "ammeter.jfif" } },
            { LeverEquipmentType.Voltmeter, new[] { "voltmeter.jfif" } },
            { LeverEquipmentType.Magnet, new[] { "magnet.jfif" } },
            { LeverEquipmentType.BunsenBurner, new[] { "bunsen_burner.png" } },
            { LeverEquipmentType.SandBag, new[] { "sand_bag.png" } },
            { LeverEquipmentType.Pulley, new[] { "sand_bag.png" } }
        };

    public static void ClearCache() => Cache.Clear();

    public static Sprite GetSprite(LeverEquipmentType type)
    {
        if (Cache.TryGetValue(type, out Sprite cached) && cached != null)
            return cached;

        Sprite sprite = LoadFromDisk(type) ?? CreateFallbackSprite(type);
        Cache[type] = sprite;
        return sprite;
    }

    public static Color GetColor(LeverEquipmentType type)
    {
        switch (type)
        {
            case LeverEquipmentType.Book: return new Color(0.45f, 0.55f, 0.85f);
            case LeverEquipmentType.NewtonSpringBalance: return new Color(0.75f, 0.75f, 0.78f);
            case LeverEquipmentType.WoodenStrip: return new Color(0.62f, 0.45f, 0.28f);
            case LeverEquipmentType.SupportPivot: return new Color(0.5f, 0.5f, 0.55f);
            case LeverEquipmentType.Ruler: return new Color(0.9f, 0.85f, 0.55f);
            case LeverEquipmentType.Beaker: return new Color(0.7f, 0.85f, 1f);
            case LeverEquipmentType.MeasuringCylinder: return new Color(0.55f, 0.8f, 1f);
            case LeverEquipmentType.Thermometer: return new Color(0.9f, 0.35f, 0.35f);
            case LeverEquipmentType.Stopwatch: return new Color(0.2f, 0.25f, 0.35f);
            case LeverEquipmentType.Ammeter: return new Color(0.35f, 0.55f, 0.45f);
            case LeverEquipmentType.Voltmeter: return new Color(0.45f, 0.4f, 0.65f);
            case LeverEquipmentType.Magnet: return new Color(0.85f, 0.25f, 0.25f);
            case LeverEquipmentType.BunsenBurner: return new Color(0.85f, 0.4f, 0.15f);
            case LeverEquipmentType.SandBag: return new Color(0.78f, 0.65f, 0.42f);
            case LeverEquipmentType.Pulley: return new Color(0.55f, 0.58f, 0.62f);
            default: return new Color(0.75f, 0.78f, 0.82f);
        }
    }

    private static Sprite LoadFromDisk(LeverEquipmentType type)
    {
        if (!FileNames.TryGetValue(type, out string[] names)) return null;

        foreach (string fileName in names)
        {
            string streaming = Path.Combine(Application.streamingAssetsPath, "EquipmentSprites", fileName);
            Sprite sprite = LoadFile(streaming);
            if (sprite != null) return sprite;

            string dataPath = Path.Combine(Application.dataPath, "Resources", "LeverEquipment", fileName);
            sprite = LoadFile(dataPath);
            if (sprite != null) return sprite;

            string resourcesPath = "LeverEquipment/" + Path.GetFileNameWithoutExtension(fileName);
            var tex = Resources.Load<Texture2D>(resourcesPath);
            if (tex != null)
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        return null;
    }

    private static Sprite LoadFile(string fullPath)
    {
        if (!File.Exists(fullPath)) return null;
        try
        {
            byte[] data = File.ReadAllBytes(fullPath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(data)) return null;
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
        catch
        {
            return null;
        }
    }

    private static Sprite CreateFallbackSprite(LeverEquipmentType type)
    {
        int w = 128, h = 128;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color fill = GetColor(type);
        var pixels = new Color[w * h];
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            bool border = x < 4 || y < 4 || x >= w - 4 || y >= h - 4;
            pixels[y * w + x] = border ? fill * 0.7f : fill;
        }
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }
}
