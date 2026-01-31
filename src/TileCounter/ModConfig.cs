using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace TileCounter;

public sealed class ModConfig
{
    public bool SimpleBorder { get; set; } = false;
    public bool CountSelectedTiles { get; set; } = true;
    public bool CountHarvestableTiles { get; set; } = true;
    public bool CountDryTiles { get; set; } = true;
    public bool CountSeedableTiles { get; set; } = true;
    public bool CountDiggableTiles { get; set; } = true;

    public KeybindList SelectionModeKeys { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.C),
        new Keybind(SButton.ControllerY, SButton.DPadUp));

    public KeybindList SelectTileKey { get; set; } = new(
        new Keybind(SButton.MouseLeft),
        new Keybind(SButton.ControllerA));
}