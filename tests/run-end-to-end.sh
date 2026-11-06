#!/usr/bin/env bash

set -Eeuo pipefail

script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd -- "${script_directory}/.." && pwd)"
project="${repository_root}/tests/HypnoTech.Demo.EndToEndTests/HypnoTech.Demo.EndToEndTests.csproj"
output="${repository_root}/tests/HypnoTech.Demo.EndToEndTests/bin/Release/net10.0"
node="${output}/.playwright/node/linux-x64/node"
cli="${output}/.playwright/package/cli.js"
browsers=("chromium")

usage() {
    echo "Usage: $0 [--browser chromium|firefox|webkit | --all-browsers]"
}

if [[ $# -gt 0 ]]; then
    case "$1" in
        --all-browsers)
            [[ $# -eq 1 ]] || { usage; exit 2; }
            browsers=("chromium" "firefox" "webkit")
            ;;
        --browser)
            [[ $# -eq 2 ]] || { usage; exit 2; }
            case "$2" in
                chromium|firefox|webkit) browsers=("$2") ;;
                *) usage; exit 2 ;;
            esac
            ;;
        *)
            usage
            exit 2
            ;;
    esac
fi

cd "${repository_root}"

dotnet build "${project}" -c Release -m:1

"${node}" "${cli}" install "${browsers[@]}"

export DOCKER_HOST="${DOCKER_HOST:-unix:///run/user/$(id -u)/podman/podman.sock}"
export TESTCONTAINERS_RYUK_DISABLED="${TESTCONTAINERS_RYUK_DISABLED:-true}"

for browser_name in "${browsers[@]}"; do
    echo "Running Playwright journeys with ${browser_name}"
    PLAYWRIGHT_BROWSER="${browser_name}" dotnet test "${project}" -c Release --no-build
done
