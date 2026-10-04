# Kettle & Cog

> *Grow it. Brew it. Automate it. Cheers!*

A cozy top-down pixel-art factory builder set in a beer brewery, built with Unity.
You grow barley, wheat and hops, malt, mash, boil, ferment and package beer, sell it, and reinvest
until a single copper kettle has become a fully automated brewery valley. The progression and
"one more batch" feel come from Factorio and Satisfactory, the warmth from Stardew Valley.

**Read the design first: [docs/GameDesignDocument.md](docs/GameDesignDocument.md)**

## Status

Concept and project skeleton. What exists today:

- The game design document: name, overview and core loop, architecture, buildings, progression,
  engagement mechanics, ratios and roadmap.
- The Unity folder structure and assembly definitions, matching GDD §3.
- The engine-free simulation core: grid, buildings, crafting, inventories, fixed-tick clock,
  calendar, wallet and commands, covered by EditMode tests.

## Requirements

- **Unity 6 LTS** (6000.0.x or newer). Package versions in `Packages/manifest.json` target
  6000.0; a newer editor updates them on first open.
- Optional: **.NET 8 SDK**, to run the simulation tests without Unity.

## Getting started

1. In Unity Hub: **Add → Add project from disk** → select this folder, and open it with Unity 6 LTS.
2. The first import generates `Library/`, `ProjectSettings/` and `.meta` files.
   **Commit `ProjectSettings/` and every `.meta` file**; `Library/` stays ignored.
3. Do the one-time setup below and commit the result.

### One-time project setup

The project settings and render-pipeline assets are created by the editor, so they are not in
the repo yet:

1. **Editor:** *Project Settings → Editor*: set Default Behaviour Mode to **2D**. Asset
   Serialization should be **Force Text** and Version Control **Visible Meta Files** (both are
   the defaults).
2. **URP 2D:** in `Assets/_Project/Settings`, use *Create → Rendering → URP Asset (with 2D
   Renderer)*. Assign it under *Project Settings → Graphics* and *Quality*. On the 2D Renderer
   Data, set Transparency Sort Mode to **Custom Axis (0, 1, 0)** for Y-sorting.
3. **Pixel-art import preset:** configure one texture with Pixels Per Unit **16**, Filter **Point**,
   Compression **None** and Mip Maps **off**. Save it as a Preset in `Assets/_Project/Settings`,
   then add it in *Project Settings → Preset Manager* for `TextureImporter` with the filter
   `glob:"Assets/_Project/Art/**"`.
4. **Content:** in `Assets/_Project/Data`, use *Create → Kettle & Cog → Content Database*, plus
   items, recipes and buildings from the same menu.
5. **Scene:** create `Assets/_Project/Scenes/Game.unity` with:
   - a `Simulation` GameObject that has a `SimulationRunner` with the Content Database assigned;
   - a Main Camera with a **Pixel Perfect Camera** (Assets PPU 16, reference resolution 640×360).

## Running the tests

- **In Unity:** *Window → General → Test Runner → EditMode → Run All*.
- **Without Unity:** this compiles the same sources from `Assets/` with plain NUnit and suits CI
  without a Unity licence.

  ```bash
  dotnet test Tools/SimTests
  ```

## Project layout

```
Assets/_Project/
  Art/ Audio/ Data/ Prefabs/ Scenes/ Settings/ UI/
  Scripts/
    Core/          KettleCog.Core          grid maths, directions (no UnityEngine)
    Simulation/    KettleCog.Simulation    world, systems, commands (no UnityEngine, deterministic)
    Content/       KettleCog.Content       ScriptableObject authoring → runtime definitions
    Presentation/  KettleCog.Presentation  SimulationRunner, rendering, input
    UI/            KettleCog.UI            UI Toolkit screens (planned)
    Editor/        KettleCog.Editor        validators, ratio calculator (planned)
  Tests/EditMode/  KettleCog.Tests.EditMode
Packages/manifest.json
Tools/SimTests/    dotnet test harness for the simulation
docs/              game design document
```

## Architecture rules

These are the short version of GDD §3:

1. **The simulation is pure C#.** `Core` and `Simulation` set `noEngineReferences: true`: no
   MonoBehaviours, `UnityEngine.Random` or `Time` in there.
2. **Presentation only reads.** Views read simulation state and events; they never own game state.
3. **Player actions are commands** (`ISimCommand`), applied at the start of the next tick. This
   supports undo, replays and co-op later.
4. **Content is data.** ScriptableObjects are baked into immutable runtime definitions and referenced
   by stable string ids (`item.pale_malt`). Never rename an id after release.
5. **Fixed 20 ticks per second.** Rendering interpolates with `SimulationClock.Alpha`, and the
   simulation never allocates in its tick loop.
