#!/usr/bin/env bash
# Optional, credential-gated code signing + notarization for the macOS build.
#
# Called by package.sh with the staged app folder. Signing is OFF by default: if
# ACTIVATIONPLANNER_SIGN is not set, this prints a note and returns without signing, so the same
# pipeline produces an UNSIGNED build. When configured, it code-signs the app with a Developer ID
# Application certificate (hardened runtime) and, if notarization credentials are present, notarizes
# it so the app avoids the Gatekeeper "unidentified developer" block.
#
# This script NEVER embeds certificates, keys, or passwords. All secrets come from environment
# variables / your login keychain that YOU set up, and YOU run the signed build.
#
# Enable by setting:
#   ACTIVATIONPLANNER_SIGN=developerid
#   APPLE_SIGN_IDENTITY="Developer ID Application: Your Name (TEAMID)"   # a cert in your keychain
# Notarization (optional but recommended) - EITHER a stored notarytool profile:
#   APPLE_NOTARY_PROFILE="ap-notary"        # created once: xcrun notarytool store-credentials ...
# OR the three raw values:
#   APPLE_ID="you@example.com"  APPLE_TEAM_ID="TEAMID"  APPLE_APP_PASSWORD="app-specific-password"
#
# NOTE: for a fully *stapled* distribution, macOS wants a .app bundle (usually inside a .dmg). This
# script signs the shipped executable + bundled Mach-O binaries and can notarize a zip; producing a
# proper .app/.dmg is a recommended future step.
set -euo pipefail

APP_DIR="${1:?sign-macos: pass the staged app folder}"

if [ -z "${ACTIVATIONPLANNER_SIGN:-}" ]; then
    echo "   (code signing not configured - producing an UNSIGNED build.)"
    echo "    Set ACTIVATIONPLANNER_SIGN=developerid (and APPLE_SIGN_IDENTITY) to enable."
    exit 0
fi
if [ "$ACTIVATIONPLANNER_SIGN" != "developerid" ]; then
    echo "sign-macos: unknown ACTIVATIONPLANNER_SIGN mode '$ACTIVATIONPLANNER_SIGN' (use 'developerid')." >&2
    exit 1
fi
: "${APPLE_SIGN_IDENTITY:?sign-macos: APPLE_SIGN_IDENTITY not set}"
command -v codesign >/dev/null 2>&1 || { echo "sign-macos: codesign not found (Xcode command line tools required)." >&2; exit 1; }

echo "-> Code signing (Developer ID, hardened runtime): $APP_DIR"
# Sign nested Mach-O binaries first, then the main executable.
while IFS= read -r -d '' f; do
    codesign --force --timestamp --options runtime --sign "$APPLE_SIGN_IDENTITY" "$f" || true
done < <(find "$APP_DIR" -type f \( -name '*.dylib' -o -perm -u+x \) -print0)
codesign --force --timestamp --options runtime --sign "$APPLE_SIGN_IDENTITY" "$APP_DIR/ActivationPlanner.UI"
echo "   signed."

# --- Notarization (optional) ---------------------------------------------------------------------
notarize=0
if [ -n "${APPLE_NOTARY_PROFILE:-}" ] || { [ -n "${APPLE_ID:-}" ] && [ -n "${APPLE_TEAM_ID:-}" ] && [ -n "${APPLE_APP_PASSWORD:-}" ]; }; then
    notarize=1
fi
if [ "$notarize" -eq 1 ] && command -v xcrun >/dev/null 2>&1; then
    zip="$APP_DIR.notarize.zip"
    echo "-> Notarizing (this can take a few minutes)..."
    ( cd "$(dirname "$APP_DIR")" && ditto -c -k --keepParent "$(basename "$APP_DIR")" "$zip" )
    if [ -n "${APPLE_NOTARY_PROFILE:-}" ]; then
        xcrun notarytool submit "$zip" --keychain-profile "$APPLE_NOTARY_PROFILE" --wait
    else
        xcrun notarytool submit "$zip" --apple-id "$APPLE_ID" --team-id "$APPLE_TEAM_ID" --password "$APPLE_APP_PASSWORD" --wait
    fi
    rm -f "$zip"
    echo "   notarized. (Stapling needs a .app/.dmg - recommended future step.)"
else
    echo "   (notarization not configured - signed but not notarized; Gatekeeper may still warn.)"
fi
