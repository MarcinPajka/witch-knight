# Witch Knight

A full-overhaul mod for **Hollow Knight** that plays like a run-based action roguelite, starring an original moth witch from a lost lunar branch of the Moth Tribe.

> Work in progress. See [docs/concept.md](docs/concept.md) for the design and milestones.

## Requirements

- Hollow Knight 1.5.x (you need your own copy of the game)
- The [Hollow Knight Modding API](https://github.com/hk-modding/api), installed with [Lumafly](https://github.com/TheMulhima/Lumafly) or Scarab

## Legal

This is an unofficial fan project, not affiliated with Team Cherry or Supergiant Games.
The repository contains only original code and original art. It does not include or redistribute any files from Hollow Knight or any other game; the mod changes the game at runtime on the player's own install.

- Code: [MIT](LICENSE)
- Original art and audio (once added under `assets/`): [CC BY-NC-SA 4.0](https://creativecommons.org/licenses/by-nc-sa/4.0/)

## Building

You need the .NET SDK (8 or newer) and a Hollow Knight install with the Modding API.

1. Copy `LocalBuildProperties.example.props` to `LocalBuildProperties.props` and set `HollowKnightRefs` to your game's `Managed` folder.
2. Run `dotnet build WitchKnight/WitchKnight.csproj`. If `ModsDir` is set, the mod is copied into the game.

Every push and pull request is also built on GitHub. Pushing a `v*` tag publishes a release zip.
