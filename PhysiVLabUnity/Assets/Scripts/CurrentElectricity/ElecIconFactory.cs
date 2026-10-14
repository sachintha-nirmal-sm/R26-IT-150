using System.Collections.Generic;
using UnityEngine;

public static class ElecIconFactory
{
    private static readonly Dictionary<ElecEquipmentType, Sprite> Cache = new Dictionary<ElecEquipmentType, Sprite>();
    private static readonly Dictionary<ElectricalComponentType, Sprite> CompCache = new Dictionary<ElectricalComponentType, Sprite>();
    private static Sprite whiteSprite;

    public static void ClearCache()
    {
        Cache.Clear();
        CompCache.Clear();
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

    public static Sprite GetSprite(ElecEquipmentType type)
    {
        if (Cache.TryGetValue(type, out Sprite cached) && cached != null) return cached;
        Sprite sprite = LoadPhoto(type) ?? DrawEquipment(type);
        Cache[type] = sprite;
        return sprite;
    }

    public static Sprite GetComponentSprite(ElectricalComponentType type)
    {
        if (CompCache.TryGetValue(type, out Sprite cached) && cached != null) return cached;
        Sprite sprite;
        switch (type)
        {
            case ElectricalComponentType.DryCell: sprite = DrawCell(); break;
            case ElectricalComponentType.Bulb: sprite = DrawBulb(false); break;
            case ElectricalComponentType.Ammeter: sprite = DrawMeter("A", new Color(0.15f, 0.45f, 0.78f)); break;
            case ElectricalComponentType.Voltmeter: sprite = DrawMeter("V", new Color(0.72f, 0.22f, 0.22f)); break;
            default: sprite = DrawCard(new Color(0.82f, 0.86f, 0.92f)); break;
        }
        CompCache[type] = sprite;
        return sprite;
    }

    public static Color GetColor(ElecEquipmentType type)
    {
        switch (type)
        {
            case ElecEquipmentType.DryCell1:
            case ElecEquipmentType.DryCell2: return new Color(0.55f, 0.72f, 0.42f);
            case ElecEquipmentType.Bulb: return new Color(0.95f, 0.85f, 0.40f);
            case ElecEquipmentType.Ammeter: return new Color(0.45f, 0.70f, 0.90f);
            case ElecEquipmentType.Voltmeter: return new Color(0.90f, 0.55f, 0.55f);
            case ElecEquipmentType.ConductingWires: return new Color(0.35f, 0.35f, 0.38f);
            case ElecEquipmentType.CircuitBoard: return new Color(0.30f, 0.55f, 0.38f);
            default: return new Color(0.82f, 0.86f, 0.92f);
        }
    }

    private static Sprite LoadPhoto(ElecEquipmentType type)
    {
        string name = type.ToString().ToLowerInvariant();
        var tex = Resources.Load<Texture2D>("EquipmentSprites/" + name);
        if (tex == null) return null;
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static Sprite DrawEquipment(ElecEquipmentType type)
    {
        switch (type)
        {
            case ElecEquipmentType.DryCell1:
            case ElecEquipmentType.DryCell2: return DrawCell();
            case ElecEquipmentType.Bulb: return DrawBulb(false);
            case ElecEquipmentType.Ammeter: return DrawMeter("A", new Color(0.15f, 0.45f, 0.78f));
            case ElecEquipmentType.Voltmeter: return DrawMeter("V", new Color(0.72f, 0.22f, 0.22f));
            case ElecEquipmentType.ConductingWires: return DrawWires();
            case ElecEquipmentType.CircuitBoard: return DrawBoard();
            case ElecEquipmentType.Ruler: return DrawRuler();
            case ElecEquipmentType.Thermometer: return DrawThermometer();
            case ElecEquipmentType.Beaker: return DrawBeaker();
            case ElecEquipmentType.MeasuringCylinder: return DrawCylinder();
            case ElecEquipmentType.Magnet: return DrawMagnet();
            case ElecEquipmentType.Stopwatch: return DrawStopwatch();
            case ElecEquipmentType.BunsenBurner: return DrawBurner();
            case ElecEquipmentType.Compass: return DrawCompass();
            case ElecEquipmentType.NewtonSpringBalance: return DrawBalance();
            case ElecEquipmentType.Spring: return DrawSpring();
            case ElecEquipmentType.Pulley: return DrawPulley();
            case ElecEquipmentType.Lever: return DrawLever();
            case ElecEquipmentType.Clay: return DrawClay();
            case ElecEquipmentType.HeavyWeight: return DrawWeight();
            case ElecEquipmentType.Barometer: return DrawBarometer();
            case ElecEquipmentType.IncorrectAmmeter: return DrawMeter("AC", new Color(0.45f, 0.45f, 0.48f));
            default: return DrawCard(GetColor(type));
        }
    }

    private static Sprite DrawCell()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 40, 88, 176, 80, 18, new Color(0.62f, 0.78f, 0.42f));
        FillRect(px, s, s, 196, 96, 28, 64, new Color(0.82f, 0.55f, 0.18f));
        FillRect(px, s, s, 220, 112, 16, 32, new Color(0.75f, 0.75f, 0.78f));
        FillRect(px, s, s, 48, 112, 18, 32, new Color(0.12f, 0.12f, 0.14f));
        FillRect(px, s, s, 70, 118, 22, 6, Color.white);
        FillRect(px, s, s, 78, 110, 6, 22, Color.white);
        FillRect(px, s, s, 52, 118, 16, 6, Color.white);
        return Tex(px, s, s);
    }

