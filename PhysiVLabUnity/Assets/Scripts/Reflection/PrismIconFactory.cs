using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedural apparatus icons for the Prism practical (no external art required).
/// </summary>
public static class PrismIconFactory
{
    static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    public static void ClearCache() => Cache.Clear();

    public static Sprite GetSprite(string id)
    {
        if (Cache.TryGetValue(id, out var cached) && cached != null)
            return cached;

        var sprite = CreateSprite(id);
        Cache[id] = sprite;
        return sprite;
    }

    static Sprite CreateSprite(string id)
    {
        const int s = 128;
        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color[s * s];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color(0, 0, 0, 0);

        switch (id)
        {
            case "prism": DrawPrism(pixels, s); break;
            case "screen": DrawScreen(pixels, s); break;
            case "cardboard": DrawCardboardSlit(pixels, s); break;
            case "mirror": DrawMirror(pixels, s); break;
            case "torch": DrawTorch(pixels, s); break;
            case "lens": DrawLens(pixels, s); break;
            case "spring": DrawSpringBalance(pixels, s); break;
            case "beaker": DrawBeaker(pixels, s); break;
            case "tape": DrawTape(pixels, s); break;
            default: DrawFallback(pixels, s, Color.gray); break;
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.hideFlags = HideFlags.DontSave;
        var sp = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100f);
        sp.hideFlags = HideFlags.DontSave;
        return sp;
    }

    static void Set(Color[] p, int s, int x, int y, Color c)
    {
        if (x < 0 || y < 0 || x >= s || y >= s) return;
        int i = y * s + x;
        // alpha blend over existing
        Color d = p[i];
        float a = c.a + d.a * (1f - c.a);
        if (a < 0.001f) { p[i] = default; return; }
        p[i] = new Color(
            (c.r * c.a + d.r * d.a * (1f - c.a)) / a,
            (c.g * c.a + d.g * d.a * (1f - c.a)) / a,
            (c.b * c.a + d.b * d.a * (1f - c.a)) / a,
            a);
    }

    static void FillRect(Color[] p, int s, int x0, int y0, int w, int h, Color c)
    {
        for (int y = y0; y < y0 + h; y++)
        for (int x = x0; x < x0 + w; x++)
            Set(p, s, x, y, c);
    }

    static void FillCircle(Color[] p, int s, int cx, int cy, int r, Color c)
    {
        int r2 = r * r;
        for (int y = cy - r; y <= cy + r; y++)
        for (int x = cx - r; x <= cx + r; x++)
            if ((x - cx) * (x - cx) + (y - cy) * (y - cy) <= r2)
                Set(p, s, x, y, c);
    }

    static void DrawPrism(Color[] p, int s)
    {
        // Equilateral-ish glass triangle
        Color glass = new Color(0.55f, 0.85f, 1f, 0.95f);
        Color edge = new Color(0.2f, 0.45f, 0.7f, 1f);
        int topX = s / 2, topY = s - 18;
        int leftX = 18, leftY = 22;
        int rightX = s - 18, rightY = 22;

        for (int y = 0; y < s; y++)
        for (int x = 0; x < s; x++)
        {
            if (PointInTriangle(x, y, topX, topY, leftX, leftY, rightX, rightY))
                Set(p, s, x, y, glass);
        }
        // simple edge accents
        DrawLine(p, s, topX, topY, leftX, leftY, edge, 3);
        DrawLine(p, s, topX, topY, rightX, rightY, edge, 3);
        DrawLine(p, s, leftX, leftY, rightX, rightY, edge, 3);
    }

    static void DrawScreen(Color[] p, int s)
    {
        FillRect(p, s, 40, 16, 16, 96, new Color(0.35f, 0.35f, 0.38f, 1f)); // stand
        FillRect(p, s, 28, 20, 72, 88, new Color(0.96f, 0.96f, 0.98f, 1f)); // screen
        FillRect(p, s, 28, 20, 72, 6, new Color(0.7f, 0.7f, 0.75f, 1f));
        // tiny spectrum hint
        Color[] bands = { Color.red, new Color(1f, 0.5f, 0f), Color.yellow, Color.green, Color.blue, new Color(0.4f, 0f, 0.6f) };
        for (int i = 0; i < bands.Length; i++)
            FillRect(p, s, 36 + i * 9, 50, 8, 28, bands[i]);
    }

