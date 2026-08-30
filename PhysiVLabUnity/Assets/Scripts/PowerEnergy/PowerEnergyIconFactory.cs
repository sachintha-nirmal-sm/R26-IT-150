using System.Collections.Generic;
using UnityEngine;

public static class PowerEnergyIconFactory
{
    private static readonly Dictionary<PowerEnergyEquipmentType, Sprite> Cache = new Dictionary<PowerEnergyEquipmentType, Sprite>();
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

    public static Sprite GetSprite(PowerEnergyEquipmentType type)
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
            case "correct": sprite = DrawTick(); break;
            case "wrong": sprite = DrawCross(); break;
            case "lab": sprite = DrawLab(); break;
            default: sprite = White(); break;
        }
        Extra[key] = sprite;
        return sprite;
    }

    public static Color GetColor(PowerEnergyEquipmentType type)
    {
        switch (type)
        {
            case PowerEnergyEquipmentType.PowerSupply: return new Color(0.20f, 0.45f, 0.72f);
            case PowerEnergyEquipmentType.Voltmeter: return new Color(0.78f, 0.22f, 0.22f);
            case PowerEnergyEquipmentType.Ammeter: return new Color(0.15f, 0.48f, 0.78f);
            case PowerEnergyEquipmentType.ElectricalAppliance: return new Color(0.95f, 0.78f, 0.28f);
            case PowerEnergyEquipmentType.Bulb: return new Color(1f, 0.88f, 0.35f);
            case PowerEnergyEquipmentType.Fan: return new Color(0.35f, 0.62f, 0.82f);
            case PowerEnergyEquipmentType.Iron: return new Color(0.55f, 0.55f, 0.60f);
            case PowerEnergyEquipmentType.Kettle: return new Color(0.22f, 0.62f, 0.52f);
            case PowerEnergyEquipmentType.Timer: return new Color(0.40f, 0.42f, 0.50f);
            case PowerEnergyEquipmentType.Calculator: return new Color(0.28f, 0.32f, 0.38f);
            case PowerEnergyEquipmentType.ObservationSheet: return new Color(0.96f, 0.93f, 0.82f);
            case PowerEnergyEquipmentType.Switch: return new Color(0.85f, 0.55f, 0.18f);
            case PowerEnergyEquipmentType.Wire: return new Color(0.90f, 0.72f, 0.18f);
            default: return new Color(0.82f, 0.84f, 0.88f);
        }
    }

    private static Sprite Draw(PowerEnergyEquipmentType type)
    {
        switch (type)
        {
            case PowerEnergyEquipmentType.PowerSupply: return DrawSupply();
            case PowerEnergyEquipmentType.Voltmeter: return DrawMeter(new Color(0.78f, 0.18f, 0.18f), "V");
            case PowerEnergyEquipmentType.Ammeter: return DrawMeter(new Color(0.12f, 0.42f, 0.78f), "A");
            case PowerEnergyEquipmentType.ElectricalAppliance: return DrawBulb();
            case PowerEnergyEquipmentType.Bulb: return DrawBulb();
            case PowerEnergyEquipmentType.Fan: return DrawFan();
            case PowerEnergyEquipmentType.Iron: return DrawIron();
            case PowerEnergyEquipmentType.Kettle: return DrawKettle();
            case PowerEnergyEquipmentType.Timer: return DrawTimer();
            case PowerEnergyEquipmentType.Calculator: return DrawCalculator();
            case PowerEnergyEquipmentType.ObservationSheet: return DrawSheet();
            case PowerEnergyEquipmentType.Switch: return DrawSwitch();
            case PowerEnergyEquipmentType.Wire: return DrawWire();
            case PowerEnergyEquipmentType.NewtonBalance: return DrawBalance();
            case PowerEnergyEquipmentType.Spring: return DrawSpring();
            case PowerEnergyEquipmentType.WoodenBlock: return DrawBlock();
            case PowerEnergyEquipmentType.Thermometer: return DrawThermometer();
            case PowerEnergyEquipmentType.MeasuringCylinder: return DrawCylinder();
            case PowerEnergyEquipmentType.Beaker: return DrawBeaker();
            case PowerEnergyEquipmentType.Magnet: return DrawMagnet();
            case PowerEnergyEquipmentType.Ruler: return DrawRuler();
            case PowerEnergyEquipmentType.Stopwatch: return DrawTimer();
            case PowerEnergyEquipmentType.Pulley: return DrawPulley();
            default: return DrawCard(GetColor(type));
        }
    }

    private static Sprite DrawSupply()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 36, 70, 184, 116, 12, new Color(0.18f, 0.32f, 0.55f));
        FillRect(px, s, s, 52, 88, 152, 80, new Color(0.12f, 0.18f, 0.28f));
        FillCircle(px, s, s, 90, 128, 14, new Color(0.85f, 0.20f, 0.18f));
        FillCircle(px, s, s, 166, 128, 14, new Color(0.18f, 0.18f, 0.20f));
        FillRect(px, s, s, 84, 188, 12, 28, new Color(0.75f, 0.75f, 0.78f));
        FillRect(px, s, s, 160, 188, 12, 28, new Color(0.75f, 0.75f, 0.78f));
        return Tex(px, s, s);
    }

    private static Sprite DrawMeter(Color ring, string _)
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 86, ring);
        FillCircle(px, s, s, 128, 128, 64, new Color(0.96f, 0.97f, 0.99f));
        FillRect(px, s, s, 124, 70, 8, 58, ring);
        FillCircle(px, s, s, 128, 128, 8, ring);
        return Tex(px, s, s);
    }

    private static Sprite DrawBulb()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 96, 52, new Color(1f, 0.90f, 0.38f));
        FillCircle(px, s, s, 128, 96, 28, new Color(1f, 0.97f, 0.72f));
        FillRect(px, s, s, 108, 148, 40, 52, new Color(0.55f, 0.56f, 0.60f));
        FillRect(px, s, s, 114, 200, 28, 18, new Color(0.40f, 0.40f, 0.44f));
        return Tex(px, s, s);
    }

    private static Sprite DrawFan()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 110, 70, new Color(0.55f, 0.78f, 0.92f));
        FillCircle(px, s, s, 128, 110, 18, new Color(0.22f, 0.32f, 0.42f));
        FillEllipse(px, s, s, 128, 58, 16, 36, new Color(0.30f, 0.55f, 0.78f));
        FillEllipse(px, s, s, 176, 132, 36, 16, new Color(0.30f, 0.55f, 0.78f));
        FillEllipse(px, s, s, 80, 132, 36, 16, new Color(0.30f, 0.55f, 0.78f));
        FillRect(px, s, s, 118, 178, 20, 40, new Color(0.40f, 0.42f, 0.48f));
        return Tex(px, s, s);
    }

    private static Sprite DrawIron()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 40, 130, 176, 48, 16, new Color(0.62f, 0.64f, 0.70f));
        FillRoundRect(px, s, s, 70, 78, 116, 58, 12, new Color(0.28f, 0.45f, 0.72f));
        FillRect(px, s, s, 92, 52, 72, 28, new Color(0.85f, 0.86f, 0.90f));
        return Tex(px, s, s);
    }

    private static Sprite DrawKettle()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 70, 70, 116, 130, 22, new Color(0.18f, 0.58f, 0.50f));
        FillRect(px, s, s, 86, 52, 84, 22, new Color(0.14f, 0.40f, 0.36f));
        FillEllipse(px, s, s, 198, 120, 18, 36, new Color(0.18f, 0.58f, 0.50f));
        FillRect(px, s, s, 78, 200, 100, 16, new Color(0.22f, 0.22f, 0.24f));
        return Tex(px, s, s);
    }

    private static Sprite DrawTimer()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 140, 72, new Color(0.70f, 0.73f, 0.80f));
        FillCircle(px, s, s, 128, 140, 54, new Color(0.96f, 0.97f, 0.99f));
        FillRect(px, s, s, 124, 92, 8, 48, new Color(0.18f, 0.22f, 0.30f));
        FillRect(px, s, s, 118, 48, 20, 18, new Color(0.55f, 0.58f, 0.64f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCalculator()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 68, 36, 120, 184, 10, new Color(0.22f, 0.24f, 0.28f));
        FillRect(px, s, s, 80, 48, 96, 40, new Color(0.72f, 0.86f, 0.78f));
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 3; c++)
                FillRoundRect(px, s, s, 84 + c * 30, 102 + r * 28, 24, 22, 4, new Color(0.40f, 0.42f, 0.48f));
        return Tex(px, s, s);
    }

    private static Sprite DrawSheet()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 58, 36, 140, 184, 6, new Color(0.97f, 0.95f, 0.86f));
        FillRect(px, s, s, 74, 70, 108, 8, new Color(0.55f, 0.62f, 0.72f));
        FillRect(px, s, s, 74, 100, 108, 8, new Color(0.55f, 0.62f, 0.72f));
        FillRect(px, s, s, 74, 130, 108, 8, new Color(0.55f, 0.62f, 0.72f));
        FillRect(px, s, s, 74, 160, 80, 8, new Color(0.55f, 0.62f, 0.72f));
        return Tex(px, s, s);
    }

    private static Sprite DrawSwitch()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 48, 88, 160, 80, 12, new Color(0.88f, 0.88f, 0.90f));
        FillRoundRect(px, s, s, 88, 102, 80, 52, 8, new Color(0.90f, 0.55f, 0.16f));
        return Tex(px, s, s);
    }

    private static Sprite DrawWire()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 30, 118, 196, 14, new Color(0.90f, 0.72f, 0.18f));
        FillCircle(px, s, s, 36, 125, 16, new Color(0.55f, 0.55f, 0.58f));
        FillCircle(px, s, s, 220, 125, 16, new Color(0.55f, 0.55f, 0.58f));
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

    private static Sprite DrawSpring()
    {
        int s = 256;
        var px = Clear(s, s);
        for (int i = 0; i < 10; i++)
            FillEllipse(px, s, s, 128, 48 + i * 18, 40, 10, new Color(0.62f, 0.66f, 0.72f));
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

    private static Sprite DrawLab()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 8, 140, 240, 28, new Color(0.55f, 0.62f, 0.72f));
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
