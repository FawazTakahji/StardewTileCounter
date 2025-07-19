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
    public KeybindList KSelectionModeKeys { get; set; } = new(new Keybind(SButton.LeftControl), new Keybind(SButton.U));
    public KeybindList GSelectionModeKeys { get; set; } = new(new Keybind(SButton.LeftShoulder), new Keybind(SButton.RightShoulder));
    public SButton KSelectKey { get; set; } = SButton.MouseLeft;
    public SButton GSelectKey { get; set; } = SButton.ControllerA;
}