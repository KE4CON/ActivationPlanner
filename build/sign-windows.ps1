<#
.SYNOPSIS
  Delegates Windows code signing to the ONE shared, cross-repo signer.

.DESCRIPTION
  Called by package.ps1 with the file(s) to sign. Rather than each repo carrying its own signing
  config, this delegates to a single central signer set up once for ALL your apps - by default
  "C:\Dev\Signing and Distribution\azure\sign.ps1" (which uses the Azure Artifact/Trusted Signing
  'sign' tool with az-cli credentials and its own metadata.json).

  Signing stays OPTIONAL and CREDENTIAL-GATED: the central script itself skips cleanly (produces an
  UNSIGNED build) when you're not logged in to Azure (az login). If the central signer can't be
  found at all, this shim also skips cleanly, so the build always succeeds.

  Resolution order for the central signer:
    1. $env:SIGN_SCRIPT      - full path to your shared sign.ps1 (set once, works in every repo)
    2. $env:SIGNING_DIR      - a folder containing sign.ps1
    3. Default: "C:\Dev\Signing and Distribution\azure\sign.ps1"

.EXAMPLE
  # one-time, so every repo signs with no per-repo setup:
  setx SIGN_SCRIPT "C:\Dev\Signing and Distribution\azure\sign.ps1"
#>
[CmdletBinding()]
param([Parameter(Mandatory = $true, ValueFromRemainingArguments = $true)][string[]]$Files)
$ErrorActionPreference = "Stop"

$signScript = $env:SIGN_SCRIPT
if (-not $signScript -and $env:SIGNING_DIR) {
    $signScript = Join-Path $env:SIGNING_DIR "sign.ps1"
}
if (-not $signScript) {
    $signScript = "C:\Dev\Signing and Distribution\azure\sign.ps1"
}

if (-not (Test-Path $signScript)) {
    Write-Host "   (shared signer not found - producing an UNSIGNED build.)" -ForegroundColor DarkYellow
    Write-Host "    Looked for: $signScript" -ForegroundColor DarkGray
    Write-Host "    Set `$env:SIGN_SCRIPT to your central sign.ps1 to enable signing in every repo." -ForegroundColor DarkGray
    return
}

Write-Host "-> Signing via shared signer: $signScript" -ForegroundColor Yellow
& $signScript @Files
