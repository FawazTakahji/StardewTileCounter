using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace TileCounter;

public static class Helpers
{
    private static readonly Vector2[] Directions4 = {
        new(0, 1),  // up
        new(0, -1), // down
        new(1, 0),  // right
        new(-1, 0)  // left
    };

    private static readonly Vector2[] Directions8 = {
        new(0, 1),   // up
        new(0, -1),  // down
        new(1, 0),   // right
        new(-1, 0),  // left
        new(1, 1),   // up right
        new(1, -1),  // down right
        new(-1, 1),  // up left
        new(-1, -1)  // down left
    };

    public static void RegisterConfig(IGenericModConfigMenuApi api, IManifest manifest, IModHelper modHelper)
    {
        api.Register(
            manifest,
            () => ModConfig.Instance = new ModConfig(),
            () => modHelper.WriteConfig(ModConfig.Instance));

        // General
        api.AddSectionTitle(manifest, I18n.Settings_Title_General);

        api.AddBoolOption(
            manifest,
            () => ModConfig.Instance.SimpleBorder,
            value => ModConfig.Instance.SimpleBorder = value,
            I18n.Settings_Option_SimpleBorder,
            I18n.Settings_Option_SimpleBorder_Description);

        api.AddBoolOption(
            manifest,
            () => ModConfig.Instance.EightWayScan,
            value => ModConfig.Instance.EightWayScan = value,
            I18n.Settings_Option_8wayScan,
            I18n.Settings_Option_8wayScan_Description);

        // HUD
        api.AddSectionTitle(manifest, I18n.Settings_Title_Hud);

        api.AddBoolOption(
            manifest,
            () => ModConfig.Instance.CountSelectedTiles,
            value => ModConfig.Instance.CountSelectedTiles = value,
            I18n.Settings_Option_CountSelectedTiles,
            I18n.Settings_Option_CountSelectedTiles_Description);

        api.AddBoolOption(
            manifest,
            () => ModConfig.Instance.CountHarvestableTiles,
            value => ModConfig.Instance.CountHarvestableTiles = value,
            I18n.Settings_Option_CountHarvestableTiles,
            I18n.Settings_Option_CountHarvestableTiles_Description);

        api.AddBoolOption(
            manifest,
            () => ModConfig.Instance.CountDryTiles,
            value => ModConfig.Instance.CountDryTiles = value,
            I18n.Settings_Option_CountDryTiles,
            I18n.Settings_Option_CountDryTiles_Description);

        api.AddBoolOption(
            manifest,
            () => ModConfig.Instance.CountSeedableTiles,
            value => ModConfig.Instance.CountSeedableTiles = value,
            I18n.Settings_Option_CountSeedableTiles,
            I18n.Settings_Option_CountSeedableTiles_Description);

        api.AddBoolOption(
            manifest,
            () => ModConfig.Instance.CountDiggableTiles,
            value => ModConfig.Instance.CountDiggableTiles = value,
            I18n.Settings_Option_CountDiggableTiles,
            I18n.Settings_Option_CountDiggableTiles_Description);

        // Keybinds
        api.AddSectionTitle(manifest, I18n.Settings_Title_Keybinds);

        api.AddKeybindList(
            manifest,
            () => ModConfig.Instance.ScanLocationKeys,
            keys => ModConfig.Instance.ScanLocationKeys = keys,
            I18n.Settings_Keybinds_ScanLocation,
            I18n.Settings_Keybinds_ScanLocation_Description);

        api.AddKeybindList(
            manifest,
            () => ModConfig.Instance.ScanConnectedKeys,
            keys => ModConfig.Instance.ScanConnectedKeys = keys,
            I18n.Settings_Keybinds_ScanConnected,
            I18n.Settings_Keybinds_ScanConnected_Description);

        api.AddKeybindList(
            manifest,
            () => ModConfig.Instance.SelectionModeKeys,
            keys => ModConfig.Instance.SelectionModeKeys = keys,
            I18n.Settings_Keybinds_ToggleSelectionMode,
            I18n.Settings_Keybinds_ToggleSelectionMode_Description);

        api.AddKeybindList(
            manifest,
            () => ModConfig.Instance.SelectTileKey,
            key => ModConfig.Instance.SelectTileKey = key,
            I18n.Settings_Keybinds_SelectTile,
            I18n.Settings_Keybinds_SelectTile_Description);
    }