    static void DrawCardboardSlit(Color[] p, int s)
    {
        FillRect(p, s, 24, 20, 80, 88, new Color(0.62f, 0.38f, 0.18f, 1f));
        FillRect(p, s, 60, 28, 8, 72, new Color(0.08f, 0.08f, 0.1f, 1f)); // slit
        FillRect(p, s, 24, 20, 80, 8, new Color(0.45f, 0.28f, 0.12f, 1f));
    }

    static void DrawMirror(Color[] p, int s)
    {
        FillRect(p, s, 30, 22, 68, 84, new Color(0.55f, 0.58f, 0.62f, 1f)); // frame
        FillRect(p, s, 36, 28, 56, 72, new Color(0.78f, 0.88f, 0.95f, 1f)); // glass
        // shine
        for (int i = 0; i < 40; i++)
            FillRect(p, s, 42 + i / 2, 70 - i, 3, 10, new Color(1f, 1f, 1f, 0.35f));
    }

    static void DrawTorch(Color[] p, int s)
    {
        FillRect(p, s, 28, 50, 55, 28, new Color(0.25f, 0.25f, 0.28f, 1f)); // body
        FillRect(p, s, 78, 44, 18, 40, new Color(0.45f, 0.45f, 0.5f, 1f)); // head
        FillCircle(p, s, 100, 64, 14, new Color(1f, 0.92f, 0.35f, 0.95f)); // glow
        FillCircle(p, s, 100, 64, 7, new Color(1f, 1f, 0.85f, 1f));
    }

    static void DrawLens(Color[] p, int s)
    {
        FillCircle(p, s, 64, 64, 40, new Color(0.55f, 0.78f, 0.95f, 0.55f));
        FillCircle(p, s, 64, 64, 28, new Color(0.7f, 0.88f, 1f, 0.45f));
        FillRect(p, s, 20, 60, 88, 8, new Color(0.4f, 0.45f, 0.5f, 1f)); // holder
    }

    static void DrawSpringBalance(Color[] p, int s)
    {
        FillRect(p, s, 52, 20, 24, 70, new Color(0.75f, 0.75f, 0.78f, 1f));
        FillRect(p, s, 56, 28, 16, 50, new Color(0.9f, 0.9f, 0.92f, 1f));
        // spring zig
        for (int i = 0; i < 6; i++)
            FillRect(p, s, 48 + (i % 2) * 16, 30 + i * 8, 14, 4, new Color(0.55f, 0.55f, 0.6f, 1f));
        FillCircle(p, s, 64, 100, 10, new Color(0.85f, 0.55f, 0.2f, 1f)); // hook weight
    }

    static void DrawBeaker(Color[] p, int s)
    {
        FillRect(p, s, 36, 28, 56, 72, new Color(0.65f, 0.85f, 0.95f, 0.55f));
        FillRect(p, s, 40, 32, 48, 40, new Color(0.35f, 0.65f, 0.95f, 0.55f)); // liquid
        FillRect(p, s, 32, 90, 64, 8, new Color(0.7f, 0.85f, 0.95f, 0.8f)); // rim
    }

    static void DrawTape(Color[] p, int s)
    {
        FillCircle(p, s, 64, 64, 38, new Color(0.95f, 0.8f, 0.25f, 1f));
        FillCircle(p, s, 64, 64, 18, new Color(0.3f, 0.3f, 0.32f, 1f));
        FillRect(p, s, 64, 60, 50, 10, new Color(0.95f, 0.85f, 0.4f, 1f));
    }

    static void DrawFallback(Color[] p, int s, Color c) => FillRect(p, s, 24, 24, 80, 80, c);

    static bool PointInTriangle(int px, int py, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        float d1 = Sign(px, py, x1, y1, x2, y2);
        float d2 = Sign(px, py, x2, y2, x3, y3);
        float d3 = Sign(px, py, x3, y3, x1, y1);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    static float Sign(int px, int py, int x1, int y1, int x2, int y2)
        => (px - x2) * (y1 - y2) - (x1 - x2) * (py - y2);

    static void DrawLine(Color[] p, int s, int x0, int y0, int x1, int y1, Color c, int thickness)
    {
        int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
        if (steps < 1) steps = 1;
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));
            for (int oy = -thickness / 2; oy <= thickness / 2; oy++)
            for (int ox = -thickness / 2; ox <= thickness / 2; ox++)
                Set(p, s, x + ox, y + oy, c);
        }
    }
}
