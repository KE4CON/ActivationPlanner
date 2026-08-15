#!/usr/bin/env bash
# ============================================================
# Activation Planner - macOS .app bundle + .dmg
#
# Builds "Activation Planner.app" (and a .dmg) from a self-contained publish. No Apple Developer
# account or signing required to BUILD - the release workflow signs/notarizes separately (and only
# when configured). Unsigned, users bypass Gatekeeper with right-click -> Open on first launch.
#
# Usage:
#   bash build/make-macos-app.sh [osx-arm64|osx-x64] [--dmg-only]
#
# Requirements (macOS only): .NET 10 SDK; create-dmg optional (brew install create-dmg -> falls
# back to hdiutil). Mirrors the KE4CON/APRS-Command pattern.
# ============================================================
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
RID="${1:-osx-arm64}"

# --dmg-only (or DMG_ONLY=1) repackages the EXISTING .app into the .dmg without rebuilding it -
# used by the signing flow: build once, sign+notarize+staple the .app, then repackage the stapled
# .app into the .dmg (a plain re-run would rebuild the .app unsigned).
DMG_ONLY=0
if [[ "${2:-}" == "--dmg-only" || "${DMG_ONLY:-0}" == "1" ]]; then DMG_ONLY=1; fi

if [[ "$RID" != "osx-arm64" && "$RID" != "osx-x64" ]]; then
  echo "Usage: $0 [osx-arm64|osx-x64] [--dmg-only]" >&2
  exit 2
fi

VERSION="$(git -C "$REPO_ROOT" describe --tags --abbrev=0 2>/dev/null | sed 's/^v//' || echo "0.0.0-dev")"
ARCH="${RID#osx-}"
PUBLISH_DIR="$REPO_ROOT/artifacts/publish/$RID"
INSTALLER_DIR="$REPO_ROOT/artifacts/installers"
EXECUTABLE="ActivationPlanner.UI"
APP_NAME="Activation Planner"
APP_BUNDLE="$INSTALLER_DIR/$APP_NAME.app"
DMG_OUT="$INSTALLER_DIR/ActivationPlanner-$RID.dmg"

mkdir -p "$INSTALLER_DIR"

if [[ "$DMG_ONLY" -eq 1 ]]; then
  if [[ ! -d "$APP_BUNDLE" ]]; then
    echo "ERROR: --dmg-only requires an existing $APP_BUNDLE (build it first)." >&2
    exit 3
  fi
  echo "Repackaging existing (signed) $APP_BUNDLE into the DMG..."
else

# -- 1. Publish --------------------------------------------------------------------------------
if [[ ! -f "$PUBLISH_DIR/$EXECUTABLE" ]]; then
  echo "Publishing $RID..."
  dotnet publish "$REPO_ROOT/ActivationPlanner.UI/ActivationPlanner.UI.csproj" \
    -c Release -r "$RID" --self-contained true \
    -p:PublishSingleFile=false -p:Version="$VERSION" \
    -o "$PUBLISH_DIR"
fi

# -- 2. .app bundle ----------------------------------------------------------------------------
echo "Building $APP_NAME.app..."
rm -rf "$APP_BUNDLE"
mkdir -p "$APP_BUNDLE/Contents/MacOS" "$APP_BUNDLE/Contents/Resources"
cp -R "$PUBLISH_DIR"/. "$APP_BUNDLE/Contents/MacOS/"
chmod +x "$APP_BUNDLE/Contents/MacOS/$EXECUTABLE"

# Bundle the license notices (Item #19) and, if staged, the engines (else the app runs in sample mode).
[[ -d "$REPO_ROOT/licenses" ]] && cp -R "$REPO_ROOT/licenses" "$APP_BUNDLE/Contents/MacOS/licenses"
cp "$REPO_ROOT/docs/THIRD_PARTY_LICENSES.md" "$APP_BUNDLE/Contents/MacOS/licenses/" 2>/dev/null || true
if [[ -d "$REPO_ROOT/third_party/$RID" ]]; then
  mkdir -p "$APP_BUNDLE/Contents/MacOS/tools"
  [[ -d "$REPO_ROOT/third_party/$RID/voacap" ]] && cp -R "$REPO_ROOT/third_party/$RID/voacap" "$APP_BUNDLE/Contents/MacOS/tools/voacap"
  [[ -d "$REPO_ROOT/third_party/$RID/nec" ]] && cp -R "$REPO_ROOT/third_party/$RID/nec" "$APP_BUNDLE/Contents/MacOS/tools/nec"
fi

cat > "$APP_BUNDLE/Contents/Info.plist" << PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN"
  "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key>              <string>Activation Planner</string>
  <key>CFBundleDisplayName</key>       <string>Activation Planner</string>
  <key>CFBundleIdentifier</key>        <string>com.ke4con.activation-planner</string>
  <key>CFBundleVersion</key>           <string>$VERSION</string>
  <key>CFBundleShortVersionString</key><string>$VERSION</string>
  <key>CFBundleExecutable</key>        <string>$EXECUTABLE</string>
  <key>CFBundlePackageType</key>       <string>APPL</string>
  <key>LSMinimumSystemVersion</key>    <string>12.0</string>
  <key>NSHighResolutionCapable</key>   <true/>
  <key>NSHumanReadableCopyright</key>  <string>Copyright 2026 KE4CON. AGPL v3 / GPL v3.</string>
  <key>LSArchitecturePriority</key>
  <array><string>$ARCH</string></array>
  <key>NSAppTransportSecurity</key>
  <dict><key>NSAllowsLocalNetworking</key><true/></dict>
</dict>
</plist>
PLIST

echo "  -> $APP_BUNDLE"

fi  # end build-the-.app section (skipped in --dmg-only mode)

# -- 3. .dmg -----------------------------------------------------------------------------------
rm -f "$DMG_OUT"
DMG_STAGING="$(mktemp -d)"
trap "rm -rf '$DMG_STAGING'" EXIT
cp -R "$APP_BUNDLE" "$DMG_STAGING/"

if command -v create-dmg >/dev/null 2>&1; then
  echo "Building DMG with create-dmg..."
  create-dmg \
    --volname "Activation Planner" \
    --window-pos 200 120 --window-size 560 400 \
    --icon-size 100 \
    --icon "Activation Planner.app" 140 190 \
    --hide-extension "Activation Planner.app" \
    --app-drop-link 420 190 \
    "$DMG_OUT" "$DMG_STAGING/" 2>/dev/null || {
      echo "  create-dmg failed, falling back to hdiutil..."
      hdiutil create -volname "Activation Planner" -srcfolder "$DMG_STAGING" -ov -format UDZO "$DMG_OUT"
    }
elif command -v hdiutil >/dev/null 2>&1; then
  echo "Building DMG with hdiutil..."
  hdiutil create -volname "Activation Planner" -srcfolder "$DMG_STAGING" -ov -format UDZO "$DMG_OUT"
else
  echo "Neither create-dmg nor hdiutil found. App bundle is ready at: $APP_BUNDLE"
  exit 0
fi

echo ""
echo "Done."
echo "  App bundle : $APP_BUNDLE"
echo "  DMG        : $DMG_OUT"