    public static Vector2 GetTileInFrontOfPlayer()
    {
        return Game1.player.FacingDirection switch
        {
            0 => new Vector2(Game1.player.Tile.X, Game1.player.Tile.Y - 1), // up
            1 => new Vector2(Game1.player.Tile.X + 1, Game1.player.Tile.Y), // right
            2 => new Vector2(Game1.player.Tile.X, Game1.player.Tile.Y + 1), // down
            3 => new Vector2(Game1.player.Tile.X - 1, Game1.player.Tile.Y), // left
            _ => new Vector2(Game1.player.Tile.X, Game1.player.Tile.Y - 1) // maybe throw instead ?
        };
    }

    private static Vector2 TileToScreenCoordinates(Vector2 tile)
    {
        return new Vector2(tile.X * Game1.tileSize - Game1.viewport.X, tile.Y * Game1.tileSize - Game1.viewport.Y);
    }

    public static void RenderNoTextures(SpriteBatch spriteBatch, Vector2 currentTile, Vector2? selectedTile)
    {
        Vector2 screenTile = TileToScreenCoordinates(currentTile);
        if (currentTile == selectedTile)
        {
            spriteBatch.Draw(
                Game1.staminaRect,
                screenTile,
                new Rectangle(0, 0, 64, 64),
                Color.Red * 0.5f);
        }
        else if (selectedTile == null && currentTile != selectedTile)
        {
            spriteBatch.Draw(
                Game1.staminaRect,
                screenTile,
                new Rectangle(0, 0, 64, 64),
                Color.Green * 0.5f);
        }
        else if (currentTile != selectedTile)
        {
            int minX = (int)Math.Min(currentTile.X, selectedTile.Value.X);
            int maxX = (int)Math.Max(currentTile.X, selectedTile.Value.X);
            int minY = (int)Math.Min(currentTile.Y, selectedTile.Value.Y);
            int maxY = (int)Math.Max(currentTile.Y, selectedTile.Value.Y);

            Vector2 screenTopLeft = TileToScreenCoordinates(new Vector2(minX, minY));
            int width = maxX - minX + 1;
            int height = maxY - minY + 1;

            spriteBatch.Draw(
                Game1.staminaRect,
                new Rectangle((int)screenTopLeft.X, (int)screenTopLeft.Y, width * Game1.tileSize, height * Game1.tileSize),
                new Rectangle(0, 0, Game1.tileSize, Game1.tileSize),
                Color.Green * 0.5f);
        }
    }

