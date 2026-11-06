#!/usr/bin/env bash

set -Eeuo pipefail

script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd -- "${script_directory}/.." && pwd)"
project="${repository_root}/tests/HypnoTech.Demo.EndToEndTests/HypnoTech.Demo.EndToEndTests.csproj"
output="${repository_root}/tests/HypnoTech.Demo.EndToEndTests/bin/Release/net10.0"
node="${output}/.playwright/node/linux-x64/node"
cli="${output}/.playwright/package/cli.js"

cd "${repository_root}"

dotnet build "${project}" -c Release

"${node}" "${cli}" install chromium

export DOCKER_HOST="${DOCKER_HOST:-unix:///run/user/$(id -u)/podman/podman.sock}"
export TESTCONTAINERS_RYUK_DISABLED="${TESTCONTAINERS_RYUK_DISABLED:-true}"

dotnet test "${project}" -c Release --no-build
