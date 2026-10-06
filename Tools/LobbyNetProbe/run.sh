#!/bin/bash
# Run a multi-client lobby probe in the nearest Photon region and the
# farthest reachable region. Each case starts independent client processes
# that join one room and compare team, ready, and master state.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
MONO="/opt/unity/editors/2018.2.11f1/Data/MonoBleedingEdge/bin-linux64/mono"
OUT="${LOBBY_PROBE_OUT:-/opt/cursor/artifacts/lobby-probe}"
ENV_LABEL="${LOBBY_PROBE_ENV:-$(hostname)}"
CLIENTS="${LOBBY_PROBE_CLIENTS:-3}"
mkdir -p "$OUT"
EXE="$("$ROOT/Tools/LobbyNetProbe/build.sh" /tmp/lobby-net-probe)"

echo "survey from $ENV_LABEL"
"$MONO" "$EXE" --survey --out "$OUT/regions.json"

python3 - "$OUT/regions.json" << 'PY'
import json, sys
data = json.load(open(sys.argv[1]))
ok = [r for r in data.get("regions", []) if r.get("rttMs", -1) > 0]
if not ok:
    sys.stderr.write("no reachable Photon regions\n")
    sys.exit(1)
best = min(ok, key=lambda r: r["rttMs"])
worst = max(ok, key=lambda r: r["rttMs"])
open(sys.argv[1] + ".pick", "w").write(best["code"] + "\n" + worst["code"] + "\n")
print("best", best["code"], best["rttMs"], "worst", worst["code"], worst["rttMs"])
PY

BEST="$(sed -n '1p' "$OUT/regions.json.pick")"
WORST="$(sed -n '2p' "$OUT/regions.json.pick")"

run_case() {
  local region="$1"
  local name="$2"
  local dir="$OUT/$name"
  mkdir -p "$dir"
  local room="lb$(date +%s)$RANDOM"
  local pids=()
  echo "case $name region $region room $room clients $CLIENTS"
  local i
  for ((i = 2; i <= CLIENTS; i++)); do
    "$MONO" "$EXE" --role join --room "$room" --region "$region" --nick "c$i" \
      --env "$ENV_LABEL" --expect "$CLIENTS" --wait 25 --hold 12 \
      --out "$dir/c$i.json" > "$dir/c$i.log" 2>&1 &
    pids+=("$!")
  done
  "$MONO" "$EXE" --role host --room "$room" --region "$region" --nick host \
    --env "$ENV_LABEL" --expect "$CLIENTS" --wait 25 --hold 12 \
    --out "$dir/host.json" > "$dir/host.log" 2>&1 &
  pids+=("$!")
  local status=0
  local pid
  for pid in "${pids[@]}"; do
    if ! wait "$pid"; then
      status=1
    fi
  done
  set +e
  "$MONO" "$EXE" --compare "$dir"/host.json "$dir"/c*.json | tee "$dir/summary.txt"
  local compare_status=${PIPESTATUS[0]}
  set -e
  if [[ $compare_status -ne 0 ]]; then
    status=$compare_status
  fi
  echo "$status" > "$dir/exit-code"
  return 0
}

run_case "$BEST" "best-$BEST"
if [[ "$WORST" == "$BEST" ]]; then
  echo "only one reachable region ($BEST); skipped the distant-region case" | tee "$OUT/worst-skipped.txt"
else
  run_case "$WORST" "worst-$WORST"
fi

python3 - "$OUT" << 'PY'
import json, os, sys
root = sys.argv[1]
lines = ["Lobby probe summary", ""]
for name in sorted(os.listdir(root)):
    summary = os.path.join(root, name, "summary.txt")
    if os.path.isfile(summary):
        lines.append("== " + name + " ==")
        lines.append(open(summary).read().rstrip())
        lines.append("")
regions = os.path.join(root, "regions.json")
if os.path.isfile(regions):
    data = json.load(open(regions))
    lines.append("Region RTT from " + str(data.get("hostname")))
    for row in data.get("regions", []):
        lines.append("  {0:6} {1:5} ms  {2}".format(row["code"], row["rttMs"], row["address"]))
    lines.append("")
text = "\n".join(lines) + "\n"
open(os.path.join(root, "summary.txt"), "w").write(text)
sys.stdout.write(text)
PY
