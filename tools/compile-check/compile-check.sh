#!/usr/bin/env bash
# Licence-free compile check for Sheytoonak (NOT a Unity build).
#
# Compiles every project assembly with the C# compiler that ships inside the Unity 6000.0.84f1
# editor, against that editor's own API assemblies, ugui 2.0.0 and Input System 1.20.0 (both taken
# from the same editor install), then runs the EditMode tests with a minimal reflection runner.
# The editor comes from GameCI's Docker image layer, pinned by digest; no Unity licence is used.
#
# What it does NOT prove: Unity's own script compilation pipeline (asmdef resolution, defines Unity
# adds, code generation), asset import, scene generation, IL2CPP, or the Android player build.
#
# Usage: bash tools/compile-check/compile-check.sh [WORK_DIR]   (needs ~6 GB free, curl, python3)
set -euo pipefail
HERE=$(cd "$(dirname "$0")" && pwd); PROJ=$(cd "$HERE/../.." && pwd)
WORK=${1:-${TMPDIR:-/tmp}/sheytoonak-compile-check}; mkdir -p "$WORK"
IMAGE=unityci/editor
# Editor layer of unityci/editor:ubuntu-6000.0.84f1-base-3 (linux/amd64), checked 2026-10-21.
LAYER=sha256:605ac50b56c596818fae397211fee255e70d5ff1801d2759bd8d88d87a8062f6
D=$WORK/opt/unity/Editor/Data
if [ ! -x "$D/NetCoreRuntime/dotnet" ]; then
  echo "Downloading the editor layer (~4.8 GB, only needed files are kept)..."
  TOKEN=$(curl -fsS "https://auth.docker.io/token?service=registry.docker.io&scope=repository:$IMAGE:pull" \
    | python3 -c 'import sys,json;print(json.load(sys.stdin)["token"])')
  curl -fsSL -H "Authorization: Bearer $TOKEN" "https://registry-1.docker.io/v2/$IMAGE/blobs/$LAYER" \
    | tar -xz -C "$WORK" --wildcards 'opt/unity/Editor/Data/Resources/PackageManager/BuiltInPackages/*' \
      'opt/unity/Editor/Data/Resources/PackageManager/Editor/com.unity.inputsystem-*' \
      'opt/unity/Editor/Data/Managed/*' 'opt/unity/Editor/Data/NetCoreRuntime/*' \
      'opt/unity/Editor/Data/DotNetSdkRoslyn/*' 'opt/unity/Editor/Data/NetStandard/*'
fi
PM=$D/Resources/PackageManager
IS_VERSION=$(python3 -c "import json;print(json.load(open('$PROJ/Packages/manifest.json'))['dependencies']['com.unity.inputsystem'])")
IS_TGZ=$PM/Editor/com.unity.inputsystem-$IS_VERSION.tgz
[ -f "$IS_TGZ" ] || { echo "Input System $IS_VERSION is not bundled with this editor ($IS_TGZ)"; exit 1; }
rm -rf "$WORK/is" && mkdir -p "$WORK/is" && tar -xzf "$IS_TGZ" -C "$WORK/is"

