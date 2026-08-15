<#
.SYNOPSIS
  Maintainer packaging script (Windows) - builds a ready-to-ship Activation Planner distributable.

.DESCRIPTION
  Publishes the app self-contained (no separate .NET install needed on the user's machine),
  assembles the bundled VOACAP/NEC2++ engines from third_party/<rid>/ into the tools/ layout that
  ExternalToolLocator expects, copies the license notices, and zips the result into dist/.

  Run build-engines.ps1 first to populate third_party/<rid>/. If the engines are missing, the
  package is still produced (the app runs in sample-data mode), and this script warns loudly.

.EXAMPLE
  ./build/package.ps1                      # win-x64, version from the .csproj/default
  ./build/package.ps1 -Rid win-arm64 -Version 1.0.0
#>
[CmdletBinding()]
param(
    [string]$Rid = "win-x64",
    [string]$Version = "1.0.0",
    [switch]$SkipPublish,
    [switch]$NoZip          # publish + assemble only; skip zipping (CI signs the folder, then zips)
)
$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$Project  = Join-Path $RepoRoot "ActivationPlanner.UI\ActivationPlanner.UI.csproj"
$Dist     = Join-Path $RepoRoot "dist"
$StageRoot= Join-Path $Dist "ActivationPlanner-$Rid"
$AppDir   = $StageRoot                       # app lives at the root of the distributable folder

Write-Host "== Activation Planner packaging ==" -ForegroundColor Cyan
Write-Host "   RID     : $Rid"
Write-Host "   Version : $Version"
Write-Host "   Output  : $StageRoot"

# --- 1. Publish the app (self-contained) ---------------------------------------------------------
if (-not $SkipPublish) {
    if (Test-Path $StageRoot) { Remove-Item $StageRoot -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $AppDir | Out-Null
    Write-Host "-> dotnet publish (self-contained $Rid)..." -ForegroundColor Yellow
    & dotnet publish $Project -c Release -r $Rid --self-contained true `
        -p:Version=$Version -p:PublishSingleFile=false -o $AppDir
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed ($LASTEXITCODE)" }
} else {
    Write-Host "-> Skipping publish (using existing $AppDir)" -ForegroundColor DarkYellow
}

# --- 2. Bundle the engines from third_party/<rid> ------------------------------------------------
$engineSrc = Join-Path $RepoRoot "third_party\$Rid"
$toolsDst  = Join-Path $AppDir "tools"
$haveVoacap = $false; $haveNec = $false
if (Test-Path $engineSrc) {
    New-Item -ItemType Directory -Force -Path $toolsDst | Out-Null
    foreach ($tool in @("voacap","nec")) {
        $src = Join-Path $engineSrc $tool
        if (Test-Path $src) {
            Copy-Item $src (Join-Path $toolsDst $tool) -Recurse -Force
            if ($tool -eq "voacap") { $haveVoacap = $true } else { $haveNec = $true }
        }
    }
}
if (-not $haveVoacap) { Write-Warning "VOACAP engine not found in $engineSrc\voacap - app will run in SAMPLE mode. Run build-engines.ps1." }
if (-not $haveNec)    { Write-Warning "NEC2++ engine not found in $engineSrc\nec - antenna patterns will be SAMPLE. Run build-engines.ps1." }

# --- 3. License notices (Item #19: must ship + be shown) -----------------------------------------
$licSrc = Join-Path $RepoRoot "licenses"
$licDst = Join-Path $AppDir "licenses"
if (Test-Path $licSrc) { Copy-Item $licSrc $licDst -Recurse -Force }
Copy-Item (Join-Path $RepoRoot "docs\THIRD_PARTY_LICENSES.md") $licDst -Force -ErrorAction SilentlyContinue

# --- 3b. Sign the Windows app (optional, credential-gated) ---------------------------------------
# Only Windows builds get Authenticode signing. Signs the .exe BEFORE zipping so the app the user
# extracts and runs is signed. Skips cleanly (unsigned) if signing is not configured. A .zip itself
# cannot be Authenticode-signed, so we sign the executable inside it.
if ($Rid -like "win*") {
    $exe = Join-Path $AppDir "ActivationPlanner.UI.exe"
    & (Join-Path $PSScriptRoot "sign-windows.ps1") $exe
}

# --- 4. Zip the distributable --------------------------------------------------------------------
# CI passes -NoZip so it can sign the staged files first, then zip in a later step.
$zip = Join-Path $Dist "ActivationPlanner-$Version-$Rid.zip"
if (-not $NoZip) {
    if (Test-Path $zip) { Remove-Item $zip -Force }
    Write-Host "-> Compressing to $zip ..." -ForegroundColor Yellow
    Compress-Archive -Path (Join-Path $StageRoot "*") -DestinationPath $zip
} else {
    Write-Host "-> Skipping zip (-NoZip): staged folder left at $StageRoot" -ForegroundColor DarkYellow
}

Write-Host "== Done ==" -ForegroundColor Green
Write-Host "   Folder : $StageRoot"
if (-not $NoZip) { Write-Host "   Zip    : $zip" }
if (-not ($haveVoacap -and $haveNec)) {
    Write-Host "   NOTE   : engines missing - this build runs in sample mode until they are bundled." -ForegroundColor DarkYellow
}
