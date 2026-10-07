# NoodKa Roadmap

NoodKa is being built incrementally.

The roadmap describes the direction of the product, not a promise that every item will be implemented exactly as written. Priorities may change as we learn from real usage, technical constraints, security requirements, and creator feedback.

---

## Completed Foundations

The following foundations are already in place:

- Initial NoodKa solution structure
- Clear Domain, Application, Infrastructure, and API boundaries
- React/TypeScript web application
- ASP.NET Core API
- SQLite-based development persistence
- Story, Episode, Scene, and Shot hierarchy
- Character profiles and reference support
- Asset catalog and local asset storage
- Cinematic prompt generation
- AI image generation integration
- Saved-shot image generation workflow
- Automated backend test coverage
- Public GitHub repository
- Public-release security review
- Development documentation
- Security documentation
- Architecture documentation

These foundations provide the base for building the actual NoodKa creative experience.

---

## Current Focus

### Documentation and Engineering Foundation

The immediate goal is to establish a strong development foundation before adding major new product capabilities.

This includes:

- README and project documentation
- Development workflow rules
- Security practices
- Architecture documentation
- Roadmap documentation
- Public repository discipline
- Repeatable build and test workflows
- Clear separation between development and production concerns

A strong foundation should make future development faster and safer rather than slower.

---

## Near-Term Product Priorities

### 1. Story Production Workflow

Make the existing Story → Episode → Scene → Shot structure feel like a coherent creative workflow.

Potential improvements include:

- Easier story creation
- Better episode management
- Scene organization
- Shot creation and editing
- Better navigation between production levels
- More useful story and production summaries
- Improved creative workflow from concept to generated assets

The goal is to make NoodKa feel like a studio rather than a collection of technical screens.

---

### 2. Character Continuity

Characters are central to visual storytelling.

Future work should improve:

- Character creation
- Character profiles
- Character references
- Visual consistency
- Character reuse across shots
- Character relationships
- Character-aware prompting
- Consistent appearance across generated images

Character continuity should become one of NoodKa's strongest creative capabilities.

---

### 3. Asset Management

Expand the existing asset system into a more useful creative asset workflow.

Potential areas include:

- Better asset browsing
- Asset metadata
- Asset categorization
- Character assets
- Location assets
- Prop assets
- Generated image organization
- Reusing assets across stories and shots
- Asset relationships
- Better preview and selection workflows

The asset system should help creators build a reusable creative library instead of repeatedly recreating the same elements.

---

### 4. Studio Experience

Continue evolving the web application into a cohesive creative studio.

Areas may include:

- Better navigation
- Better editing experiences
- Improved shot workflow
- Improved asset selection
- Better previews
- More intuitive creative controls
- Clearer generation states
- Better error handling
- More useful progress feedback

Frontend complexity should be managed deliberately. Componentization should be introduced where it improves maintainability and reuse rather than simply increasing the number of files.

---

### 5. AI Generation Workflow

Expand AI capabilities while keeping the workflow understandable and controllable.

Potential capabilities include:

- Better cinematic prompts
- Character-aware generation
- Scene-aware generation
- Asset-aware generation
- Generation history
- Regeneration
- Variation workflows
- Image refinement
- Consistency controls
- Provider abstraction where useful

AI generation should remain an assistive creative tool. The creator should remain in control of the story and creative decisions.

---

## Production Readiness

NoodKa's current architecture is a development foundation and is not yet a production-ready hosted service.

Before operating a public multi-user service, the platform will need capabilities such as:

### Security

- Authentication
- Authorization
- Multi-user isolation
- Secure secret management
- API protection
- Input validation
- Rate limiting
- Abuse prevention
- Secure file handling

### AI Usage and Cost Control

AI generation creates direct operating costs.

Production capabilities will likely need:

- Per-user usage tracking
- Usage quotas
- Generation limits
- Cost monitoring
- Abuse protection
- Provider usage monitoring
- Clear user-facing limits
- Appropriate billing or subscription controls

NoodKa should not expose an unlimited expensive AI generation endpoint without safeguards.

### Data and Persistence

Production persistence may require:

- A production-grade database strategy
- Database migrations
- Backup and recovery
- Data isolation
- Data lifecycle policies
- Reliability monitoring

SQLite is appropriate for the current development foundation, but production requirements should determine whether and when a different database architecture is justified.

### Storage

Production asset storage may require:

