using System.Collections.Generic;
using UnityEngine;

public static class FrictionIconFactory
{
    private static readonly Dictionary<FrictionEquipmentType, Sprite> Cache = new Dictionary<FrictionEquipmentType, Sprite>();
    private static readonly Dictionary<string, Sprite> Extra = new Dictionary<string, Sprite>();
    private static Sprite whiteSprite;

    public static void ClearCache()
    {
        Cache.Clear();
        Extra.Clear();
        whiteSprite = null;
    }

    public static Sprite White()
    {
        if (whiteSprite != null) return whiteSprite;
        var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        var px = new Color[64];
        for (int i = 0; i < 64; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        whiteSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 100f);
        return whiteSprite;
    }

    public static Sprite GetSprite(FrictionEquipmentType type)
    {
        if (Cache.TryGetValue(type, out Sprite cached) && cached != null) return cached;
        Sprite sprite = Draw(type);
        Cache[type] = sprite;
        return sprite;
    }

    public static Sprite GetNamed(string key)
    {
        if (Extra.TryGetValue(key, out Sprite cached) && cached != null) return cached;
        Sprite sprite;
        switch (key)
        {
            case "block": sprite = DrawBlock(); break;
            case "sandpaper": sprite = DrawSandpaper(); break;
            case "balance": sprite = DrawBalance(); break;
            case "table": sprite = DrawTable(); break;
            case "ruler": sprite = DrawRuler(); break;
            case "arrow": sprite = DrawArrow(); break;
            case "correct": sprite = DrawBadge(new Color(0.12f, 0.62f, 0.35f)); break;
            case "wrong": sprite = DrawBadge(new Color(0.78f, 0.18f, 0.18f)); break;
            case "lab": sprite = DrawLab(); break;
            default: sprite = White(); break;
        }
        Extra[key] = sprite;
        return sprite;
    }

    public static Color GetColor(FrictionEquipmentType type)
    {
        switch (type)
        {
            case FrictionEquipmentType.WoodenBlock: return new Color(0.72f, 0.48f, 0.22f);
            case FrictionEquipmentType.NewtonBalance: return new Color(0.55f, 0.62f, 0.72f);
            case FrictionEquipmentType.Sandpaper: return new Color(0.78f, 0.62f, 0.38f);
            case FrictionEquipmentType.LabTable: return new Color(0.55f, 0.42f, 0.28f);
            case FrictionEquipmentType.MeasuringRuler: return new Color(0.95f, 0.85f, 0.45f);
            case FrictionEquipmentType.RecordingSheet: return new Color(0.95f, 0.93f, 0.82f);
            case FrictionEquipmentType.ForceDisplay: return new Color(0.20f, 0.55f, 0.85f);
            default: return new Color(0.82f, 0.86f, 0.92f);
        }
    }

    private static Sprite Draw(FrictionEquipmentType type)
    {
        switch (type)
        {
            case FrictionEquipmentType.WoodenBlock: return DrawBlock();
            case FrictionEquipmentType.NewtonBalance: return DrawBalance();
            case FrictionEquipmentType.Sandpaper: return DrawSandpaper();
            case FrictionEquipmentType.LabTable: return DrawTable();
            case FrictionEquipmentType.MeasuringRuler: return DrawRuler();
            case FrictionEquipmentType.RecordingSheet: return DrawSheet();
            case FrictionEquipmentType.ForceDisplay: return DrawMeter("N", new Color(0.15f, 0.45f, 0.78f));
            case FrictionEquipmentType.Ammeter: return DrawMeter("A", new Color(0.15f, 0.45f, 0.78f));
            case FrictionEquipmentType.Voltmeter: return DrawMeter("V", new Color(0.72f, 0.22f, 0.22f));
            case FrictionEquipmentType.Bulb: return DrawBulb();
            case FrictionEquipmentType.DryCell: return DrawCell();
            case FrictionEquipmentType.Beaker: return DrawBeaker();
            case FrictionEquipmentType.MeasuringCylinder: return DrawCylinder();
            case FrictionEquipmentType.Thermometer: return DrawThermometer();
            case FrictionEquipmentType.Magnet: return DrawMagnet();
            case FrictionEquipmentType.Stopwatch: return DrawStopwatch();
            case FrictionEquipmentType.Pulley: return DrawPulley();
            case FrictionEquipmentType.Spring: return DrawSpring();
            case FrictionEquipmentType.MassHanger: return DrawHanger();
            case FrictionEquipmentType.Compass: return DrawCompass();
            case FrictionEquipmentType.BunsenBurner: return DrawBurner();
            default: return DrawCard(GetColor(type));
        }
    }