    public static void RenderTextures(SpriteBatch spriteBatch, Vector2 currentTile, Vector2? selectedTile)
    {
        if (currentTile == selectedTile)
        {
            spriteBatch.Draw(
                Textures.MainTexture,
                TileToScreenCoordinates(currentTile),
                Textures.RedBox.Complete,
                Color.White);
        }
        else if (selectedTile == null && currentTile != selectedTile)
        {
            spriteBatch.Draw(
                Textures.MainTexture,
                TileToScreenCoordinates(currentTile),
                Textures.GreenBox.Complete,
                Color.White);
        }
        else if (currentTile != selectedTile)
        {
            int minX = (int)Math.Min(currentTile.X, selectedTile.Value.X);
            int maxX = (int)Math.Max(currentTile.X, selectedTile.Value.X);
            int minY = (int)Math.Min(currentTile.Y, selectedTile.Value.Y);
            int maxY = (int)Math.Max(currentTile.Y, selectedTile.Value.Y);

            int tileWidth = maxX - minX + 1;
            int tileHeight = maxY - minY + 1;

            for (int y = 0; y < tileHeight; y++)
            {
                for (int x = 0; x < tileWidth; x++)
                {
                    Vector2 tileWorldPos = new Vector2(minX + x, minY + y);
                    Vector2 tileScreenPos = TileToScreenCoordinates(tileWorldPos);
                    (Rectangle rect, float rot) texture;

                    if (tileWidth == 1)
                    {
                        if (y == 0)
                        {
                            texture = (Textures.GreenBox.ThreeLines.Rect, Textures.GreenBox.ThreeLines.TopRightLeft);
                        }
                        else if (y == tileHeight - 1)
                        {
                            texture = (Textures.GreenBox.ThreeLines.Rect, Textures.GreenBox.ThreeLines.BottomLeftRight);
                        }
                        else
                        {
                            texture = (Textures.GreenBox.TwoLines.Rect, Textures.GreenBox.TwoLines.LeftRight);
                        }
                    }
                    else if (tileHeight == 1)
                    {
                        if (x == 0)
                        {
                            texture = (Textures.GreenBox.ThreeLines.Rect, Textures.GreenBox.ThreeLines.TopBottomLeft);
                        }
                        else if (x == tileWidth - 1)
                        {
                            texture = (Textures.GreenBox.ThreeLines.Rect, Textures.GreenBox.ThreeLines.TopRightBottom);
                        }
                        else
                        {
                            texture = (Textures.GreenBox.TwoLines.Rect, Textures.GreenBox.TwoLines.TopBottom);
                        }
                    }
                    else
                    {
                        if (x == 0 && y == 0)
                        {
                            texture = (Textures.GreenBox.Corner.Rect, Textures.GreenBox.Corner.TopLef);
                        }
                        else if (x == tileWidth - 1 && y == 0)
                        {
                            texture = (Textures.GreenBox.Corner.Rect, Textures.GreenBox.Corner.TopRight);
                        }
                        else if (x == 0 && y == tileHeight - 1)
                        {
                            texture = (Textures.GreenBox.Corner.Rect, Textures.GreenBox.Corner.BottomLeft);
                        }
                        else if (x == tileWidth - 1 && y == tileHeight - 1)
                        {
                            texture = (Textures.GreenBox.Corner.Rect, Textures.GreenBox.Corner.BottomRight);
                        }
                        else if (y == 0)
                        {
                            texture = (Textures.GreenBox.Line.Rect, Textures.GreenBox.Line.Top);
                        }
                        else if (y == tileHeight - 1)
                        {
                            texture = (Textures.GreenBox.Line.Rect, Textures.GreenBox.Line.Bottom);
                        }
                        else if (x == 0)
                        {
                            texture = (Textures.GreenBox.Line.Rect, Textures.GreenBox.Line.Left);
                        }
                        else if (x == tileWidth - 1)
                        {
                            texture = (Textures.GreenBox.Line.Rect, Textures.GreenBox.Line.Right);
                        }
                        else
                        {
                            texture = (Textures.GreenBox.Overlay, 0f);
                        }
                    }

                    Vector2 origin = new Vector2(texture.rect.Width / 2f, texture.rect.Height / 2f);
                    spriteBatch.Draw(
                        Textures.MainTexture,
                        tileScreenPos + origin,
                        texture.rect,
                        Color.White,
                        texture.rot,
                        origin,
                        Vector2.One,
                        SpriteEffects.None,
                        0f);
                }
            }
        }
    }

