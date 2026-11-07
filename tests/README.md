# HypnoTech tests

The test projects use xUnit v3 with Microsoft Testing Platform. Package versions are
centralised in `tests/Directory.Packages.props`; coverage is informational and has no
global threshold.

## Fast suites

These projects do not need a container runtime:

```bash
dotnet test tests/HypnoTech.Cms.UnitTests -c Release
dotnet test tests/HypnoTech.Common.UnitTests -c Release
dotnet test tests/HypnoTech.ArchitectureTests -c Release
```

## MongoDB integration suite

The Testcontainers fixture starts `mongo:4.0.3` (the production compatibility
baseline), `mongo:8.0.32-noble` (a pinned supported release), and `mongo:9.0.4` (the
current major release). The tests create unique databases and delete the containers
and databases they create.

With Docker, run:

```bash
dotnet test tests/HypnoTech.Cms.MongoDb.IntegrationTests -c Release
```

With rootless Podman, its Docker-compatible API socket is sufficient; `podman-docker`
is not required:

```bash
systemctl --user start podman.socket
DOCKER_HOST="unix://${XDG_RUNTIME_DIR}/podman/podman.sock" \
TESTCONTAINERS_RYUK_DISABLED=true \
dotnet test tests/HypnoTech.Cms.MongoDb.IntegrationTests -c Release
```

Ryuk is disabled because a rootless Podman socket cannot provide the privileged helper
container it expects. Testcontainers still removes the test containers through normal
fixture disposal.

## Public example integration suite

The demo suite starts a pinned MongoDB 8.0 container, launches the real admin host,
seeds an isolated database, and checks representative CMS, navigation, Media, and
Ecommerce data. It also verifies that the documented admin password is stored only as
a bcrypt hash and that real payment routes remain blocked while pricing routes stay
available. A dependency-free local Kestrel double exercises the legacy PayPal REST
contract for OAuth, successful creation, validation failure, and execution; it never
contacts live or sandbox PayPal.

With rootless Podman:

```bash
DOCKER_HOST="unix://${XDG_RUNTIME_DIR}/podman/podman.sock" \
TESTCONTAINERS_RYUK_DISABLED=true \
dotnet test tests/HypnoTech.Demo.IntegrationTests -c Release
```

## Focused browser suite

The Playwright suite starts one pinned MongoDB 8.0 container and the real Lantern
House Admin and Website on dynamic ports. Five isolated journeys cover
authentication and authorization, dynamic CMS authoring and ordering, a real image
upload reaching the public site, Media, server-calculated basket/checkout totals, and
mobile navigation in Chromium, Firefox, and WebKit. The checkout journey cannot submit
an order or contact a payment provider.

With rootless Podman, one command builds the hosts and tests, installs the Chromium
revision pinned by Playwright when needed, and runs the suite:

```bash
./tests/run-end-to-end.sh
```

Chromium is the quick default. Use `--browser firefox`, `--browser webkit`, or
`--all-browsers` to select another engine or run all three. The explicit browser form
is suitable for a later GitHub Actions matrix. The admin test host uses ephemeral
HTTPS so the engine's secure authentication-cookie policy is tested without being
weakened for local automation.

Use a Playwright-supported Ubuntu/Debian host for the complete native three-engine
run. Fedora can run the Chromium and Firefox checks, but its WebKit dependencies have
different library ABIs; the supported Ubuntu CI run is the WebKit source of truth.

Failures retain a screenshot, trace, browser errors, and application logs under
`TestResults/Playwright/<browser>/<test-name>/`. Detailed trace instructions are in
[`HypnoTech.Demo.EndToEndTests/README.md`](HypnoTech.Demo.EndToEndTests/README.md).

## Full suite and coverage

Once the container API is available, run the solution through the runtime-aware
wrapper:

```bash
./tests/run-solution-tests.sh
```

The wrapper uses `DOCKER_HOST` when it is already configured. Otherwise it selects a
running rootless Podman socket when present and leaves the normal Docker default
unchanged on Docker hosts. Extra `dotnet test` arguments are forwarded unchanged, so
`./tests/run-solution-tests.sh --no-build` is suitable after a Release build.

On a fresh machine, run `./tests/run-end-to-end.sh` once first so the pinned Chromium
binary is available to the end-to-end project included in the solution.

Generate an informational Cobertura report for a focused project with:

```bash
dotnet test tests/HypnoTech.Cms.UnitTests -c Release \
  --coverage \
  --coverage-output-format cobertura \
  --coverage-output artifacts/coverage/cms.cobertura.xml
```

Test output and coverage reports are written below the ignored `TestResults/` folder.

## Corrected legacy behaviour

The first characterisation run exposed several legacy failure modes. They are now
covered as corrected behaviour on all MongoDB targets:

- searching a mapping with no searchable fields returns an empty result;
- a registered child whose reference ID is null remains unset;
- moving an ordered item beyond the collection boundary is a no-op;
- repeated Common, Ecommerce, and Media MongoDB startup reuses existing BSON class maps
  and model-registry entries.

These assertions protect the fixes during structural refactoring and async conversion.
