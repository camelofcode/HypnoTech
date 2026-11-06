# Contributing

HypnoTech is currently undergoing a compatibility-sensitive public-readiness programme.
Bug reports, design feedback, and documentation corrections are welcome once the
repository is public, but unsolicited code contributions are not yet accepted.

This temporary restriction avoids ambiguous ownership while the source licence and a
future dual-licensing/contributor agreement are being decided. Opening a pull request
does not transfer copyright or grant the maintainer permission to relicense the work.
Please agree the contribution and its licensing with the maintainer before writing
code. This policy will be revisited before broader external contributions are enabled.

## Reporting issues

Before opening an issue:

1. Check the roadmap and existing issues.
2. Reproduce the problem against the current public branch if possible.
3. Remove credentials, private URLs, production data, and material from private sites.
4. For security issues, follow [SECURITY.md](SECURITY.md) instead of filing publicly.

Include the operating system, .NET SDK, Node/npm versions when relevant, database and
driver versions, reproduction steps, expected behaviour, and actual behaviour.

## Repository rules

Any agreed change must:

- preserve routes, public namespaces, assembly identities, static asset paths, and
  persisted MongoDB names unless a migration is explicitly approved;
- add characterisation tests before changing dynamic CMS behaviour;
- keep required runtime assets self-hosted and dependency versions pinned;
- edit frontend sources rather than committed generated bundles;
- avoid private Esoteric or SelfHypnosis content and changes;
- leave additional payment-provider work out of scope;
- update `PUBLIC_READINESS_PLAN.md` when roadmap work or a material decision changes.

Use the build and verification commands in [README.md](README.md). More detailed agent
and maintainer guidance is recorded in [AGENTS.md](AGENTS.md).
