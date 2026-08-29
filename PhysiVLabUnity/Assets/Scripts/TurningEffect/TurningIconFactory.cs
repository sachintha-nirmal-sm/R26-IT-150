using System.Collections.Generic;
using UnityEngine;

public static class TurningIconFactory
{
    private static readonly Dictionary<TurningEquipmentType, Sprite> Cache = new Dictionary<TurningEquipmentType, Sprite>();
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

    public static Sprite GetSprite(TurningEquipmentType type)
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
            case "stick": sprite = DrawStick(); break;
            case "drill": sprite = DrawDrill(); break;
            case "washer": sprite = DrawWasher(); break;
            case "screw": sprite = DrawScrew(); break;
            case "wire": sprite = DrawWire(); break;
            case "balance": sprite = DrawBalance(); break;
            case "table": sprite = DrawTable(); break;
            default: sprite = White(); break;
        }
        Extra[key] = sprite;
        return sprite;
    }

    public static Color GetColor(TurningEquipmentType type)
    {
        switch (type)
        {
            case TurningEquipmentType.WoodenStick: return new Color(0.72f, 0.52f, 0.22f);
            case TurningEquipmentType.RubberWashers: return new Color(0.28f, 0.28f, 0.30f);
            case TurningEquipmentType.Drill: return new Color(0.35f, 0.42f, 0.50f);
            case TurningEquipmentType.NewtonBalance: return new Color(0.78f, 0.22f, 0.18f);
            case TurningEquipmentType.LabTable: return new Color(0.55f, 0.42f, 0.28f);
            case TurningEquipmentType.ScrewNail: return new Color(0.62f, 0.64f, 0.68f);
            case TurningEquipmentType.Wire: return new Color(0.85f, 0.72f, 0.28f);
            default: return new Color(0.82f, 0.86f, 0.92f);
        }
    }

    private static Sprite Draw(TurningEquipmentType type)
    {
        switch (type)
        {
            case TurningEquipmentType.WoodenStick: return DrawStick();
            case TurningEquipmentType.RubberWashers: return DrawWasher();
            case TurningEquipmentType.Drill: return DrawDrill();
            case TurningEquipmentType.NewtonBalance: return DrawBalance();
            case TurningEquipmentType.LabTable: return DrawTable();
            case TurningEquipmentType.ScrewNail: return DrawScrew();
            case TurningEquipmentType.Wire: return DrawWire();
            case TurningEquipmentType.Trolley: return DrawTrolley();
            case TurningEquipmentType.Pulley: return DrawPulley();
            case TurningEquipmentType.WoodenBlock: return DrawBlock();
            case TurningEquipmentType.Sandpaper: return DrawSandpaper();
            case TurningEquipmentType.Spring: return DrawSpring();
            case TurningEquipmentType.Ammeter: return DrawMeter("A", new Color(0.15f, 0.45f, 0.78f));
            case TurningEquipmentType.Voltmeter: return DrawMeter("V", new Color(0.72f, 0.22f, 0.22f));
            case TurningEquipmentType.Bulb: return DrawBulb();
            case TurningEquipmentType.DryCell: return DrawCell();
            case TurningEquipmentType.Beaker: return DrawBeaker();
            case TurningEquipmentType.MeasuringCylinder: return DrawCylinder();
            case TurningEquipmentType.Thermometer: return DrawThermometer();
            case TurningEquipmentType.Magnet: return DrawMagnet();
            case TurningEquipmentType.Stopwatch: return DrawStopwatch();
            case TurningEquipmentType.MassHanger: return DrawHanger();
            case TurningEquipmentType.Compass: return DrawCompass();
            case TurningEquipmentType.BunsenBurner: return DrawBurner();
            default: return DrawCard(GetColor(type));
        }
    }

    private static Sprite DrawStick()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 18, 108, 220, 40, 6, new Color(0.78f, 0.58f, 0.28f));
        FillRect(px, s, s, 18, 124, 220, 4, new Color(0.62f, 0.42f, 0.16f));
        for (int i = 0; i < 5; i++)
        {
            int x = 36 + i * 44;
            FillCircle(px, s, s, x, 128, 6, new Color(0.18f, 0.18f, 0.20f));
            FillRect(px, s, s, x - 1, 98, 2, 12, new Color(0.25f, 0.22f, 0.18f));
        }
        return Tex(px, s, s);
    }

    private static Sprite DrawDrill()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 70, 70, 70, 110, 12, new Color(0.28f, 0.38f, 0.48f));
        FillRect(px, s, s, 140, 108, 70, 18, new Color(0.55f, 0.58f, 0.62f));
        FillRect(px, s, s, 200, 112, 28, 10, new Color(0.70f, 0.72f, 0.75f));
        FillCircle(px, s, s, 105, 92, 14, new Color(0.85f, 0.55f, 0.15f));
        return Tex(px, s, s);
    }

    private static Sprite DrawWasher()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 58, new Color(0.22f, 0.22f, 0.24f));
        FillCircle(px, s, s, 128, 128, 22, Color.clear);
        FillCircle(px, s, s, 128, 128, 50, new Color(0.32f, 0.32f, 0.34f));
        FillCircle(px, s, s, 128, 128, 22, Color.clear);
        return Tex(px, s, s);
    }

    private static Sprite DrawScrew()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 58, 28, new Color(0.62f, 0.64f, 0.68f));
        FillRect(px, s, s, 118, 80, 20, 140, new Color(0.70f, 0.72f, 0.76f));
        for (int i = 0; i < 8; i++)
            FillRect(px, s, s, 114, 100 + i * 14, 28, 4, new Color(0.48f, 0.50f, 0.54f));
        FillRect(px, s, s, 122, 48, 12, 16, new Color(0.35f, 0.36f, 0.38f));
        return Tex(px, s, s);
    }

    private static Sprite DrawWire()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 78, 128, 36, new Color(0.85f, 0.72f, 0.28f));
        FillCircle(px, s, s, 78, 128, 20, Color.clear);
        FillCircle(px, s, s, 168, 128, 36, new Color(0.78f, 0.62f, 0.22f));
        FillCircle(px, s, s, 168, 128, 20, Color.clear);
        return Tex(px, s, s);
    }

    private static Sprite DrawBalance()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 88, 36, 44, 170, 8, new Color(0.78f, 0.22f, 0.16f));
        FillRect(px, s, s, 132, 48, 70, 16, new Color(0.88f, 0.40f, 0.28f));
        FillRect(px, s, s, 148, 68, 12, 90, new Color(0.95f, 0.85f, 0.35f));
        FillCircle(px, s, s, 110, 48, 12, new Color(0.85f, 0.88f, 0.92f));
        FillCircle(px, s, s, 110, 200, 10, new Color(0.25f, 0.28f, 0.32f));
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

    private static Sprite DrawTrolley()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 40, 90, 176, 70, 8, new Color(0.22f, 0.48f, 0.82f));
        FillCircle(px, s, s, 70, 172, 18, new Color(0.90f, 0.48f, 0.12f));
        FillCircle(px, s, s, 186, 172, 18, new Color(0.90f, 0.48f, 0.12f));
        return Tex(px, s, s);
    }

    private static Sprite DrawPulley()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 118, 58, new Color(0.18f, 0.62f, 0.38f));
        FillCircle(px, s, s, 128, 118, 22, new Color(0.88f, 0.92f, 0.90f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBlock()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 48, 88, 160, 90, 8, new Color(0.72f, 0.48f, 0.22f));
        return Tex(px, s, s);
    }

    private static Sprite DrawSandpaper()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 28, 70, 200, 110, 8, new Color(0.78f, 0.62f, 0.38f));
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
        return Tex(px, s, s);
    }

    private static Sprite DrawMagnet()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 60, 80, 50, 110, new Color(0.8f, 0.18f, 0.18f));
        FillRect(px, s, s, 146, 80, 50, 110, new Color(0.18f, 0.35f, 0.8f));
        return Tex(px, s, s);
    }

    private static Sprite DrawStopwatch()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 140, 70, new Color(0.70f, 0.72f, 0.78f));
        FillCircle(px, s, s, 128, 140, 52, new Color(0.95f, 0.96f, 0.98f));
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
        return Tex(px, s, s);
    }

    private static Sprite DrawCompass()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 78, new Color(0.9f, 0.93f, 0.85f));
        FillRect(px, s, s, 122, 70, 12, 50, new Color(0.8f, 0.15f, 0.15f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBurner()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 88, 150, 80, 50, new Color(0.35f, 0.38f, 0.42f));
        FillCircle(px, s, s, 128, 70, 22, new Color(0.3f, 0.55f, 0.95f));
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