    private static Sprite DrawBlock()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 48, 88, 160, 90, 8, new Color(0.72f, 0.48f, 0.22f));
        FillRect(px, s, s, 56, 96, 144, 12, new Color(0.82f, 0.60f, 0.32f));
        FillRect(px, s, s, 48, 130, 160, 6, new Color(0.58f, 0.36f, 0.16f));
        FillRect(px, s, s, 48, 150, 160, 4, new Color(0.58f, 0.36f, 0.16f));
        return Tex(px, s, s);
    }

    private static Sprite DrawSandpaper()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 28, 70, 200, 110, 8, new Color(0.78f, 0.62f, 0.38f));
        for (int y = 80; y < 170; y += 8)
            for (int x = 40; x < 220; x += 8)
                FillCircle(px, s, s, x, y, 2, new Color(0.45f, 0.32f, 0.18f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBalance()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 70, 40, 40, 170, 8, new Color(0.55f, 0.60f, 0.68f));
        FillRect(px, s, s, 110, 48, 90, 18, new Color(0.72f, 0.75f, 0.80f));
        FillRect(px, s, s, 122, 70, 14, 100, new Color(0.85f, 0.55f, 0.18f));
        FillCircle(px, s, s, 129, 178, 10, new Color(0.25f, 0.28f, 0.32f));
        FillCircle(px, s, s, 90, 48, 12, new Color(0.85f, 0.88f, 0.92f));
        return Tex(px, s, s);
    }

    private static Sprite DrawTable()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 24, 90, 208, 28, new Color(0.58f, 0.40f, 0.22f));
        FillRect(px, s, s, 40, 118, 18, 80, new Color(0.46f, 0.32f, 0.18f));
        FillRect(px, s, s, 198, 118, 18, 80, new Color(0.46f, 0.32f, 0.18f));
        return Tex(px, s, s);
    }

    private static Sprite DrawRuler()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 20, 108, 216, 40, 6, new Color(0.95f, 0.85f, 0.42f));
        for (int i = 0; i < 12; i++)
            FillRect(px, s, s, 32 + i * 16, 112, 3, i % 5 == 0 ? 28 : 16, new Color(0.25f, 0.22f, 0.16f));
        return Tex(px, s, s);
    }

    private static Sprite DrawSheet()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 58, 36, 140, 184, 8, new Color(0.97f, 0.95f, 0.86f));
        for (int i = 0; i < 7; i++)
            FillRect(px, s, s, 74, 60 + i * 20, 108, 4, new Color(0.72f, 0.78f, 0.86f));
        return Tex(px, s, s);
    }

    private static Sprite DrawArrow()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 30, 118, 150, 20, new Color(0.18f, 0.48f, 0.82f));
        for (int i = 0; i < 40; i++)
            FillRect(px, s, s, 170 + i, 88 + i / 2, 8, 80 - i, new Color(0.18f, 0.48f, 0.82f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBadge(Color color)
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 70, color);
        return Tex(px, s, s);
    }

    private static Sprite DrawLab()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 0, 0, 256, 256, new Color(0.90f, 0.93f, 0.97f));
        FillRect(px, s, s, 0, 180, 256, 76, new Color(0.55f, 0.42f, 0.28f));
        return Tex(px, s, s);
    }

    private static Sprite DrawMeter(string letter, Color ring)
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 78, ring);
        FillCircle(px, s, s, 128, 128, 58, new Color(0.95f, 0.96f, 0.98f));
        FillRect(px, s, s, 118, 70, 20, 70, ring);
        return Tex(px, s, s);
    }

    private static Sprite DrawBulb()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 100, 48, new Color(1f, 0.92f, 0.45f));
        FillRect(px, s, s, 110, 148, 36, 50, new Color(0.55f, 0.55f, 0.58f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCell()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 70, 80, 116, 96, 10, new Color(0.25f, 0.55f, 0.35f));
        FillRect(px, s, s, 186, 108, 18, 40, new Color(0.75f, 0.75f, 0.78f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBeaker()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 78, 70, 100, 130, new Color(0.75f, 0.88f, 0.95f));
        FillRect(px, s, s, 70, 60, 116, 18, new Color(0.85f, 0.9f, 0.95f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCylinder()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 100, 40, 56, 176, new Color(0.75f, 0.88f, 0.95f));
        return Tex(px, s, s);
    }

    private static Sprite DrawThermometer()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 118, 40, 20, 150, new Color(0.85f, 0.9f, 0.95f));
        FillCircle(px, s, s, 128, 200, 22, new Color(0.82f, 0.18f, 0.18f));
        FillRect(px, s, s, 122, 90, 12, 110, new Color(0.82f, 0.18f, 0.18f));
        return Tex(px, s, s);
    }

    private static Sprite DrawMagnet()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 60, 80, 50, 110, new Color(0.8f, 0.18f, 0.18f));
        FillRect(px, s, s, 146, 80, 50, 110, new Color(0.18f, 0.35f, 0.8f));
        FillRect(px, s, s, 60, 80, 136, 36, new Color(0.55f, 0.55f, 0.58f));
        return Tex(px, s, s);
    }

    private static Sprite DrawStopwatch()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 140, 70, new Color(0.70f, 0.72f, 0.78f));
        FillCircle(px, s, s, 128, 140, 52, new Color(0.95f, 0.96f, 0.98f));
        FillRect(px, s, s, 122, 70, 12, 28, new Color(0.45f, 0.48f, 0.52f));
        FillRect(px, s, s, 124, 100, 8, 50, new Color(0.18f, 0.2f, 0.24f));
        return Tex(px, s, s);
    }

    private static Sprite DrawPulley()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 110, 60, new Color(0.6f, 0.62f, 0.68f));
        FillCircle(px, s, s, 128, 110, 22, new Color(0.85f, 0.86f, 0.9f));
        FillRect(px, s, s, 124, 168, 8, 50, new Color(0.4f, 0.4f, 0.42f));
        return Tex(px, s, s);
    }

    private static Sprite DrawSpring()
    {
        int s = 256;
        var px = Clear(s, s);
        for (int i = 0; i < 8; i++)
            FillEllipse(px, s, s, 128, 50 + i * 20, 40, 10, new Color(0.55f, 0.58f, 0.62f));
        return Tex(px, s, s);
    }

    private static Sprite DrawHanger()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 118, 40, 20, 80, new Color(0.45f, 0.48f, 0.52f));
        FillRect(px, s, s, 70, 120, 116, 18, new Color(0.45f, 0.48f, 0.52f));
        FillRect(px, s, s, 88, 140, 80, 50, new Color(0.35f, 0.38f, 0.42f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCompass()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 78, new Color(0.9f, 0.93f, 0.85f));
        FillRect(px, s, s, 122, 70, 12, 50, new Color(0.8f, 0.15f, 0.15f));
        FillRect(px, s, s, 122, 136, 12, 50, new Color(0.15f, 0.15f, 0.18f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBurner()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 88, 150, 80, 50, new Color(0.35f, 0.38f, 0.42f));
        FillRect(px, s, s, 118, 90, 20, 70, new Color(0.45f, 0.48f, 0.52f));
        FillCircle(px, s, s, 128, 70, 22, new Color(0.3f, 0.55f, 0.95f));
        FillCircle(px, s, s, 128, 58, 12, new Color(1f, 0.75f, 0.2f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCard(Color color)
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 28, 28, 200, 200, 18, color);
        return Tex(px, s, s);
    }

    private static Color[] Clear(int w, int h)
    {
        var px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        return px;
    }

    private static Sprite Tex(Color[] px, int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels(px);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    private static void FillRect(Color[] px, int w, int h, int x, int y, int rw, int rh, Color c)
    {
        int x2 = Mathf.Min(w, x + rw);
        int y2 = Mathf.Min(h, y + rh);
        x = Mathf.Max(0, x); y = Mathf.Max(0, y);
        for (int yy = y; yy < y2; yy++)
            for (int xx = x; xx < x2; xx++)
                px[yy * w + xx] = c;
    }

    private static void FillCircle(Color[] px, int w, int h, int cx, int cy, int r, Color c)
    {
        int r2 = r * r;
        for (int y = cy - r; y <= cy + r; y++)
        {
            if (y < 0 || y >= h) continue;
            for (int x = cx - r; x <= cx + r; x++)
            {
                if (x < 0 || x >= w) continue;
                int dx = x - cx, dy = y - cy;
                if (dx * dx + dy * dy <= r2) px[y * w + x] = c;
            }
        }
    }

    private static void FillEllipse(Color[] px, int w, int h, int cx, int cy, int rx, int ry, Color c)
    {
        for (int y = cy - ry; y <= cy + ry; y++)
        {
            if (y < 0 || y >= h) continue;
            for (int x = cx - rx; x <= cx + rx; x++)
            {
                if (x < 0 || x >= w) continue;
                float nx = (x - cx) / (float)rx;
                float ny = (y - cy) / (float)ry;
                if (nx * nx + ny * ny <= 1f) px[y * w + x] = c;
            }
        }
    }

    private static void FillRoundRect(Color[] px, int w, int h, int x, int y, int rw, int rh, int rad, Color c)
    {
        FillRect(px, w, h, x + rad, y, rw - 2 * rad, rh, c);
        FillRect(px, w, h, x, y + rad, rw, rh - 2 * rad, c);
        FillCircle(px, w, h, x + rad, y + rad, rad, c);
        FillCircle(px, w, h, x + rw - rad, y + rad, rad, c);
        FillCircle(px, w, h, x + rad, y + rh - rad, rad, c);
        FillCircle(px, w, h, x + rw - rad, y + rh - rad, rad, c);
    }
}
