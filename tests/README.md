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
available.

With rootless Podman:

```bash
DOCKER_HOST="unix://${XDG_RUNTIME_DIR}/podman/podman.sock" \
TESTCONTAINERS_RYUK_DISABLED=true \
dotnet test tests/HypnoTech.Demo.IntegrationTests -c Release
```

## Full suite and coverage

Once the container API is configured, run the solution:

```bash
dotnet test HypnoTech.slnx -c Release
```

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
