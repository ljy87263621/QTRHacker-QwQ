# QTRHacker

QTRHacker is a Windows desktop toolset for Terraria. It combines a WPF
interface, native CLR helpers, memory access utilities, runtime patches, and
game data helpers for local Terraria experimentation.

The current tree includes support work for Terraria 1.4.5.6.

## Requirements

- Windows 10 or later, x64
- Terraria for Windows
- [.NET 7 Desktop Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/7.0) x86
- .NET Framework 4.6 for Terraria
- Visual Studio 2022 with .NET desktop development and Desktop development with C++
  if you want to build the full solution

## Repository Layout

- `src/QTRHacker` - WPF desktop application.
- `src/QTRHacker.Core` - Terraria object wrappers, patch management, and shared
  runtime logic.
- `src/QTRHacker.Patches` - managed runtime patches injected into the game
  process.
- `src/QHackLib` - remote memory, CLR, and hook helpers.
- `src/QHackCLR` - native CLR/DAC support project.
- `src/Launcher` - native launcher project.
- `res` - bundled scripts and content resources.
- `GameRefs` and `refdlls` - reference assemblies and native dependencies used
  by the solution.

## Build

Open `QTRHacker.sln` in Visual Studio 2022 and build the `Release|x86`
configuration.

From a developer command prompt, you can also run:

```powershell
dotnet restore QTRHacker.sln
dotnet build QTRHacker.sln -c Release -p:Platform=x86
```

Build outputs are written under `bin\Release`.

## Usage Notes

- Keep the generated binaries and bundled resources together.
- Start Terraria before using features that attach to the game process.
- Use this project responsibly and prefer local/offline testing.

## Contact

Discord: https://discord.gg/bzKc9vM

QQ: 2393868407

Group: 850984295
