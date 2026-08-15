# Security Policy

MediaVault is under active pre-release development. This repository is the
workspace entry point and contains the Aspire AppHost, setup scripts, and
documentation. The application implementation lives in the independent
`MediaVault.Api` and `MediaVault.Clients` repositories.

## Supported versions

| Version | Supported |
| --- | --- |
| Current default branch | Security fixes are considered |
| Tagged or deployed releases | No support commitment yet |
| Older branches and commits | Not supported |

This table describes maintenance intent, not a promised response or
remediation time.

## Report a vulnerability

Do not disclose suspected vulnerabilities, exploit details, credentials,
private data, or reproduction secrets in a public issue, pull request,
discussion, or commit.

For vulnerabilities in the workspace entry point, Aspire AppHost, setup
scripts, or documentation, use GitHub's private
[Report a vulnerability](https://github.com/Megaraz/MediaVault/security/advisories/new)
form.

For vulnerabilities in the application implementation, use the private
reporting form for the affected repository:

- [MediaVault.Api](https://github.com/Megaraz/MediaVault.Api/security/advisories/new)
- [MediaVault.Clients](https://github.com/Megaraz/MediaVault.Clients/security/advisories/new)

If the appropriate private form is unexpectedly unavailable, open a normal
issue asking only for the form to be restored or for a private security
contact. Do not include vulnerability details.

Include the affected area, impact, reproduction conditions, and any suggested
mitigation in the private report. Share only the minimum sensitive material
needed to investigate. Reports will be assessed in the context of a
solo-maintained pre-release project; no response time or disclosure date is
promised.
