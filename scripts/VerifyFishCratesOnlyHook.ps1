$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$hookPath = Join-Path $repoRoot 'src/QTRHacker.Patches/FishCratesOnlyHook.cs'
$togglesPath = Join-Path $repoRoot 'src/QTRHacker.Patches/PlayerToggles.cs'

if (-not (Test-Path -LiteralPath $hookPath)) {
    throw "FishCratesOnlyHook.cs was not found at $hookPath"
}

if (-not (Test-Path -LiteralPath $togglesPath)) {
    throw "PlayerToggles.cs was not found at $togglesPath"
}

$source = Get-Content -LiteralPath $hookPath -Raw
$togglesSource = Get-Content -LiteralPath $togglesPath -Raw

function Require-Source {
    param(
        [string]$Description,
        [scriptblock]$Predicate
    )

    if (-not (& $Predicate)) {
        throw "Fish crates only hook check failed: $Description"
    }
}

Require-Source 'patches SetFishingCheckResults, where the final bobber result is decided' {
    $source -match '"SetFishingCheckResults"'
}

Require-Source 'takes over FishingCheck_RollItemDrop before sonar and bobber state consume it' {
    $source -match '"FishingCheck_RollItemDrop"' -and
    $source -match 'FishingCheckRollItemDropPrefix' -and
    $source -match 'return false;'
}

Require-Source 'updates the FishingContext.Fisher value used by FishDropsDB' {
    $source -match 'Terraria\.GameContent\.FishDropRules' -and
    $source -match '\.Fisher\s*='
}

Require-Source 'reads the Terraria 1.4.5.6 static Projectile._context field' {
    $source -match 'BindingFlags\.Static' -and
    $source -match 'FishingContextField\.IsStatic'
}

Require-Source 'prevents enemy spawns from overriding the crate item result' {
    $source -match 'rolledEnemySpawn\s*=\s*0'
}

Require-Source 'guards final item results with ItemID.Sets.IsFishingCrate' {
    $source -match 'ItemID\.Sets\.IsFishingCrate'
}

Require-Source 'does not collapse every fallback crate to wooden or pearlwood' {
    $source -notmatch 'SelectFallbackCrate\(\)'
}

Require-Source 'selects fallback crates from fishing rarity tiers' {
    $source -match 'fisher\.legendary' -and
    $source -match 'fisher\.veryrare' -and
    $source -match 'fisher\.rare' -and
    $source -match 'fisher\.uncommon'
}

Require-Source 'routes the legacy bobber guard through the shared crate selector' {
    $togglesSource -match 'FishCratesOnlyHook\.SelectFallbackCrate\(' -and
    $togglesSource -notmatch 'private static int SelectFallbackCrate\(\)'
}

Write-Host 'OK FishCratesOnlyHook static regression checks'
