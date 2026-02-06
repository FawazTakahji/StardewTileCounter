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
    private bool IsScanning => _isScanningArea || _isScanningConnectedTiles;
    private bool _isScanningArea;
    private bool _isScanningConnectedTiles;
    private bool InSelectionMode => _inAreaSelectionMode || _inConnectedSelectionMode;
    private bool _inAreaSelectionMode;
    private bool _inConnectedSelectionMode;
    private Vector2? _selectedFirstTile;

    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        try
        {
            ModConfig.Instance = Helper.ReadConfig<ModConfig>();
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
        if (!InSelectionMode || !Context.IsPlayerFree)
        {
            return;
        }

        Vector2 tile = Game1.wasMouseVisibleThisFrame ? Game1.currentCursorTile : Helpers.GetTileInFrontOfPlayer();

        if (!Textures.Loaded || ModConfig.Instance.SimpleBorder)
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

        if (ModConfig.Instance.ScanLocationKeys.JustPressed())
        {
            ScanCurrentLocation();
        }
        else if (ModConfig.Instance.ScanConnectedKeys.JustPressed() && !_inAreaSelectionMode)
        {
            if (!_inConnectedSelectionMode)
            {
                _inConnectedSelectionMode = true;
                Game1.playSound("breathin");
            }
            else
            {
                _inConnectedSelectionMode = false;
                Game1.playSound("breathout");
            }

            foreach (Keybind keybind in ModConfig.Instance.ScanConnectedKeys.Keybinds)
            {
                foreach (SButton button in keybind.Buttons)
                {
                    Helper.Input.Suppress(button);
                }
            }
        }
        else if (ModConfig.Instance.SelectionModeKeys.JustPressed() && !_inConnectedSelectionMode)
        {
            if (!_inAreaSelectionMode)
            {
                _inAreaSelectionMode = true;
                Game1.playSound("breathin");
            }
            else
            {
                _inAreaSelectionMode = false;
                _selectedFirstTile = null;
                Game1.playSound("breathout");
            }

            foreach (Keybind keybind in ModConfig.Instance.SelectionModeKeys.Keybinds)
            {
                foreach (SButton button in keybind.Buttons)
                {
                    Helper.Input.Suppress(button);
                }
            }
        }
        else if (InSelectionMode && ModConfig.Instance.SelectTileKey.JustPressed())
        {
            TileClicked(Game1.wasMouseVisibleThisFrame ? Game1.currentCursorTile : Helpers.GetTileInFrontOfPlayer());

            foreach (Keybind keybind in ModConfig.Instance.SelectTileKey.Keybinds)
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
        _inAreaSelectionMode = false;
        _inConnectedSelectionMode = false;
    }

    private void TileClicked(Vector2 tile)
    {
        if (_inConnectedSelectionMode)
        {
            ScanConnectedTiles(tile).SafeFireAndForget(ex =>
            {
                Monitor.Log($"Error while scanning tiles: {ex}", LogLevel.Error);
            });
            _inConnectedSelectionMode = false;
            return;
        }

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
            _inAreaSelectionMode = false;
            _selectedFirstTile = null;
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
            Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_Error_NoBackLayer(), HUDMessage.error_type));
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
        if (IsScanning)
        {
            return;
        }

        try
        {
            _isScanningArea = true;

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

            Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_Info_CountingTiles())
            {
                type = "TileCounter_Progress",
                messageSubject = ItemRegistry.Create("170")
            });

            // Avoid blocking main thread
            await Task.Run(() =>
            {
                if (ModConfig.Instance.CountSeedableTiles || ModConfig.Instance.CountHarvestableTiles || ModConfig.Instance.CountDryTiles)
                {
                    foreach (var (pos, value) in Game1.currentLocation.terrainFeatures.Pairs)
                    {
                        if (pos.X >= minX && pos.X <= maxX && pos.Y >= minY && pos.Y <= maxY)
                        {
                            if (value is HoeDirt dirt &&
                                !Game1.currentLocation.IsTileOccupiedBy(pos, mask, CollisionMask.TerrainFeatures))
                            {
                                if (ModConfig.Instance.CountSeedableTiles && dirt.crop == null)
                                {
                                    seedableTiles++;
                                }
                                else if (ModConfig.Instance.CountHarvestableTiles && dirt.readyForHarvest())
                                {
                                    harvestableTiles++;
                                }

                                if (ModConfig.Instance.CountDryTiles && !dirt.isWatered())
                                {
                                    dryTiles++;
                                }
                            }
                        }
                    }
                }

                if (ModConfig.Instance.CountDiggableTiles)
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

            ShowCount((maxX - minX + 1) * (maxY - minY + 1), harvestableTiles, dryTiles, seedableTiles, diggableTiles);
        }
        finally
        {
            Game1.playSound("coin");
            _isScanningArea = false;
        }
    }

    private async Task ScanConnectedTiles(Vector2 startingTile)
    {
        if (IsScanning)
        {
            return;
        }

        Dictionary<Vector2, HoeDirt> tilledTiles = Game1.currentLocation.terrainFeatures.Pairs
            .Where(pair => pair.Value is HoeDirt)
            .ToDictionary(pair => pair.Key, pair => (HoeDirt)pair.Value);

        if (!tilledTiles.ContainsKey(startingTile))
        {
            return;
        }

        try
        {
            _isScanningConnectedTiles = true;

            int seedableTiles = 0;
            int dryTiles = 0;
            int harvestableTiles = 0;
            int selectedTiles = 0;

            Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_Info_CountingTiles())
            {
                type = "TileCounter_Progress",
                messageSubject = ItemRegistry.Create("170")
            });

            // Avoid blocking main thread
            await Task.Run(() =>
            {
                HashSet<Vector2> connectedTiles = new();
                Queue<Vector2> queue = new();

                queue.Enqueue(startingTile);
                connectedTiles.Add(startingTile);

                List<Vector2> directions = new() {
                    new(0, 1), // up
                    new(0, -1), // down
                    new(1, 0), // right
                    new(-1, 0), // left
                };
                if (ModConfig.Instance.EightWayScan)
                {
                    directions.Add(new(1, 1)); // up right
                    directions.Add(new(1, -1)); // down right
                    directions.Add(new(-1, 1)); // up left
                    directions.Add(new(-1, -1)); // down left
                }
                const CollisionMask mask = CollisionMask.Buildings | CollisionMask.Flooring | CollisionMask.Furniture
                                           | CollisionMask.Objects | CollisionMask.LocationSpecific |
                                           CollisionMask.TerrainFeatures;

                while (queue.Count > 0)
                {
                    Vector2 current = queue.Dequeue();
                    HoeDirt dirt = tilledTiles[current];
                    if (ModConfig.Instance.CountSeedableTiles
                        && dirt.crop == null
                        && !Game1.currentLocation.IsTileOccupiedBy(current, mask, CollisionMask.TerrainFeatures))
                    {
                        seedableTiles++;
                    }
                    else if (ModConfig.Instance.CountHarvestableTiles && dirt.readyForHarvest())
                    {
                        harvestableTiles++;
                    }

                    if (ModConfig.Instance.CountDryTiles && !dirt.isWatered())
                    {
                        dryTiles++;
                    }

                    foreach (Vector2 direction in directions)
                    {
                        Vector2 next = current + direction;
                        if (tilledTiles.ContainsKey(next) && connectedTiles.Add(next))
                        {
                            queue.Enqueue(next);
                        }
                    }
                }

                selectedTiles = connectedTiles.Count;
            });

            ShowCount(selectedTiles, harvestableTiles, dryTiles, seedableTiles);
        }
        finally
        {
            Game1.playSound("coin");
            _isScanningConnectedTiles = false;
        }
    }

    private void ShowCount(int selectedTiles, int harvestableTiles, int dryTiles, int seedableTiles, int diggableTiles = 0)
    {
        for (int i = Game1.hudMessages.Count - 1; i >= 0; i--)
        {
            if (Game1.hudMessages[i].type?.StartsWith("TileCounter_") == true)
            {
                Game1.hudMessages.RemoveAt(i);
            }
        }

        if (ModConfig.Instance.CountSelectedTiles)
        {
            Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_Info_SelectedTiles(selectedTiles))
            {
                type = "TileCounter_Selected",
                messageSubject = new Object("293", 1)
            });
        }

        if (harvestableTiles > 0)
        {
            Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_Info_HarvestableTiles(harvestableTiles))
            {
                type = "TileCounter_Harvestable",
                messageSubject = new Object("24", 1)
            });
        }

        if (dryTiles > 0)
        {
            Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_Info_DryTiles(dryTiles))
            {
                type = "TileCounter_Dry",
                messageSubject = new Object("407", 1)
            });
        }

        if (seedableTiles > 0)
        {
            Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_Info_SeedableTiles(seedableTiles))
            {
                type = "TileCounter_Seedable",
                messageSubject = new Object("472", 1)
            });
        }

        if (diggableTiles > 0)
        {
            Game1.addHUDMessage(new HUDMessage(I18n.HudMessage_Info_DiggableTiles(diggableTiles))
            {
                type = "TileCounter_Diggable",
                messageSubject = new Hoe()
            });
        }
    }
}