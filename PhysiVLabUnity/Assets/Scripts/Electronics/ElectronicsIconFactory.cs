using System.Collections.Generic;
using UnityEngine;

public static class ElectronicsIconFactory
{
    private static readonly Dictionary<ElectronicsEquipmentType, Sprite> Cache = new Dictionary<ElectronicsEquipmentType, Sprite>();
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

    public static Sprite GetSprite(ElectronicsEquipmentType type)
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
            case "diode": sprite = DrawDiode(false); break;
            case "diode-rev": sprite = DrawDiode(true); break;
            case "battery": sprite = DrawBattery(false); break;
            case "battery-rev": sprite = DrawBattery(true); break;
            case "bulb-on": sprite = DrawBulb(true); break;
            case "bulb-off": sprite = DrawBulb(false); break;
            case "switch-on": sprite = DrawSwitch(true); break;
            case "switch-off": sprite = DrawSwitch(false); break;
            case "rays": sprite = DrawRays(); break;
            case "correct": sprite = DrawTick(); break;
            case "wrong": sprite = DrawCross(); break;
            case "arrow-fwd": sprite = DrawArrow(true); break;
            case "arrow-rev": sprite = DrawArrow(false); break;
            default: sprite = White(); break;
        }
        Extra[key] = sprite;
        return sprite;
    }

    public static Color GetColor(ElectronicsEquipmentType type)
    {
        switch (type)
        {
            case ElectronicsEquipmentType.Diode: return new Color(0.22f, 0.55f, 0.72f);
            case ElectronicsEquipmentType.Bulb: return new Color(1f, 0.88f, 0.35f);
            case ElectronicsEquipmentType.DryCells: return new Color(0.20f, 0.45f, 0.72f);
            case ElectronicsEquipmentType.Switch: return new Color(0.85f, 0.55f, 0.18f);
            case ElectronicsEquipmentType.Breadboard: return new Color(0.92f, 0.78f, 0.42f);
            case ElectronicsEquipmentType.Wires: return new Color(0.90f, 0.72f, 0.18f);
            case ElectronicsEquipmentType.Ammeter: return new Color(0.15f, 0.48f, 0.78f);
            case ElectronicsEquipmentType.Voltmeter: return new Color(0.78f, 0.22f, 0.22f);
            default: return new Color(0.82f, 0.84f, 0.88f);
        }
    }

    private static Sprite Draw(ElectronicsEquipmentType type)
    {
        switch (type)
        {
            case ElectronicsEquipmentType.Diode: return DrawDiode(false);
            case ElectronicsEquipmentType.Bulb: return DrawBulb(false);
            case ElectronicsEquipmentType.DryCells: return DrawBattery(false);
            case ElectronicsEquipmentType.Switch: return DrawSwitch(false);
            case ElectronicsEquipmentType.Breadboard: return DrawBoard();
            case ElectronicsEquipmentType.Wires: return DrawWire();
            case ElectronicsEquipmentType.Ammeter: return DrawMeter(new Color(0.12f, 0.42f, 0.78f));
            case ElectronicsEquipmentType.Voltmeter: return DrawMeter(new Color(0.78f, 0.18f, 0.18f));
            case ElectronicsEquipmentType.NewtonBalance: return DrawBalance();
            case ElectronicsEquipmentType.Spring: return DrawSpring();
            case ElectronicsEquipmentType.WoodenBlock: return DrawBlock();
            case ElectronicsEquipmentType.Thermometer: return DrawThermometer();
            case ElectronicsEquipmentType.MeasuringCylinder: return DrawCylinder();
            case ElectronicsEquipmentType.Beaker: return DrawBeaker();
            case ElectronicsEquipmentType.Magnet: return DrawMagnet();
            case ElectronicsEquipmentType.Ruler: return DrawRuler();
            case ElectronicsEquipmentType.Stopwatch: return DrawTimer();
            case ElectronicsEquipmentType.Pulley: return DrawPulley();
            default: return DrawCard(GetColor(type));
        }
    }

    private static Sprite DrawDiode(bool reversed)
    {
        int s = 256;
        var px = Clear(s, s);
        Color body = new Color(0.18f, 0.22f, 0.28f);
        Color band = new Color(0.88f, 0.78f, 0.18f);
        Color lead = new Color(0.70f, 0.72f, 0.76f);
        if (!reversed)
        {
            FillRect(px, s, s, 20, 118, 48, 20, lead);
            FillRoundRect(px, s, s, 68, 88, 120, 80, 12, body);
            FillRect(px, s, s, 156, 88, 22, 80, band);
            FillRect(px, s, s, 188, 118, 48, 20, lead);
            FillTriangle(px, s, s, 84, 98, 148, 128, 84, 158, new Color(0.35f, 0.78f, 0.92f));
            FillRect(px, s, s, 148, 98, 8, 60, new Color(0.95f, 0.95f, 0.98f));
        }
        else
        {
            FillRect(px, s, s, 188, 118, 48, 20, lead);
            FillRoundRect(px, s, s, 68, 88, 120, 80, 12, body);
            FillRect(px, s, s, 78, 88, 22, 80, band);
            FillRect(px, s, s, 20, 118, 48, 20, lead);
            FillTriangle(px, s, s, 172, 98, 108, 128, 172, 158, new Color(0.92f, 0.45f, 0.28f));
            FillRect(px, s, s, 100, 98, 8, 60, new Color(0.95f, 0.95f, 0.98f));
        }
        return Tex(px, s, s);
    }

    private static Sprite DrawBattery(bool reversed)
    {
        int s = 256;
        var px = Clear(s, s);
        Color cell = new Color(0.18f, 0.42f, 0.72f);
        Color cap = new Color(0.55f, 0.58f, 0.62f);
        Color pos = new Color(0.85f, 0.18f, 0.16f);
        Color neg = new Color(0.12f, 0.12f, 0.14f);
        FillRoundRect(px, s, s, 28, 70, 90, 116, 10, cell);
        FillRoundRect(px, s, s, 138, 70, 90, 116, 10, cell);
        FillRect(px, s, s, 58, 50, 28, 22, cap);
        FillRect(px, s, s, 168, 50, 28, 22, cap);
        if (!reversed)
        {
            FillCircle(px, s, s, 73, 128, 12, pos);
            FillCircle(px, s, s, 183, 128, 12, neg);
        }
        else
        {
            FillCircle(px, s, s, 73, 128, 12, neg);
            FillCircle(px, s, s, 183, 128, 12, pos);
        }
        return Tex(px, s, s);
    }

    private static Sprite DrawBulb(bool on)
    {
        int s = 256;
        var px = Clear(s, s);
        Color glass = on ? new Color(1f, 0.92f, 0.38f) : new Color(0.78f, 0.82f, 0.88f);
        Color inner = on ? new Color(1f, 0.98f, 0.72f) : new Color(0.90f, 0.92f, 0.94f);
        FillCircle(px, s, s, 128, 96, 52, glass);
        FillCircle(px, s, s, 128, 96, 28, inner);
        FillRect(px, s, s, 108, 148, 40, 52, new Color(0.55f, 0.56f, 0.60f));
        FillRect(px, s, s, 114, 200, 28, 18, new Color(0.40f, 0.40f, 0.44f));
        if (on)
        {
            FillRect(px, s, s, 124, 20, 8, 22, new Color(1f, 0.85f, 0.2f));
            FillRect(px, s, s, 20, 96, 22, 8, new Color(1f, 0.85f, 0.2f));
            FillRect(px, s, s, 214, 96, 22, 8, new Color(1f, 0.85f, 0.2f));
        }
        return Tex(px, s, s);
    }

    private static Sprite DrawSwitch(bool on)
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 40, 88, 176, 80, 12, new Color(0.28f, 0.30f, 0.36f));
        FillCircle(px, s, s, 78, 128, 16, new Color(0.85f, 0.85f, 0.88f));
        FillCircle(px, s, s, 178, 128, 16, new Color(0.85f, 0.85f, 0.88f));
        if (on) FillRect(px, s, s, 78, 120, 100, 16, new Color(0.20f, 0.78f, 0.42f));
        else FillRect(px, s, s, 70, 70, 16, 70, new Color(0.92f, 0.55f, 0.18f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBoard()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 18, 40, 220, 176, 10, new Color(0.88f, 0.72f, 0.38f));
        for (int r = 0; r < 6; r++)
            for (int c = 0; c < 10; c++)
                FillCircle(px, s, s, 42 + c * 18, 70 + r * 22, 3, new Color(0.22f, 0.22f, 0.24f));
        return Tex(px, s, s);
    }

    private static Sprite DrawWire()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 30, 118, 196, 18, new Color(0.85f, 0.18f, 0.16f));
        FillCircle(px, s, s, 30, 127, 14, new Color(0.75f, 0.75f, 0.78f));
        FillCircle(px, s, s, 226, 127, 14, new Color(0.75f, 0.75f, 0.78f));
        return Tex(px, s, s);
    }

    private static Sprite DrawRays()
    {
        int s = 256;
        var px = Clear(s, s);
        Color c = new Color(1f, 0.92f, 0.35f, 0.85f);
        FillRect(px, s, s, 122, 10, 12, 40, c);
        FillRect(px, s, s, 122, 206, 12, 40, c);
        FillRect(px, s, s, 10, 122, 40, 12, c);
        FillRect(px, s, s, 206, 122, 40, 12, c);
        return Tex(px, s, s);
    }

    private static Sprite DrawArrow(bool forward)
    {
        int s = 256;
        var px = Clear(s, s);
        Color c = forward ? new Color(0.12f, 0.62f, 0.42f) : new Color(0.78f, 0.22f, 0.18f);
        FillRect(px, s, s, 40, 118, 130, 20, c);
        if (forward) FillTriangle(px, s, s, 160, 90, 220, 128, 160, 166, c);
        else FillTriangle(px, s, s, 96, 90, 36, 128, 96, 166, c);
        return Tex(px, s, s);
    }

    private static Sprite DrawMeter(Color ring)
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 86, ring);
        FillCircle(px, s, s, 128, 128, 64, new Color(0.96f, 0.97f, 0.99f));
        FillRect(px, s, s, 124, 70, 8, 58, ring);
        FillCircle(px, s, s, 128, 128, 8, ring);
        return Tex(px, s, s);
    }

    private static Sprite DrawBalance()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 118, 60, 20, 140, new Color(0.45f, 0.48f, 0.55f));
        FillRect(px, s, s, 40, 70, 176, 16, new Color(0.45f, 0.48f, 0.55f));
        FillCircle(px, s, s, 60, 120, 22, new Color(0.72f, 0.74f, 0.78f));
        FillCircle(px, s, s, 196, 120, 22, new Color(0.72f, 0.74f, 0.78f));
        return Tex(px, s, s);
    }

    private static Sprite DrawSpring()
    {
        int s = 256;
        var px = Clear(s, s);
        for (int i = 0; i < 6; i++)
            FillEllipse(px, s, s, 128, 50 + i * 28, 40, 10, new Color(0.55f, 0.58f, 0.62f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBlock()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 48, 88, 160, 90, 8, new Color(0.72f, 0.48f, 0.22f));
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

    private static Sprite DrawCylinder()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 100, 40, 56, 176, new Color(0.75f, 0.88f, 0.95f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBeaker()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 64, 70, 128, 140, new Color(0.75f, 0.88f, 0.95f, 0.8f));
        FillRect(px, s, s, 72, 110, 112, 92, new Color(0.55f, 0.78f, 0.95f, 0.7f));
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

    private static Sprite DrawRuler()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 16, 108, 224, 40, 6, new Color(0.92f, 0.86f, 0.62f));
        return Tex(px, s, s);
    }

    private static Sprite DrawTimer()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 70, new Color(0.32f, 0.34f, 0.40f));
        FillCircle(px, s, s, 128, 128, 52, new Color(0.94f, 0.95f, 0.97f));
        FillRect(px, s, s, 124, 80, 8, 48, new Color(0.18f, 0.18f, 0.22f));
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

    private static Sprite DrawTick()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 80, new Color(0.12f, 0.62f, 0.35f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCross()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 80, new Color(0.78f, 0.16f, 0.16f));
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

    private static void FillTriangle(Color[] px, int w, int h, int x1, int y1, int x2, int y2, int x3, int y3, Color c)
    {
        int minX = Mathf.Max(0, Mathf.Min(x1, Mathf.Min(x2, x3)));
        int maxX = Mathf.Min(w - 1, Mathf.Max(x1, Mathf.Max(x2, x3)));
        int minY = Mathf.Max(0, Mathf.Min(y1, Mathf.Min(y2, y3)));
        int maxY = Mathf.Min(h - 1, Mathf.Max(y1, Mathf.Max(y2, y3)));
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (Inside(x, y, x1, y1, x2, y2, x3, y3))
                    px[y * w + x] = c;
            }
        }
    }

    private static bool Inside(int x, int y, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        float d1 = Sign(x, y, x1, y1, x2, y2);
        float d2 = Sign(x, y, x2, y2, x3, y3);
        float d3 = Sign(x, y, x3, y3, x1, y1);
        bool hasNeg = d1 < 0 || d2 < 0 || d3 < 0;
        bool hasPos = d1 > 0 || d2 > 0 || d3 > 0;
        return !(hasNeg && hasPos);
    }

    private static float Sign(int x1, int y1, int x2, int y2, int x3, int y3)
    {
        return (x1 - x3) * (y2 - y3) - (x2 - x3) * (y1 - y3);
    }
}
