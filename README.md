# Burebista Traditional Tipi

A portable traditional hide tipi for **The Long Dark**, built as an independent MelonLoader mod.

## Features

- Craftable packed tipi that can be carried and reused.
- Deploy with `F3`, rotate with `F4`, and pack with `F2` after extinguishing the fire.
- Roll or close the entrance hide with `E`.
- Native campfire placed in the center, initially extinguished.
- `+10 °C` warmth bonus and wind protection while inside.
- Wolf painting on the entrance.
- `F6` adds one packed tipi for testing.

## Crafting recipe

Craft at a workbench with a knife and light:

- 20 sticks
- 6 cured wolf pelts
- 4 cured guts
- 10 stones

Crafting time: 24 in-game hours. The finished item weighs 8 kg and does not decay.

## Requirements

- The Long Dark
- MelonLoader
- ModComponent

## Installation

Download `BurebistaTraditionalTipi-v1.1.zip` from the Releases page, extract it, and copy its `Mods` folder into the main The Long Dark folder. Close the game before replacing an older version.

## Building

Set `TLDPath` to the game directory and run:

```powershell
.\scripts\build.ps1
```

The gameplay DLL is compiled from `src/Main.cs`. The `modcomponent` folder contains the item builder, recipe, and localization source used for the inventory item.

## License

Copyright © 2026 Burebista. Source-visible release; see [LICENSE](LICENSE).