- Cloud object storage
- Secure access controls
- Upload validation
- Asset lifecycle management
- Backup/recovery strategy
- Efficient delivery of generated media

### Background Processing

As AI generation becomes more capable and expensive, background processing may become necessary for:

- Image generation
- Video generation
- Audio generation
- Large processing jobs
- Retry handling
- Queue management
- Long-running workflows

These capabilities should be introduced when actual product requirements justify them.

### Observability

Production operations will eventually need:

- Structured logging
- Error tracking
- Metrics
- Health monitoring
- AI provider monitoring
- Cost monitoring
- Performance monitoring
- Operational alerts

### Deployment

A production service will also require:

- Deployment automation
- Environment management
- Secret management
- Infrastructure configuration
- Database deployment
- Storage configuration
- Backup procedures
- Recovery procedures
- Safe upgrade processes

---

## Future Creative Capabilities

The long-term product direction may expand beyond image generation.

Potential areas include:

### Video

- AI video generation
- Shot-to-video workflows
- Scene sequences
- Character continuity across video
- Storyboard-to-video workflows

### Audio

- Voice generation
- Character voices
- Narration
- Sound effects
- Music
- Dialogue workflows

### Story-to-Production

A longer-term goal is to allow creators to move naturally from:

```text
Idea
  ↓
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
Images
  ↓
Video
  ↓
Audio
  ↓
Finished Creative Work
```

This does not mean every step needs to be automated.

The goal is to reduce repetitive work while preserving creative control.

---

## Filipino Creative and Cultural Capabilities

NoodKa's identity is important.

The product should become increasingly useful for Filipino creators and Filipino stories.

Potential areas include:

- Filipino language support
- Taglish workflows
- Filipino cultural context
- Filipino locations
- Filipino character archetypes
- Local storytelling styles
- Filipino visual references
- Local creator workflows
- Affordable access for Filipino creators

These capabilities should be treated as product strengths rather than cosmetic localization.

NoodKa should help demonstrate that AI creative tools can be built with Filipino creators and stories in mind.

---

## Collaboration

As the product matures, collaboration may become valuable.

Potential capabilities include:

- Shared projects
- Creator teams
- Roles and permissions
- Comments
- Reviews
- Version history
- Shared asset libraries
- Production workflows

Collaboration should only be introduced when the product has a clear need for it.

---

## Platform and Business Evolution

If NoodKa becomes a hosted service, the product may eventually require:

- User accounts
- Subscription plans
- Usage-based limits
- Payment integration
- AI cost management
- Creator analytics
- Service monitoring
- Customer support workflows

The business model should support the product's goal of making AI creative tools accessible while keeping the service financially sustainable.

---

## What We Should Avoid

NoodKa should avoid adding complexity simply because a technology is available.

Examples include:

- Premature microservices
- Unnecessary infrastructure
- Over-engineered abstractions
- Features without a clear creator benefit
- Unlimited expensive AI operations
- Security-sensitive shortcuts
- Duplicate implementations
- Large architectural rewrites without a demonstrated need

The question should always be:

> Does this make NoodKa meaningfully better for creators?

---

## Roadmap Philosophy

The roadmap should remain flexible.

Priorities should be driven by:

1. Creator value
2. Product usability
3. Security
4. Reliability
5. Maintainability
6. Operating cost
7. Actual usage patterns

Technical sophistication is not the goal.

**A useful, secure, maintainable creative product is the goal.**

---

## Long-Term Vision

NoodKa should become more than an AI image generator.

The long-term vision is an AI-powered creative studio where Filipino creators can develop stories, characters, scenes, shots, images, video, and audio within one connected workflow.

The technology should support the creator rather than replace the creator.

The product should remain accessible, understandable, and culturally relevant as it grows.

Most importantly, NoodKa should be something we can be proud to build as a Filipino AI product.

---

## Roadmap Decision Rule

Before adding a major feature, ask:

1. Does it help creators?
2. Does it support the NoodKa vision?
3. Is there a real user problem behind it?
4. Is the current architecture sufficient?
5. What security risks does it introduce?
6. What operating cost does it introduce?
7. Can we build it incrementally?
8. Can we test it properly?
9. Will it make NoodKa easier or harder to use?
10. Are we building the right thing, or simply building something because we can?

The roadmap should evolve with the product.

We should build NoodKa deliberately, one meaningful capability at a time.
