<#
.SYNOPSIS
  Optional, credential-gated Authenticode signing for the Windows build.

.DESCRIPTION
  Called by package.ps1 with the path to ActivationPlanner.UI.exe. Signing is OFF by default: if the
  ACTIVATIONPLANNER_SIGN environment variable is not set, this prints a note and returns without
  signing, so the same pipeline produces an UNSIGNED build. When configured, it signs (and RFC3161
  timestamps) the executable so the app avoids the SmartScreen "unknown publisher" warning.

  This script NEVER embeds certificates, keys, or passwords. All secrets come from environment
  variables that YOU set before running package.ps1, and YOU run the signed build.

  Modes (set ACTIVATIONPLANNER_SIGN):
    trustedsigning  - Azure Trusted Signing (recommended). Uses the 'TrustedSigning' PowerShell
                      module (Invoke-TrustedSigning). Also needs:
                        TRUSTED_SIGNING_ENDPOINT   e.g. https://eus.codesigning.azure.net
                        TRUSTED_SIGNING_ACCOUNT    your Trusted Signing account name
                        TRUSTED_SIGNING_PROFILE    your certificate profile name
                      plus Azure auth (az login, or AZURE_TENANT_ID / AZURE_CLIENT_ID /
                      AZURE_CLIENT_SECRET for a service principal).
    keyvault        - A code-signing cert in Azure Key Vault, signed with AzureSignTool. Also needs:
                        AZURE_KEYVAULT_URL         https://<vault>.vault.azure.net
                        AZURE_KEYVAULT_CERT        certificate name
                      plus Azure auth (AZURE_TENANT_ID / AZURE_CLIENT_ID / AZURE_CLIENT_SECRET).

.EXAMPLE
  $env:ACTIVATIONPLANNER_SIGN = "trustedsigning"   # (set the mode-specific vars too)
  ./build/package.ps1                               # package.ps1 calls this signer automatically
#>
[CmdletBinding()]
param([Parameter(Mandatory = $true)][string]$ExePath)
$ErrorActionPreference = "Stop"

$mode = $env:ACTIVATIONPLANNER_SIGN
if ([string]::IsNullOrWhiteSpace($mode)) {
    Write-Host "   (code signing not configured - producing an UNSIGNED build.)" -ForegroundColor DarkYellow
    Write-Host "    Set `$env:ACTIVATIONPLANNER_SIGN to 'trustedsigning' or 'keyvault' to enable." -ForegroundColor DarkGray
    return
}
if (-not (Test-Path $ExePath)) { throw "sign-windows: file to sign not found: $ExePath" }

$timestampUrl = "http://timestamp.acs.microsoft.com"

switch ($mode.ToLowerInvariant()) {
    "trustedsigning" {
        foreach ($v in "TRUSTED_SIGNING_ENDPOINT","TRUSTED_SIGNING_ACCOUNT","TRUSTED_SIGNING_PROFILE") {
            if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($v))) {
                throw "sign-windows: ACTIVATIONPLANNER_SIGN=trustedsigning but $v is not set."
            }
        }
        if (-not (Get-Module -ListAvailable -Name TrustedSigning)) {
            throw "sign-windows: the 'TrustedSigning' PowerShell module is not installed. Run: Install-Module -Name TrustedSigning -Scope CurrentUser"
        }
        Import-Module TrustedSigning
        Write-Host "-> Signing (Azure Trusted Signing): $ExePath" -ForegroundColor Yellow
        Invoke-TrustedSigning `
            -Endpoint            $env:TRUSTED_SIGNING_ENDPOINT `
            -CodeSigningAccountName $env:TRUSTED_SIGNING_ACCOUNT `
            -CertificateProfileName $env:TRUSTED_SIGNING_PROFILE `
            -Files               $ExePath `
            -FileDigest          SHA256 `
            -TimestampRfc3161    $timestampUrl `
            -TimestampDigest     SHA256
    }
    "keyvault" {
        foreach ($v in "AZURE_KEYVAULT_URL","AZURE_KEYVAULT_CERT") {
            if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($v))) {
                throw "sign-windows: ACTIVATIONPLANNER_SIGN=keyvault but $v is not set."
            }
        }
        $azureSignTool = (Get-Command AzureSignTool -ErrorAction SilentlyContinue)?.Source
        if (-not $azureSignTool) {
            throw "sign-windows: AzureSignTool not found. Install it with: dotnet tool install --global AzureSignTool"
        }
        Write-Host "-> Signing (Azure Key Vault): $ExePath" -ForegroundColor Yellow
        # AzureSignTool reads AZURE_TENANT_ID/CLIENT_ID/CLIENT_SECRET from the environment for auth.
        & AzureSignTool sign `
            --azure-key-vault-url         $env:AZURE_KEYVAULT_URL `
            --azure-key-vault-certificate $env:AZURE_KEYVAULT_CERT `
            --azure-key-vault-tenant-id   $env:AZURE_TENANT_ID `
            --azure-key-vault-client-id   $env:AZURE_CLIENT_ID `
            --azure-key-vault-client-secret $env:AZURE_CLIENT_SECRET `
            --file-digest sha256 `
            --timestamp-rfc3161 $timestampUrl `
            --timestamp-digest sha256 `
            $ExePath
        if ($LASTEXITCODE -ne 0) { throw "AzureSignTool failed ($LASTEXITCODE)." }
    }
    default {
        throw "sign-windows: unknown ACTIVATIONPLANNER_SIGN mode '$mode' (use 'trustedsigning' or 'keyvault')."
    }
}
Write-Host "   signed + timestamped." -ForegroundColor Green
