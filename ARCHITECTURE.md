# NoodKa Architecture

This document describes the current architecture of NoodKa.

The purpose is to keep architectural decisions understandable as the project grows and to prevent future changes from unintentionally breaking the project's core structure.

This document describes the implementation as it exists today. Future production architecture may evolve as product requirements become clearer.

## Architectural Overview

NoodKa currently consists of a React web application and a layered .NET backend.

```text
┌──────────────────────────────┐
│        NoodKa.Web            │
│      React + TypeScript      │
└──────────────┬───────────────┘
               │ HTTP
               ▼
┌──────────────────────────────┐
│        NoodKa.Api            │
│       ASP.NET Core           │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│     NoodKa.Application       │
│   Use Cases / Interfaces     │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│       NoodKa.Domain          │
│   Core Business Concepts     │
└──────────────────────────────┘

        Infrastructure
              │
       ┌──────┼─────────┐
       ▼      ▼         ▼
    SQLite  Storage   OpenAI
```

The main architectural goal is to keep business concepts independent from infrastructure details wherever practical.

## Projects

### NoodKa.Domain

The Domain project contains the core business concepts of NoodKa.

Examples include concepts associated with:

- Stories
- Episodes
- Scenes
- Shots
- Characters
- Creative assets

The Domain layer should remain independent from infrastructure concerns.

It should not depend on:

- ASP.NET Core
- database implementations
- OpenAI client implementations
- frontend code

## NoodKa.Application

The Application project contains application-level contracts and use-case logic.

Examples include interfaces and services for:

- Asset management
- Story management
- Character management
- Shot generation
- AI image generation
- Cinematic prompt construction

The Application layer defines what the system needs without requiring the concrete infrastructure implementation.

## NoodKa.Infrastructure

The Infrastructure project contains concrete implementations of external or persistence-related concerns.

Current responsibilities include:

- SQLite repositories
- Asset catalog persistence
- Local asset storage
- OpenAI image generation
- Infrastructure-specific implementations of Application interfaces

Infrastructure depends on Application and Domain.

The goal is to keep infrastructure replaceable where practical.

For example, a future cloud storage implementation should be able to replace local storage without forcing the Domain model to understand cloud-storage details.

## NoodKa.Api

The API project provides the HTTP boundary for the backend.

It currently configures dependency injection and exposes endpoints for areas including:

- Assets
- Stories
- Episodes
- Scenes
- Shots
- Shot image generation
- Characters

The API currently uses ASP.NET Core minimal API style.

The API is responsible for translating HTTP requests into application operations and returning appropriate responses.

Business logic should not unnecessarily accumulate inside the API entry point.

## NoodKa.Web

NoodKa.Web is the React and TypeScript frontend.

Current technology includes:

- React
- TypeScript
- Vite
- Oxlint

The frontend communicates with the NoodKa API.

The web application currently provides the development-facing creative studio experience.

The frontend is intentionally separate from the backend projects.

## Dependency Direction

The current backend dependency direction is:

```text
NoodKa.Api
    ↓
NoodKa.Application
    ↓
NoodKa.Domain

NoodKa.Infrastructure
    ↓
NoodKa.Application
    ↓
NoodKa.Domain
```

The Domain project has no project dependency on the other NoodKa projects.

The API uses Infrastructure implementations through dependency injection.

This keeps concrete infrastructure concerns away from the core Domain model.

## Story Hierarchy

The current storytelling model follows this hierarchy:

```text
Story
  │
  ├── Episode
  │      │
  │      └── Scene
  │             │
  │             └── Shot
  │
  └── Characters / Creative Assets
```

This structure reflects the intended creative workflow.

A Story provides the larger narrative context.

An Episode organizes a portion of the story.

A Scene represents a specific narrative situation.

A Shot represents an individual cinematic unit that can be generated and refined.

## Shots

Shots currently contain information used to describe cinematic intent.

Examples include:

- Duration
- Action
- Emotion
- Camera
- Lighting

The shot-generation workflow can use this information to construct an AI generation prompt.

Generated images can then become assets associated with the shot.

## Characters

NoodKa has a dedicated character model and persistence layer.

Character information currently includes concepts such as:

- Name
- Gender
- Personality description
- Appearance
- Hair
- Typical clothing
- Visual style
- Character references

Character references can provide additional material for maintaining visual identity.

The long-term goal is to improve character continuity across generated creative content.

## Assets

NoodKa separates asset catalog information from physical asset storage.

The current system includes:

- Asset catalog
- Local asset storage
- Asset metadata
- Associations between assets and creative entities

