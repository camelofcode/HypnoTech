# Lantern House browser tests

Run the focused Chromium suite from the repository root:

```bash
./tests/run-end-to-end.sh
```

Select one engine explicitly or run the complete browser matrix with:

```bash
./tests/run-end-to-end.sh --browser firefox
./tests/run-end-to-end.sh --browser webkit
./tests/run-end-to-end.sh --all-browsers
```

MongoDB is the default persistence provider. The same journeys can run against the
two relational adapters:

```bash
./tests/run-end-to-end.sh --provider postgres
./tests/run-end-to-end.sh --provider sqlserver
```

The command builds the test project, installs the exact selected browser revisions
required by the pinned Playwright package when they are not already cached, starts
the selected disposable database through the rootless Podman API, launches the real
Admin and Website applications on dynamic local ports, and runs the same xUnit
journeys headlessly in each selected engine.

The admin host uses an ephemeral self-signed HTTPS certificate so its `Secure`
authentication cookie is exercised consistently by all three engines. The certificate
exists only for the test run, and only the isolated Playwright browser context accepts
it.

Playwright's native browser dependencies are officially supported on its documented
Ubuntu/Debian hosts. Chromium and Firefox also run on this repository's Fedora
development machine, but native WebKit requires library ABIs Fedora does not ship.
The authoritative WebKit run is therefore the supported Ubuntu environment used by
the future GitHub Actions job; no Fedora compatibility symlinks are required.

Each test receives a fresh browser context. The provider uses a unique disposable
database and uploaded files use a unique temporary directory for the suite. On
failure, the test writes a screenshot, Playwright trace, browser errors, and both
application logs under
`TestResults/Playwright/<browser>/<provider>/<test-name>/`. Open a trace with the
generated Playwright CLI, for example:

```bash
tests/HypnoTech.Demo.EndToEndTests/bin/Release/net10.0/.playwright/node/linux-x64/node \
  tests/HypnoTech.Demo.EndToEndTests/bin/Release/net10.0/.playwright/package/cli.js \
  show-trace TestResults/Playwright/<browser>/<test-name>/trace.zip
```

The suite deliberately stops at the calculation-only checkout. It never calls a
payment, order-completion, email, download-code, or fulfilment route.
