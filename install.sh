#!/usr/bin/env bash
# Activation Planner - one-command setup for macOS and Linux.
#
# The easy way to install Activation Planner. Run it and press Enter at each prompt to accept the
# default. It installs the app (and the bundled VOACAP / NEC2++ engines if present), shows the
# required license notices, and offers to launch.
#
# Run it two ways:
#   * From an unpacked download folder (the one containing the ActivationPlanner.UI program), or
#   * Point it at the downloaded tarball:   ./install.sh --package ActivationPlanner-1.0.0-linux-x64.tar.gz
#
# Options:
#   --package <file>   install from a .tar.gz instead of the current folder
#   --dir <path>       install location (default: $HOME/ActivationPlanner)
#   --yes              accept all defaults and the licenses, no questions (for automation)
#   --no-launch        do not offer to launch at the end
set -euo pipefail

PACKAGE=""
INSTALL_DIR="$HOME/ActivationPlanner"
ASSUME_YES=0
NO_LAUNCH=0
while [ $# -gt 0 ]; do
    case "$1" in
        --package) PACKAGE="$2"; shift 2;;
        --dir) INSTALL_DIR="$2"; shift 2;;
        --yes) ASSUME_YES=1; shift;;
        --no-launch) NO_LAUNCH=1; shift;;
        *) echo "Unknown option: $1"; exit 1;;
    esac
done

ask() { # ask "question" "default" -> echoes the answer
    local q="$1" d="$2" a=""
    if [ "$ASSUME_YES" -eq 1 ]; then echo "$d"; return; fi
    read -r -p "$q [$d]: " a || true
    if [ -z "$a" ]; then echo "$d"; else echo "$a"; fi
}

echo ""
echo "===== Activation Planner - Setup ====="
echo "A pre-operation planning tool for ham radio (POTA / SOTA / Field Day / EMCOMM)."
echo ""

# --- 1. Find the source files (this folder, or a tarball) ----------------------------------------
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WORK_DIR="$SCRIPT_DIR"
TEMP=""
if [ -n "$PACKAGE" ]; then
    [ -f "$PACKAGE" ] || { echo "Package not found: $PACKAGE"; exit 1; }
    TEMP="$(mktemp -d)"
    echo "-> Unpacking $PACKAGE ..."
    tar -xzf "$PACKAGE" -C "$TEMP"
    # The tarball contains a single top folder (ActivationPlanner-<rid>); descend into it.
    WORK_DIR="$(find "$TEMP" -maxdepth 2 -name ActivationPlanner.UI -type f -printf '%h\n' 2>/dev/null | head -n1)"
    [ -n "$WORK_DIR" ] || WORK_DIR="$TEMP"
fi
APP="$WORK_DIR/ActivationPlanner.UI"
if [ ! -f "$APP" ]; then
    echo "Could not find the ActivationPlanner.UI program in '$WORK_DIR'."
    echo "Run this from the unpacked download folder, or use --package <tarball>."
    exit 1
fi

# --- 2. Show the license notices (Item #19: must be shown at install) -----------------------------
if [ -d "$WORK_DIR/licenses" ]; then
    echo ""
    echo "This product bundles two external engines under their own licenses:"
    echo "  * VOACAP  - U.S. Government (NTIA/ITS) work; NTIA disclaimer applies."
    echo "  * NEC2++  - GNU GPL v2; source offer included."
    echo "Full notices are in the 'licenses' folder and will be copied into the install."
    if [ "$ASSUME_YES" -ne 1 ]; then
        read -r -p "Press Enter to accept and continue (Ctrl+C to cancel) " _ || true
    fi
else
    echo "WARNING: No 'licenses' folder found next to the app - your download may be incomplete."
fi

# --- 3. Choose install location + copy -----------------------------------------------------------
INSTALL_DIR="$(ask "Install location" "$INSTALL_DIR")"
echo "-> Installing to: $INSTALL_DIR"
if [ -d "$INSTALL_DIR" ]; then
    ans="$(ask "That folder exists. Overwrite it? (y/n)" "y")"
    case "$ans" in y|Y|yes|YES) rm -rf "$INSTALL_DIR";; *) echo "Install cancelled."; exit 1;; esac
fi
mkdir -p "$INSTALL_DIR"
echo "-> Copying files ..."
cp -R "$WORK_DIR/." "$INSTALL_DIR/"
INSTALLED_APP="$INSTALL_DIR/ActivationPlanner.UI"
chmod +x "$INSTALLED_APP" 2>/dev/null || true
find "$INSTALL_DIR/tools" -type f \( -name 'voacapl' -o -name 'nec2++' -o -name 'nec2c' \) -exec chmod +x {} \; 2>/dev/null || true

# --- 4. Report engine status ---------------------------------------------------------------------
echo ""
if [ -d "$INSTALL_DIR/tools/voacap" ]; then echo "   VOACAP engine: bundled (real propagation predictions)."
else echo "   VOACAP engine: NOT bundled - the app runs with sample predictions."; fi
if [ -d "$INSTALL_DIR/tools/nec" ]; then echo "   NEC2++ engine: bundled (custom antenna modeling)."
else echo "   NEC2++ engine: NOT bundled - antenna patterns are representative samples."; fi

# --- 5. Optional Linux .desktop launcher ---------------------------------------------------------
if [ "$(uname)" = "Linux" ]; then
    ans="$(ask "Create an application menu entry? (y/n)" "y")"
    case "$ans" in
        y|Y|yes|YES)
            APPS_DIR="$HOME/.local/share/applications"
            mkdir -p "$APPS_DIR"
            cat > "$APPS_DIR/activation-planner.desktop" <<EOF
[Desktop Entry]
Type=Application
Name=Activation Planner
Comment=Ham radio operating session planner
Exec="$INSTALLED_APP"
Path=$INSTALL_DIR
Terminal=false
Categories=Utility;HamRadio;
EOF
            echo "   Menu entry created: $APPS_DIR/activation-planner.desktop"
            ;;
    esac
fi

# --- 6. Done / launch ----------------------------------------------------------------------------
[ -n "$TEMP" ] && rm -rf "$TEMP" 2>/dev/null || true
echo ""
echo "===== Setup complete ====="
echo "Installed to: $INSTALL_DIR"
echo "Start it any time with: \"$INSTALLED_APP\""
if [ "$NO_LAUNCH" -ne 1 ]; then
    ans="$(ask "Launch Activation Planner now? (y/n)" "y")"
    case "$ans" in y|Y|yes|YES) ( "$INSTALLED_APP" >/dev/null 2>&1 & ) ;; esac
fi
