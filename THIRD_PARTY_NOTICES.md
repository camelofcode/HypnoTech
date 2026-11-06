# Third-party notices

HypnoTech includes or references third-party software. This inventory was prepared on
2026-11-06 from the tracked files, embedded headers, `package-lock.json`, restored NuGet
metadata, and `dotnet list HypnoTech.slnx package --include-transitive --no-restore`.
It is an attribution record, not a substitute for the upstream licence terms.

## Browser assets distributed in this repository

The `src/**/lib` trees are vendored source snapshots and copies may also appear in
generated `src/**/wwwroot` output. Existing upstream licence files and headers must be
kept with these files. The common MIT and SIL OFL 1.1 texts are preserved under
`docs/licenses/`; TinyMCE, Bootstrap, jQuery, jQuery Validation, and jQuery Validation
Unobtrusive also retain their upstream licence files in place.

| Component | Vendored version/evidence | Licence and attribution | Tracked location |
| --- | --- | --- | --- |
| Bootstrap | 4.3.1 | MIT; Copyright 2011–2019 The Bootstrap Authors and Twitter, Inc. | `src/Cms/HypnoTech.Cms.Frontend/lib/bootstrap` |
| Popper.js | Snapshot bundled in 2017; exact version was not recorded | MIT; Copyright Federico Zivolo 2017; notice embedded in the file | `src/Cms/HypnoTech.Cms.Frontend/lib/bootstrap/popper.min.js` |
| Start Bootstrap Simple Sidebar | 2019 snapshot; exact release was not recorded | MIT; Copyright 2013–2019 Start Bootstrap; notice embedded in the file | `src/Cms/HypnoTech.Cms.Frontend/lib/bootstrapSidebar` |
| Cookies.js | 1.2.4-pre | Public-domain dedication; Scott Hamper; notice embedded in the file | CMS and Ecommerce frontend `lib/cookie-js` |
| Font Awesome Free | 5.13.0 | Code: MIT; icons: CC BY 4.0; fonts: SIL OFL 1.1; Fonticons, Inc.; terms are identified in the SCSS header | `src/Cms/HypnoTech.Cms.Frontend/lib/fontawesome` |
| jQuery | 3.3.1 (CMS), 3.5.1 (Frontend) | MIT; JS Foundation and other contributors | CMS and Frontend `lib/jquery` |
| jQuery Validation | 1.17.0 | MIT; Copyright Jörn Zaefferer and contributors | `src/Cms/HypnoTech.Cms.Frontend/lib/jquery-validation` |
| jQuery Validation Unobtrusive | 3.2.11 | Apache-2.0; .NET Foundation | `src/Cms/HypnoTech.Cms.Frontend/lib/jquery-validation-unobtrusive` |
| Knockout | 3.5.1 | MIT; Knockout.js team | CMS, Ecommerce, and Media frontend `lib/knockout/knockout.js` |
| Knockout Validation | Vendored snapshot; upstream version was not recorded | MIT; Eric M. Barnard and contributors; notice embedded in the file | Ecommerce and Media frontend `lib/knockout/knockout.validation.js` |
| Lodash core custom build | 4.17.15 | MIT; OpenJS Foundation and other contributors; notice embedded in the file | `src/Cms/HypnoTech.Cms.Frontend/lib/lodash` |
| TinyMCE Community | 5.2.1 | LGPL-2.1-or-later or commercial licence; Copyright Tiny Technologies, Inc.; full LGPL 2.1 text retained | `src/Cms/HypnoTech.Cms.Frontend/lib/tinymce` |
| Navigo | Vendored snapshot; upstream version was not recorded; SHA-256 `3b1cd4166cd8c2ed6dfc6ffabd135e59c042ce1085ab2f8f01f1c1919ba7a54f` | MIT; Krasimir Tsonev and contributors | `src/Frontend/HypnoTech.Frontend.Frontend/lib/navigo` |

Upstream licence references:

- Font Awesome Free: <https://fontawesome.com/license/free>
- Creative Commons Attribution 4.0: <https://creativecommons.org/licenses/by/4.0/>
- TinyMCE: <https://github.com/tinymce/tinymce/blob/5.2.1/LICENSE.TXT>
- Navigo: <https://github.com/krasimir/navigo>

