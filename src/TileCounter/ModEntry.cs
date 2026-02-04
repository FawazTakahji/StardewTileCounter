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

namespace TileCounter;

public class ModEntry : Mod
{
    public static ModConfig Config = new();

    private bool _isScanning;
    private bool _inSelectionMode;
    private Vector2? _selectedFirstTile;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        try
        {
            Config = Helper.ReadConfig<ModConfig>();
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

        Helpers.RegisterConfig(configMenu, ModManifest, Helper);
    }

    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (!_inSelectionMode || !Context.IsPlayerFree)
        {
            return;
        }

        Vector2 tile = Game1.wasMouseVisibleThisFrame ? Game1.currentCursorTile : Helpers.GetTileInFrontOfPlayer();

        if (!Textures.Loaded || Config.SimpleBorder)
        {
            Helpers.RenderNoTextures(e.SpriteBatch, tile, _selectedFirstTile);
        }
        else
        {
            Helpers.RenderTextures(e.SpriteBatch, tile, _selectedFirstTile);
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree)
        {
            return;
        }

        if (Config.ScanLocationKeys.JustPressed())
        {
            ScanCurrentLocation();
        }
        else if (Config.SelectionModeKeys.JustPressed())
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

            foreach (Keybind keybind in Config.SelectionModeKeys.Keybinds)
            {
                foreach (SButton button in keybind.Buttons)
                {
                    Helper.Input.Suppress(button);
                }
            }
        }
        else if (_inSelectionMode && Config.SelectTileKey.JustPressed())
        {
            TileClicked(Game1.wasMouseVisibleThisFrame ? Game1.currentCursorTile : Helpers.GetTileInFrontOfPlayer());

            foreach (Keybind keybind in Config.SelectTileKey.Keybinds)
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
                if (Config.CountSeedableTiles || Config.CountHarvestableTiles || Config.CountDryTiles)
                {
                    foreach (var (pos, value) in Game1.currentLocation.terrainFeatures.Pairs)
                    {
                        if (pos.X >= minX && pos.X <= maxX && pos.Y >= minY && pos.Y <= maxY)
                        {
                            if (value is HoeDirt dirt &&
                                !Game1.currentLocation.IsTileOccupiedBy(pos, mask, CollisionMask.TerrainFeatures))
                            {
                                if (Config.CountSeedableTiles && dirt.crop == null)
                                {
                                    seedableTiles++;
                                }
                                else if (Config.CountHarvestableTiles && dirt.readyForHarvest())
                                {
                                    harvestableTiles++;
                                }

                                if (Config.CountDryTiles && !dirt.isWatered())
                                {
                                    dryTiles++;
                                }
                            }
                        }
                    }
                }

                if (Config.CountDiggableTiles)
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

            if (Config.CountSelectedTiles)
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