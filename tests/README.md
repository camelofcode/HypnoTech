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

The Testcontainers fixture starts both `mongo:4.0.3` (the production compatibility
baseline) and `mongo:8.0.32-noble` (a pinned supported release). The tests create unique
databases and delete the containers and databases they create.

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

## Current behaviour captured deliberately

The characterisation suite records several behaviours that may deserve later fixes:

- searching a mapping with no searchable fields produces an invalid empty `$or` and
  MongoDB rejects the request;
- a registered child whose reference ID is null causes child population to throw;
- moving an ordered item beyond the collection boundary throws;
- constructing the Common MongoDB registration more than once in one process attempts
  to register the same BSON class maps again and throws.

Change these assertions only in a reviewed behaviour-fix change with MongoDB 4.0.3
coverage, rather than weakening them during structural refactoring.
