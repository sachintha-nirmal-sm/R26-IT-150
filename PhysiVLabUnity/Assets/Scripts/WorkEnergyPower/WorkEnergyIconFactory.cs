using System.Collections.Generic;
using UnityEngine;

public static class WorkEnergyIconFactory
{
    private static readonly Dictionary<WorkEnergyEquipmentType, Sprite> Cache = new Dictionary<WorkEnergyEquipmentType, Sprite>();
    private static Sprite whiteSprite;
    private static Sprite ballSprite;
    private static Sprite claySprite;

    private static readonly Dictionary<WorkEnergyEquipmentType, string[]> ResourceNames = new Dictionary<WorkEnergyEquipmentType, string[]>
    {
        { WorkEnergyEquipmentType.Ruler, new[] { "ruler" } },
        { WorkEnergyEquipmentType.DepthRuler, new[] { "ruler" } },
        { WorkEnergyEquipmentType.Balance, new[] { "balance", "newton_balance" } },
        { WorkEnergyEquipmentType.NewtonSpringBalance, new[] { "newton_balance", "balance" } },
        { WorkEnergyEquipmentType.Beaker, new[] { "beaker" } },
        { WorkEnergyEquipmentType.MeasuringCylinder, new[] { "measuring_cylinder" } },
        { WorkEnergyEquipmentType.Thermometer, new[] { "thermometer" } },
        { WorkEnergyEquipmentType.Ammeter, new[] { "ammeter" } },
        { WorkEnergyEquipmentType.Voltmeter, new[] { "voltmeter" } },
        { WorkEnergyEquipmentType.Magnet, new[] { "magnet" } },
        { WorkEnergyEquipmentType.Stopwatch, new[] { "stopwatch" } },
        { WorkEnergyEquipmentType.BunsenBurner, new[] { "bunsen_burner" } }
    };

    public static void ClearCache()
    {
        Cache.Clear();
        whiteSprite = null;
        ballSprite = null;
        claySprite = null;
    }