The missing historical version metadata above is recorded rather than guessed. A future
dependency-refresh change should replace each unknown snapshot with a verified,
integrity-pinned version while preserving runtime behaviour.

The Phase 1 audit also found 18 corrupted Montserrat `.ttf` files in
`src/Cms/HypnoTech.Cms.Frontend/fonts`. They failed `fc-query`, contained replacement bytes,
and were not referenced by authored or generated engine CSS. They were removed with the
copyright holder's approval rather than being published as working fonts or replaced by
an unverified release. They remain recoverable from the production baseline tag and
private bundle if historical analysis is ever needed.

## npm build dependencies

`node_modules` is ignored and is not distributed in this repository. Exact resolved
versions, integrity hashes, sources, and declared licences are recorded in
`package-lock.json`. Direct build dependencies are:

| Package | Version | Licence | Purpose |
| --- | --- | --- | --- |
| `browserslist` | 4.29.1 | MIT | Browser target selection |
| `lightningcss` | 1.33.0 | MPL-2.0 | CSS transformation/minification |
| `sass` | 1.105.0 | MIT | CMS SCSS compilation |
| `terser` | 5.51.2 | BSD-2-Clause | JavaScript minification |

Resolved transitive packages group as follows (platform variants have the same version
and licence as their parent):

- MIT: `@jridgewell/gen-mapping` 0.3.13, `@jridgewell/resolve-uri` 3.1.2,
  `@jridgewell/source-map` 0.3.11, `@jridgewell/sourcemap-codec` 1.6.0,
  `@jridgewell/trace-mapping` 0.3.31, `@parcel/watcher` 2.6.0 and its platform
  packages, `acorn` 8.18.0, `browserslist` 4.29.1, `buffer-from` 1.1.2,
  `chokidar` 5.0.0, `commander` 2.20.3, `escalade` 3.2.0, `immutable` 5.1.9,
  `is-extglob` 2.1.1, `is-glob` 4.0.3, `node-addon-api` 7.1.1,
  `node-releases` 2.0.57, `picomatch` 4.0.7, `readdirp` 5.1.1, `sass` 1.105.0,
  `source-map-support` 0.5.21, and `update-browserslist-db` 1.3.3.
- Apache-2.0: `baseline-browser-mapping` 2.11.26 and `detect-libc` 2.1.2.
- CC-BY-4.0: `caniuse-lite` 1.0.30001812.
- ISC: `electron-to-chromium` 1.5.439 and `picocolors` 1.1.1.
- MPL-2.0: `lightningcss` 1.33.0 and its platform packages.
- BSD-3-Clause: `source-map` 0.6.1 and `source-map-js` 1.2.1.
- BSD-2-Clause: `terser` 5.51.2.

## NuGet dependencies

NuGet packages are restored from their package feeds and are not committed to this
repository. The table includes all unique direct and transitive packages resolved for
the solution, not only explicit `PackageReference` entries.

