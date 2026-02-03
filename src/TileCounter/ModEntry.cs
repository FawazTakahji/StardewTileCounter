using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Layers;
using Object = StardewValley.Object;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

namespace TileCounter;

public class ModEntry : Mod
{
    private ModConfig _config = new();

    private bool _isScanning;
    private bool _inSelectionMode;
    private Vector2? _selectedFirstTile;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        try
        {
            _config = Helper.ReadConfig<ModConfig>();
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed to load config: {ex}", LogLevel.Error);
        }
        try
        {
            Textures.MainTexture = helper.ModContent.Load<Texture2D>("assets/textures.png");
            Textures.Loaded = true;
        }
        catch (Exception ex)
        {
            Monitor.Log($"Couldn't load textures: {ex}", LogLevel.Error);
        }

        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.Display.RenderedWorld += OnRenderedWorld;
        helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        helper.Events.Player.Warped += OnWarped;
        helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        IGenericModConfigMenuApi? configMenu;
        try
        {
            configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                Monitor.Log("Couldn't get Generic Mod Config Menu API; config menu won't be available.", LogLevel.Warn);
                return;
            }
        }
        catch (Exception ex)
        {
            Monitor.Log($"Failed to get Generic Mod Config Menu API: {ex}", LogLevel.Error);
            return;
        }

        configMenu.Register(
            ModManifest,
            () => _config = new ModConfig(),
            () => Helper.WriteConfig(_config));

        configMenu.AddBoolOption(
            ModManifest,
            () => _config.SimpleBorder,
            value => _config.SimpleBorder = value,
            I18n.SimpleBorder);
        configMenu.AddBoolOption(
            ModManifest,
            () => _config.CountSelectedTiles,
            value => _config.CountSelectedTiles = value,
            I18n.CountSelectedTiles);
        configMenu.AddBoolOption(
            ModManifest,
            () => _config.CountHarvestableTiles,
            value => _config.CountHarvestableTiles = value,
            I18n.CountHarvestableTiles);
        configMenu.AddBoolOption(
            ModManifest,
            () => _config.CountDryTiles,
            value => _config.CountDryTiles = value,
            I18n.CountDryTiles);
        configMenu.AddBoolOption(
            ModManifest,
            () => _config.CountSeedableTiles,
            value => _config.CountSeedableTiles = value,
            I18n.CountSeedableTiles);
        configMenu.AddBoolOption(
            ModManifest,
            () => _config.CountDiggableTiles,
            value => _config.CountDiggableTiles = value,
            I18n.CountDiggableTiles);

        configMenu.AddSectionTitle(ModManifest, I18n.Keybinds);
        configMenu.AddKeybindList(
            ModManifest,
            () => _config.ScanLocationKeys,
            keys => _config.ScanLocationKeys = keys,
            I18n.ScanCurrentLocation);

        configMenu.AddKeybindList(
            ModManifest,
            () => _config.SelectionModeKeys,
            keys => _config.SelectionModeKeys = keys,
            I18n.ToggleSelectionMode);

        configMenu.AddKeybindList(
            ModManifest,
            () => _config.SelectTileKey,
            key => _config.SelectTileKey = key,
            I18n.SelectKey);
    }

    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!_inSelectionMode || !Context.IsPlayerFree)
        {
            return;
        }

        Vector2 tile = Game1.wasMouseVisibleThisFrame ? Game1.currentCursorTile : GetTileInFrontOfPlayer();

        if (!Textures.Loaded || _config.SimpleBorder)
        {
            RenderNoTextures(e.SpriteBatch, tile, _selectedFirstTile);
        }
        else
        {
            RenderTextures(e.SpriteBatch, tile, _selectedFirstTile);
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree)
        {
            return;
        }

        if (_config.ScanLocationKeys.JustPressed())
        {
            ScanCurrentLocation();
        }
        else if (_config.SelectionModeKeys.JustPressed())
        {
            if (!_inSelectionMode)
            {
                _inSelectionMode = true;
                Game1.playSound("breathin");
            }
            else
            {
                _inSelectionMode = false;
                _selectedFirstTile = null;
                Game1.playSound("breathout");
            }

            foreach (Keybind keybind in _config.SelectionModeKeys.Keybinds)
            {
                foreach (SButton button in keybind.Buttons)
                {
                    Helper.Input.Suppress(button);
                }
            }
        }
        else if (_inSelectionMode && _config.SelectTileKey.JustPressed())
        {
            TileClicked(Game1.wasMouseVisibleThisFrame ? Game1.currentCursorTile : GetTileInFrontOfPlayer());

            foreach (Keybind keybind in _config.SelectTileKey.Keybinds)
            {
                foreach (SButton button in keybind.Buttons)
                {
                    Helper.Input.Suppress(button);
                }
            }
        }
    }

    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        _selectedFirstTile = null;
    }

    private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
    {
        _selectedFirstTile = null;
        _inSelectionMode = false;
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

    private void TileClicked(Vector2 tile)
    {
        if (_selectedFirstTile == null)
        {
            _selectedFirstTile = tile;
            Game1.playSound("bigSelect");
        }
        else if (_selectedFirstTile == tile)
        {
            _selectedFirstTile = null;
            Game1.playSound("bigDeSelect");
        }
        else
        {
            Vector2 firstTile = _selectedFirstTile.Value;
            _inSelectionMode = false;
            _selectedFirstTile = null;
            Game1.playSound("coin");
            ScanTiles(firstTile, tile).SafeFireAndForget(ex =>
            {
                Monitor.Log($"Error while scanning tiles: {ex}", LogLevel.Error);
            });
        }
    }

    private static Vector2 GetTileInFrontOfPlayer()
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

    private void ScanCurrentLocation()
    {
        Layer? backLayer = Game1.currentLocation.map.GetLayer("Back");
        if (backLayer == null)
        {
            Game1.addHUDMessage(new HUDMessage(I18n.NoBackLayer(), HUDMessage.error_type));
            return;
        }

        Vector2 lastTile = new Vector2(backLayer.LayerWidth - 1, backLayer.LayerHeight - 1);
        ScanTiles(Vector2.Zero, lastTile).SafeFireAndForget(ex =>
        {
            Monitor.Log($"Error while scanning tiles: {ex}", LogLevel.Error);
        });
    }

    private async Task ScanTiles(Vector2 pos1, Vector2 pos2)
    {
        if (_isScanning)
        {
            return;
        }

        try
        {
            _isScanning = true;

            int minX = (int)Math.Min(pos1.X, pos2.X);
            int maxX = (int)Math.Max(pos1.X, pos2.X);
            int minY = (int)Math.Min(pos1.Y, pos2.Y);
            int maxY = (int)Math.Max(pos1.Y, pos2.Y);

            const CollisionMask mask = CollisionMask.Buildings | CollisionMask.Flooring | CollisionMask.Furniture
                                       | CollisionMask.Objects | CollisionMask.LocationSpecific |
                                       CollisionMask.TerrainFeatures;

            int seedableTiles = 0;
            int dryTiles = 0;
            int diggableTiles = 0;
            int harvestableTiles = 0;

            Game1.addHUDMessage(new HUDMessage(I18n.CountingTiles())
            {
                type = "TileCounter_Progress",
                messageSubject = ItemRegistry.Create("170")
            });

            // Avoid blocking main thread
            await Task.Run(() =>
            {
                if (_config.CountSeedableTiles || _config.CountHarvestableTiles || _config.CountDryTiles)
                {
                    foreach (var (pos, value) in Game1.currentLocation.terrainFeatures.Pairs)
                    {
                        if (pos.X >= minX && pos.X <= maxX && pos.Y >= minY && pos.Y <= maxY)
                        {
                            if (value is HoeDirt dirt &&
                                !Game1.currentLocation.IsTileOccupiedBy(pos, mask, CollisionMask.TerrainFeatures))
                            {
                                if (_config.CountSeedableTiles && dirt.crop == null)
                                {
                                    seedableTiles++;
                                }
                                else if (_config.CountHarvestableTiles && dirt.readyForHarvest())
                                {
                                    harvestableTiles++;
                                }

                                if (_config.CountDryTiles && !dirt.isWatered())
                                {
                                    dryTiles++;
                                }
                            }
                        }
                    }
                }

                if (_config.CountDiggableTiles)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        for (int y = minY; y <= maxY; y++)
                        {
                            if (Game1.currentLocation.doesTileHaveProperty(x, y, "Diggable", "Back") != null)
                            {
                                if (!Game1.currentLocation.IsTileOccupiedBy(new Vector2(x, y), mask))
                                {
                                    diggableTiles++;
                                }
                            }
                        }
                    }
                }
            });

            for (int i = Game1.hudMessages.Count - 1; i >= 0; i--)
            {
                if (Game1.hudMessages[i].type?.StartsWith("TileCounter_") == true)
                {
                    Game1.hudMessages.RemoveAt(i);
                }
            }

            if (_config.CountSelectedTiles)
            {
                Game1.addHUDMessage(new HUDMessage(I18n.SelectedTiles((maxX - minX + 1) * (maxY - minY + 1)))
                {
                    type = "TileCounter_Selected",
                    messageSubject = new Object("293", 1)
                });
            }

            if (harvestableTiles > 0)
            {
                Game1.addHUDMessage(new HUDMessage(I18n.HarvestableTiles(harvestableTiles))
                {
                    type = "TileCounter_Harvestable",
                    messageSubject = new Object("24", 1)
                });
            }

            if (dryTiles > 0)
            {
                Game1.addHUDMessage(new HUDMessage(I18n.DryTiles(dryTiles))
                {
                    type = "TileCounter_Dry",
                    messageSubject = new Object("407", 1)
                });
            }

            if (seedableTiles > 0)
            {
                Game1.addHUDMessage(new HUDMessage(I18n.SeedableTiles(seedableTiles))
                {
                    type = "TileCounter_Seedable",
                    messageSubject = new Object("472", 1)
                });
            }

            if (diggableTiles > 0)
            {
                Game1.addHUDMessage(new HUDMessage(I18n.DiggableTiles(diggableTiles))
                {
                    type = "TileCounter_Diggable",
                    messageSubject = new Hoe()
                });
            }
        }
        finally
        {
            _isScanning = false;
        }
    }
}