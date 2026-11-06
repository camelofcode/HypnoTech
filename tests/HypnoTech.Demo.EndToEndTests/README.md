# Lantern House browser tests

Run the focused Chromium suite from the repository root:

```bash
./tests/run-end-to-end.sh
```

The command builds the test project, installs the exact Chromium revision required by
the pinned Playwright package when it is not already cached, starts MongoDB through the
rootless Podman API, launches the real Admin and Website applications on dynamic local
ports, and runs the xUnit journeys headlessly.

Each test receives a fresh browser context. MongoDB uses a unique disposable database
and uploaded files use a unique temporary directory for the suite. On failure, the
test writes a screenshot, Playwright trace, browser errors, and both application logs
under `TestResults/Playwright/<test-name>/`. Open a trace with the generated Playwright
CLI, for example:

```bash
tests/HypnoTech.Demo.EndToEndTests/bin/Release/net10.0/.playwright/node/linux-x64/node \
  tests/HypnoTech.Demo.EndToEndTests/bin/Release/net10.0/.playwright/package/cli.js \
  show-trace TestResults/Playwright/<test-name>/trace.zip
```

The suite deliberately stops at the calculation-only checkout. It never calls a
payment, order-completion, email, download-code, or fulfilment route.