    private static Sprite DrawBulb(bool on)
    {
        int s = 256;
        var px = Clear(s, s);
        Color glass = on ? new Color(1f, 0.92f, 0.45f) : new Color(0.82f, 0.88f, 0.94f);
        FillCircle(px, s, s, 128, 118, 62, glass);
        FillCircle(px, s, s, 118, 132, 18, new Color(1f, 1f, 1f, 0.35f));
        FillRect(px, s, s, 108, 175, 40, 28, new Color(0.55f, 0.55f, 0.58f));
        FillRect(px, s, s, 114, 200, 28, 22, new Color(0.72f, 0.58f, 0.22f));
        FillRect(px, s, s, 120, 110, 4, 40, new Color(0.35f, 0.35f, 0.4f));
        FillRect(px, s, s, 132, 118, 4, 32, new Color(0.35f, 0.35f, 0.4f));
        return Tex(px, s, s);
    }

    private static Sprite DrawMeter(string symbol, Color ring)
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 90, new Color(0.93f, 0.95f, 0.98f));
        FillCircle(px, s, s, 128, 128, 78, Color.white);
        FillCircle(px, s, s, 128, 128, 8, ring);
        DrawArc(px, s, s, 128, 140, 54, ring);
        FillRect(px, s, s, 124, 88, 8, 48, ring);
        return Tex(px, s, s);
    }

    private static Sprite DrawWires()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 30, 70, 196, 14, new Color(0.82f, 0.18f, 0.18f));
        FillRect(px, s, s, 30, 120, 196, 14, new Color(0.12f, 0.12f, 0.14f));
        FillRect(px, s, s, 30, 170, 196, 14, new Color(0.18f, 0.45f, 0.82f));
        FillCircle(px, s, s, 36, 77, 10, new Color(0.75f, 0.75f, 0.2f));
        FillCircle(px, s, s, 220, 77, 10, new Color(0.75f, 0.75f, 0.2f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBoard()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 24, 36, 208, 184, 12, new Color(0.22f, 0.48f, 0.32f));
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 6; j++)
                FillCircle(px, s, s, 48 + i * 22, 60 + j * 24, 4, new Color(0.12f, 0.12f, 0.12f));
        return Tex(px, s, s);
    }

    private static Sprite DrawRuler()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 28, 108, 200, 40, new Color(0.95f, 0.85f, 0.45f));
        for (int i = 0; i < 10; i++)
            FillRect(px, s, s, 40 + i * 18, 108, 3, i % 2 == 0 ? 18 : 10, new Color(0.2f, 0.2f, 0.2f));
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

    private static Sprite DrawBeaker()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 70, 70, 116, 140, new Color(0.75f, 0.88f, 0.95f, 0.9f));
        FillRect(px, s, s, 78, 150, 100, 50, new Color(0.45f, 0.7f, 0.9f, 0.7f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCylinder()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 100, 36, 56, 184, new Color(0.75f, 0.88f, 0.95f, 0.9f));
        FillRect(px, s, s, 104, 140, 48, 70, new Color(0.4f, 0.65f, 0.9f, 0.7f));
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
        FillCircle(px, s, s, 128, 128, 70, new Color(0.7f, 0.72f, 0.78f));
        FillCircle(px, s, s, 128, 128, 54, Color.white);
        FillRect(px, s, s, 124, 80, 8, 48, new Color(0.2f, 0.2f, 0.2f));
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

    private static Sprite DrawCompass()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 78, new Color(0.9f, 0.93f, 0.85f));
        FillRect(px, s, s, 122, 70, 12, 50, new Color(0.8f, 0.15f, 0.15f));
        FillRect(px, s, s, 122, 136, 12, 50, new Color(0.15f, 0.15f, 0.18f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBalance()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 70, 40, 28, 170, new Color(0.55f, 0.58f, 0.62f));
        FillRect(px, s, s, 98, 48, 90, 18, new Color(0.7f, 0.72f, 0.76f));
        FillRect(px, s, s, 110, 66, 16, 90, new Color(0.85f, 0.55f, 0.2f));
        return Tex(px, s, s);
    }

    private static Sprite DrawSpring()
    {
        int s = 256;
        var px = Clear(s, s);
        for (int i = 0; i < 8; i++)
            FillRect(px, s, s, 90, 40 + i * 24, 76, 10, new Color(0.55f, 0.58f, 0.62f));
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

    private static Sprite DrawLever()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 30, 120, 196, 18, new Color(0.62f, 0.42f, 0.22f));
        FillRect(px, s, s, 118, 138, 20, 50, new Color(0.4f, 0.4f, 0.42f));
        return Tex(px, s, s);
    }

    private static Sprite DrawClay()
    {
        int s = 256;
        var px = Clear(s, s);
        FillEllipse(px, s, s, 128, 128, 90, 50, new Color(0.76f, 0.50f, 0.30f));
        return Tex(px, s, s);
    }

    private static Sprite DrawWeight()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 132, 70, new Color(0.42f, 0.46f, 0.55f));
        FillRect(px, s, s, 118, 50, 20, 30, new Color(0.28f, 0.3f, 0.34f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBarometer()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 110, 40, 36, 176, new Color(0.7f, 0.78f, 0.85f));
        FillRect(px, s, s, 118, 80, 20, 120, new Color(0.75f, 0.75f, 0.78f));
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

    private static void DrawArc(Color[] px, int w, int h, int cx, int cy, int r, Color c)
    {
        for (int i = 20; i <= 160; i += 2)
        {
            float a = i * Mathf.Deg2Rad;
            int x = cx + Mathf.RoundToInt(Mathf.Cos(a) * r);
            int y = cy - Mathf.RoundToInt(Mathf.Sin(a) * r);
            FillCircle(px, w, h, x, y, 3, c);
        }
    }
}
