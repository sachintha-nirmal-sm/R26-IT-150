using System.Collections.Generic;
using UnityEngine;

public static class NewtonsLawsIconFactory
{
    private static readonly Dictionary<NewtonEquipmentType, Sprite> Cache = new Dictionary<NewtonEquipmentType, Sprite>();
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

    public static Sprite GetSprite(NewtonEquipmentType type)
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
            case "trolley": sprite = DrawTrolley(); break;
            case "track": sprite = DrawTrack(); break;
            case "ruler": sprite = DrawRuler(); break;
            case "pulley": sprite = DrawPulley(); break;
            case "string": sprite = DrawString(); break;
            case "hanger": sprite = DrawHanger(); break;
            case "mass": sprite = DrawMass(); break;
            case "balloon": sprite = DrawBalloon(); break;
            case "straw": sprite = DrawStraw(); break;
            case "spring": sprite = DrawBalance(); break;
            case "stopwatch": sprite = DrawStopwatch(); break;
            case "arrow":
            case "arrowRight": sprite = DrawArrow(true); break;
            case "arrowLeft": sprite = DrawArrow(false); break;
            case "correct": sprite = DrawBadge(new Color(0.12f, 0.62f, 0.35f)); break;
            case "wrong": sprite = DrawBadge(new Color(0.78f, 0.18f, 0.18f)); break;
            default: sprite = White(); break;
        }
        Extra[key] = sprite;
        return sprite;
    }

    public static Color GetColor(NewtonEquipmentType type)
    {
        switch (type)
        {
            case NewtonEquipmentType.DynamicsTrolley: return new Color(0.18f, 0.48f, 0.78f);
            case NewtonEquipmentType.StraightTrack: return new Color(0.55f, 0.58f, 0.62f);
            case NewtonEquipmentType.NewtonSpringBalance: return new Color(0.85f, 0.55f, 0.2f);
            case NewtonEquipmentType.MassBlocks: return new Color(0.45f, 0.42f, 0.48f);
            case NewtonEquipmentType.WeightHanger: return new Color(0.35f, 0.38f, 0.42f);
            case NewtonEquipmentType.Stopwatch: return new Color(0.70f, 0.72f, 0.78f);
            case NewtonEquipmentType.Ruler: return new Color(0.95f, 0.85f, 0.45f);
            case NewtonEquipmentType.Balloon: return new Color(0.86f, 0.22f, 0.28f);
            case NewtonEquipmentType.String: return new Color(0.62f, 0.48f, 0.28f);
            case NewtonEquipmentType.Pulley: return new Color(0.60f, 0.62f, 0.68f);
            case NewtonEquipmentType.RecordingTable: return new Color(0.95f, 0.93f, 0.82f);
            case NewtonEquipmentType.Calculator: return new Color(0.25f, 0.28f, 0.34f);
            default: return new Color(0.82f, 0.86f, 0.92f);
        }
    }

    private static Sprite Draw(NewtonEquipmentType type)
    {
        switch (type)
        {
            case NewtonEquipmentType.DynamicsTrolley: return DrawTrolley();
            case NewtonEquipmentType.StraightTrack: return DrawTrack();
            case NewtonEquipmentType.NewtonSpringBalance: return DrawBalance();
            case NewtonEquipmentType.MassBlocks: return DrawMass();
            case NewtonEquipmentType.WeightHanger: return DrawHanger();
            case NewtonEquipmentType.Stopwatch: return DrawStopwatch();
            case NewtonEquipmentType.Ruler: return DrawRuler();
            case NewtonEquipmentType.Balloon: return DrawBalloon();
            case NewtonEquipmentType.String: return DrawString();
            case NewtonEquipmentType.Pulley: return DrawPulley();
            case NewtonEquipmentType.RecordingTable: return DrawSheet();
            case NewtonEquipmentType.Calculator: return DrawCalculator();
            case NewtonEquipmentType.Straw: return DrawStraw();
            case NewtonEquipmentType.Ammeter: return DrawMeter("A", new Color(0.15f, 0.45f, 0.78f));
            case NewtonEquipmentType.Voltmeter: return DrawMeter("V", new Color(0.72f, 0.22f, 0.22f));
            case NewtonEquipmentType.Bulb: return DrawBulb();
            case NewtonEquipmentType.DryCell: return DrawCell();
            case NewtonEquipmentType.Beaker: return DrawBeaker();
            case NewtonEquipmentType.MeasuringCylinder: return DrawCylinder();
            case NewtonEquipmentType.Thermometer: return DrawThermometer();
            case NewtonEquipmentType.Magnet: return DrawMagnet();
            case NewtonEquipmentType.BunsenBurner: return DrawBurner();
            case NewtonEquipmentType.Compass: return DrawCompass();
            case NewtonEquipmentType.Microscope: return DrawMicroscope();
            case NewtonEquipmentType.Pipette: return DrawPipette();
            default: return DrawCard(GetColor(type));
        }
    }

    private static Sprite DrawTrolley()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 36, 108, 184, 58, 12, new Color(0.16f, 0.46f, 0.78f));
        FillRoundRect(px, s, s, 70, 78, 100, 40, 8, new Color(0.12f, 0.36f, 0.62f));
        FillRect(px, s, s, 82, 86, 30, 20, new Color(0.55f, 0.82f, 0.95f));
        FillRect(px, s, s, 128, 86, 30, 20, new Color(0.55f, 0.82f, 0.95f));
        FillCircle(px, s, s, 78, 172, 22, new Color(0.16f, 0.16f, 0.18f));
        FillCircle(px, s, s, 178, 172, 22, new Color(0.16f, 0.16f, 0.18f));
        FillCircle(px, s, s, 78, 172, 10, new Color(0.72f, 0.74f, 0.78f));
        FillCircle(px, s, s, 178, 172, 10, new Color(0.72f, 0.74f, 0.78f));
        return Tex(px, s, s);
    }

    private static Sprite DrawTrack()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 18, 108, 220, 44, new Color(0.42f, 0.46f, 0.52f));
        FillRect(px, s, s, 18, 124, 220, 8, new Color(0.92f, 0.92f, 0.78f));
        for (int i = 0; i <= 5; i++) FillRect(px, s, s, 28 + i * 36, 96, 4, 22, Color.white);
        return Tex(px, s, s);
    }

    private static Sprite DrawRuler()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 28, 108, 200, 40, new Color(0.95f, 0.85f, 0.45f));
        for (int i = 0; i < 10; i++) FillRect(px, s, s, 40 + i * 18, 108, 3, i % 2 == 0 ? 18 : 10, new Color(0.2f, 0.2f, 0.2f));
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

    private static Sprite DrawBalloon()
    {
        int s = 256;
        var px = Clear(s, s);
        FillEllipse(px, s, s, 128, 110, 70, 86, new Color(0.86f, 0.18f, 0.28f));
        FillEllipse(px, s, s, 108, 88, 18, 24, new Color(0.95f, 0.45f, 0.5f));
        FillRect(px, s, s, 120, 190, 16, 28, new Color(0.55f, 0.55f, 0.58f));
        FillRect(px, s, s, 124, 218, 8, 18, new Color(0.35f, 0.35f, 0.38f));
        return Tex(px, s, s);
    }

    private static Sprite DrawStraw()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 28, 118, 200, 18, new Color(0.92f, 0.92f, 0.95f));
        FillRect(px, s, s, 28, 122, 200, 8, new Color(0.75f, 0.82f, 0.9f));
        return Tex(px, s, s);
    }

    private static Sprite DrawString()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 20, 124, 216, 8, new Color(0.55f, 0.42f, 0.22f));
        FillRect(px, s, s, 210, 80, 8, 50, new Color(0.55f, 0.42f, 0.22f));
        return Tex(px, s, s);
    }

    private static Sprite DrawHanger()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 118, 40, 20, 40, new Color(0.45f, 0.48f, 0.52f));
        FillRect(px, s, s, 70, 80, 116, 16, new Color(0.35f, 0.38f, 0.42f));
        FillRect(px, s, s, 88, 96, 28, 90, new Color(0.55f, 0.32f, 0.22f));
        FillRect(px, s, s, 140, 96, 28, 70, new Color(0.55f, 0.32f, 0.22f));
        return Tex(px, s, s);
    }

    private static Sprite DrawMass()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 70, 80, 116, 96, 10, new Color(0.42f, 0.40f, 0.46f));
        FillRect(px, s, s, 88, 70, 80, 16, new Color(0.32f, 0.32f, 0.36f));
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

    private static Sprite DrawBalance()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 70, 40, 28, 170, new Color(0.55f, 0.58f, 0.62f));
        FillRect(px, s, s, 98, 48, 90, 18, new Color(0.7f, 0.72f, 0.76f));
        FillRect(px, s, s, 110, 66, 16, 90, new Color(0.85f, 0.55f, 0.2f));
        FillRect(px, s, s, 108, 156, 20, 8, new Color(0.78f, 0.18f, 0.18f));
        return Tex(px, s, s);
    }

    private static Sprite DrawSheet()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 48, 36, 160, 184, new Color(0.96f, 0.94f, 0.86f));
        for (int i = 0; i < 7; i++) FillRect(px, s, s, 64, 60 + i * 20, 128, 4, new Color(0.75f, 0.78f, 0.82f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCalculator()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 68, 36, 120, 184, 12, new Color(0.22f, 0.24f, 0.28f));
        FillRect(px, s, s, 80, 50, 96, 40, new Color(0.75f, 0.85f, 0.72f));
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 3; c++)
                FillRect(px, s, s, 84 + c * 32, 104 + r * 26, 24, 18, new Color(0.45f, 0.48f, 0.52f));
        return Tex(px, s, s);
    }

    private static Sprite DrawMeter(string _, Color ring)
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 78, ring);
        FillCircle(px, s, s, 128, 128, 58, Color.white);
        FillRect(px, s, s, 124, 80, 8, 50, new Color(0.15f, 0.15f, 0.18f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBulb()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 100, 58, new Color(0.95f, 0.85f, 0.25f));
        FillRect(px, s, s, 108, 150, 40, 50, new Color(0.55f, 0.55f, 0.58f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCell()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 70, 90, 116, 70, new Color(0.55f, 0.22f, 0.18f));
        FillRect(px, s, s, 186, 108, 18, 34, new Color(0.75f, 0.75f, 0.78f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBeaker()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 78, 70, 100, 140, new Color(0.75f, 0.88f, 0.95f));
        FillRect(px, s, s, 86, 130, 84, 70, new Color(0.45f, 0.72f, 0.9f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCylinder()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 100, 40, 56, 176, new Color(0.75f, 0.88f, 0.95f));
        FillRect(px, s, s, 104, 120, 48, 88, new Color(0.4f, 0.7f, 0.9f));
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

    private static Sprite DrawMicroscope()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 70, 190, 116, 22, new Color(0.35f, 0.38f, 0.42f));
        FillRect(px, s, s, 118, 80, 20, 120, new Color(0.45f, 0.48f, 0.55f));
        FillCircle(px, s, s, 128, 70, 28, new Color(0.25f, 0.28f, 0.32f));
        FillRect(px, s, s, 148, 110, 40, 14, new Color(0.55f, 0.58f, 0.62f));
        return Tex(px, s, s);
    }

    private static Sprite DrawPipette()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 118, 40, 20, 140, new Color(0.75f, 0.88f, 0.95f));
        FillRect(px, s, s, 110, 36, 36, 28, new Color(0.85f, 0.55f, 0.55f));
        FillRect(px, s, s, 122, 180, 12, 40, new Color(0.65f, 0.8f, 0.9f));
        return Tex(px, s, s);
    }

    private static Sprite DrawArrow(bool right)
    {
        int s = 256;
        var px = Clear(s, s);
        Color c = right ? new Color(0.12f, 0.55f, 0.28f) : new Color(0.78f, 0.22f, 0.18f);
        FillRect(px, s, s, 40, 110, 140, 36, c);
        int dir = right ? 1 : -1;
        int tip = right ? 210 : 46;
        for (int i = 0; i < 50; i++)
            FillRect(px, s, s, tip + dir * (i < 25 ? 0 : 0) - (right ? 0 : i), 80 + i / 2, 8, 96 - i, c);
        FillRect(px, s, s, right ? 170 : 40, 80, 40, 96, c);
        return Tex(px, s, s);
    }

    private static Sprite DrawBadge(Color color)
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 80, color);
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
