#!/bin/sh
# Runs on container start (the nginx image executes /docker-entrypoint.d/*.sh),
# so the API URL can be changed via the API_BASE_URL env var without rebuilding.
set -e
cfg=/usr/share/nginx/html/appsettings.json
printf '{\n  "ApiBaseUrl": "%s"\n}\n' "$API_BASE_URL" > "$cfg"
rm -f "$cfg.br" "$cfg.gz"
echo "FinTrack: ApiBaseUrl set to $API_BASE_URL"
