# Security Policy

## Supported versions

| Branch | Supported          |
| ------ | ------------------ |
| `main` | :white_check_mark: |
| other  | :x:                |

Only the `main` branch (the version deployed to production) receives security fixes.

## Reporting a vulnerability

Please **do not** open a public GitHub issue, discussion or pull request for security problems.

Report vulnerabilities privately through GitHub private vulnerability reporting:
<https://github.com/Nikhilvig8/Tass_CPSC/security/advisories/new>

(or **Security** tab -> **Report a vulnerability**). Please include:

- the affected page, controller or file,
- steps to reproduce, and
- the impact you observed.

## Disclosure process

1. We acknowledge the report within 5 working days.
2. We confirm the vulnerability and assess its severity.
3. We prepare and deploy a fix, then share the timeline with the reporter.
4. We publish a security advisory once the fix is released. Please keep the issue
   confidential until then (coordinated disclosure).

## Handling secrets

Connection strings and application secrets are never committed. They live in the
git-ignored `connectionStrings.config` and `secrets.config` files (templates:
`*.config.example`). If a secret is ever committed by mistake, treat it as leaked
and rotate it.

See `SECURITY_REMEDIATION.md` for the history of security fixes.
