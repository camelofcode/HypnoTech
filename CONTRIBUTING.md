# Contributing

HypnoTech is undergoing a compatibility-sensitive public-readiness programme. Bug
reports and design feedback are welcome once the repository is public, but external
code or documentation contributions are not accepted.

The engine is maintained as a single-copyright-holder project so Christopher Clayton
can continue using it in commercial/client work while offering the public source under
PolyForm Noncommercial terms. Please do not open pull requests or submit patches: they
will be closed without merging. Opening a pull request does not transfer copyright or
grant permission to relicense the submitted work.

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
