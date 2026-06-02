# Player Properties Editor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expand the player properties tab from appearance plus max life/mana into grouped common player attributes, including fishing power and luck.

**Architecture:** Keep the color editor, add a typed field descriptor list for common player properties, and bind the UI to grouped field view models. Refresh reads each descriptor from `Player`; Apply writes each edited value back.

**Tech Stack:** C# WPF, existing MVVM view models, existing localization JSON, existing `QTRHacker.Functions.Test --static` console checks.

---

### Task 1: Field Catalog and Static Test

**Files:**
- Modify: `src/QTRHacker/ViewModels/PlayerEditor/PlayerPropertiesEditorViewModel.cs`
- Modify: `src/QTRHacker.Functions.Test/QTRHacker.Functions.Test.csproj`
- Modify: `src/QTRHacker.Functions.Test/Program.cs`

- [ ] Add a public static field catalog for common player properties.
- [ ] Add a static compatibility test that requires groups for fishing/luck and core property names.
- [ ] Run `dotnet run --project src/QTRHacker.Functions.Test/QTRHacker.Functions.Test.csproj -- --static` and verify it fails before implementation.

### Task 2: View Model Binding

**Files:**
- Modify: `src/QTRHacker/ViewModels/PlayerEditor/PlayerPropertiesEditorViewModel.cs`

- [ ] Add grouped field view models with typed parsing for `int`, `float`, `byte`, and `bool`.
- [ ] Update `Update()` to refresh all fields from `Player`.
- [ ] Update `ApplyToGame()` to apply color fields and grouped player fields.
- [ ] Keep the legacy `MaxLife` and `MaxMana` properties synchronized for compatibility.

### Task 3: WPF Layout and Localization

**Files:**
- Modify: `src/QTRHacker/Views/PlayerEditor/PlayerPropertiesEditor.xaml`
- Modify: `src/QTRHacker/Views/PlayerEditor/PlayerEditorWindow.xaml`
- Modify: `src/QTRHacker/Localization/Content/zh.json`
- Modify: `src/QTRHacker/Localization/Content/en.json`

- [ ] Replace the old two-column numeric editor with a scrollable grouped layout.
- [ ] Add labels for all new common properties in Chinese and English.
- [ ] Increase the player editor height enough for the scrollable properties page.

### Task 4: Verification

**Files:**
- No additional files.

- [ ] Run `dotnet run --project src/QTRHacker.Functions.Test/QTRHacker.Functions.Test.csproj -- --static`.
- [ ] Run `dotnet build src/QTRHacker/QTRHacker.csproj -p:Configuration=Debug -p:Platform=x86`.
