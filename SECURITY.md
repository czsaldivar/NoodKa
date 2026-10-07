# NoodKa Security Guide

NoodKa is a public GitHub repository.

Security must therefore be considered during development, code review, Git operations, deployment, and future product design.

## Core Principle

> **If it is secret, it does not belong in Git.**

Never commit credentials or sensitive information to the repository.

## Never Commit Secrets

The following must never be committed:

- OpenAI API keys
- Other API keys
- Passwords
- Access tokens
- Refresh tokens
- Private certificates
- Private keys
- Production credentials
- Database credentials
- Cloud credentials
- Connection strings containing secrets
- Personal authentication data
- Sensitive user information

If something is intended to remain private, it should not be stored in source control.

## Environment Configuration

Local development configuration should remain outside Git when it contains secrets.

NoodKa's repository ignores local environment files such as:

```text
.env
.env.*
```

while allowing an example configuration file such as:

```text
.env.example
```

An example configuration may document required variable names and safe placeholder values, but must never contain real credentials.

## API Keys

AI providers and other external services may require API credentials.

API keys should be supplied through secure local or deployment configuration.

Do not:

- hard-code API keys in source code
- place API keys in frontend source
- commit API keys to Git
- place production credentials in documentation
- expose secret values through diagnostic endpoints
- print secret values into logs

A frontend application should never receive a provider secret that is intended to remain server-side.

## If a Secret Is Accidentally Committed

Deleting the file is not automatically sufficient.

If a real secret is committed:

1. Stop further distribution of the affected commit.
2. Identify the exposed credential.
3. Rotate or revoke the credential immediately.
4. Determine whether the secret exists in Git history.
5. Remove sensitive history when necessary.
6. Verify the repository after cleanup.
7. Replace the credential through secure configuration.

Assume a credential is compromised once it has been pushed to a public repository.

## Generated Media

Generated AI media should not automatically be committed to Git.

The repository ignores generated media locations including:

```text
assets/generated-images/
assets/generated-videos/
assets/generated-audio/
assets/generated-renders/
```

Generated output can become large, expensive to store, and difficult to manage through source control.

Future production storage should use an appropriate asset-storage strategy rather than Git.

## Local Databases

NoodKa currently uses SQLite during development.

Local application data should not be committed to the repository.

The repository ignores local application data locations such as:

```text
App_Data/
```

Production database architecture and data protection will be addressed separately when NoodKa moves toward a hosted service.

## Development-Only Endpoints

Development and diagnostic endpoints must be treated as potentially unsafe for production.

NoodKa currently includes development-specific functionality that is intended for local development.

Development-only endpoints must:

- be clearly identified
- remain appropriately restricted
- avoid exposing secrets
- avoid exposing private data
- avoid performing uncontrolled expensive operations
- be reviewed before production deployment

A development endpoint should never become publicly accessible simply because it was useful during development.

## Input Validation

All externally supplied data should eventually be treated as untrusted input.

Production APIs will require appropriate validation for:

- identifiers
- text fields
- uploaded content
- generated asset requests
- story data
- character data
- AI generation parameters

Validation should occur at appropriate application boundaries rather than relying only on frontend validation.

## Authentication and Authorization

The current development foundation is not a complete multi-user security model.

Before a hosted service is released, NoodKa will require:

- user authentication
- authorization
- ownership checks
- user isolation
- secure session/token handling
- protection of administrative operations

A user's ability to access an object must not be determined solely by whether the object identifier is known.

## AI Cost Protection

AI generation can create real operating costs.

Before NoodKa becomes a public service, AI generation endpoints will require appropriate controls such as:

- authentication
- rate limiting
- usage quotas
- per-user limits
- request validation
- cost monitoring
- abuse detection
- appropriate model and quality controls

A public endpoint must not allow an unauthenticated or uncontrolled caller to generate unlimited paid AI requests.

## File and Asset Security

Future hosted asset handling must consider:

- file type validation
- file size limits
- safe filenames
- path traversal protection
- storage isolation
- access authorization
- malware and unsafe-content considerations where appropriate
- signed or authorized access to private assets

User-provided files should never be assumed to be safe simply because they have a familiar extension.

## Database Security

Production database design must consider:

- authentication
- authorization
- least-privilege access
- connection security
- migrations
- backups
- recovery
- data isolation
- sensitive-data handling

Development SQLite files are not a substitute for a production data-security strategy.

## Logging

Logs should help diagnose problems without exposing secrets or sensitive user information.

Never intentionally log:

- API keys
- passwords
- authentication tokens
- private credentials
- sensitive personal information

Production logging should also consider retention, access control, and cost.

## Error Handling

Production errors should provide enough information for troubleshooting without revealing sensitive implementation details.

Avoid returning:

- stack traces to untrusted users
- connection strings
- secret configuration values
- filesystem paths that reveal sensitive infrastructure
- internal credentials

Detailed diagnostics belong in appropriately protected server-side logs.

## Public Repository Review

Before pushing a significant milestone to the public repository, review:

```text
[ ] No API keys
[ ] No passwords
[ ] No tokens
[ ] No private keys
[ ] No production credentials
[ ] No sensitive personal data
[ ] No local machine paths that should remain private
[ ] No local databases
[ ] No generated media that should remain local
[ ] No temporary backups
[ ] No unnecessary diagnostic output
[ ] Development-only endpoints remain appropriately controlled
[ ] Documentation does not expose secrets
```

Useful checks include:

```powershell
git status --short
git diff --check
git diff
```

A broader secret scan should also be performed before major public releases.

## Dependency Security

Third-party dependencies should be reviewed as the project evolves.

Before production release, NoodKa should establish a process for:

- dependency updates
- security advisories
- vulnerable package detection
- removing unnecessary dependencies
- reviewing new dependencies before adoption

A dependency should provide meaningful value before becoming part of the product.

## Deployment Security

Production deployment will require security controls beyond the local development environment.

These will include appropriate:

- secret management
- HTTPS/TLS
- access controls
- infrastructure permissions
- network restrictions
- database protection
- storage protection
- monitoring
- backups
- recovery procedures

Deployment configuration must never require committing production secrets to the repository.

## Responsible Vulnerability Reporting

If a security vulnerability is discovered in NoodKa, avoid publicly publishing sensitive exploit details before the issue can be evaluated and addressed.

A future production project should establish a dedicated security reporting process and contact method.

## Security Mindset

Security is not a final checklist performed immediately before launch.

It is part of normal development.

When implementing a feature, ask:

1. What data does this feature access?
2. Who should be allowed to access it?
3. Can this operation create financial cost?
4. Can user input abuse it?
5. Does it expose sensitive information?
6. Does it behave differently in development and production?
7. What happens if the caller is malicious?

## Current Security Position

NoodKa's current public repository has been reviewed for obvious credential exposure and common repository artifacts before being made public.

That review does not mean NoodKa is production-secure.

The project is still a development foundation.

Production security will require additional work across application security, authentication, authorization, infrastructure, storage, database design, monitoring, and operational controls.

## Security Principle

NoodKa should remain safe to develop in public.

The standard is simple:

> **Never trade security for convenience when the cost of doing it correctly is reasonable.**
