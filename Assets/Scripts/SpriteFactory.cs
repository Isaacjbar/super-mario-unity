using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates every sprite in code (pixel art), so the project needs no external art assets.
/// </summary>
public static class SpriteFactory
{
    const int Size = 16;
    const float Ppu = 16f;

    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    static readonly Dictionary<char, Color> Palette = new Dictionary<char, Color>
    {
        { 'r', new Color32(220, 40, 30, 255) },   // red
        { 's', new Color32(250, 190, 130, 255) }, // skin
        { 'b', new Color32(120, 60, 20, 255) },   // brown
        { 'u', new Color32(40, 80, 220, 255) },   // blue
        { 'y', new Color32(250, 220, 40, 255) },  // yellow
        { 'k', new Color32(20, 20, 20, 255) },    // black
        { 'w', new Color32(255, 255, 255, 255) }, // white
        { 'g', new Color32(60, 170, 50, 255) },   // green
        { 'o', new Color32(230, 130, 40, 255) },  // orange
        { 'd', new Color32(80, 40, 15, 255) },    // dark brown
    };

    // ---------- Public sprites ----------

    public static Sprite PlayerIdle => Get("p_idle", () => Compose(LegsStand));
    public static Sprite PlayerWalk1 => Get("p_w1", () => Compose(LegsWalk1));
    public static Sprite PlayerWalk2 => Get("p_w2", () => Compose(LegsWalk2));
    public static Sprite PlayerJump => Get("p_jump", () => Compose(LegsJump));

    public static Sprite Goomba => Get("goomba", () => FromRows(GoombaRows));

    public static Sprite Grass => Get("grass", () => Procedural(Size, Size, (x, y) =>
    {
        if (y >= 13) return new Color32(70, 190, 50, 255);
        if (y == 12) return new Color32(40, 130, 30, 255);
        return Dirt(x, y);
    }));

    public static Sprite DirtTile => Get("dirt", () => Procedural(Size, Size, (x, y) => Dirt(x, y)));

    public static Sprite Brick => Get("brick", () => Procedural(Size, Size, (x, y) =>
    {
        bool hLine = y % 4 == 0;
        int offset = ((y / 4) % 2 == 0) ? 0 : 4;
        bool vLine = (x + offset) % 8 == 0;
        if (hLine || vLine) return new Color32(90, 35, 10, 255);
        return new Color32(200, 90, 40, 255);
    }));

    public static Sprite Question => Get("question", () => FromRows(new[]
    {
        "kyyyyyyyyyyyyyyk",
        "yoyyyyyyyyyyyyoy",
        "yyyyykkkkkyyyyyy",
        "yyyykkyyyykkyyyy",
        "yyyykkyyyykkyyyy",
        "yyyyyyyyyykkyyyy",
        "yyyyyyyykkyyyyyy",
        "yyyyyyykkyyyyyyy",
        "yyyyyyykkyyyyyyy",
        "yyyyyyyyyyyyyyyy",
        "yyyyyyykkyyyyyyy",
        "yyyyyyykkyyyyyyy",
        "yoyyyyyyyyyyyyoy",
        "yyyyyyyyyyyyyyyy",
        "oooooooooooooooo",
        "kooooooooooooook",
    }));

    public static Sprite UsedBlock => Get("used", () => Procedural(Size, Size, (x, y) =>
    {
        bool border = x == 0 || y == 0 || x == 15 || y == 15;
        if (border) return new Color32(50, 30, 15, 255);
        return new Color32(130, 100, 70, 255);
    }));

    public static Sprite Stone => Get("stone", () => Procedural(Size, Size, (x, y) =>
    {
        bool border = x == 0 || y == 0;
        bool shade = x == 15 || y == 15;
        if (border) return new Color32(230, 200, 170, 255);
        if (shade) return new Color32(90, 55, 30, 255);
        return new Color32(180, 120, 70, 255);
    }));

    public static Sprite Pipe(bool left, bool top) => Get("pipe" + left + top, () => Procedural(Size, Size, (x, y) =>
    {
        bool outline = (left && x == 0) || (!left && x == 15) || (top && y == 15);
        if (outline) return new Color32(10, 60, 10, 255);
        bool highlight = left ? (x >= 3 && x <= 5) : (x >= 9 && x <= 10);
        if (highlight) return new Color32(130, 230, 110, 255);
        return new Color32(40, 170, 40, 255);
    }));

    public static Sprite CoinSprite => Get("coin", () => Procedural(Size, Size, (x, y) =>
    {
        float dx = (x - 7.5f) / 5f;
        float dy = (y - 7.5f) / 7f;
        float d = dx * dx + dy * dy;
        if (d > 1f) return Color.clear;
        if (d > 0.7f) return new Color32(200, 140, 10, 255);
        if (Mathf.Abs(x - 7.5f) < 1.2f && y > 4 && y < 11) return new Color32(255, 240, 150, 255);
        return new Color32(250, 210, 30, 255);
    }));

