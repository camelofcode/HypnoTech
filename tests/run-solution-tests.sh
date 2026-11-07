#!/usr/bin/env bash

set -Eeuo pipefail

script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd -- "${script_directory}/.." && pwd)"

if [[ -z "${DOCKER_HOST:-}" ]]; then
    runtime_directory="${XDG_RUNTIME_DIR:-/run/user/$(id -u)}"
    podman_socket="${runtime_directory}/podman/podman.sock"

    if [[ -S "${podman_socket}" ]]; then
        export DOCKER_HOST="unix://${podman_socket}"
        export TESTCONTAINERS_RYUK_DISABLED="${TESTCONTAINERS_RYUK_DISABLED:-true}"
    fi
fi

cd "${repository_root}"
dotnet test HypnoTech.slnx -c Release "$@"