OUT=$WORK/out; rm -rf "$OUT"; mkdir -p "$OUT"
DOTNET=$D/NetCoreRuntime/dotnet; CSC=$D/DotNetSdkRoslyn/csc.dll
NETSTD="-r:$D/NetStandard/ref/2.1.0/netstandard.dll $(ls $D/NetStandard/compat/2.1.0/shims/netfx/*.dll | sed 's/^/-r:/' | tr '\n' ' ')"
ENGINE=$(ls $D/Managed/UnityEngine/UnityEngine*.dll | sed 's/^/-r:/' | tr '\n' ' ')
EDITOR=$(ls $D/Managed/UnityEngine/UnityEditor*.dll | sed 's/^/-r:/' | tr '\n' ' ')
V=""; for y in 2017 2018 2019 2020 2021 2022 2023; do for m in 1 2 3 4; do V="$V;UNITY_${y}_${m}_OR_NEWER"; done; done
# Approximation of the defines Unity adds: Android target, Active Input Handling = Both, plus the
# Input System's own versionDefines that apply to 6000.0.84f1 with ugui and physics installed.
COMMON="UNITY_6000_0_OR_NEWER;UNITY_6000_0_84;UNITY_6000_0;UNITY_6000;UNITY_5_3_OR_NEWER;UNITY_5_4_OR_NEWER;UNITY_5_5_OR_NEWER;UNITY_5_6_OR_NEWER$V;NET_STANDARD_2_0;NET_STANDARD_2_1;NET_STANDARD;NETSTANDARD2_1;NETSTANDARD;CSHARP_7_3_OR_NEWER;ENABLE_INPUT_SYSTEM;ENABLE_LEGACY_INPUT_MANAGER;UNITY_ANDROID;PLATFORM_ANDROID;UNITY_64;ENABLE_IL2CPP;UNITY_INPUT_SYSTEM_ENABLE_PHYSICS;UNITY_INPUT_SYSTEM_ENABLE_UI;UNITY_INPUT_SYSTEM_PLATFORM_SCROLL_DELTA;UNITY_INPUT_SYSTEM_INPUT_MODULE_SCROLL_DELTA;UNITY_INPUT_SYSTEM_SENDPOINTERHOVERTOPARENT"
PLAYER="-define:$COMMON"
EDIT="-define:$COMMON;UNITY_EDITOR;UNITY_EDITOR_LINUX;UNITY_EDITOR_64;UNITY_INCLUDE_TESTS"
CS="$DOTNET $CSC -nologo -noconfig -nostdlib -langversion:9.0 -target:library -nowarn:1701,1702,CS0618 -deterministic"
fail=0
compile() { # dir-label assembly-name rsp args...   (output keeps the real assembly name: InternalsVisibleTo)
  local label=$1 asm=$2 list=$3; shift 3; mkdir -p "$OUT/$label"
  if $CS -out:"$OUT/$label/$asm.dll" "$@" @"$list" > "$OUT/$label.log" 2>&1; then
    echo "OK   $label/$asm ($(wc -l < "$list") files)"
  else
    echo "FAIL $label/$asm"; grep -E 'error' "$OUT/$label.log" | sort -u | head -40; fail=1
  fi
}
find "$PM/BuiltInPackages/com.unity.ugui/Runtime/UGUI" -name '*.cs' > "$OUT/ugui.rsp"
find "$WORK/is/package/InputSystem" -name '*.cs' -not -path '*/Plugins/InputForUI/*' > "$OUT/is.rsp"
find "$PROJ/Assets/Scripts/Core" -name '*.cs' > "$OUT/core.rsp"
find "$PROJ/Assets/Scripts" -name '*.cs' -not -path '*/Core/*' -not -path '*/Notifications/*' > "$OUT/game.rsp"
# Scripts/Notifications is its own Android-only assembly on com.unity.mobile.notifications (not in this toolchain): CI compiles it.
find "$PROJ/Assets/Editor" -name '*.cs' > "$OUT/editor.rsp"
find "$PROJ/Assets/Tests/EditMode" -name '*.cs' > "$OUT/tests.rsp"
NUNIT_DIR=$PM/BuiltInPackages/com.unity.ext.nunit/net40/unity-custom
compile ugui UnityEngine.UI "$OUT/ugui.rsp" $NETSTD $ENGINE $PLAYER
compile inputsystem Unity.InputSystem "$OUT/is.rsp" -unsafe $NETSTD $ENGINE -r:"$OUT/ugui/UnityEngine.UI.dll" $PLAYER
compile core Sheytoonak.Core "$OUT/core.rsp" $NETSTD $PLAYER   # noEngineReferences
compile game-player Sheytoonak.Game "$OUT/game.rsp" $NETSTD $ENGINE -r:"$OUT/core/Sheytoonak.Core.dll" -r:"$OUT/inputsystem/Unity.InputSystem.dll" $PLAYER
compile game-editor Sheytoonak.Game "$OUT/game.rsp" $NETSTD $ENGINE $EDITOR -r:"$OUT/core/Sheytoonak.Core.dll" -r:"$OUT/inputsystem/Unity.InputSystem.dll" $EDIT
[ $fail -eq 0 ] && compile editor Sheytoonak.Editor "$OUT/editor.rsp" $NETSTD $ENGINE $EDITOR -r:"$OUT/core/Sheytoonak.Core.dll" -r:"$OUT/game-editor/Sheytoonak.Game.dll" -r:"$OUT/inputsystem/Unity.InputSystem.dll" $EDIT
compile tests Sheytoonak.Tests.EditMode "$OUT/tests.rsp" $NETSTD $ENGINE -r:"$OUT/core/Sheytoonak.Core.dll" -r:"$NUNIT_DIR/nunit.framework.dll" $EDIT
[ $fail -eq 0 ] || exit 1

R=$(ls -d $D/NetCoreRuntime/shared/Microsoft.NETCore.App/*/ | head -1)
RT_VERSION=$(basename "$R")
refs=""; for f in System.Private.CoreLib System.Runtime System.Console System.Linq System.Collections System.Reflection.Primitives System.Runtime.Loader; do refs="$refs -r:$R/$f.dll"; done
$DOTNET $CSC -nologo -nostdlib -out:"$OUT/RunTests.dll" $refs "$HERE/RunTests.cs"
echo "{\"runtimeOptions\":{\"tfm\":\"net6.0\",\"framework\":{\"name\":\"Microsoft.NETCore.App\",\"version\":\"$RT_VERSION\"}}}" > "$OUT/RunTests.runtimeconfig.json"
$DOTNET "$OUT/RunTests.dll" "$OUT/tests/Sheytoonak.Tests.EditMode.dll" "$OUT/core" "$NUNIT_DIR"
