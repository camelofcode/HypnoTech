# Production baseline — 2026-11-06

This document records the engine state preserved before public-readiness work began.
It is evidence for restoration and compatibility checks; it is not a promise that old
production dependencies will remain supported indefinitely on `main`.

## Source control

| Item | Value |
| --- | --- |
| Commit | `95c9f6a38c2830ecfcb3a7a32d26ea9644f57a52` |
| Annotated tag | `production-baseline-2026-11-06` |
| Production support branch | `support/legacy-production` |
| New default branch | `main` |
| Transitional legacy branch | `master`, frozen at the baseline commit |
| Public-readiness branch point | `95c9f6a38c2830ecfcb3a7a32d26ea9644f57a52` |
| Repository visibility at capture | Private |

Both live consumer sites used this engine commit when the baseline was captured. The
support branch is for production compatibility fixes only. Dependency upgrades,
structural work, examples, and public-release work belong on `main` via feature branches.

## Production data compatibility

| Component | Version |
| --- | --- |
| MongoDB Server | 4.0.3 |
| MongoDB .NET/C# driver | 3.4.3 |

The server and driver pair above is the compatibility baseline. Do not advance the
driver or raise the MongoDB server compatibility floor on the support branch without a
separate migration decision and an integration test against MongoDB 4.0.3.

## Captured development toolchain

| Tool | Captured version or constraint |
| --- | --- |
| .NET SDK selected by `global.json` | 10.0.401 |
| `global.json` requested SDK | 10.0.100 with `latestFeature` roll-forward |
| .NET runtime | 10.0.12 |
| Node.js | 24.18.0 |
| Node.js repository constraint | 22 or newer |
| npm | 11.16.0 |
| npm repository package-manager version | 11.16.0 |
| Git | 2.56.0 |
| Capture operating system | Fedora Linux 44, x64 |

The captured versions describe the verified workstation, not the complete future
support matrix. CI will define and document the supported toolchain during the public
release work.

## Baseline verification

The following commands were verified from the engine repository root:

```sh
dotnet restore HypnoTech.slnx
dotnet build HypnoTech.slnx --configuration Release
npm run check:frontend
```

Results:

- All 21 .NET projects restored and built successfully.
- Release build completed with zero warnings and zero errors.
- `frontend`, `media`, `ecommerce`, and `cms` generated outputs were all current.

`npm ci` remains the clean-install command for automation. The baseline frontend check
used the already installed dependencies and the committed lockfile.

## Tracked static-asset fingerprints

Each fingerprint is the SHA-256 of the sorted `sha256sum` manifest for every tracked
file below the named directory. File paths are part of the inner manifest, so a rename
changes the aggregate fingerprint even when bytes are unchanged.

| Tracked output tree | Files | Aggregate SHA-256 |
| --- | ---: | --- |
| `src/HypnoTech.Cms/wwwroot` | 182 | `a5dac16eea3ed5465512de3c465f1c5e8be4e0d036435cf59205abfc28019481` |
| `src/HypnoTech.Frontend/wwwroot` | 2 | `1e441d24bdb6bc57e10f82f72acee5df98478e371bcbca97eecea8107a077334` |
| `src/HypnoTech.Ecommerce.Frontend/wwwroot` | 2 | `4ccd1f185c43f83ef5e1a9f3cd3c5515b36cbb45ba2d91bbb64405b5a6c883f5` |
| `src/HypnoTech.Media.Frontend/wwwroot` | 2 | `84bf89f62d341a3c66e03f37babced5c4c4330237878afc7951cad3259a25431` |

Reproduce a fingerprint with GNU coreutils:

```sh
asset_dir=src/HypnoTech.Cms/wwwroot
git ls-files -z "$asset_dir" \
  | LC_ALL=C sort -z \
  | xargs -0 -r sha256sum \
  | sha256sum
```

## Build-manifest fingerprints

| File | SHA-256 |
| --- | --- |
| `global.json` | `b48d6da8fa62c0da7cc8a2da4ea036ac55836660a448ba533135abb5b8451b73` |
| `package.json` | `080fc65a168fce15d8c73c74be08a782d37d0e0418e7d5a6c5398602d08c3bd7` |
| `package-lock.json` | `df439961b67c0c9de215124f9cb9714ea2474fb924b9149994815d48099c6a66` |
| `HypnoTech.slnx` | `218b556abd0fbb49ff6f428adce56dfb5a53661e73059b4ac51cd08a4b2972f5` |

## Portable backup

A verified Git bundle was created outside the repository:

```text
HypnoTech-production-baseline-2026-11-06.bundle
```

| Item | Value |
| --- | --- |
| Size at capture | 6.0 MB |
| SHA-256 | `8b963e6c86e214acca9532912bbf0b4b29ff5c579f2705d7f045cadd60f17cc6` |
| Verification | `git bundle verify` reported complete history |
| Included branches | `master`, `dotnet10-upgrade-plan` |

The bundle was created before the new baseline tag and branches, but it contains the
complete commit history including the baseline commit. The remote annotated tag and
support branch provide the named recovery points. Store the bundle on durable private
backup storage.
