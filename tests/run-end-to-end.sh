#!/usr/bin/env bash

set -Eeuo pipefail

script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd -- "${script_directory}/.." && pwd)"
project="${repository_root}/tests/HypnoTech.Demo.EndToEndTests/HypnoTech.Demo.EndToEndTests.csproj"
output="${repository_root}/tests/HypnoTech.Demo.EndToEndTests/bin/Release/net10.0"
node="${output}/.playwright/node/linux-x64/node"
cli="${output}/.playwright/package/cli.js"
browsers=("chromium")
provider="MongoDb"

usage() {
    echo "Usage: $0 [--browser chromium|firefox|webkit | --all-browsers] [--provider mongo|postgres|sqlserver]"
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --help|-h)
            usage
            exit 0
            ;;
        --all-browsers)
            browsers=("chromium" "firefox" "webkit")
            shift
            ;;
        --browser)
            [[ $# -ge 2 ]] || { usage; exit 2; }
            case "$2" in
                chromium|firefox|webkit) browsers=("$2") ;;
                *) usage; exit 2 ;;
            esac
            shift 2
            ;;
        --provider)
            [[ $# -ge 2 ]] || { usage; exit 2; }
            case "${2,,}" in
                mongo|mongodb) provider="MongoDb" ;;
                postgres|postgresql) provider="PostgreSql" ;;
                sqlserver|mssql) provider="SqlServer" ;;
                *) usage; exit 2 ;;
            esac
            shift 2
            ;;
        *)
            usage
            exit 2
            ;;
    esac
done

cd "${repository_root}"

dotnet build "${project}" -c Release -m:1

"${node}" "${cli}" install "${browsers[@]}"

export DOCKER_HOST="${DOCKER_HOST:-unix:///run/user/$(id -u)/podman/podman.sock}"
export TESTCONTAINERS_RYUK_DISABLED="${TESTCONTAINERS_RYUK_DISABLED:-true}"

for browser_name in "${browsers[@]}"; do
    echo "Running Playwright journeys with ${browser_name} and ${provider}"
    PLAYWRIGHT_BROWSER="${browser_name}" DEMO_PROVIDER="${provider}" \
        dotnet test "${project}" -c Release --no-build
done
