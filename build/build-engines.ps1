<#
.SYNOPSIS
  Maintainer engine builder (Windows) - builds VOACAP + NEC2++ into third_party/win-x64/.

.DESCRIPTION
  Building voacapl (Fortran) and necpp (C++) on Windows needs a Unix-style toolchain. The reliable
  route is MSYS2 (https://www.msys2.org). This script finds an MSYS2/Git-for-Windows bash and runs
  the shared build-engines.sh under it, targeting win-x64. If it cannot find a suitable bash with a
  Fortran compiler, it prints exactly what to install.

  IMPORTANT (Item #19): binaries built under MSYS2/Cygwin need their runtime DLLs beside the .exe
  (e.g. the MSYS2 gcc/gfortran runtime, or cygwin1.dll for a Cygwin build). After building, copy
  those DLLs into third_party/win-x64/voacap and third_party/win-x64/nec, or build a static/native
  variant. The packaging script bundles whatever is present in those folders.

.EXAMPLE
  ./build/build-engines.ps1
#>
[CmdletBinding()]
param()
$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$sh = Join-Path $PSScriptRoot "build-engines.sh"

Write-Host "== Activation Planner - build engines (Windows) ==" -ForegroundColor Cyan

# Prefer an MSYS2 bash (has a real toolchain); fall back to any bash on PATH.
$candidates = @(
    "C:\msys64\usr\bin\bash.exe",
    "C:\msys64\mingw64.exe",
    "$env:ProgramFiles\Git\bin\bash.exe"
) | Where-Object { Test-Path $_ }
$bash = $candidates | Select-Object -First 1
if (-not $bash) { $bash = (Get-Command bash -ErrorAction SilentlyContinue).Source }

if (-not $bash) {
    Write-Warning "No bash toolchain found."
    Write-Host ""
    Write-Host "Install MSYS2 (https://www.msys2.org), then in an 'MSYS2 MINGW64' shell run:" -ForegroundColor Yellow
    Write-Host "  pacman -S git make mingw-w64-x86_64-gcc mingw-w64-x86_64-gcc-fortran autoconf automake libtool"
    Write-Host "  cd '$($RepoRoot -replace '\\','/')'"
    Write-Host "  build/build-engines.sh win-x64"
    exit 1
}

Write-Host "Using bash: $bash" -ForegroundColor DarkCyan
Write-Host "Delegating to build-engines.sh (win-x64)..." -ForegroundColor Yellow
# Convert the script path to a form bash accepts.
$shPath = $sh -replace '\\','/'
& $bash -lc "'$shPath' win-x64"
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Engine build did not complete. If the failure was a missing Fortran/C++ compiler, install the MSYS2 packages listed above and retry from an MSYS2 MINGW64 shell."
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Reminder: copy the MSYS2 runtime DLLs next to the built .exe files in" -ForegroundColor DarkYellow
Write-Host "  third_party/win-x64/voacap  and  third_party/win-x64/nec" -ForegroundColor DarkYellow
Write-Host "so they run on a machine without MSYS2 installed." -ForegroundColor DarkYellow
