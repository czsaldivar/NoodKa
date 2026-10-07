# 🇵🇭 NoodKa

NoodKa is an AI-powered storytelling and creative studio focused on building tools for Filipino creators and stories.

NoodKa combines structured storytelling, characters, scenes, cinematic shots, creative assets, and AI generation into a single workflow.

> **Project status:** NoodKa is an evolving project. The current implementation is a development foundation and is not yet a production-ready hosted service.

## Vision

NoodKa aims to become a Filipino AI creative platform that makes it easier to develop stories from an initial idea into production-ready creative assets.

The long-term workflow is envisioned as:

```text
Story
  ↓
Episodes
  ↓
Scenes
  ↓
Shots
  ↓
Characters / Locations / Props
  ↓
AI Generation
  ↓
Creative Assets
```

The goal is to build technology that Filipino creators can be proud of while keeping the platform accessible and practical.

## Current Capabilities

The current foundation includes:

- Story persistence
- Episode, scene, and shot persistence
- Character profiles and references
- Creative asset catalog
- Local asset storage
- Cinematic prompt generation
- OpenAI image generation
- Saved-shot image generation
- React + TypeScript web interface
- SQLite persistence
- Automated .NET tests

## Architecture

NoodKa currently follows a layered .NET architecture:

```text
NoodKa.Web
    ↓
NoodKa.Api
    ↓
NoodKa.Application
    ↓
NoodKa.Domain

NoodKa.Infrastructure
    ↓
Application / Domain
    ↓
SQLite / Local Storage / OpenAI
```

See [ARCHITECTURE.md](ARCHITECTURE.md) for more detail.

## Technology

### Backend

- .NET 10
- C#
- ASP.NET Core
- SQLite
- OpenAI API

### Frontend

- React
- TypeScript
- Vite
- Oxlint

### Testing

- xUnit
- .NET test infrastructure

## Repository Structure

```text
NoodKa/
├── src/
│   ├── NoodKa.Api/
│   ├── NoodKa.Application/
│   ├── NoodKa.Domain/
│   ├── NoodKa.Infrastructure/
│   └── NoodKa.Web/
│
├── tests/
│   ├── NoodKa.Application.Tests/
│   ├── NoodKa.Domain.Tests/
│   └── NoodKa.Infrastructure.Tests/
│
├── README.md
├── DEVELOPMENT.md
├── SECURITY.md
├── ARCHITECTURE.md
└── ROADMAP.md
```

## Getting Started

From the repository root:

```powershell
dotnet restore
dotnet build
dotnet test
```

For the web application:

```powershell
cd src/NoodKa.Web
npm install
npm run build
npm run dev
```

## Configuration

Secrets and API credentials must never be committed to Git.

Use local environment configuration for development and follow [SECURITY.md](SECURITY.md) for security rules.

## Development Workflow

NoodKa follows a disciplined development workflow:

1. Inspect the current implementation.
2. Make the smallest appropriate change.
3. Build the solution.
4. Run tests.
5. Validate the web application when applicable.
6. Review the Git diff.
7. Commit meaningful milestones.
8. Push successful milestones to GitHub.

See [DEVELOPMENT.md](DEVELOPMENT.md).

## Security

NoodKa is a public repository.

Never commit:

- API keys
- Passwords
- Tokens
- Private certificates
- Local secret files
- Production credentials
- Private user data

See [SECURITY.md](SECURITY.md).

## Roadmap

The project is being developed incrementally toward a complete AI storytelling and production workflow.

See [ROADMAP.md](ROADMAP.md).

## Project Status

NoodKa is actively under development.

The public repository represents the canonical development source. Production deployment, authentication, multi-user security, usage controls, cloud infrastructure, and operational monitoring will be addressed before a public hosted service is released.

## Contributing

Development should preserve the project's architecture, security standards, Filipino creative focus, and long-term product vision.

Before making significant changes, review:

- [DEVELOPMENT.md](DEVELOPMENT.md)
- [SECURITY.md](SECURITY.md)
- [ARCHITECTURE.md](ARCHITECTURE.md)
- [ROADMAP.md](ROADMAP.md)

## License

The project's final license and distribution model will be established as the product and contribution strategy are finalized.
