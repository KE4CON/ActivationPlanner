#!/usr/bin/env bash
# Maintainer packaging script (macOS / Linux) - builds a ready-to-ship Activation Planner bundle.
#
# Publishes the app self-contained (no separate .NET install needed on the user's machine),
# assembles the bundled VOACAP/NEC2++ engines from third_party/<rid>/ into the tools/ layout that
# ExternalToolLocator expects, copies the license notices, and tars the result into dist/.
#
# Run build-engines.sh first to populate third_party/<rid>/. If the engines are missing, the bundle
# is still produced (the app runs in sample-data mode) and this script warns.
#
# Usage:
#   build/package.sh [rid] [version]
#   build/package.sh linux-x64 1.0.0
#   build/package.sh osx-arm64 1.0.0
set -euo pipefail

RID="${1:-linux-x64}"
VERSION="${2:-1.0.0}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT="$REPO_ROOT/ActivationPlanner.UI/ActivationPlanner.UI.csproj"
DIST="$REPO_ROOT/dist"
STAGE="$DIST/ActivationPlanner-$RID"

echo "== Activation Planner packaging =="
echo "   RID     : $RID"
echo "   Version : $VERSION"
echo "   Output  : $STAGE"

# --- 1. Publish the app (self-contained) ---------------------------------------------------------
rm -rf "$STAGE"
mkdir -p "$STAGE"
echo "-> dotnet publish (self-contained $RID)..."
dotnet publish "$PROJECT" -c Release -r "$RID" --self-contained true \
    -p:Version="$VERSION" -p:PublishSingleFile=false -o "$STAGE"

# --- 2. Bundle the engines from third_party/<rid> ------------------------------------------------
ENGINE_SRC="$REPO_ROOT/third_party/$RID"
TOOLS_DST="$STAGE/tools"
have_voacap=0; have_nec=0
if [ -d "$ENGINE_SRC" ]; then
    mkdir -p "$TOOLS_DST"
    if [ -d "$ENGINE_SRC/voacap" ]; then cp -R "$ENGINE_SRC/voacap" "$TOOLS_DST/voacap"; have_voacap=1; fi
    if [ -d "$ENGINE_SRC/nec" ];    then cp -R "$ENGINE_SRC/nec"    "$TOOLS_DST/nec";    have_nec=1; fi
    # Ensure the engine binaries are executable.
    find "$TOOLS_DST" -type f \( -name 'voacapl' -o -name 'nec2++' -o -name 'nec2c' \) -exec chmod +x {} \; 2>/dev/null || true
fi
[ "$have_voacap" -eq 1 ] || echo "WARNING: VOACAP engine not found in $ENGINE_SRC/voacap - app will run in SAMPLE mode. Run build-engines.sh."
[ "$have_nec" -eq 1 ]    || echo "WARNING: NEC2++ engine not found in $ENGINE_SRC/nec - antenna patterns will be SAMPLE. Run build-engines.sh."

# --- 3. License notices (Item #19: must ship + be shown) -----------------------------------------
if [ -d "$REPO_ROOT/licenses" ]; then cp -R "$REPO_ROOT/licenses" "$STAGE/licenses"; fi
cp "$REPO_ROOT/docs/THIRD_PARTY_LICENSES.md" "$STAGE/licenses/" 2>/dev/null || true

# --- 3b. Sign the macOS app (optional, credential-gated) -----------------------------------------
# Only macOS builds get Developer ID signing/notarization. Skips cleanly (unsigned) if signing is
# not configured. Runs before tarring so the shipped app is signed.
case "$RID" in
    osx-*) bash "$SCRIPT_DIR/sign-macos.sh" "$STAGE";;
esac

# --- 4. Make the app executable + tar the bundle -------------------------------------------------
chmod +x "$STAGE/ActivationPlanner.UI" 2>/dev/null || true
TARBALL="$DIST/ActivationPlanner-$VERSION-$RID.tar.gz"
rm -f "$TARBALL"
echo "-> Compressing to $TARBALL ..."
tar -czf "$TARBALL" -C "$DIST" "ActivationPlanner-$RID"

echo "== Done =="
echo "   Folder : $STAGE"
echo "   Tar    : $TARBALL"
if [ "$have_voacap" -ne 1 ] || [ "$have_nec" -ne 1 ]; then
    echo "   NOTE   : engines missing - this build runs in sample mode until they are bundled."
fi
