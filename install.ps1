<#
.SYNOPSIS
  Activation Planner - one-command setup for Windows.

.DESCRIPTION
  The easy way to install Activation Planner. Run it and press Enter at each prompt to accept the
  default. It installs the app (and the bundled VOACAP / NEC2++ engines if present), shows the
  required license notices, makes a Start Menu shortcut, and offers to launch.

  You can run it two ways:
    * From an unzipped download folder (the one containing ActivationPlanner.UI.exe), or
    * Point it at the downloaded .zip with  -Package <path-to-zip>

.EXAMPLE
  ./install.ps1
  ./install.ps1 -Package .\ActivationPlanner-1.0.0-win-x64.zip
  ./install.ps1 -InstallDir "D:\Apps\ActivationPlanner" -AcceptLicenses -NoPrompt
#>
[CmdletBinding()]
param(
    [string]$Package,                                   # path to a .zip; omit to install from this folder
    [string]$InstallDir = "$env:LOCALAPPDATA\Programs\Activation Planner",
    [switch]$AcceptLicenses,                            # skip the "press Enter to accept" gate (for automation)
    [switch]$NoPrompt,                                  # accept all defaults, no questions (for automation)
    [switch]$NoLaunch
)
$ErrorActionPreference = "Stop"

function Ask([string]$question, [string]$default) {
    if ($NoPrompt) { return $default }
    $a = Read-Host "$question [$default]"
    if ([string]::IsNullOrWhiteSpace($a)) { return $default } else { return $a }
}

Write-Host ""
Write-Host "===== Activation Planner - Setup =====" -ForegroundColor Cyan
Write-Host "A pre-operation planning tool for ham radio (POTA / SOTA / Field Day / EMCOMM)."
Write-Host ""

# --- 1. Find the source files (this folder, or a zip) --------------------------------------------
$workDir = $PSScriptRoot
$temp = $null
if ($Package) {
    if (-not (Test-Path $Package)) { throw "Package not found: $Package" }
    $temp = Join-Path ([System.IO.Path]::GetTempPath()) ("apsetup_" + [System.IO.Path]::GetRandomFileName())
    New-Item -ItemType Directory -Force -Path $temp | Out-Null
    Write-Host "-> Unpacking $Package ..." -ForegroundColor Yellow
    Expand-Archive -Path $Package -DestinationPath $temp -Force
    $workDir = $temp
}
$exe = Join-Path $workDir "ActivationPlanner.UI.exe"
if (-not (Test-Path $exe)) {
    throw "Could not find ActivationPlanner.UI.exe in '$workDir'. Run this from the unzipped download folder, or use -Package <zip>."
}

# --- 2. Show the license notices (Item #19: must be shown at install) -----------------------------
$licDir = Join-Path $workDir "licenses"
if (Test-Path $licDir) {
    Write-Host ""
    Write-Host "This product bundles two external engines under their own licenses:" -ForegroundColor Cyan
    Write-Host "  * VOACAP  - U.S. Government (NTIA/ITS) work; NTIA disclaimer applies."
    Write-Host "  * NEC2++  - GNU GPL v2; source offer included."
    Write-Host "Full notices are in the 'licenses' folder and will be copied into the install."
    if (-not $AcceptLicenses -and -not $NoPrompt) {
        Read-Host "Press Enter to accept and continue (Ctrl+C to cancel)" | Out-Null
    }
} else {
    Write-Warning "No 'licenses' folder found next to the app - your download may be incomplete."
}

# --- 3. Choose install location ------------------------------------------------------------------
$InstallDir = Ask "Install location" $InstallDir
Write-Host "-> Installing to: $InstallDir" -ForegroundColor Yellow
if (Test-Path $InstallDir) {
    if ((Ask "That folder exists. Overwrite it? (y/n)" "y") -match '^(y|yes)$') {
        Remove-Item $InstallDir -Recurse -Force
    } else { throw "Install cancelled." }
}
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null

# --- 4. Copy files -------------------------------------------------------------------------------
Write-Host "-> Copying files ..." -ForegroundColor Yellow
Copy-Item (Join-Path $workDir "*") $InstallDir -Recurse -Force
$installedExe = Join-Path $InstallDir "ActivationPlanner.UI.exe"

# --- 5. Report engine status ---------------------------------------------------------------------
$haveVoacap = Test-Path (Join-Path $InstallDir "tools\voacap")
$haveNec    = Test-Path (Join-Path $InstallDir "tools\nec")
Write-Host ""
if ($haveVoacap) { Write-Host "   VOACAP engine: bundled (real propagation predictions)." -ForegroundColor Green }
else { Write-Host "   VOACAP engine: NOT bundled - the app runs with sample predictions." -ForegroundColor DarkYellow }
if ($haveNec) { Write-Host "   NEC2++ engine: bundled (custom antenna modeling)." -ForegroundColor Green }
else { Write-Host "   NEC2++ engine: NOT bundled - antenna patterns are representative samples." -ForegroundColor DarkYellow }

# --- 6. Start Menu shortcut ----------------------------------------------------------------------
if ((Ask "Create a Start Menu shortcut? (y/n)" "y") -match '^(y|yes)$') {
    $startMenu = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs"
    $lnk = Join-Path $startMenu "Activation Planner.lnk"
    $ws = New-Object -ComObject WScript.Shell
    $sc = $ws.CreateShortcut($lnk)
    $sc.TargetPath = $installedExe
    $sc.WorkingDirectory = $InstallDir
    $sc.Description = "Activation Planner"
    $sc.Save()
    Write-Host "   Shortcut created: $lnk" -ForegroundColor Green
}

# --- 7. Done / launch ----------------------------------------------------------------------------
if ($temp -and (Test-Path $temp)) { Remove-Item $temp -Recurse -Force -ErrorAction SilentlyContinue }
Write-Host ""
Write-Host "===== Setup complete =====" -ForegroundColor Green
Write-Host "Installed to: $InstallDir"
if (-not $NoLaunch) {
    if ((Ask "Launch Activation Planner now? (y/n)" "y") -match '^(y|yes)$') {
        Start-Process $installedExe
    }
}
