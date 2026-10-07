# NoodKa Development Guide

This document defines the development practices and engineering rules for NoodKa.

NoodKa is a public GitHub project, so development discipline, security, maintainability, and clear project history are especially important.

## Source of Truth

The GitHub `main` branch is the canonical NoodKa source of truth.

Development should be performed against the current state of `main`.

Before making changes:

1. Check the current Git status.
2. Inspect the relevant source files.
3. Understand the existing implementation.
4. Make changes against the current implementation rather than assumptions about an older version.

## Inspect Before Modifying

Do not blindly replace or rewrite source files.

When a change depends on existing code:

1. Inspect the current file.
2. Identify the exact implementation being changed.
3. Confirm the expected surrounding structure.
4. Make the smallest appropriate change.
5. Inspect the resulting diff.

This is particularly important for large files and the NoodKa web application.

## PowerShell-Driven Development

PowerShell is the primary development interface for repeatable local changes.

Whenever practical:

- Use copy-and-pasteable PowerShell commands.
- Automate file creation and replacement.
- Avoid requiring manual editing of large files.
- Use scripts that verify expected source content before modifying it.
- Stop the operation when an expected source pattern is missing.

Development scripts should use portable project-relative paths rather than machine-specific absolute paths.

## Incremental Changes

NoodKa should be developed incrementally.

Prefer:

```text
Inspect
  ↓
Small Change
  ↓
Build
  ↓
Test
  ↓
Review Diff
  ↓
Commit
  ↓
Push
```

Avoid combining unrelated changes into a single milestone.

Small changes make failures easier to diagnose and Git history easier to understand.

## Preserve Architectural Boundaries

NoodKa currently uses a layered architecture involving:

- NoodKa.Domain
- NoodKa.Application
- NoodKa.Infrastructure
- NoodKa.Api
- NoodKa.Web

Changes should respect the responsibilities of these layers.

Do not introduce dependencies between layers simply because doing so is convenient.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the current architecture.

## Build and Test Before Commit

A meaningful source change should be validated before it is committed.

For backend changes:

```powershell
dotnet build
dotnet test
```

For web changes:

```powershell
cd src/NoodKa.Web
npm run build
npm run lint
```

Return to the repository root after web validation when continuing Git operations.

A failed build or test should normally be resolved before committing the related change.

## Git Workflow

The expected workflow is:

```powershell
git status
```

Inspect the implementation and make the change.

Then validate:

```powershell
dotnet build
dotnet test
```

and, when applicable:

```powershell
cd src/NoodKa.Web
npm run build
npm run lint
cd ../..
```

Review:

```powershell
git diff
git diff --check
git status --short
```

Then create a meaningful commit:

```powershell
git add <specific-files>
git commit -m "Describe the milestone"
```

After a successful milestone:

```powershell
git push origin main
```

## Meaningful Commits

Commit messages should describe the actual milestone.

Good examples:

```text
Build NoodKa studio foundation
Add character editing workflow
Integrate asset catalog with shot generation
Document NoodKa development standards
```

Avoid meaningless messages such as:

```text
update
changes
fix
stuff
test
```

A commit should represent a coherent, understandable point in the project's history.

## Push Successful Milestones

Because GitHub `main` is the canonical source of truth, successful milestones should be pushed.

The goal is to keep:

```text
Local working tree
        ↓
Local Git history
        ↓
GitHub main
```

synchronized at meaningful development checkpoints.

## Never Commit Secrets

Never commit:

- API keys
- Passwords
- Access tokens
- Private certificates
- Production credentials
- Secret configuration
- Personal credentials
- Sensitive user information

Use local environment configuration or an appropriate secret-management system.

If a secret is ever accidentally committed:

1. Stop.
2. Rotate or revoke the exposed secret.
3. Determine whether Git history must be cleaned.
4. Verify the repository again.
5. Do not assume deleting the file is sufficient.

See [SECURITY.md](SECURITY.md).

## Keep Generated and Local Files Out of Git

Do not commit development artifacts such as:

- `bin/`
- `obj/`
- local databases
- local generated media
- temporary backups
- inspection output
- machine-specific configuration
- editor state
- temporary context files

The `.gitignore` should be maintained as the project evolves.

## Public Repository Discipline

NoodKa is public.

Before pushing a significant milestone, consider:

- Could this expose a secret?
- Could this expose private data?
- Could this reveal a local machine path?
- Is this generated or temporary content?
- Is this development-only behavior clearly controlled?
- Does the documentation still match the implementation?
- Does the change introduce unnecessary security or cost risk?

Public source code should be treated as code that anyone can inspect.

## Development vs Production

A feature working locally does not mean it is production-ready.

Before NoodKa becomes a public hosted service, the system will require appropriate:

- Authentication
- Authorization
- User isolation
- Input validation
- Rate limiting
- Usage quotas
- AI cost controls
- Secret management
- Database security
- Storage security
- Logging
- Monitoring
- Error handling
- Backup and recovery
- Deployment controls

Development conveniences must not automatically become production behavior.

## Development-Only Endpoints

Development endpoints must remain clearly controlled.

For example, test or diagnostic endpoints should not be exposed in production simply because they are useful during development.

Any endpoint introduced for development purposes must be reviewed before production deployment.

## Avoid Duplicate Functionality

Before implementing a new feature:

1. Search the existing codebase.
2. Determine whether similar functionality already exists.
3. Reuse existing abstractions where appropriate.
4. Extend the existing implementation when that is cleaner than creating a parallel system.

Duplicate implementations increase maintenance cost and create inconsistent behavior.

## Documentation Must Match Reality

Documentation is part of the project.

When architecture, setup, security rules, or development practices change, update the relevant documentation.

Do not document planned functionality as though it already exists.

The following documents should remain aligned with the actual project:

- [README.md](README.md)
- [DEVELOPMENT.md](DEVELOPMENT.md)
- [SECURITY.md](SECURITY.md)
- [ARCHITECTURE.md](ARCHITECTURE.md)
- [ROADMAP.md](ROADMAP.md)

## Quality Over Speed

NoodKa is intended to become a real product, not simply a demonstration.

Prefer:

- Understandable code
- Clear architecture
- Automated tests
- Incremental changes
- Reproducible development
- Security-conscious decisions
- Maintainable interfaces
- Honest documentation

A slower change that leaves the project healthier is usually better than a fast change that creates technical debt.

## Engineering Partnership

Development decisions should be challenged when necessary.

The goal is not for every proposed idea to be accepted automatically.

When an approach introduces unnecessary complexity, security risk, architectural problems, duplication, or significant future maintenance cost, it should be identified and discussed before implementation.

NoodKa should evolve through deliberate engineering decisions while preserving its Filipino creative vision.
