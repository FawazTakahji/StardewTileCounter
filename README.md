# Tile Counter
Tile Counter is a utility mod for Stardew Valley that allows you to quickly count tiles with specific properties—such as harvestable crops, unwatered dirt, or empty tilled soil—within a selected area or connected group.

[Nexus Page](https://www.nexusmods.com/stardewvalley/mods/35853)

## Installation
1. Install the latest version of [SMAPI](https://smapi.io).
2. Download this mod and extract it into your `Stardew Valley/Mods` folder.
3. (Optional) Install [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) to easily change keybinds and settings in-game.

## Features
- Gamepad support
- Three Scan Modes:
  - Area Scan: Select two corners to scan a rectangular area.
  - Connected Scan: count all connected tilled soil (perfect for irregular crop layouts).
  - Location Scan: Instantly scans the entire map.
- Smart Selection: Automatically uses your mouse cursor; if using a gamepad or hiding the mouse, it targets the tile in front of your character.
- Visual Borders: Dynamic, high-quality borders highlight your selection. Includes a "Simple Border" mode for a cleaner look.
- Configurable Toggles: Choose exactly what you want to count:
  - Total selected tiles.
  - Harvestable crops.
  - Dry (unwatered) tilled soil.
  - Seedable (empty) tilled soil.
  - Diggable (hoeable) tiles.

## Controls
| Action | Keyboard | Xbox Gamepad | Playstation Gamepad |
| :-: | :-: | :-: | :-: |
| Scan Current Location | Ctrl + V | Y + DPad Right | Triangle + DPad Right |
| Scan Connected Soil | Ctrl + X | Y + DPad Down | Triangle + DPad Down |
| Toggle Area Mode | Ctrl + C | Y + DPad Up | Triangle + DPad Up |
| Select Tile | Mouse Left Click | A | Cross |

Note: Connected Scan requires you to click on a piece of tilled soil to start the scan.

## Configuration
Using Generic Mod Config Menu, you can:
- Eight-Way Scan: Toggle whether the connected scan checks diagonally or only the four cardinal directions.
- Simple Border: Switch between the decorative textured border and a simple semi-transparent color overlay.
- HUD Settings: Enable or disable specific count notifications to reduce clutter.

## Screenshots
<details>
  <summary>Border</summary>

  ![Border](assets/screenshots/border.png)
</details>
<details>
  <summary>Simple Border</summary>

  ![Simple Border](assets/screenshots/simpleborder.png)
</details>
<details>
  <summary>Notification</summary>

  ![Notification](assets/screenshots/notification.png)
</details>

## Building from Source
The mod build package should locate your game folder automatically. If you need to specify it manually, edit the `.csproj` file:
```xml
<PropertyGroup>
    ...
    <GamePath>YOUR_GAME_PATH_HERE</GamePath>
    ...
</PropertyGroup>
```
Alternatively, you can set a global path by creating a `stardewvalley.targets` file in your user profile folder (`%userprofile%` on Windows or `~` on Linux/macOS):
```xml
<Project>
   <PropertyGroup>
      <GamePath>YOUR_GAME_PATH_HERE</GamePath>
   </PropertyGroup>
</Project>
```
