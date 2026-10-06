#!/bin/bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
MONO="/opt/unity/editors/2018.2.11f1/Data/MonoBleedingEdge/bin-linux64/mono"
MCS="/opt/unity/editors/2018.2.11f1/Data/MonoBleedingEdge/lib/mono/4.5/mcs.exe"
OUT="${1:-/tmp/lobby-net-probe}"
mkdir -p "$OUT"
cp "$ROOT/Assets/Plugins/Photon3Unity3D.dll" "$OUT/"
cp "$ROOT/Assets/Newtonsoft.Json.dll" "$OUT/"
"$MONO" "$MCS" -sdk:4.5 -optimize- \
  -r:"$OUT/Photon3Unity3D.dll" \
  -r:"$OUT/Newtonsoft.Json.dll" \
  -out:"$OUT/LobbyNetProbe.exe" \
  "$ROOT/Tools/LobbyNetProbe/LobbyNetProbe.cs"
echo "$OUT/LobbyNetProbe.exe"
