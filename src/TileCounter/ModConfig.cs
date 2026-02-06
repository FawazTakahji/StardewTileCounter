using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace TileCounter;

public sealed class ModConfig
{
    public static ModConfig Instance { get; set; } = new();

    public bool SimpleBorder { get; set; } = false;
    public bool CountSelectedTiles { get; set; } = true;
    public bool CountHarvestableTiles { get; set; } = true;
    public bool CountDryTiles { get; set; } = true;
    public bool CountSeedableTiles { get; set; } = true;
    public bool CountDiggableTiles { get; set; } = true;

    public KeybindList ScanLocationKeys { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.V),
        new Keybind(SButton.ControllerY, SButton.DPadRight));

    public KeybindList ScanConnectedKeys { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.X),
        new Keybind(SButton.ControllerY, SButton.DPadDown));

    public bool EightWayScan { get; set; } = true;

    public KeybindList SelectionModeKeys { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.C),
        new Keybind(SButton.ControllerY, SButton.DPadUp));

    public KeybindList SelectTileKey { get; set; } = new(
        new Keybind(SButton.MouseLeft),
        new Keybind(SButton.ControllerA));
}