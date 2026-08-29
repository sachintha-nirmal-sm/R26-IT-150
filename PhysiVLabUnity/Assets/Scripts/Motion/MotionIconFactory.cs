using System.Collections.Generic;
using UnityEngine;

public static class MotionIconFactory
{
    private static readonly Dictionary<MotionEquipmentType, Sprite> Cache = new Dictionary<MotionEquipmentType, Sprite>();
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

    public static Sprite GetSprite(MotionEquipmentType type)
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
            case "arrow": sprite = DrawArrow(); break;
            case "correct": sprite = DrawBadge(new Color(0.12f, 0.62f, 0.35f)); break;
            case "wrong": sprite = DrawBadge(new Color(0.78f, 0.18f, 0.18f)); break;
            case "car": sprite = DrawCar(); break;
            case "track": sprite = DrawTrack(); break;
            default: sprite = White(); break;
        }
        Extra[key] = sprite;
        return sprite;
    }

    public static Color GetColor(MotionEquipmentType type)
    {
        switch (type)
        {
            case MotionEquipmentType.ToyCar: return new Color(0.86f, 0.22f, 0.22f);
            case MotionEquipmentType.StraightTrack: return new Color(0.55f, 0.58f, 0.62f);
            case MotionEquipmentType.MetreRuler: return new Color(0.95f, 0.85f, 0.45f);
            case MotionEquipmentType.Stopwatch: return new Color(0.70f, 0.72f, 0.78f);
            case MotionEquipmentType.DistanceMarkers: return new Color(0.20f, 0.55f, 0.85f);
            case MotionEquipmentType.StartingMarker: return new Color(0.18f, 0.62f, 0.32f);
            case MotionEquipmentType.RecordingTable: return new Color(0.95f, 0.93f, 0.82f);
            case MotionEquipmentType.Calculator: return new Color(0.25f, 0.28f, 0.34f);
            default: return new Color(0.82f, 0.86f, 0.92f);
        }
    }

    private static Sprite Draw(MotionEquipmentType type)
    {
        switch (type)
        {
            case MotionEquipmentType.ToyCar: return DrawCar();
            case MotionEquipmentType.StraightTrack: return DrawTrack();
            case MotionEquipmentType.MetreRuler: return DrawRuler();
            case MotionEquipmentType.Stopwatch: return DrawStopwatch();
            case MotionEquipmentType.DistanceMarkers: return DrawMarker(new Color(0.15f, 0.45f, 0.82f));
            case MotionEquipmentType.StartingMarker: return DrawMarker(new Color(0.12f, 0.62f, 0.32f));
            case MotionEquipmentType.RecordingTable: return DrawSheet();
            case MotionEquipmentType.Calculator: return DrawCalculator();
            case MotionEquipmentType.NewtonSpringBalance: return DrawBalance();
            case MotionEquipmentType.Ammeter: return DrawMeter("A", new Color(0.15f, 0.45f, 0.78f));
            case MotionEquipmentType.Voltmeter: return DrawMeter("V", new Color(0.72f, 0.22f, 0.22f));
            case MotionEquipmentType.Bulb: return DrawBulb();
            case MotionEquipmentType.DryCell: return DrawCell();
            case MotionEquipmentType.Beaker: return DrawBeaker();
            case MotionEquipmentType.MeasuringCylinder: return DrawCylinder();
            case MotionEquipmentType.Thermometer: return DrawThermometer();
            case MotionEquipmentType.Magnet: return DrawMagnet();
            case MotionEquipmentType.Pulley: return DrawPulley();
            case MotionEquipmentType.Lever: return DrawLever();
            case MotionEquipmentType.Clay: return DrawClay();
            case MotionEquipmentType.BunsenBurner: return DrawBurner();
            case MotionEquipmentType.Compass: return DrawCompass();
            default: return DrawCard(GetColor(type));
        }
    }

    private static Sprite DrawCar()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 36, 110, 184, 56, 14, new Color(0.86f, 0.18f, 0.18f));
        FillRoundRect(px, s, s, 78, 78, 90, 42, 10, new Color(0.72f, 0.14f, 0.14f));
        FillRect(px, s, s, 88, 86, 32, 22, new Color(0.55f, 0.82f, 0.95f));
        FillRect(px, s, s, 128, 86, 32, 22, new Color(0.55f, 0.82f, 0.95f));
        FillCircle(px, s, s, 78, 170, 22, new Color(0.16f, 0.16f, 0.18f));
        FillCircle(px, s, s, 178, 170, 22, new Color(0.16f, 0.16f, 0.18f));
        FillCircle(px, s, s, 78, 170, 10, new Color(0.72f, 0.74f, 0.78f));
        FillCircle(px, s, s, 178, 170, 10, new Color(0.72f, 0.74f, 0.78f));
        FillRect(px, s, s, 210, 128, 16, 10, new Color(1f, 0.85f, 0.25f));
        return Tex(px, s, s);
    }

    private static Sprite DrawTrack()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 18, 108, 220, 44, new Color(0.42f, 0.46f, 0.52f));
        FillRect(px, s, s, 18, 124, 220, 8, new Color(0.92f, 0.92f, 0.78f));
        for (int i = 0; i <= 5; i++)
            FillRect(px, s, s, 28 + i * 36, 96, 4, 22, Color.white);
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

    private static Sprite DrawStopwatch()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 132, 74, new Color(0.62f, 0.66f, 0.72f));
        FillCircle(px, s, s, 128, 132, 58, Color.white);
        FillRect(px, s, s, 124, 88, 8, 48, new Color(0.18f, 0.18f, 0.2f));
        FillRect(px, s, s, 128, 128, 36, 6, new Color(0.78f, 0.18f, 0.18f));
        FillRect(px, s, s, 118, 46, 20, 18, new Color(0.45f, 0.48f, 0.52f));
        return Tex(px, s, s);
    }

    private static Sprite DrawMarker(Color color)
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 118, 50, 20, 140, color);
        FillCircle(px, s, s, 128, 48, 22, color);
        return Tex(px, s, s);
    }

    private static Sprite DrawSheet()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 52, 36, 152, 184, 8, new Color(0.97f, 0.95f, 0.86f));
        for (int i = 0; i < 7; i++)
            FillRect(px, s, s, 68, 60 + i * 20, 120, 6, new Color(0.72f, 0.74f, 0.78f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCalculator()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 68, 36, 120, 184, 10, new Color(0.22f, 0.24f, 0.28f));
        FillRect(px, s, s, 80, 48, 96, 40, new Color(0.72f, 0.86f, 0.72f));
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                FillRect(px, s, s, 82 + c * 24, 100 + r * 26, 18, 18, new Color(0.45f, 0.48f, 0.52f));
        return Tex(px, s, s);
    }

    private static Sprite DrawArrow()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 40, 118, 140, 20, new Color(0.12f, 0.42f, 0.78f));
        for (int i = 0; i < 28; i++)
            FillRect(px, s, s, 170 + i, 108 + i / 2, 8, 40 - i, new Color(0.12f, 0.42f, 0.78f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBadge(Color color)
    {
        int s = 128;
        var px = Clear(s, s);
        FillCircle(px, s, s, 64, 64, 50, color);
        return Tex(px, s, s);
    }

    private static Sprite DrawMeter(string symbol, Color ring)
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 90, new Color(0.93f, 0.95f, 0.98f));
        FillCircle(px, s, s, 128, 128, 78, Color.white);
        FillCircle(px, s, s, 128, 128, 8, ring);
        FillRect(px, s, s, 124, 88, 8, 48, ring);
        return Tex(px, s, s);
    }

    private static Sprite DrawBulb()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 118, 62, new Color(0.82f, 0.88f, 0.94f));
        FillRect(px, s, s, 108, 175, 40, 28, new Color(0.55f, 0.55f, 0.58f));
        FillRect(px, s, s, 114, 200, 28, 22, new Color(0.72f, 0.58f, 0.22f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCell()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 40, 88, 176, 80, 18, new Color(0.62f, 0.78f, 0.42f));
        FillRect(px, s, s, 196, 96, 28, 64, new Color(0.82f, 0.55f, 0.18f));
        FillRect(px, s, s, 70, 118, 22, 6, Color.white);
        FillRect(px, s, s, 78, 110, 6, 22, Color.white);
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
