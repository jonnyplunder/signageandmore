# Signage & More

Brackets and hanging signs for [Vintage Story](https://www.vintagestory.at/).

Place a wall bracket, then hang vanilla lanterns or custom signs from it — something the base game does not allow with attachable blocks alone.

**Game version:** 1.22.0+  
**Mod type:** Code (client + server)

## Features

- **Wall brackets** that attach to solid surfaces
- **Hanging signs** in wood and metal variants
- Mount system so signs only attach to brackets (not directly to walls)
- Vanilla lanterns can hang from brackets

More content (lamp posts, additional shapes, crafting) is planned.

## Installation

1. Download the latest release from [ModDB](https://mods.vintagestory.at/) *(page TBD)* or this repository’s Releases.
2. Place the `.zip` in your Vintage Story `Mods` folder.
3. Start the game — the mod must be enabled on both client and server.

## Building from source

Requirements:

- .NET SDK (10.x)
- Vintage Story installed
- Environment variable `VINTAGE_STORY` pointing to the game install directory

```powershell
# Compile for local testing (Visual Studio / Rider / CLI)
dotnet build SignageAndMore/SignageAndMore.csproj -c Debug

# Package a release zip into Releases/
.\build.ps1
```

Launch profile **Client** (or **Server**) in Visual Studio uses `--addModPath` so the built mod loads automatically.

## Credits

- **FuegoFish** — creator, design, models
- **RodinPandarex** — code

## License

[Creative Commons Attribution-NonCommercial 4.0 International](https://creativecommons.org/licenses/by-nc/4.0/) (CC BY-NC 4.0).

You may copy, modify, and redistribute this mod for **non-commercial** purposes, as long as you credit **FuegoFish** and **RodinPandarex**. Commercial use is not permitted. See [LICENSE](LICENSE) for the full terms.
