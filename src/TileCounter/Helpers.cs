using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace TileCounter;

public static class Helpers
{
    public static void RegisterConfig(IGenericModConfigMenuApi api, IManifest manifest, IModHelper modHelper)
    {
        api.Register(
            manifest,
            () => ModEntry.Config = new ModConfig(),
            () => modHelper.WriteConfig(ModEntry.Config));

        api.AddBoolOption(
            manifest,
            () => ModEntry.Config.SimpleBorder,
            value => ModEntry.Config.SimpleBorder = value,
            I18n.SimpleBorder);
        api.AddBoolOption(
            manifest,
            () => ModEntry.Config.CountSelectedTiles,
            value => ModEntry.Config.CountSelectedTiles = value,
            I18n.CountSelectedTiles);
        api.AddBoolOption(
            manifest,
            () => ModEntry.Config.CountHarvestableTiles,
            value => ModEntry.Config.CountHarvestableTiles = value,
            I18n.CountHarvestableTiles);
        api.AddBoolOption(
            manifest,
            () => ModEntry.Config.CountDryTiles,
            value => ModEntry.Config.CountDryTiles = value,
            I18n.CountDryTiles);
        api.AddBoolOption(
            manifest,
            () => ModEntry.Config.CountSeedableTiles,
            value => ModEntry.Config.CountSeedableTiles = value,
            I18n.CountSeedableTiles);
        api.AddBoolOption(
            manifest,
            () => ModEntry.Config.CountDiggableTiles,
            value => ModEntry.Config.CountDiggableTiles = value,
            I18n.CountDiggableTiles);

        api.AddSectionTitle(manifest, I18n.Keybinds);
        api.AddKeybindList(
            manifest,
            () => ModEntry.Config.ScanLocationKeys,
            keys => ModEntry.Config.ScanLocationKeys = keys,
            I18n.ScanCurrentLocation);

        api.AddKeybindList(
            manifest,
            () => ModEntry.Config.SelectionModeKeys,
            keys => ModEntry.Config.SelectionModeKeys = keys,
            I18n.ToggleSelectionMode);

        api.AddKeybindList(
            manifest,
            () => ModEntry.Config.SelectTileKey,
            key => ModEntry.Config.SelectTileKey = key,
            I18n.SelectKey);
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

    private static void RenderNoTextures(SpriteBatch spriteBatch, Vector2 currentTile, Vector2? selectedTile)
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

    private static void RenderTextures(SpriteBatch spriteBatch, Vector2 currentTile, Vector2? selectedTile)
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
                            // middle
                            continue;
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
}