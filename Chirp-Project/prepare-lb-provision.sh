#!/bin/bash
set -euo pipefail

# Usage:
#   ./prepare-lb-provision.sh <lb_name> <role> <priority> <minitwit1_ip> <minitwit2_ip> <peer_lb_name>
#
# Resolves:
# - this LB private IP from metadata
# - peer LB private IP from DigitalOcean API (waits until available)
# Then calls:
#   ./provision-lb.sh <lb_name> <role> <priority> <minitwit1_ip> <minitwit2_ip> <lb_private_ip> <peer_lb_private_ip>

if [ "$#" -ne 6 ]; then
  echo "Usage: $0 <lb_name> <role> <priority> <minitwit1_ip> <minitwit2_ip> <peer_lb_name>" >&2
  exit 1
fi

LB_NAME="$1"
ROLE="$2"
PRIORITY="$3"
MINITWIT1_IP="$4"
MINITWIT2_IP="$5"
PEER_LB_NAME="$6"

# Keepalived notify script uses DO_TOKEN, so prefer it; fall back to DIGITAL_OCEAN_TOKEN.
if [ -f /root/.bash_profile ]; then
  # shellcheck disable=SC1091
  . /root/.bash_profile
fi

DO_API_TOKEN="${DO_TOKEN:-${DIGITAL_OCEAN_TOKEN:-}}"
if [ -z "${DO_API_TOKEN}" ]; then
  echo "Missing DO_TOKEN (or DIGITAL_OCEAN_TOKEN) for peer lookup." >&2
  exit 1
fi

METADATA_BASE="http://169.254.169.254/metadata/v1"
SELF_PRIVATE_IP="$(curl -fsS "${METADATA_BASE}/interfaces/private/0/ipv4/address")"

if [ -z "${SELF_PRIVATE_IP}" ]; then
  echo "Could not resolve this droplet private IP from metadata." >&2
  exit 1
fi

get_peer_private_ip() {
  local peer_name="$1"

  curl -fsS -H "Authorization: Bearer ${DO_API_TOKEN}" \
    "https://api.digitalocean.com/v2/droplets?per_page=200" | \
    python3 - "$peer_name" <<'PY'
import json
import sys

peer = sys.argv[1]
data = json.load(sys.stdin)

for droplet in data.get("droplets", []):
    if droplet.get("name") != peer:
        continue
    for net in droplet.get("networks", {}).get("v4", []):
        if net.get("type") == "private" and net.get("ip_address"):
            print(net["ip_address"])
            raise SystemExit(0)

print("")
PY
}

echo "Waiting for peer load balancer '${PEER_LB_NAME}' private IP..."
PEER_PRIVATE_IP=""
for _ in $(seq 1 60); do
  PEER_PRIVATE_IP="$(get_peer_private_ip "$PEER_LB_NAME" || true)"
  if [ -n "${PEER_PRIVATE_IP}" ]; then
    break
  fi
  sleep 5
done

if [ -z "${PEER_PRIVATE_IP}" ]; then
  echo "Timed out waiting for peer LB private IP for '${PEER_LB_NAME}'." >&2
  exit 1
fi

echo "Resolved private IPs: self=${SELF_PRIVATE_IP}, peer=${PEER_PRIVATE_IP}"

./provision-lb.sh \
  "${LB_NAME}" \
  "${ROLE}" \
  "${PRIORITY}" \
  "${MINITWIT1_IP}" \
  "${MINITWIT2_IP}" \
  "${SELF_PRIVATE_IP}" \
  "${PEER_PRIVATE_IP}"
