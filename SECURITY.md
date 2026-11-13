# Security policy

## Supported versions

HypnoTech is publicly released as source through GitHub. Security reports are evaluated
against the latest stable release and current `main`.

| Version or line | Status |
| --- | --- |
| `1.1.x` | Supported stable release line |
| `main` | Active development; reports accepted |
| `1.0.x` | Superseded by `1.1.x` |
| `support/legacy-production` | Preserved compatibility branch; no guaranteed public security maintenance |

When a fix is appropriate, it may land on `main` and in a new patch release. No fixed
maintenance window or response SLA is promised for this portfolio project.

## Reporting a vulnerability

Please do not disclose a suspected vulnerability in a public issue, discussion, pull
request, or social-media post.

Use GitHub's **Security** tab to
[privately report a vulnerability](https://github.com/camelofcode/HypnoTech/security/advisories/new).
Private vulnerability reporting is enabled. If that facility is unavailable, contact
the repository owner through the maintainer profile and ask for a private reporting
channel without including exploit details in the initial public message.

Include, where possible:

- the affected commit or version;
- the affected component and configuration;
- clear reproduction steps or a proof of concept;
- the likely impact and any known mitigations;
- whether the issue is already public.

Receipt will be acknowledged as soon as practical. Reports will be triaged privately
and coordinated disclosure will be preferred.

Never include real credentials, private-site data, or production database contents in
a report. Use synthetic examples and redact tokens, hostnames, and personal data.
