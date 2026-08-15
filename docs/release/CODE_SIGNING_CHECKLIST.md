# Code Signing Setup Checklist (Windows + macOS)

Set these **once, at the account level**, and every repo — including Activation Planner — inherits
them. The release workflow (`.github/workflows/release.yml`) stays **dormant** (ships unsigned) until
the on-switches are set, then signs automatically when you push a version tag.

> **Nothing sensitive lives in any repo file.** The workflow only references `secrets.*` / `vars.*`;
> GitHub encrypts secrets and masks them in logs.

---

## Where to set everything

**Account level (applies to all your repos):**
GitHub → your avatar → **Settings** → **Secrets and variables** → **Actions**
→ two tabs: **Secrets** and **Variables**.

Direct link: `https://github.com/settings/secrets/actions`

(If you already added any of these at the *repo* level for APRS-Command, re-add them here at the
**account** level to make them global.)

---

## Part 1 — Windows (Azure Trusted Signing)

**On-switch:** the `AZURE_SIGNING_ACCOUNT` variable. Until it's set, Windows ships unsigned.

### Variables (Settings → Secrets and variables → Actions → **Variables**)
- [ ] `AZURE_SIGNING_ENDPOINT` = `https://eus.codesigning.azure.net/`
- [ ] `AZURE_SIGNING_ACCOUNT` = `aprscommandsign`
- [ ] `AZURE_SIGNING_CERT_PROFILE` = `rospopo-public`

### Secrets (Settings → Secrets and variables → Actions → **Secrets**)
From a service principal with the **"Trusted Signing Certificate Profile Signer"** role
(`az ad sp create-for-rbac --name ap-signing --role "Trusted Signing Certificate Profile Signer" --scopes <your signing account resource id>`):
- [ ] `AZURE_TENANT_ID`
- [ ] `AZURE_CLIENT_ID`
- [ ] `AZURE_CLIENT_SECRET`

### Local Windows signing (optional, no CI)
- [ ] Run `az login` once on your laptop. Then `build/package.ps1` signs via the shared signer
      (`C:\Dev\Signing and Distribution\azure\sign.ps1`). Not logged in → ships unsigned. No other setup.

---

## Part 2 — macOS (Apple Developer ID)

**On-switch:** the `APPLE_SIGNING_IDENTITY` secret. Until it's set, macOS ships unsigned (Gatekeeper
fallback). **Signing runs on GitHub's cloud macOS runner — you do NOT need to be on your Mac to sign
a release; you only need the Mac once to create/export the credentials below.**

### One-time credential creation
- [ ] **Developer ID Application certificate:** create it (Apple Developer portal or Xcode →
      Settings → Accounts → Manage Certificates → +), then in **Keychain Access** right-click it →
      **Export** → save as a `.p12` and set a password (you'll use it as `APPLE_CERT_PASSWORD`).
- [ ] Note the identity string exactly, e.g. `Developer ID Application: James Rospopo (TEAMID)`.
- [ ] **App Store Connect API key** (for notarization): App Store Connect → **Users and Access** →
      **Integrations** → **App Store Connect API** → **+** → role **Developer**. Download the `.p8`
      (one time only), and note the **Key ID** and the **Issuer ID** shown on that page.

### Base64-encode the two files (run on your Mac)
- [ ] `base64 -i "Developer ID Application.p12" | pbcopy`  → paste into `APPLE_CERT_P12_BASE64`
- [ ] `base64 -i AuthKey_XXXXXXXXXX.p8 | pbcopy`           → paste into `APPLE_API_KEY_P8_BASE64`

### Secrets (Settings → Secrets and variables → Actions → **Secrets**)
- [ ] `APPLE_SIGNING_IDENTITY`   = `Developer ID Application: James Rospopo (TEAMID)`  ← the on-switch
- [ ] `APPLE_CERT_P12_BASE64`    = (base64 of the `.p12`)
- [ ] `APPLE_CERT_PASSWORD`      = (the password you set when exporting the `.p12`)
- [ ] `APPLE_API_KEY_P8_BASE64`  = (base64 of the `.p8`)
- [ ] `APPLE_API_KEY_ID`         = (the Key ID)
- [ ] `APPLE_API_ISSUER_ID`      = (the Issuer ID)

---

## Part 3 — Verify it works

- [ ] Commit/push, then tag a release:  `git tag v1.0.0 && git push origin v1.0.0`
- [ ] Watch the run under the repo's **Actions** tab.
- [ ] Windows: the "Sign (Azure Trusted Signing)" step runs (not skipped).
- [ ] macOS: the "Sign + notarize + staple" step runs (not skipped).
- [ ] A **draft Release** appears with the signed `.zip` / `.dmg` / `.tar.gz` assets.
- [ ] Download the Windows `.zip`, run it — **no SmartScreen "unknown publisher"** warning.
- [ ] Download the macOS `.dmg`, open the app — **no Gatekeeper "unidentified developer"** block.

---

## Quick reference — every name at a glance

| Type | Name | Value / source |
|---|---|---|
| Variable | `AZURE_SIGNING_ENDPOINT` | `https://eus.codesigning.azure.net/` |
| Variable | `AZURE_SIGNING_ACCOUNT` | `aprscommandsign` (Windows on-switch) |
| Variable | `AZURE_SIGNING_CERT_PROFILE` | `rospopo-public` |
| Secret | `AZURE_TENANT_ID` | Azure service principal |
| Secret | `AZURE_CLIENT_ID` | Azure service principal |
| Secret | `AZURE_CLIENT_SECRET` | Azure service principal |
| Secret | `APPLE_SIGNING_IDENTITY` | Developer ID string (macOS on-switch) |
| Secret | `APPLE_CERT_P12_BASE64` | base64 of exported `.p12` |
| Secret | `APPLE_CERT_PASSWORD` | `.p12` export password |
| Secret | `APPLE_API_KEY_P8_BASE64` | base64 of `.p8` |
| Secret | `APPLE_API_KEY_ID` | App Store Connect key ID |
| Secret | `APPLE_API_ISSUER_ID` | App Store Connect issuer ID |

_Not legal/security advice; these are configuration steps. Keep the exported `.p12`/`.p8` files
safe and out of any repo — only their base64 goes into GitHub secrets._