    public static Sprite White()
    {
        if (whiteSprite != null) return whiteSprite;
        var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        var px = new Color[64];
        for (int i = 0; i < 64; i++) px[i] = Color.white;
        tex.SetPixels(px);
        tex.Apply();
        whiteSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 100f);
        return whiteSprite;
    }

    public static Sprite GetSprite(WorkEnergyEquipmentType type)
    {
        if (Cache.TryGetValue(type, out Sprite cached) && cached != null) return cached;
        Sprite sprite = LoadPhoto(type) ?? DrawPictorial(type);
        Cache[type] = sprite;
        return sprite;
    }

    public static Sprite BallSprite()
    {
        if (ballSprite != null) return ballSprite;
        ballSprite = DrawWeight();
        return ballSprite;
    }

    public static Sprite ClaySprite()
    {
        if (claySprite != null) return claySprite;
        claySprite = DrawClaySurface();
        return claySprite;
    }

    public static Color GetColor(WorkEnergyEquipmentType type)
    {
        switch (type)
        {
            case WorkEnergyEquipmentType.Clay: return new Color(0.78f, 0.52f, 0.32f);
            case WorkEnergyEquipmentType.HeavyWeight: return new Color(0.42f, 0.46f, 0.55f);
            case WorkEnergyEquipmentType.ReleaseStand: return new Color(0.55f, 0.58f, 0.62f);
            case WorkEnergyEquipmentType.ReleaseMechanism: return new Color(0.28f, 0.52f, 0.78f);
            case WorkEnergyEquipmentType.Ruler: return new Color(0.95f, 0.85f, 0.45f);
            case WorkEnergyEquipmentType.DepthRuler: return new Color(0.9f, 0.75f, 0.35f);
            case WorkEnergyEquipmentType.ClayTray: return new Color(0.55f, 0.4f, 0.28f);
            case WorkEnergyEquipmentType.Balance: return new Color(0.7f, 0.72f, 0.78f);
            default: return new Color(0.82f, 0.86f, 0.92f);
        }
    }

    private static Sprite LoadPhoto(WorkEnergyEquipmentType type)
    {
        if (!ResourceNames.TryGetValue(type, out string[] names)) return null;
        foreach (string name in names)
        {
            var tex = Resources.Load<Texture2D>("WorkEnergyEquipment/" + name);
            if (tex == null) continue;
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }
        return null;
    }

    private static Sprite DrawPictorial(WorkEnergyEquipmentType type)
    {
        switch (type)
        {
            case WorkEnergyEquipmentType.Clay: return DrawClayLump();
            case WorkEnergyEquipmentType.HeavyWeight: return DrawWeight();
            case WorkEnergyEquipmentType.ReleaseStand: return DrawStand();
            case WorkEnergyEquipmentType.ReleaseMechanism: return DrawClamp();
            case WorkEnergyEquipmentType.ClayTray: return DrawTray();
            case WorkEnergyEquipmentType.Balance: return DrawBalance();
            case WorkEnergyEquipmentType.Ruler:
            case WorkEnergyEquipmentType.DepthRuler: return DrawRuler();
            case WorkEnergyEquipmentType.Beaker: return DrawBeaker();
            default: return DrawCard(GetColor(type));
        }
    }

    private static Sprite DrawWeight()
    {
        int s = 256;
        var px = Clear(s, s);
        FillCircle(px, s, s, 128, 132, 86, new Color(0.38f, 0.42f, 0.50f));
        FillCircle(px, s, s, 118, 148, 28, new Color(0.82f, 0.86f, 0.92f, 0.55f));
        FillRect(px, s, s, 118, 210, 20, 28, new Color(0.28f, 0.3f, 0.34f));
        return Tex(px, s, s);
    }

    private static Sprite DrawClayLump()
    {
        int s = 256;
        var px = Clear(s, s);
        Color clay = new Color(0.76f, 0.50f, 0.30f);
        FillEllipse(px, s, s, 128, 110, 96, 58, clay);
        FillEllipse(px, s, s, 128, 92, 88, 40, clay * 1.08f);
        FillEllipse(px, s, s, 90, 108, 30, 18, clay * 0.85f);
        FillEllipse(px, s, s, 160, 118, 26, 16, clay * 0.9f);
        return Tex(px, s, s);
    }

    private static Sprite DrawClaySurface()
    {
        int w = 220, h = 80;
        var px = Clear(w, h);
        Color clay = new Color(0.74f, 0.50f, 0.30f);
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float n = ((x * 17 + y * 11) % 23) / 23f * 0.12f;
            bool rim = y < 8 || y > h - 10 || x < 6 || x > w - 7;
            px[y * w + x] = rim ? clay * 0.72f : clay + new Color(n, n * 0.4f, 0f, 0f);
        }
        return Tex(px, w, h);
    }

    private static Sprite DrawStand()
    {
        int s = 256;
        var px = Clear(s, s);
        Color metal = new Color(0.52f, 0.55f, 0.60f);
        FillRect(px, s, s, 48, 28, 160, 22, metal * 0.75f);
        FillRect(px, s, s, 118, 40, 22, 190, metal);
        FillRect(px, s, s, 118, 168, 78, 14, metal * 1.1f);
        FillRect(px, s, s, 186, 150, 16, 50, new Color(0.28f, 0.48f, 0.72f));
        return Tex(px, s, s);
    }

    private static Sprite DrawClamp()
    {
        int s = 256;
        var px = Clear(s, s);
        Color blue = new Color(0.22f, 0.48f, 0.78f);
        FillRect(px, s, s, 70, 118, 116, 22, blue);
        FillRect(px, s, s, 70, 90, 22, 78, blue);
        FillRect(px, s, s, 164, 90, 22, 78, blue);
        FillCircle(px, s, s, 128, 168, 18, new Color(0.85f, 0.55f, 0.18f));
        return Tex(px, s, s);
    }

    private static Sprite DrawTray()
    {
        int s = 256;
        var px = Clear(s, s);
        Color wood = new Color(0.55f, 0.38f, 0.22f);
        FillRect(px, s, s, 36, 70, 184, 110, wood);
        FillRect(px, s, s, 50, 86, 156, 78, new Color(0.76f, 0.52f, 0.32f));
        FillRect(px, s, s, 36, 70, 184, 14, wood * 0.7f);
        return Tex(px, s, s);
    }

    private static Sprite DrawBalance()
    {
        int s = 256;
        var px = Clear(s, s);
        Color metal = new Color(0.62f, 0.64f, 0.70f);
        FillRect(px, s, s, 70, 40, 116, 18, metal * 0.7f);
        FillRect(px, s, s, 120, 50, 16, 90, metal);
        FillRect(px, s, s, 48, 132, 160, 12, metal);
        FillEllipse(px, s, s, 68, 108, 32, 14, metal * 1.1f);
        FillEllipse(px, s, s, 188, 108, 32, 14, metal * 1.1f);
        return Tex(px, s, s);
    }

    private static Sprite DrawRuler()
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 40, 96, 176, 64, new Color(0.96f, 0.86f, 0.42f));
        for (int i = 0; i < 9; i++)
        {
            int x = 52 + i * 18;
            FillRect(px, s, s, x, 96, 3, i % 2 == 0 ? 28 : 16, new Color(0.2f, 0.2f, 0.18f));
        }
        return Tex(px, s, s);
    }

    private static Sprite DrawBeaker()
    {
        int s = 256;
        var px = Clear(s, s);
        Color glass = new Color(0.72f, 0.88f, 1f, 0.9f);
        FillRect(px, s, s, 78, 48, 100, 150, glass);
        FillRect(px, s, s, 70, 188, 116, 18, glass * 0.85f);
        FillRect(px, s, s, 84, 58, 88, 70, new Color(0.55f, 0.78f, 0.95f, 0.55f));
        return Tex(px, s, s);
    }

    private static Sprite DrawCard(Color fill)
    {
        int s = 256;
        var px = Clear(s, s);
        FillRect(px, s, s, 28, 28, 200, 200, fill);
        FillRect(px, s, s, 28, 28, 200, 10, fill * 0.7f);
        FillRect(px, s, s, 28, 218, 200, 10, fill * 0.7f);
        return Tex(px, s, s);
    }

    private static Color[] Clear(int w, int h)
    {
        var px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
        return px;
    }

    private static void FillRect(Color[] px, int w, int h, int x, int y, int rw, int rh, Color c)
    {
        for (int yy = y; yy < y + rh; yy++)
        for (int xx = x; xx < x + rw; xx++)
        {
            if (xx < 0 || yy < 0 || xx >= w || yy >= h) continue;
            px[yy * w + xx] = Blend(px[yy * w + xx], c);
        }
    }

    private static void FillCircle(Color[] px, int w, int h, int cx, int cy, int r, Color c)
    {
        int r2 = r * r;
        for (int yy = cy - r; yy <= cy + r; yy++)
        for (int xx = cx - r; xx <= cx + r; xx++)
        {
            if (xx < 0 || yy < 0 || xx >= w || yy >= h) continue;
            int dx = xx - cx, dy = yy - cy;
            if (dx * dx + dy * dy <= r2) px[yy * w + xx] = Blend(px[yy * w + xx], c);
        }
    }

    private static void FillEllipse(Color[] px, int w, int h, int cx, int cy, int rx, int ry, Color c)
    {
        for (int yy = cy - ry; yy <= cy + ry; yy++)
        for (int xx = cx - rx; xx <= cx + rx; xx++)
        {
            if (xx < 0 || yy < 0 || xx >= w || yy >= h) continue;
            float nx = (xx - cx) / (float)rx;
            float ny = (yy - cy) / (float)ry;
            if (nx * nx + ny * ny <= 1f) px[yy * w + xx] = Blend(px[yy * w + xx], c);
        }
    }

    private static Color Blend(Color a, Color b)
    {
        if (b.a >= 0.99f || a.a < 0.01f) return b;
        float o = b.a + a.a * (1f - b.a);
        if (o < 0.001f) return Color.clear;
        return new Color(
            (b.r * b.a + a.r * a.a * (1f - b.a)) / o,
            (b.g * b.a + a.g * a.a * (1f - b.a)) / o,
            (b.b * b.a + a.b * a.a * (1f - b.a)) / o,
            o);
    }

    private static Sprite Tex(Color[] px, int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.SetPixels(px);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }
}
