#!/usr/bin/env bash

set -Eeuo pipefail

script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd -- "${script_directory}/.." && pwd)"
container_name="hypnotech-demo-mongo"
log_directory="${repository_root}/.demo/logs"
admin_log="${log_directory}/admin.log"
website_log="${log_directory}/website.log"
admin_pid=""
website_pid=""

cleanup()
{
    trap - EXIT INT TERM

    process_group_is_running()
    {
        [[ -n "$1" ]] && kill -0 -- "-$1" 2>/dev/null
    }

    for process_id in "${admin_pid}" "${website_pid}"; do
        if process_group_is_running "${process_id}"; then
            kill -- "-${process_id}" 2>/dev/null || true
        fi
    done

    for _ in {1..20}; do
        if ! process_group_is_running "${admin_pid}" &&
            ! process_group_is_running "${website_pid}"; then
            break
        fi

        sleep 0.1
    done

    for process_id in "${admin_pid}" "${website_pid}"; do
        if process_group_is_running "${process_id}"; then
            kill -KILL -- "-${process_id}" 2>/dev/null || true
        fi
    done

    for process_id in "${admin_pid}" "${website_pid}"; do
        if [[ -n "${process_id}" ]]; then
            wait "${process_id}" 2>/dev/null || true
        fi
    done

    printf '\nLantern House applications stopped. MongoDB remains available in %s.\n' "${container_name}"
}

wait_for_mongo()
{
    for _ in {1..60}; do
        if podman exec "${container_name}" mongosh --quiet \
            --eval 'quit(db.adminCommand({ ping: 1 }).ok ? 0 : 1)' >/dev/null 2>&1; then
            return 0
        fi

        sleep 1
    done

    printf 'MongoDB did not become ready. Recent container output:\n' >&2
    podman logs --tail 30 "${container_name}" >&2 || true
    return 1
}

wait_for_url()
{
    local name="$1"
    local url="$2"
    local process_id="$3"
    local log_file="$4"

    for _ in {1..60}; do
        if ! kill -0 "${process_id}" 2>/dev/null; then
            printf '%s stopped during startup. Recent output:\n' "${name}" >&2
            tail -n 30 "${log_file}" >&2 || true
            return 1
        fi

        if curl --fail --silent --output /dev/null --max-time 2 "${url}"; then
            return 0
        fi

        sleep 1
    done

    printf '%s did not become ready. Recent output:\n' "${name}" >&2
    tail -n 30 "${log_file}" >&2 || true
    return 1
}

for command_name in podman dotnet curl setsid; do
    if ! command -v "${command_name}" >/dev/null 2>&1; then
        printf 'Required command not found: %s\n' "${command_name}" >&2
        exit 1
    fi
done

trap cleanup EXIT INT TERM
mkdir -p "${log_directory}"
: >"${admin_log}"
: >"${website_log}"

cd "${repository_root}"

printf 'Starting MongoDB 8.0.32 with Podman...\n'
podman run --detach --replace --name "${container_name}" \
    --publish 127.0.0.1:27017:27017 \
    --volume hypnotech-demo-mongo:/data/db \
    docker.io/library/mongo:8.0.32 >/dev/null
wait_for_mongo

printf 'Seeding the Lantern House demo...\n'
ASPNETCORE_ENVIRONMENT=Development \
    UploadPath="${repository_root}/.demo/uploads" \
    dotnet run --project examples/HypnoTech.Demo.Admin --no-launch-profile -- --seed-only

printf 'Starting the admin and public website...\n'
setsid env ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5101 \
    UploadPath="${repository_root}/.demo/uploads" \
    dotnet run --project examples/HypnoTech.Demo.Admin --no-launch-profile \
    >"${admin_log}" 2>&1 &
admin_pid=$!

setsid env ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5100 \
    UploadPath="${repository_root}/.demo/uploads" \
    dotnet run --project examples/HypnoTech.Demo.Website --no-launch-profile \
    >"${website_log}" 2>&1 &
website_pid=$!

wait_for_url "Admin" "http://localhost:5101" "${admin_pid}" "${admin_log}"
wait_for_url "Website" "http://localhost:5100" "${website_pid}" "${website_log}"

cat <<'MESSAGE'

Lantern House is ready:

  Website: http://localhost:5100
  Admin:   http://localhost:5101
  Login:   admin
  Password: HypnoTechDemo!2026

Application logs are under .demo/logs/.
Press Ctrl+C to stop both applications. MongoDB and its demo volume are left running
so the next launch is quick; the reset commands in examples/README.md remove them.
MESSAGE

set +e
wait -n "${admin_pid}" "${website_pid}"
exit_code=$?
set -e

if [[ "${exit_code}" -ne 130 ]]; then
    printf 'One of the applications stopped unexpectedly. Check %s.\n' "${log_directory}" >&2
fi

exit "${exit_code}"