    public static HashSet<Vector2> GetConnectedTiles(GameLocation location, Vector2 startingTile, bool eightWayScan)
    {
        HashSet<Vector2> connectedTiles = new() { startingTile };

        if (!location.terrainFeatures.TryGetValue(startingTile, out TerrainFeature? startFeature) || startFeature is not HoeDirt)
        {
            return connectedTiles;
        }

        HashSet<Vector2> tilledPositions = new();
        foreach (var pair in location.terrainFeatures.Pairs)
        {
            if (pair.Value is HoeDirt)
            {
                tilledPositions.Add(pair.Key);
            }
        }

        if (!tilledPositions.Contains(startingTile))
        {
            return connectedTiles;
        }

        Queue<Vector2> queue = new();
        queue.Enqueue(startingTile);

        ReadOnlySpan<Vector2> directions = eightWayScan ? Directions8 : Directions4;

        while (queue.Count > 0)
        {
            Vector2 current = queue.Dequeue();
            foreach (Vector2 direction in directions)
            {
                Vector2 next = current + direction;
                if (tilledPositions.Contains(next) && connectedTiles.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return connectedTiles;
    }

    public static void RenderConnectedNoTextures(SpriteBatch spriteBatch, HashSet<Vector2> connectedTiles)
    {
        foreach (Vector2 tile in connectedTiles)
        {
            Vector2 screenTile = TileToScreenCoordinates(tile);
            spriteBatch.Draw(
                Game1.staminaRect,
                screenTile,
                new Rectangle(0, 0, 64, 64),
                Color.Green * 0.5f);
        }
    }

    public static void RenderConnectedTextures(SpriteBatch spriteBatch, HashSet<Vector2> connectedTiles)
    {
        foreach (Vector2 tile in connectedTiles)
        {
            Vector2 tileScreenPos = TileToScreenCoordinates(tile);

            bool hasTopBorder = !connectedTiles.Contains(new Vector2(tile.X, tile.Y - 1));
            bool hasRightBorder = !connectedTiles.Contains(new Vector2(tile.X + 1, tile.Y));
            bool hasBottomBorder = !connectedTiles.Contains(new Vector2(tile.X, tile.Y + 1));
            bool hasLeftBorder = !connectedTiles.Contains(new Vector2(tile.X - 1, tile.Y));

            int borderCount = 0;
            if (hasTopBorder)
            {
                borderCount++;
            }
            if (hasRightBorder)
            {
                borderCount++;
            }
            if (hasBottomBorder)
            {
                borderCount++;
            }
            if (hasLeftBorder)
            {
                borderCount++;
            }

            (Rectangle rect, float rot) texture;

            if (borderCount == 4)
            {
                texture = (Textures.GreenBox.Complete, 0f);
            }
            else if (borderCount == 3)
            {
                if (!hasLeftBorder)
                {
                    texture = (Textures.GreenBox.ThreeLines.Rect, Textures.GreenBox.ThreeLines.TopRightBottom);
                }
                else if (!hasBottomBorder)
                {
                    texture = (Textures.GreenBox.ThreeLines.Rect, Textures.GreenBox.ThreeLines.TopRightLeft);
                }
                else if (!hasRightBorder)
                {
                    texture = (Textures.GreenBox.ThreeLines.Rect, Textures.GreenBox.ThreeLines.TopBottomLeft);
                }
                else
                {
                    texture = (Textures.GreenBox.ThreeLines.Rect, Textures.GreenBox.ThreeLines.BottomLeftRight);
                }
            }
            else if (borderCount == 2)
            {
                if (hasLeftBorder && hasRightBorder)
                {
                    texture = (Textures.GreenBox.TwoLines.Rect, Textures.GreenBox.TwoLines.LeftRight);
                }
                else if (hasTopBorder && hasBottomBorder)
                {
                    texture = (Textures.GreenBox.TwoLines.Rect, Textures.GreenBox.TwoLines.TopBottom);
                }
                else if (hasTopBorder && hasLeftBorder)
                {
                    texture = (Textures.GreenBox.Corner.Rect, Textures.GreenBox.Corner.TopLef);
                }
                else if (hasTopBorder && hasRightBorder)
                {
                    texture = (Textures.GreenBox.Corner.Rect, Textures.GreenBox.Corner.TopRight);
                }
                else if (hasBottomBorder && hasRightBorder)
                {
                    texture = (Textures.GreenBox.Corner.Rect, Textures.GreenBox.Corner.BottomRight);
                }
                else
                {
                    texture = (Textures.GreenBox.Corner.Rect, Textures.GreenBox.Corner.BottomLeft);
                }
            }
            else if (borderCount == 0)
            {
                texture = (Textures.GreenBox.Overlay, 0f);
            }
            else
            {
                if (hasTopBorder)
                {
                    texture = (Textures.GreenBox.Line.Rect, Textures.GreenBox.Line.Top);
                }
                else if (hasRightBorder)
                {
                    texture = (Textures.GreenBox.Line.Rect, Textures.GreenBox.Line.Right);
                }
                else if (hasBottomBorder)
                {
                    texture = (Textures.GreenBox.Line.Rect, Textures.GreenBox.Line.Bottom);
                }
                else
                {
                    texture = (Textures.GreenBox.Line.Rect, Textures.GreenBox.Line.Left);
                }
            }

            Vector2 origin = new Vector2(texture.rect.Width / 2f, texture.rect.Height / 2f);
            spriteBatch.Draw(
                Textures.MainTexture,
                tileScreenPos + origin,
                texture.rect,
                Color.White,
                texture.rot,
                origin,
                Vector2.One,
                SpriteEffects.None,
                0f);
        }
    }
}