| Package | Resolved version | Declared licence |
| --- | --- | --- |
| BCrypt.Net-Next | 4.2.0 | MIT |
| BraintreeHttp-Dotnet | 0.3.0 | Not declared in package metadata |
| DnsClient | 1.6.1 | Apache-2.0 |
| Handlebars.Net | 2.4.3 | MIT |
| HtmlAgilityPack | 1.13.0 | MIT |
| Humanizer.Core | 3.0.10 | MIT |
| Microsoft.Extensions.Configuration | 10.0.12 | MIT |
| Microsoft.Extensions.Configuration.Abstractions | 10.0.12 | MIT |
| Microsoft.Extensions.Configuration.Binder | 10.0.0 | MIT |
| Microsoft.Extensions.DependencyInjection | 10.0.12 | MIT |
| Microsoft.Extensions.DependencyInjection.Abstractions | 10.0.12 | MIT |
| Microsoft.Extensions.DependencyModel | 10.0.0 / 10.0.10 | MIT |
| Microsoft.Extensions.Diagnostics.Abstractions | 10.0.0 | MIT |
| Microsoft.Extensions.FileProviders.Abstractions | 10.0.0 | MIT |
| Microsoft.Extensions.Hosting.Abstractions | 10.0.0 | MIT |
| Microsoft.Extensions.Logging | 10.0.0 | MIT |
| Microsoft.Extensions.Logging.Abstractions | 10.0.0 | MIT |
| Microsoft.Extensions.Options | 10.0.0 | MIT |
| Microsoft.Extensions.Primitives | 10.0.12 | MIT |
| Microsoft.ApplicationInsights | 2.23.0 | MIT |
| Microsoft.Bcl.AsyncInterfaces | 6.0.0 | MIT |
| Microsoft.DiaSymReader | 2.2.10 | MIT |
| Microsoft.Playwright | 1.64.1 | MIT |
| Microsoft.Testing.Extensions.CodeCoverage | 18.11.2 | Microsoft .NET Library licence (licence file in package) |
| Microsoft.Testing.Extensions.Telemetry | 2.5.1 | MIT |
| Microsoft.Testing.Extensions.TrxReport.Abstractions | 2.5.1 | MIT |
| Microsoft.Testing.Platform / MSBuild | 2.5.1 | MIT |
| Microsoft.Win32.Registry | 5.0.0 | MIT |
| MongoDB.Bson | 3.4.3 | Apache-2.0 |
| MongoDB.Driver | 3.4.3 | Apache-2.0 |
| Newtonsoft.Json | 13.0.4 | MIT |
| PayPal | 2.0.0-rc2 | Not declared in package metadata |
| Serilog | 4.3.0 | Apache-2.0 |
| Serilog.AspNetCore | 10.0.0 | Apache-2.0 |
| Serilog.Extensions.Hosting | 10.0.0 | Apache-2.0 |
| Serilog.Extensions.Logging | 10.0.0 | Apache-2.0 |
| Serilog.Formatting.Compact | 3.0.0 | Apache-2.0 |
| Serilog.Settings.Configuration | 10.0.0 | Apache-2.0 |
| Serilog.Sinks.Console | 6.1.1 | Apache-2.0 |
| Serilog.Sinks.Debug | 3.0.0 | Apache-2.0 |
| Serilog.Sinks.File | 7.0.0 | Apache-2.0 |
| SharpCompress | 0.50.4 | MIT |
| SkiaSharp | 4.152.1 | MIT |
| SkiaSharp.NativeAssets.Linux | 4.152.1 | MIT |
| SkiaSharp.NativeAssets.macOS | 4.152.1 | MIT |
| SkiaSharp.NativeAssets.Win32 | 4.152.1 | MIT |
| Snappier | 1.3.1 | BSD-3-Clause |
| System.Security.AccessControl | 6.0.1 | MIT |
| Testcontainers / Testcontainers.MongoDb | 4.16.0 | MIT |
| BouncyCastle.Cryptography | 2.7.0 | MIT |
| Docker.DotNet.Enhanced and transport packages | 4.3.3 | MIT |
| SharpZipLib | 1.4.2 | MIT |
| SSH.NET | 2026.0.0 | MIT |
| xunit.v3 and component packages | 4.0.2 | Apache-2.0 |
| xunit.analyzers | 2.2.0 | Apache-2.0 |
| ZstdSharp.Port | 0.7.3 | MIT |

The Playwright package is restored from NuGet. Its matching Chromium binary is
downloaded into Playwright's user cache for local/CI test execution and is not tracked
or distributed by this repository.

The legacy `PayPal` and `BraintreeHttp-Dotnet` packages contain no licence expression,
licence URL, project URL, or licence file in the restored `.nupkg`. They are referenced
from NuGet rather than redistributed here, but this metadata gap is a maintenance risk
and must not be silently described as a confirmed licence. Replacing or upgrading the
legacy PayPal integration remains outside the present payment-provider scope; any later
work must resolve this provenance explicitly.

## Maintaining this file

When a dependency or vendored file changes, update the version, licence, attribution,
location, and source here. Preserve upstream notices in distributed files. Use the
package lockfile and NuGet package metadata as evidence, and record an unknown value
instead of inferring a licence from similar projects.
