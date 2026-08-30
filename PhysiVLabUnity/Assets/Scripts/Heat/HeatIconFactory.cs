using System.Collections.Generic;
using UnityEngine;

public static class HeatIconFactory
{
    private static readonly Dictionary<HeatEquipmentType, Sprite> Cache = new Dictionary<HeatEquipmentType, Sprite>();
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

    public static Sprite GetSprite(HeatEquipmentType type)
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
            case "flame": sprite = DrawFlame(); break;
            case "bench": sprite = DrawBench(); break;
            default: sprite = White(); break;
        }
        Extra[key] = sprite;
        return sprite;
    }

    public static Color GetColor(HeatEquipmentType type)
    {
        switch (type)
        {
            case HeatEquipmentType.TestTube: return new Color(0.72f, 0.88f, 0.95f);
            case HeatEquipmentType.ColoredWater: return new Color(0.88f, 0.22f, 0.22f);
            case HeatEquipmentType.RubberStopper: return new Color(0.42f, 0.22f, 0.18f);
            case HeatEquipmentType.ThinGlassTube: return new Color(0.78f, 0.90f, 0.96f);
            case HeatEquipmentType.Beaker: return new Color(0.70f, 0.86f, 0.95f);
            case HeatEquipmentType.BunsenBurner: return new Color(0.40f, 0.42f, 0.48f);
            case HeatEquipmentType.RetortStand: return new Color(0.28f, 0.28f, 0.30f);
            case HeatEquipmentType.TripodStand: return new Color(0.35f, 0.32f, 0.28f);
            default: return new Color(0.86f, 0.82f, 0.78f);
        }
    }

    private static Sprite Draw(HeatEquipmentType type)
    {
        switch (type)
        {
            case HeatEquipmentType.TestTube: return DrawTestTube();
            case HeatEquipmentType.ColoredWater: return DrawColoredWater();
            case HeatEquipmentType.RubberStopper: return DrawStopper();
            case HeatEquipmentType.ThinGlassTube: return DrawCapillary();
            case HeatEquipmentType.Beaker: return DrawBeaker();
            case HeatEquipmentType.BunsenBurner: return DrawBurner();
            case HeatEquipmentType.RetortStand: return DrawRetort();
            case HeatEquipmentType.TripodStand: return DrawTripod();
            case HeatEquipmentType.Thermometer: return DrawThermometer();
            case HeatEquipmentType.MeasuringCylinder: return DrawCylinder();
            case HeatEquipmentType.NewtonBalance: return DrawBalance();
            case HeatEquipmentType.WoodenBlock: return DrawBlock();
            case HeatEquipmentType.Slinky: return DrawSlinky();
            case HeatEquipmentType.Ammeter: return DrawMeter(new Color(0.15f, 0.45f, 0.78f));
            case HeatEquipmentType.Voltmeter: return DrawMeter(new Color(0.72f, 0.22f, 0.22f));
            case HeatEquipmentType.Bulb: return DrawBulb();
            case HeatEquipmentType.DryCell: return DrawCell();
            case HeatEquipmentType.Magnet: return DrawMagnet();
            case HeatEquipmentType.Stopwatch: return DrawStopwatch();
            case HeatEquipmentType.MassHanger: return DrawHanger();
            case HeatEquipmentType.Compass: return DrawCompass();
            case HeatEquipmentType.Pulley: return DrawPulley();
            case HeatEquipmentType.Trolley: return DrawTrolley();
            case HeatEquipmentType.ConcaveMirror: return DrawConcaveMirror();
            case HeatEquipmentType.ConvexLens: return DrawLens();
            case HeatEquipmentType.GlassPrism: return DrawPrism();
            case HeatEquipmentType.MeterRuler: return DrawRuler();
            case HeatEquipmentType.WhiteScreen: return DrawScreen();
            case HeatEquipmentType.GlassSlab: return DrawSlab();
            case HeatEquipmentType.ConcaveLens: return DrawLens();
            default: return DrawCard(GetColor(type));
        }
    }

    private static Sprite DrawTestTube()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 100, 28, 56, 196, 22, new Color(0.78f, 0.90f, 0.96f, 0.85f));
        FillRect(px, s, s, 108, 70, 40, 130, new Color(0.86f, 0.18f, 0.18f, 0.85f));
        FillRect(px, s, s, 96, 20, 64, 18, new Color(0.70f, 0.84f, 0.92f));
        return Tex(px, s, s);
    }

    private static Sprite DrawColoredWater()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 78, 70, 100, 140, 16, new Color(0.82f, 0.16f, 0.16f));
        FillRect(px, s, s, 110, 40, 36, 40, new Color(0.55f, 0.55f, 0.58f));
        FillCircle(px, s, s, 128, 40, 16, new Color(0.35f, 0.35f, 0.38f));
        return Tex(px, s, s);
    }

    private static Sprite DrawStopper()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 78, 70, 100, 90, 12, new Color(0.42f, 0.20f, 0.14f));
        FillRect(px, s, s, 92, 150, 72, 36, new Color(0.36f, 0.16f, 0.12f));
        FillRect(px, s, s, 120, 40, 16, 40, new Color(0.75f, 0.88f, 0.94f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCapillary()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 118, 20, 20, 216, new Color(0.78f, 0.90f, 0.96f, 0.9f));
        FillRect(px, s, s, 122, 90, 12, 140, new Color(0.86f, 0.18f, 0.18f, 0.9f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBeaker()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 64, 70, 128, 140, new Color(0.75f, 0.88f, 0.95f, 0.8f));
        FillRect(px, s, s, 72, 110, 112, 92, new Color(0.55f, 0.78f, 0.95f, 0.7f));
        FillRect(px, s, s, 56, 58, 144, 16, new Color(0.70f, 0.84f, 0.92f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBurner()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 88, 150, 80, 50, new Color(0.35f, 0.38f, 0.42f));
        FillRect(px, s, s, 118, 100, 20, 52, new Color(0.28f, 0.30f, 0.34f));
        FillCircle(px, s, s, 128, 62, 26, new Color(0.35f, 0.55f, 0.95f));
        FillCircle(px, s, s, 128, 50, 14, new Color(0.85f, 0.92f, 1f));
        return Tex(px, s, s);
    }

    private static Sprite DrawFlame()
    {
        int s = 256;
        var px = Clear(s, s);
        FillEllipse(px, s, s, 128, 150, 28, 70, new Color(0.25f, 0.45f, 0.95f));
        FillEllipse(px, s, s, 128, 160, 14, 40, new Color(0.85f, 0.92f, 1f));
        return Tex(px, s, s);
    }

    private static Sprite DrawRetort()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 48, 40, 18, 190, new Color(0.18f, 0.18f, 0.20f));
        FillRect(px, s, s, 36, 220, 80, 16, new Color(0.18f, 0.18f, 0.20f));
        FillRect(px, s, s, 66, 70, 90, 14, new Color(0.18f, 0.18f, 0.20f));
        FillRect(px, s, s, 148, 58, 22, 40, new Color(0.12f, 0.12f, 0.14f));
        return Tex(px, s, s);
    }

    private static Sprite DrawTripod()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 48, 90, 160, 12, new Color(0.32f, 0.30f, 0.26f));
        FillRect(px, s, s, 52, 100, 12, 110, new Color(0.28f, 0.26f, 0.22f));
        FillRect(px, s, s, 192, 100, 12, 110, new Color(0.28f, 0.26f, 0.22f));
        FillRect(px, s, s, 122, 100, 12, 90, new Color(0.28f, 0.26f, 0.22f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBench()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 8, 140, 240, 28, new Color(0.62f, 0.42f, 0.24f));
        FillRect(px, s, s, 24, 168, 18, 70, new Color(0.48f, 0.32f, 0.18f));
        FillRect(px, s, s, 214, 168, 18, 70, new Color(0.48f, 0.32f, 0.18f));
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

    private static Sprite DrawBalance()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 88, 36, 44, 170, 8, new Color(0.78f, 0.22f, 0.16f));
        FillRect(px, s, s, 148, 68, 12, 90, new Color(0.95f, 0.85f, 0.35f));
        return Tex(px, s, s);
    }

    private static Sprite DrawBlock()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 48, 88, 160, 90, 8, new Color(0.72f, 0.48f, 0.22f));
        return Tex(px, s, s);
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
        }
        return Tex(px, s, s);
    }

    private static Sprite DrawMeter(Color ring)
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 128, 78, ring);
        FillCircle(px, s, s, 128, 128, 58, new Color(0.95f, 0.96f, 0.98f));
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

    private static Sprite DrawPulley()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 118, 58, new Color(0.18f, 0.62f, 0.38f));
        FillCircle(px, s, s, 128, 118, 22, new Color(0.88f, 0.92f, 0.90f));
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

    private static Sprite DrawConcaveMirror()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 118, 200, 20, 40, new Color(0.35f, 0.32f, 0.28f));
        FillRoundRect(px, s, s, 88, 50, 80, 150, 36, new Color(0.72f, 0.80f, 0.90f));
        return Tex(px, s, s);
    }

    private static Sprite DrawLens()
    {
        int s = 256;
        var px = Clear(s, s);
        FillEllipse(px, s, s, 128, 120, 40, 78, new Color(0.55f, 0.82f, 0.92f, 0.9f));
        return Tex(px, s, s);
    }

    private static Sprite DrawPrism()
    {
        int s = 256;
        var px = Clear(s, s);
        for (int y = 60; y < 200; y++)
        {
            int half = (int)((y - 60) * 0.55f);
            FillRect(px, s, s, 128 - half, y, half * 2, 2, new Color(0.70f, 0.88f, 0.95f));
        }
        return Tex(px, s, s);
    }

    private static Sprite DrawRuler()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 16, 108, 224, 40, 6, new Color(0.92f, 0.86f, 0.62f));
        return Tex(px, s, s);
    }

    private static Sprite DrawScreen()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 70, 36, 116, 164, 6, new Color(0.96f, 0.96f, 0.93f));
        return Tex(px, s, s);
    }

    private static Sprite DrawSlab()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRoundRect(px, s, s, 48, 88, 160, 80, 8, new Color(0.70f, 0.88f, 0.95f, 0.85f));
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
