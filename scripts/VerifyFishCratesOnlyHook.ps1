$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$hookPath = Join-Path $repoRoot 'src/QTRHacker.Patches/FishCratesOnlyHook.cs'

if (-not (Test-Path -LiteralPath $hookPath)) {
    throw "FishCratesOnlyHook.cs was not found at $hookPath"
}

$source = Get-Content -LiteralPath $hookPath -Raw

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

Require-Source 'prevents enemy spawns from overriding the crate item result' {
    $source -match 'rolledEnemySpawn\s*=\s*0'
}

Require-Source 'guards final item results with ItemID.Sets.IsFishingCrate' {
    $source -match 'ItemID\.Sets\.IsFishingCrate'
}

Write-Host 'OK FishCratesOnlyHook static regression checks'
