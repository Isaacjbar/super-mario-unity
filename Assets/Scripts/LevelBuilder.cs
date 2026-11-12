using UnityEngine;

/// <summary>
/// Builds the whole level procedurally from a tile grid.
/// Tile (x, y) is centered at world position (x, y). Ground top surface is at y = 1.5.
/// </summary>
public static class LevelBuilder
{
    public const int Width = 110;
    public const int Height = 12;

    // Tile chars:
    // G grass  D dirt  B brick  ? question  S stone  C coin  F pole  H pole+flag
    // L/R pipe top (left/right)   l/r pipe body (left/right)
    static char[,] grid;

    public static PlayerController Build(out GameObject world)
    {
        world = new GameObject("World");
        grid = new char[Width, Height];
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                grid[x, y] = '.';

        Layout();

        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (grid[x, y] != '.')
                    SpawnTile(world.transform, grid[x, y], x, y);

        BuildWalls(world.transform);
        BuildClouds(world.transform);
        SpawnEnemies(world.transform);

        return SpawnPlayer(world.transform);
    }

    // ---------- Level design ----------

    static void Layout()
    {
        // Ground with 3-tile pits
        int[][] segments = { new[] { 0, 27 }, new[] { 31, 52 }, new[] { 56, 74 }, new[] { 78, 109 } };
        foreach (var s in segments)
            for (int x = s[0]; x <= s[1]; x++)
            {
                Put(x, 0, 'D');
                Put(x, 1, 'G');
            }

        // First blocks
        PutRow(8, 4, "B?B?B");
        PutRow(15, 3, "CCC");
        PutRow(11, 8, "CCC");

        Pipe(20, 2);
        Pipe(38, 3);

        // Coin arc over the first pit
        Put(28, 4, 'C');
        Put(29, 5, 'C');
        Put(30, 4, 'C');

        // Floating platform
        PutRow(44, 5, "BB?BB");
        PutRow(44, 6, "CCCCC");

        // Mid-level
        PutRow(60, 4, "B?B");
        Pipe(66, 2);
        PutRow(70, 5, "CCC");

        // Coin arc over the second pit
        Put(75, 4, 'C');
        Put(76, 5, 'C');
        Put(77, 4, 'C');

        // Stairs up, valley, stairs down
        for (int i = 0; i < 4; i++) Column(84 + i, i + 1);
        for (int i = 0; i < 4; i++) Column(89 + i, 4 - i);

        // Final stairs
        for (int i = 0; i < 4; i++) Column(97 + i, i + 1);
        PutRow(93, 4, "C");

        // Goal flag pole
        Put(106, 2, 'S');
        for (int y = 3; y <= 8; y++) Put(106, y, 'F');
        Put(106, 9, 'H');
    }

    static void Put(int x, int y, char c)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height) grid[x, y] = c;
    }

    static void PutRow(int x, int y, string tiles)
    {
        for (int i = 0; i < tiles.Length; i++) Put(x + i, y, tiles[i]);
    }

    /// <summary>Stone column of the given height standing on the ground.</summary>
    static void Column(int x, int height)
    {
        for (int y = 2; y < 2 + height; y++) Put(x, y, 'S');
    }

    /// <summary>Two-tile wide pipe, height in tiles above the ground.</summary>
    static void Pipe(int x, int height)
    {
        for (int y = 2; y < 2 + height; y++)
        {
            bool top = y == 1 + height;
            Put(x, y, top ? 'L' : 'l');
            Put(x + 1, y, top ? 'R' : 'r');
        }
    }

    // ---------- Spawning ----------

    static void SpawnTile(Transform parent, char c, int x, int y)
    {
        Sprite sprite;
        bool solid = true;
        int order = 0;

        switch (c)
        {
            case 'G': sprite = SpriteFactory.Grass; break;
            case 'D': sprite = SpriteFactory.DirtTile; break;
            case 'B': sprite = SpriteFactory.Brick; break;
            case '?': sprite = SpriteFactory.Question; break;
            case 'S': sprite = SpriteFactory.Stone; break;
            case 'L': sprite = SpriteFactory.Pipe(true, true); break;
            case 'R': sprite = SpriteFactory.Pipe(false, true); break;
            case 'l': sprite = SpriteFactory.Pipe(true, false); break;
            case 'r': sprite = SpriteFactory.Pipe(false, false); break;
            case 'C': sprite = SpriteFactory.CoinSprite; solid = false; order = 5; break;
            case 'F': sprite = SpriteFactory.Pole; solid = false; break;
            case 'H': sprite = SpriteFactory.PoleFlag; solid = false; break;
            default: return;
        }

        var go = new GameObject("Tile_" + c + "_" + x + "_" + y);
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, y, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;

        if (solid)
        {
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            if (c == 'B') go.AddComponent<Block>().Kind = BlockKind.Brick;
            if (c == '?') go.AddComponent<Block>().Kind = BlockKind.Question;
        }
        else if (c == 'C')
        {
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f;
            col.isTrigger = true;
            go.AddComponent<Coin>();
        }
        else
        {
            // Flag pole / flag: trigger that finishes the level
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 1f);
            col.isTrigger = true;
            go.AddComponent<Goal>();
        }
    }

    static void BuildWalls(Transform parent)
    {
        // Invisible walls at both ends of the level
        MakeWall(parent, -1f);
        MakeWall(parent, Width);
    }

    static void MakeWall(Transform parent, float x)
    {
        var go = new GameObject("Wall");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x, 6f, 0f);
        go.AddComponent<BoxCollider2D>().size = new Vector2(1f, 30f);
    }

    static void BuildClouds(Transform parent)
    {
        for (int i = 0; i < 9; i++)
        {
            var go = new GameObject("Cloud");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(6f + i * 13f, 9f + (i % 3) * 0.8f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Cloud;
            sr.sortingOrder = -10;
        }
    }

    static void SpawnEnemies(Transform parent)
    {
        int[] xs = { 15, 24, 34, 36, 46, 50, 60, 64, 70, 81, 90, 94 };
        foreach (int x in xs)
        {
            var go = new GameObject("Goomba");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(x, 2f, 0f);
            go.AddComponent<EnemyPatrol>();
        }
    }

    static PlayerController SpawnPlayer(Transform parent)
    {
        var go = new GameObject("Player");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(2f, 2.1f, 0f);
        return go.AddComponent<PlayerController>();
    }
}
