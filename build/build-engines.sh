#!/usr/bin/env bash
# Maintainer engine builder (macOS / Linux, and Windows via MSYS2/Cygwin bash).
#
# Builds the two bundled engines from their public sources and stages them into
# third_party/<rid>/ in the exact layout ExternalToolLocator + the packaging scripts expect:
#
#   third_party/<rid>/voacap/voacapl        (or voacapl.exe on Windows)
#   third_party/<rid>/voacap/itshfbc/       (VOACAP data directory)
#   third_party/<rid>/nec/nec2++            (or nec2++.exe on Windows)
#
# It also copies NEC2++'s GPLv2 COPYING and a source snapshot into licenses/nec2++/ so the
# corresponding-source obligation always matches the exact version shipped.
#
# Sources (redistribution permitted - see docs/THIRD_PARTY_LICENSES.md):
#   VOACAP : https://github.com/jawatson/voacapl   (U.S. Gov / CC0 port)
#   NEC2++ : https://github.com/tmolteno/necpp      (GPLv2)
#
# Usage:
#   build/build-engines.sh [rid]
#   build/build-engines.sh              # auto-detect rid from this machine
#   build/build-engines.sh linux-x64
#   build/build-engines.sh --check      # only check for the required build tools, then exit
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# --- Determine the target RID --------------------------------------------------------------------
detect_rid() {
    local os arch
    case "$(uname -s)" in
        Linux)  os="linux";;
        Darwin) os="osx";;
        MINGW*|MSYS*|CYGWIN*) os="win";;
        *) os="linux";;
    esac
    case "$(uname -m)" in
        x86_64|amd64) arch="x64";;
        arm64|aarch64) arch="arm64";;
        *) arch="x64";;
    esac
    echo "$os-$arch"
}

CHECK_ONLY=0
RID=""
for a in "$@"; do
    case "$a" in
        --check) CHECK_ONLY=1;;
        *) RID="$a";;
    esac
done
[ -n "$RID" ] || RID="$(detect_rid)"

EXE_SUFFIX=""
case "$RID" in win-*) EXE_SUFFIX=".exe";; esac

# --- Preflight: required tools -------------------------------------------------------------------
missing=0
need() { command -v "$1" >/dev/null 2>&1 || { echo "  MISSING: $1 - $2"; missing=1; }; }
echo "== Checking build tools =="
need git    "install git"
need make   "install make (build-essential on Debian/Ubuntu, Xcode CLT on macOS)"
need gcc    "install a C compiler (build-essential / Xcode CLT)"
need g++    "install a C++ compiler (build-essential / Xcode CLT)"
need gfortran "install gfortran (Fortran compiler; 'sudo apt install gfortran' or 'brew install gcc')"
need autoreconf "install autotools (autoconf, automake, libtool)"
if [ "$missing" -ne 0 ]; then
    echo ""
    echo "Install the missing tools above, then re-run. Quick hints:"
    echo "  Debian/Ubuntu : sudo apt install git build-essential gfortran autoconf automake libtool"
    echo "  Fedora        : sudo dnf install git make gcc gcc-c++ gcc-gfortran autoconf automake libtool"
    echo "  macOS         : xcode-select --install ; brew install gcc autoconf automake libtool"
    echo "  Windows       : use MSYS2 (pacman -S git make mingw-w64-x86_64-gcc mingw-w64-x86_64-gcc-fortran autoconf automake libtool)"
    exit 1
fi
echo "  all present."
[ "$CHECK_ONLY" -eq 1 ] && { echo "(--check only) done."; exit 0; }

STAGE="$REPO_ROOT/third_party/$RID"
BUILD="$REPO_ROOT/build/.engine-build"
mkdir -p "$STAGE/voacap" "$STAGE/nec" "$BUILD"
echo ""
echo "== Building engines for RID: $RID =="
echo "   staging -> $STAGE"

# --- 1. VOACAP (voacapl) -------------------------------------------------------------------------
echo ""
echo "-> VOACAP (voacapl)"
VOA_SRC="$BUILD/voacapl"
[ -d "$VOA_SRC" ] || git clone --depth 1 https://github.com/jawatson/voacapl.git "$VOA_SRC"
(
    cd "$VOA_SRC"
    [ -x ./configure ] || ./autogen.sh 2>/dev/null || autoreconf -i
    ./configure --prefix="$BUILD/voacapl-install"
    make
    make install
)
# Binary: prefer the installed one, else the built one under src/.
VOA_BIN="$(find "$BUILD/voacapl-install" "$VOA_SRC" -type f -name "voacapl$EXE_SUFFIX" 2>/dev/null | head -n1)"
[ -n "$VOA_BIN" ] || { echo "ERROR: voacapl binary not found after build."; exit 1; }
cp "$VOA_BIN" "$STAGE/voacap/voacapl$EXE_SUFFIX"; chmod +x "$STAGE/voacap/voacapl$EXE_SUFFIX" 2>/dev/null || true
# Data dir (itshfbc): prefer the installed copy, else the source tree's copy.
VOA_DATA="$(find "$BUILD/voacapl-install" "$HOME" "$VOA_SRC" -maxdepth 4 -type d -name itshfbc 2>/dev/null | head -n1)"
[ -n "$VOA_DATA" ] || { echo "ERROR: VOACAP itshfbc data dir not found."; exit 1; }
rm -rf "$STAGE/voacap/itshfbc"; cp -R "$VOA_DATA" "$STAGE/voacap/itshfbc"
echo "   staged voacapl + itshfbc"

# --- 2. NEC2++ (necpp) ---------------------------------------------------------------------------
echo ""
echo "-> NEC2++ (necpp)"
NEC_SRC="$BUILD/necpp"
[ -d "$NEC_SRC" ] || git clone --depth 1 https://github.com/tmolteno/necpp.git "$NEC_SRC"
(
    cd "$NEC_SRC"
    [ -x ./configure ] || { make -f Makefile.git 2>/dev/null || autoreconf -i; }
    ./configure
    make
)
NEC_BIN="$(find "$NEC_SRC" -type f -name "nec2++$EXE_SUFFIX" 2>/dev/null | head -n1)"
[ -n "$NEC_BIN" ] || { echo "ERROR: nec2++ binary not found after build."; exit 1; }
cp "$NEC_BIN" "$STAGE/nec/nec2++$EXE_SUFFIX"; chmod +x "$STAGE/nec/nec2++$EXE_SUFFIX" 2>/dev/null || true
echo "   staged nec2++"

# --- 3. GPLv2 compliance: ship COPYING + corresponding source ------------------------------------
LIC_NEC="$REPO_ROOT/licenses/nec2++"
mkdir -p "$LIC_NEC"
cp "$NEC_SRC/COPYING" "$LIC_NEC/COPYING" 2>/dev/null || echo "WARNING: necpp COPYING not found - fetch GPLv2 text manually."
# A source snapshot satisfies the corresponding-source obligation directly in the install.
( cd "$NEC_SRC" && git archive --format=tar.gz -o "$LIC_NEC/necpp-source.tar.gz" HEAD 2>/dev/null ) \
    || echo "NOTE: could not archive necpp source; the offer in NEC2++-Source-Offer.txt still applies."
echo "   staged NEC2++ GPLv2 COPYING + source snapshot"

echo ""
echo "== Engines built and staged for $RID =="
echo "   $STAGE/voacap/voacapl$EXE_SUFFIX + itshfbc/"
echo "   $STAGE/nec/nec2++$EXE_SUFFIX"
echo "Next: build/package.sh $RID <version>   (or package.ps1 on Windows)"
