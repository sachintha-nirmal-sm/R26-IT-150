using System.Collections.Generic;
using UnityEngine;

public static class WavesIconFactory
{
    private static readonly Dictionary<WavesEquipmentType, Sprite> Cache = new Dictionary<WavesEquipmentType, Sprite>();
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

    public static Sprite GetSprite(WavesEquipmentType type)
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
            case "coil": sprite = DrawCoil(); break;
            case "hand": sprite = DrawHand(); break;
            case "arrow": sprite = DrawArrow(); break;
            case "table": sprite = DrawTable(); break;
            default: sprite = White(); break;
        }
        Extra[key] = sprite;
        return sprite;
    }

    public static Color GetColor(WavesEquipmentType type)
    {
        switch (type)
        {
            case WavesEquipmentType.Slinky: return new Color(0.55f, 0.58f, 0.66f);
            case WavesEquipmentType.Ribbons: return new Color(0.92f, 0.35f, 0.55f);
            case WavesEquipmentType.Table: return new Color(0.62f, 0.42f, 0.24f);
            case WavesEquipmentType.NewtonBalance: return new Color(0.78f, 0.22f, 0.18f);
            default: return new Color(0.82f, 0.86f, 0.92f);
        }
    }

    private static Sprite Draw(WavesEquipmentType type)
    {
        switch (type)
        {
            case WavesEquipmentType.Slinky: return DrawSlinky();
            case WavesEquipmentType.Ribbons: return DrawRibbon();
            case WavesEquipmentType.Table: return DrawTable();
            case WavesEquipmentType.NewtonBalance: return DrawBalance();
            case WavesEquipmentType.WoodenBlock: return DrawBlock();
            case WavesEquipmentType.MeterRuler: return DrawRuler();
            case WavesEquipmentType.LooseSpring: return DrawSpring();
            case WavesEquipmentType.Ammeter: return DrawMeter("A", new Color(0.15f, 0.45f, 0.78f));
            case WavesEquipmentType.Voltmeter: return DrawMeter("V", new Color(0.72f, 0.22f, 0.22f));
            case WavesEquipmentType.Bulb: return DrawBulb();
            case WavesEquipmentType.DryCell: return DrawCell();
            case WavesEquipmentType.Beaker: return DrawBeaker();
            case WavesEquipmentType.MeasuringCylinder: return DrawCylinder();
            case WavesEquipmentType.Thermometer: return DrawThermometer();
            case WavesEquipmentType.Magnet: return DrawMagnet();
            case WavesEquipmentType.Stopwatch: return DrawStopwatch();
            case WavesEquipmentType.MassHanger: return DrawHanger();
            case WavesEquipmentType.Compass: return DrawCompass();
            case WavesEquipmentType.BunsenBurner: return DrawBurner();
            case WavesEquipmentType.Pulley: return DrawPulley();
            case WavesEquipmentType.Trolley: return DrawTrolley();
            case WavesEquipmentType.Sandpaper: return DrawSandpaper();
            default: return DrawCard(GetColor(type));
        }
    }

    private static Sprite DrawSlinky()
    {
        int s = 256;
        var px = Clear(s, s);
        for (int i = 0; i < 12; i++)
        {
            int x = 28 + i * 18;
            int y = 118 + (int)(18 * Mathf.Sin(i * 0.7f));
            FillEllipse(px, s, s, x, y, 16, 28, new Color(0.58f, 0.62f, 0.70f));
            FillEllipse(px, s, s, x, y, 10, 20, Color.clear);
        }
        return Tex(px, s, s);
    }

    private static Sprite DrawCoil()
    {
        int s = 64;
        var px = Clear(s, s);
        FillEllipse(px, s, s, 32, 32, 22, 28, new Color(0.62f, 0.66f, 0.74f));
        FillEllipse(px, s, s, 32, 32, 12, 18, Color.clear);
        FillEllipse(px, s, s, 32, 32, 16, 22, new Color(0.48f, 0.52f, 0.60f));
        FillEllipse(px, s, s, 32, 32, 8, 12, Color.clear);
        return Tex(px, s, s);
    }

    private static Sprite DrawRibbon()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 108, 40, 40, 176, 8, new Color(0.92f, 0.32f, 0.55f));
        FillCircle(px, s, s, 128, 48, 22, new Color(0.85f, 0.18f, 0.42f));
        FillRect(px, s, s, 118, 200, 20, 28, new Color(0.92f, 0.45f, 0.62f));
        return Tex(px, s, s);
    }

    private static Sprite DrawTable()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 24, 90, 208, 28, new Color(0.58f, 0.40f, 0.22f));
        FillRect(px, s, s, 40, 118, 18, 80, new Color(0.46f, 0.32f, 0.18f));
        FillRect(px, s, s, 198, 118, 18, 80, new Color(0.46f, 0.32f, 0.18f));
        FillRect(px, s, s, 28, 84, 200, 8, new Color(0.72f, 0.52f, 0.30f));
        return Tex(px, s, s);
    }

    private static Sprite DrawHand()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 88, 70, 80, 110, 20, new Color(0.93f, 0.78f, 0.62f));
        FillCircle(px, s, s, 128, 70, 28, new Color(0.93f, 0.78f, 0.62f));
        FillRect(px, s, s, 108, 170, 40, 50, new Color(0.35f, 0.42f, 0.70f));
        return Tex(px, s, s);
    }

    private static Sprite DrawArrow()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 24, 118, 160, 20, new Color(0.12f, 0.16f, 0.22f));
        for (int i = 0; i < 28; i++)
            FillRect(px, s, s, 170 + i, 128 - (28 - i), 4, (28 - i) * 2, new Color(0.12f, 0.16f, 0.22f));
        return Tex(px, s, s);
    }

    private static Sprite DrawRuler()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 16, 108, 224, 40, 6, new Color(0.92f, 0.86f, 0.62f));
        for (int i = 0; i <= 10; i++)
        {
            int x = 24 + i * 20;
            FillRect(px, s, s, x, 108, 2, i % 5 == 0 ? 18 : 10, new Color(0.22f, 0.18f, 0.12f));
        }
        return Tex(px, s, s);
    }

    private static Sprite DrawBalance()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 88, 36, 44, 170, 8, new Color(0.78f, 0.22f, 0.16f));
        FillRect(px, s, s, 148, 68, 12, 90, new Color(0.95f, 0.85f, 0.35f));
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
        if (rx < 1) rx = 1;
        if (ry < 1) ry = 1;
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