    public static Sprite Pole => Get("pole", () => Procedural(Size, Size, (x, y) =>
        (x == 7 || x == 8) ? (Color)new Color32(230, 230, 230, 255) : Color.clear));

    public static Sprite PoleFlag => Get("poleflag", () => Procedural(Size, Size, (x, y) =>
    {
        if (x == 7 || x == 8) return new Color32(230, 230, 230, 255);
        // triangular pennant pointing left
        if (x >= 1 && x <= 6 && Mathf.Abs(y - 11) <= 5 && x >= 1 + Mathf.Abs(y - 11))
            return new Color32(60, 190, 70, 255);
        return Color.clear;
    }));

    public static Sprite Cloud => Get("cloud", () => Procedural(48, 24, (x, y) =>
    {
        bool In(float cx, float cy, float rx, float ry)
        {
            float dx = (x - cx) / rx, dy = (y - cy) / ry;
            return dx * dx + dy * dy <= 1f;
        }
        if (In(12, 9, 10, 7) || In(24, 12, 12, 9) || In(36, 9, 10, 7) || (y < 9 && y > 2 && x > 6 && x < 42))
            return new Color32(255, 255, 255, 255);
        return Color.clear;
    }));

    // ---------- Internals ----------

    static Color32 Dirt(int x, int y)
    {
        bool hLine = y % 8 == 0;
        int offset = ((y / 8) % 2 == 0) ? 0 : 8;
        bool vLine = (x + offset) % 16 == 0;
        if (hLine || vLine) return new Color32(100, 50, 20, 255);
        return new Color32(170, 90, 40, 255);
    }

    static Sprite Get(string key, Func<Sprite> build)
    {
        if (!cache.TryGetValue(key, out var s) || s == null)
        {
            s = build();
            s.name = key;
            cache[key] = s;
        }
        return s;
    }

    static Sprite Make(Texture2D tex)
    {
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), Ppu);
    }

    static Sprite Procedural(int w, int h, Func<int, int, Color> f)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                px[y * w + x] = f(x, y);
        tex.SetPixels32(px);
        return Make(tex);
    }

    static Sprite FromRows(string[] rows, Dictionary<char, Color> pal = null)
    {
        pal = pal ?? Palette;
        var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        var px = new Color32[Size * Size];
        for (int i = 0; i < Size; i++)
        {
            string row = i < rows.Length ? rows[i] : "";
            row = row.PadRight(Size, '.').Substring(0, Size);
            for (int x = 0; x < Size; x++)
            {
                int y = Size - 1 - i;
                px[y * Size + x] = pal.TryGetValue(row[x], out var c) ? (Color32)c : new Color32(0, 0, 0, 0);
            }
        }
        tex.SetPixels32(px);
        return Make(tex);
    }

    static Sprite Compose(string[] legs)
    {
        var rows = new List<string>(PlayerTop);
        rows.AddRange(legs);
        return FromRows(rows.ToArray());
    }

    // ---------- Pixel art ----------

    static readonly string[] PlayerTop =
    {
        "....rrrrrr......",
        "...rrrrrrrrr....",
        "...bbbssbs......",
        "..bsbsssbsss....",
        "..bsbbsssbsss...",
        "..bbssssbbbb....",
        "....ssssssss....",
        "...rruuruur.....",
        "..rrruuuuurr....",
        ".rrrruyuuyurrr..",
        ".ssruuuuuuurss..",
        ".sssuuuuuuusss..",
    };

    static readonly string[] LegsStand =
    {
        "..uuuu..uuuu....",
        ".bbbb....bbbb...",
        "bbbbb....bbbbb..",
        "................",
    };

    static readonly string[] LegsWalk1 =
    {
        "..uuu...uuuu....",
        ".bbbb...bbb.....",
        "bbbbb..bbbb.....",
        "................",
    };

    static readonly string[] LegsWalk2 =
    {
        "...uuuuuu.......",
        "..bbbbbbb.......",
        "..bbbbbbbb......",
        "................",
    };

    static readonly string[] LegsJump =
    {
        "..uuu....uuu....",
        ".bbbb.....bbbb..",
        "bbbb.......bbbb.",
        "................",
    };

    static readonly string[] GoombaRows =
    {
        "................",
        "......bbbb......",
        "....bbbbbbbb....",
        "...bbbbbbbbbb...",
        "..bbwwbbbbwwbb..",
        "..bwwkbbbbkwwb..",
        ".bbbwkbbbbkwbbb.",
        ".bbbbbbbbbbbbbb.",
        ".bbbbbbbbbbbbbb.",
        "..bbbbbbbbbbbb..",
        "....ssssssss....",
        "...ssssssssss...",
        "...ssssssssss...",
        "..kkkk....kkkk..",
        ".kkkkkk..kkkkkk.",
        "................",
    };
}
