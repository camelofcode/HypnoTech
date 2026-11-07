# Publication-history security audit — 2026-11-06

## Scope

This audit covered the HypnoTech engine repository only. It did not modify or scan the
working trees of the private Esoteric or SelfHypnosis sites.

The Git history scan traversed every ref reachable through `--all`, including the
production baseline tag, support branch, frozen `master`, `main`, and
`public-readiness`. Git reported 10 commits at the time of the first scan and 11 after
the Phase 1 foundation commit. Because
Gitleaks scans commit patches through Git history, deleted content reachable from those
commits was included. A second directory scan covered the current working tree,
including the new uncommitted Phase 1 documents.

## Scanner and integrity

- Scanner: Gitleaks 8.30.1
- Official release: <https://github.com/gitleaks/gitleaks/releases/tag/v8.30.1>
- Download source: the official `gitleaks/gitleaks` GitHub release
- Archive: `gitleaks_8.30.1_linux_x64.tar.gz`
- Integrity: matched the release's `gitleaks_8.30.1_checksums.txt`
- Installation: temporary directory outside the repository; no scanner binary or
  report was committed

## Commands

Full reachable Git history:

```bash
gitleaks git . \
  --log-opts='--all' \
  --redact=100 \
  --max-archive-depth=1 \
  --report-format=json \
  --report-path=/tmp/hypnotech-gitleaks-v8.30.1/hypnotech-report.json \
  --no-banner \
  --no-color
```

Current directory, including uncommitted Phase 1 files:

```bash
gitleaks dir . \
  --redact=100 \
  --max-archive-depth=1 \
  --report-format=json \
  --report-path=/tmp/hypnotech-gitleaks-v8.30.1/hypnotech-working-tree-report.json \
  --no-banner \
  --no-color
```

## Results

| Scan | Scope reported by scanner | Result |
| --- | --- | --- |
| Initial Git history | 10 commits; approximately 26.94 MB | No leaks found |
| Working directory | Approximately 85.07 MB | No leaks found |
| Post-commit Git history | 11 commits; approximately 26.99 MB | No leaks found |

Both commands exited successfully. Reports remained in `/tmp` and were not added to the
repository.

## Supplemental review

A path-only review searched all historical filenames for appsettings, environment,
secret, credential, certificate/key, connection, SMTP, and payment-related names. A
content-pattern review returned filenames only and looked for connection strings,
password/secret/API-key fields, SMTP/authentication terms, certificate/private-key
terms, MongoDB URIs, and HTTP URLs across every commit.

The review found expected configuration model/property names, CMS user/password flows,
the existing PayPal implementation, generated bundles, and public source URLs. It found
no tracked appsettings file, dotenv value file, certificate, private key, credential
file, or seeded credential. No secret value was printed or committed during review.

## Publication rule

Re-run the full-history command against the exact commit that will be made public and
after any later history rewrite. A clean scan does not prove that credentials never
existed outside Git; production credentials must still remain external to the public
repository. If a future scan finds a real credential, rotate it before cleaning or
publishing history.

## Final publication-candidate scan — 2026-11-07

The Phase 7 publication candidate was scanned again immediately before the repository
visibility change. The final history command above traversed all refs and reported 35
commits and approximately 27.8 MB with no leaks. A `git archive` of the exact committed
branch tip was also scanned with the directory command and reported approximately
20.7 MB with no leaks.

For completeness, an initial directory scan of the developer working directory also
inspected ignored build output and reported four generic-key matches in Playwright's
generated `bin/Debug` and `bin/Release` package files. Those files are restored copies
of Playwright's bundled `coreBundle.js` and trace-viewer terminal module; they are
ignored, absent from `git archive`, and absent from every commit. The committed-tree
and full-history scans are therefore the publication result. Reports remained under
`/tmp/hypnotech-gitleaks-v8.30.1/final` and were not committed.