This separation allows the storage implementation to evolve without forcing the rest of the application to depend on a specific storage mechanism.

## AI Image Generation

The current AI image-generation flow is conceptually:

```text
Shot
  ↓
Shot information
  ↓
Cinematic Prompt Builder
  ↓
Image Generator
  ↓
Generated Image
  ↓
Asset Storage
  ↓
Asset Catalog
  ↓
Shot / Creative Workflow
```

The application defines the required interfaces.

Infrastructure provides the concrete OpenAI implementation.

This separation allows the AI provider implementation to evolve without placing provider-specific details throughout the application.

## Persistence

The current development implementation uses SQLite.

The API currently maintains separate persistence areas for major concerns including:

- Assets
- Stories
- Characters

This is appropriate for the current development stage because it keeps local setup simple and makes the system easy to run.

The production persistence strategy may change as multi-user requirements emerge.

## Asset Storage

The current development environment uses local asset storage.

The system stores generated assets outside the source-code repository.

This keeps generated media separate from Git-managed application source.

Future hosted deployment will likely require a dedicated cloud storage architecture with appropriate access controls.

## API Endpoints

The current API exposes functionality for areas including:

```text
Assets
Stories
Episodes
Scenes
Shots
Shot Image Generation
Characters
```

There is also development-specific functionality intended for local development and testing.

Development-only endpoints must be reviewed before production deployment.

## Dependency Injection

The API configures infrastructure implementations through dependency injection.

Examples include:

```text
IImageGenerator
        ↓
OpenAIImageGenerator

IShotGenerator
        ↓
ShotGenerator

ICinematicPromptBuilder
        ↓
CinematicPromptBuilder
```

This allows application-level code to depend on abstractions rather than concrete infrastructure implementations.

## Testing Architecture

NoodKa currently has separate test projects for major backend layers:

```text
tests/
├── NoodKa.Application.Tests/
├── NoodKa.Domain.Tests/
└── NoodKa.Infrastructure.Tests/
```

Tests currently cover application, domain, and infrastructure behavior.

The goal is to keep important behavior protected as the architecture evolves.

## Frontend Architecture

The current frontend is a React and TypeScript application.

The frontend currently contains a substantial studio experience centered around the story and creative workflow.

As the application grows, the frontend should evolve toward clearer component and feature boundaries where that improves maintainability.

Large frontend files should not be split purely for the sake of splitting them. Componentization should follow meaningful responsibilities and user-facing features.

## Current Architectural Limitations

The current architecture is intentionally a development foundation.

It is not yet a complete production architecture.

Known future areas include:

- Authentication
- Authorization
- Multi-user isolation
- Production database architecture
- Cloud asset storage
- Background jobs
- AI generation queues
- Usage tracking
- Cost controls
- Rate limiting
- Monitoring
- Logging
- Backup and recovery
- Deployment automation

These concerns should be introduced deliberately rather than prematurely complicating the current development system.

## Architectural Principles

NoodKa should follow these principles:

### Keep the Core Understandable

Architecture should help developers understand the product rather than obscure it behind unnecessary abstraction.

### Prefer Clear Boundaries

Projects and components should have understandable responsibilities.

### Depend on Abstractions Where It Helps

Interfaces are useful when they create meaningful separation between application behavior and infrastructure.

Abstraction should not be introduced solely because a pattern exists.

### Keep Infrastructure Replaceable Where Practical

Storage providers, AI providers, and other external dependencies should not unnecessarily leak into core business logic.

### Build Incrementally

Architecture should evolve with real product requirements.

Avoid designing a large theoretical system before the product needs it.

### Protect the Product Vision

Technical decisions should support NoodKa's larger goal of becoming an AI creative platform for Filipino creators and stories.

## Future Architecture

The future hosted architecture will likely require additional services and infrastructure.

Possible future components include:

```text
Users
  ↓
Web Application
  ↓
Authenticated API
  ↓
Application Services
  ↓
Domain
  ├── Database
  ├── Object Storage
  ├── AI Services
  ├── Background Jobs
  └── Monitoring
```

This is a direction, not a committed implementation.

Future architecture should be based on actual product requirements, usage patterns, security needs, and operating costs.

## Architectural Decision Rule

When considering a significant architectural change, ask:

1. What problem does this solve?
2. Does the current architecture actually prevent the feature?
3. Is the new complexity justified?
4. Does the change preserve clear boundaries?
5. Does it improve maintainability?
6. Does it introduce security or cost risks?
7. Can it be introduced incrementally?
8. Will future developers understand why it exists?

The best architecture for NoodKa is not the most complicated architecture.

It is the architecture that allows the product to grow safely while remaining understandable.
