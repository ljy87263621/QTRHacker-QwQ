# Codex Handoff 2026-06-01 00:55 CST

## User Request

- Pause before 01:00 CST and preserve current session context because network may drop.
- Answer user in Chinese.
- Main goal remains: fix QTRHacker-QwQ for Terraria 1.4.5.6.

## Environment

- Repo: `D:\Dev\GitHub\QTRHacker\QTRHacker-QwQ`
- Terraria: `D:\Apps\Steam\steamapps\common\Terraria\Terraria.exe`
- Terraria version checked: `1.4.5.6`
- Required final build command:

```powershell
& 'D:\VisualStudio\IDE\MSBuild\Current\Bin\MSBuild.exe' QTRHacker.sln /restore /p:Configuration=Debug /p:Platform=x86 /v:minimal
```

## Verified Before Pause

- User had entered a world.
- `.\bin\Debug\QTRHacker.Functions.Test.exe --smoke` passed existing world smoke before the latest in-progress edits.
- Confirmed OK smoke items included:
  - InfiniteLife, InfiniteMana, InfiniteAmmo, InfiniteOxygen, InfiniteMinion, InfiniteFlyTime
  - SlowFall, FastSpeed, FastTileAndWallPlacingSpeed, MechanicalRuler, MechanicalLens
  - SuperGrabRange, ImmuneToDebuffs
  - Patches runtime initialized
- `--diagnostic` confirmed Terraria method addresses and field offsets for 1.4.5.6.
- Diagnostic still showed old AOB misses for features already migrated to managed patch side; those misses are not active failures.

## In-Progress Edits At Pause

The latest work started migrating remaining fragile AOB features to managed Harmony patches.

Edited:

- `src/QTRHacker.Functions.Test/Program.cs`
  - Added smoke probes for `CreativeMenu` and `FishCratesOnly` managed toggles.
  - `SmokePatches` now expects `QTRHacker.Patches.Boot.HarmonyPatched`.
- `src/QTRHacker.Patches/Boot.cs`
  - Added `using HarmonyLib;`
  - Added `public static bool HarmonyPatched`.
  - Added `private static Harmony HarmonyInstance`.
  - Static constructor now calls `PatchManagedHooks()` before `InitializePatchTypes()`.
  - Added `PatchManagedHooks()` using `new Harmony("QTRHacker.Patches").PatchAll(typeof(Boot).Assembly)`.
- `src/QTRHacker.Patches/PlayerToggles.cs`
  - Added static bool fields `CreativeMenu` and `FishCratesOnly`.
- `src/QTRHacker.Patches/ManagedHooks.cs`
  - New file.
  - `CreativeUIDrawPatch` temporarily sets `Main.LocalPlayer.difficulty = 3` during `CreativeUI.Draw` when `PlayerToggles.CreativeMenu` is enabled, then restores it.
  - `FishingCheckRollItemDropPatch` sets `FishingAttempt.crate = true` and clears non-crate rarity/junk/enemy flags before `Projectile.FishingCheck_RollItemDrop` when `PlayerToggles.FishCratesOnly` is enabled.

Not yet edited due to pause:

- `src/QTRHacker.Core/PatchesManager.cs`
  - Still needs properties:
    - `public bool CreativeMenu { get => GetPlayerToggle(nameof(CreativeMenu)); set => SetPlayerToggle(nameof(CreativeMenu), value); }`
    - `public bool FishCratesOnly { get => GetPlayerToggle(nameof(FishCratesOnly)); set => SetPlayerToggle(nameof(FishCratesOnly), value); }`
- `src/QTRHacker/Scripts/Functions/BuiltIn-1.cs`
  - `CreativeMenu.Enable/Disable` still uses old AOB replace; should switch to `ctx.Patches.CreativeMenu = true/false`.
- `src/QTRHacker/Scripts/Functions/BuiltIn-2.cs`
  - `FishCratesOnly.Enable/Disable` still writes old AOB byte; should switch to `ctx.Patches.FishCratesOnly = true/false`.

## Important Runtime Note

- Terraria PID `82932` was running and locking `bin\Debug\QTRHacker.Patches.dll`.
- MSBuild failed only because that DLL was locked by Terraria.
- Do not judge final build until Terraria is closed.

## Recommended Resume Steps

1. Finish the three pending edits listed above.
2. Close Terraria before building:

```powershell
Get-Process Terraria,QTRHacker.Functions.Test -ErrorAction SilentlyContinue | Stop-Process -Force
```

3. Run the required VS MSBuild command.
4. Start Terraria again:

```powershell
Start-Process -FilePath 'D:\Apps\Steam\steamapps\common\Terraria\Terraria.exe' -WorkingDirectory 'D:\Apps\Steam\steamapps\common\Terraria'
```

5. Main menu:

```powershell
.\bin\Debug\QTRHacker.Functions.Test.exe --patches-only
.\bin\Debug\QTRHacker.Functions.Test.exe --smoke
```

6. After user enters a world:

```powershell
.\bin\Debug\QTRHacker.Functions.Test.exe --smoke
```

7. Final checks:

```powershell
git diff --check
Get-ChildItem 'D:\Apps\Steam\steamapps\common\Terraria' -Filter '*crashlog*'
Get-ChildItem 'D:\Dev\GitHub\QTRHacker\QTRHacker-QwQ' -Filter 'QTRHacker.Patches*.log'
```
