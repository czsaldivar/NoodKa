using NoodKa.Application.AI.Images;
using NoodKa.Application.Assets;
using NoodKa.Application.Prompts;
using NoodKa.Application.Shots;
using NoodKa.Application.Stories;
using NoodKa.Application.Characters;
using NoodKa.Infrastructure.Characters;
using NoodKa.Domain.Characters;
using NoodKa.Infrastructure.AI.OpenAI;
using NoodKa.Infrastructure.Assets;
using NoodKa.Infrastructure.Stories;
using NoodKa.Domain.Stories;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Asset storage
var assetRootPath = builder.Configuration["Assets:RootPath"];

if (string.IsNullOrWhiteSpace(assetRootPath))
{
    assetRootPath = Path.Combine(
        builder.Environment.ContentRootPath,
        "assets");
}

builder.Services.AddSingleton<IAssetStorage>(
    _ => new LocalAssetStorage(assetRootPath));

var catalogDatabasePath = builder.Configuration["Assets:CatalogDatabasePath"];

if (string.IsNullOrWhiteSpace(catalogDatabasePath))
{
    catalogDatabasePath = Path.Combine(
        builder.Environment.ContentRootPath,
        "App_Data",
        "noodka-assets.db");
}

builder.Services.AddSingleton<IAssetCatalog>(
    _ => new SqliteAssetCatalog(catalogDatabasePath));


// Story persistence (separate from the asset catalog database)
var storyDatabasePath = builder.Configuration["Stories:DatabasePath"];

if (string.IsNullOrWhiteSpace(storyDatabasePath))
{
    storyDatabasePath = Path.Combine(
        builder.Environment.ContentRootPath,
        "App_Data",
        "noodka-stories.db");
}

builder.Services.AddSingleton<IStoryRepository>(
    _ => new SqliteStoryRepository(storyDatabasePath));
 // Character profile persistence (separate database)
var characterDatabasePath = builder.Configuration["Characters:DatabasePath"];

if (string.IsNullOrWhiteSpace(characterDatabasePath))
{
    characterDatabasePath = Path.Combine(
        builder.Environment.ContentRootPath,
        "App_Data",
        "noodka-characters.db");
}

builder.Services.AddSingleton<ICharacterRepository>(
    _ => new SqliteCharacterRepository(characterDatabasePath));
// Character reference image storage
const long MaxCharacterReferenceBytes = 5 * 1024 * 1024;
const long MaxCharacterReferenceRequestBytes =
    MaxCharacterReferenceBytes + 64 * 1024;

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxCharacterReferenceRequestBytes;
    options.ValueCountLimit = 8;
    options.ValueLengthLimit = 1024;
});

var characterReferenceRootPath =
    builder.Configuration["Characters:ReferenceRootPath"];

if (string.IsNullOrWhiteSpace(characterReferenceRootPath))
{
    characterReferenceRootPath = Path.Combine(
        builder.Environment.ContentRootPath,
        "App_Data",
        "character-references");
}

builder.Services.AddSingleton<ICharacterReferenceStorage>(
    _ => new LocalCharacterReferenceStorage(characterReferenceRootPath));
// AI image generation
builder.Services.AddScoped<IImageGenerator, OpenAIImageGenerator>();

// Cinematic shot generation pipeline
builder.Services.AddScoped<
    ICinematicPromptBuilder,
    CinematicPromptBuilder>();

builder.Services.AddScoped<IShotGenerator, ShotGenerator>();

var app = builder.Build();

 // Asset retrieval endpoints
app.MapGet("/api/assets/recent", async (
    int? take,
    IAssetCatalog assetCatalog,
    IStoryRepository storyRepository,
    CancellationToken cancellationToken) =>
{
    var count = take ?? 50;

    if (count < 1 || count > 200)
    {
        return Results.BadRequest(new
        {
            error = "The take parameter must be between 1 and 200."
        });
    }

    var assets = await assetCatalog.GetRecentAsync(count, cancellationToken);
    var stories = await storyRepository.GetAllAsync(cancellationToken);

    var shotDetails = stories
        .SelectMany(story => story.Episodes.SelectMany(episode =>
            episode.Scenes.SelectMany(scene =>
                scene.Shots.Select(shot => new
                {
                    ShotId = shot.Id,
                    StoryTitle = story.Title,
                    EpisodeTitle = episode.Title,
                    EpisodeNumber = episode.Number,
                    SceneId = scene.Id,
                    SceneNumber = scene.Number,
                    SceneLocation = scene.Location,
                    ShotSequence = shot.Sequence,
                    ShotAction = shot.Action,
                    ShotEmotion = shot.Emotion,
                    ShotCamera = shot.Camera,
                    ShotLighting = shot.Lighting
                }))))
        .ToDictionary(item => item.ShotId);

    return Results.Ok(assets.Select(asset =>
    {
        var isShot = asset.OwnerType == AssetOwnerType.Shot;
        shotDetails.TryGetValue(asset.OwnerId, out var shot);
        var hasShotDetails = isShot && shot is not null;

        return new
        {
            asset.Id,
            asset.OwnerType,
            asset.OwnerId,
            asset.Type,
            asset.ContentType,
            asset.CreatedAtUtc,
            contentUrl = $"/api/assets/{asset.Id}",
            metadataUrl = $"/api/assets/{asset.Id}/metadata",
            storyTitle = hasShotDetails ? shot!.StoryTitle : null,
            episodeTitle = hasShotDetails ? shot!.EpisodeTitle : null,
            episodeNumber = hasShotDetails ? shot!.EpisodeNumber : (int?)null,
            sceneId = hasShotDetails ? shot!.SceneId : (Guid?)null,
            sceneNumber = hasShotDetails ? shot!.SceneNumber : (int?)null,
            sceneLocation = hasShotDetails ? shot!.SceneLocation : null,
            shotSequence = hasShotDetails ? shot!.ShotSequence : (int?)null,
            shotAction = hasShotDetails ? shot!.ShotAction : null,
            shotEmotion = hasShotDetails ? shot!.ShotEmotion : null,
            shotCamera = hasShotDetails ? shot!.ShotCamera : null,
            shotLighting = hasShotDetails ? shot!.ShotLighting : null
        };
    }));
})
.WithName("GetRecentAssets");

app.MapGet("/api/assets/{assetId:guid}", async (
    Guid assetId,
    IAssetCatalog assetCatalog,
    IAssetStorage assetStorage,
    CancellationToken cancellationToken) =>
{
    var asset = await assetCatalog.GetByIdAsync(
        assetId,
        cancellationToken);

    if (asset is null)
    {
        return Results.NotFound();
    }

    var stream = await assetStorage.OpenReadAsync(
        asset.StorageKey,
        cancellationToken);

    if (stream is null)
    {
        return Results.NotFound();
    }

    return Results.Stream(
        stream,
        contentType: asset.ContentType);
})
.WithName("GetAssetContent");

app.MapGet("/api/assets/{assetId:guid}/metadata", async (
    Guid assetId,
    IAssetCatalog assetCatalog,
    CancellationToken cancellationToken) =>
{
    var asset = await assetCatalog.GetByIdAsync(
        assetId,
        cancellationToken);

    if (asset is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new
    {
        asset.Id,
        asset.OwnerType,
        asset.OwnerId,
        asset.Type,
        asset.ContentType,
        asset.CreatedAtUtc
    });
})
.WithName("GetAssetMetadata");



// Story endpoints
app.MapGet("/api/stories", async (
    IStoryRepository storyRepository,
    CancellationToken cancellationToken) =>
{
    var stories = await storyRepository.GetAllAsync(cancellationToken);

    return Results.Ok(stories.Select(story => new
    {
        story.Id,
        story.Title,
        story.Description,
        story.Country,
        story.Region,
        story.Era,
        story.Language,
        story.Genre,
        story.Tone,
        story.CulturalFlavor
    }));
})
.WithName("GetStories");
app.MapGet("/api/stories/{storyId:guid}", async (
    Guid storyId,
    IStoryRepository storyRepository,
    CancellationToken cancellationToken) =>
{
    var stories = await storyRepository.GetAllAsync(cancellationToken);
    var story = stories.FirstOrDefault(item => item.Id == storyId);

    if (story is null)
    {
        return Results.NotFound(new
        {
            error = "Story not found."
        });
    }

    return Results.Ok(new
    {
        story.Id,
        story.Title,
        story.Description,
        story.Country,
        story.Region,
        story.Era,
        story.Language,
        story.Genre,
        story.Tone,
        story.CulturalFlavor,
        Episodes = story.Episodes.Select(episode => new
        {
            episode.Id,
            episode.Number,
            episode.Title,
            Scenes = episode.Scenes.Select(scene => new
            {
                scene.Id,
                scene.Number,
                scene.Location,
                Shots = scene.Shots.Select(shot => new
                {
                    shot.Id,
                    shot.Sequence,
                    DurationSeconds = shot.Duration.TotalSeconds,
                    shot.Action,
                    shot.Emotion,
                    shot.Camera,
                    shot.Lighting
                })
            })
        })
    });
})
.WithName("GetStoryDetails");

app.MapPost("/api/stories/{storyId:guid}/episodes", async (
    Guid storyId,
    CreateEpisodeRequest body,
    IStoryRepository storyRepository,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(body.Title))
    {
        return Results.BadRequest(new
        {
            error = "Episode title is required."
        });
    }

    var episode = await storyRepository.CreateEpisodeAsync(
        storyId,
        body.Title,
        cancellationToken);

    if (episode is null)
    {
        return Results.NotFound(new
        {
            error = "Story not found."
        });
    }

    return Results.Created(
        $"/api/stories/{storyId}",
        new
        {
            episode.Id,
            episode.Number,
            episode.Title
        });
})
.WithName("CreateEpisode");
app.MapPost("/api/episodes/{episodeId:guid}/scenes", async (
    Guid episodeId,
    CreateSceneRequest body,
    IStoryRepository storyRepository,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(body.Name))
    {
        return Results.BadRequest(new
        {
            error = "Scene name is required."
        });
    }

    if (string.IsNullOrWhiteSpace(body.Location))
    {
        return Results.BadRequest(new
        {
            error = "Scene location is required."
        });
    }

    var scene = await storyRepository.CreateSceneAsync(
        episodeId,
        body.Name,
        body.Location,
        cancellationToken);
    if (scene is null)
    {
        return Results.NotFound(new
        {
            error = "Episode not found."
        });
    }

    return Results.Created(
        $"/api/stories",
        new
        {
            scene.Id,
            scene.Number,
            scene.Location
        });
})
.WithName("CreateScene");

app.MapPost("/api/scenes/{sceneId:guid}/shots", async (
    Guid sceneId,
    CreateShotRequest body,
    IStoryRepository storyRepository,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(body.Action))
    {
        return Results.BadRequest(new
        {
            error = "Shot action is required."
        });
    }

    if (body.DurationSeconds <= 0 ||
        double.IsNaN(body.DurationSeconds) ||
        double.IsInfinity(body.DurationSeconds))
    {
        return Results.BadRequest(new
        {
            error = "DurationSeconds must be a positive number."
        });
    }

    var shot = await storyRepository.CreateShotAsync(
        sceneId,
        TimeSpan.FromSeconds(body.DurationSeconds),
        body.Action,
        body.Emotion ?? string.Empty,
        body.Camera ?? string.Empty,
        body.Lighting ?? string.Empty,
        cancellationToken);

    if (shot is null)
    {
        return Results.NotFound(new
        {
            error = "Scene not found."
        });
    }

    return Results.Created(
        $"/api/stories",
        new
        {
            shot.Id,
            shot.Sequence,
            DurationSeconds = shot.Duration.TotalSeconds,
            shot.Action,
            shot.Emotion,
            shot.Camera,
            shot.Lighting
        });
})
.WithName("CreateShot");

app.MapPost("/api/stories", async (
    CreateStoryRequest body,
    IStoryRepository storyRepository,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(body.Title))
    {
        return Results.BadRequest(new
        {
            error = "Story title is required."
        });
    }

    var story = new Story(
        body.Title,
        body.Description ?? string.Empty,
        body.Country ?? "Philippines",
        body.Region ?? "Metro Manila",
        body.Era ?? "Present Day",
        body.Language ?? "Taglish",
        body.Genre ?? "Family Drama",
        body.Tone ?? "Emotional",
        body.CulturalFlavor ?? "Filipino");

    await storyRepository.AddAsync(story, cancellationToken);

    return Results.Created("/api/stories", new
    {
        story.Id,
        story.Title,
        story.Description,
        story.Country,
        story.Region,
        story.Era,
        story.Language,
        story.Genre,
        story.Tone,
        story.CulturalFlavor
    });
})
.WithName("CreateStory");

app.MapPost("/api/shots/{shotId:guid}/generate-image", async (
    Guid shotId,
    IStoryRepository storyRepository,
    IShotGenerator shotGenerator,
    CancellationToken cancellationToken) =>
{
    var stories = await storyRepository.GetAllAsync(cancellationToken);
    var match = stories
        .SelectMany(story => story.Episodes)
        .SelectMany(episode => episode.Scenes)
        .SelectMany(scene => scene.Shots.Select(shot => new { Scene = scene, Shot = shot }))
        .FirstOrDefault(item => item.Shot.Id == shotId);

    if (match is null)
        return Results.NotFound(new { error = "Shot not found." });

    var promptRequest = new CinematicPromptRequest(
        location: match.Scene.Location,
        action: match.Shot.Action,
        emotion: match.Shot.Emotion,
        camera: match.Shot.Camera,
        lighting: match.Shot.Lighting,
        visualStyle: "Photorealistic cinematic film still");

    var result = await shotGenerator.GenerateAsync(
        new ShotGenerationRequest(match.Shot.Id, promptRequest), cancellationToken);

    if (!result.Succeeded)
        return Results.Json(new { error = result.ImageResult.ErrorMessage ?? "Image generation failed.", prompt = result.Prompt }, statusCode: StatusCodes.Status502BadGateway);

    return Results.Ok(new { shotId = match.Shot.Id, prompt = result.Prompt, imageId = result.ImageResult.Id, imageUrl = $"/api/assets/{result.ImageResult.Id}" });
}).WithName("GenerateSavedShotImage");

// Character endpoints
app.MapGet("/api/characters", async (
    ICharacterRepository repository,
    CancellationToken cancellationToken) =>
{
    var characters = await repository.GetAllAsync(cancellationToken);
    return Results.Ok(characters.Select(ToCharacterResponse));
})
.WithName("GetCharacters");

app.MapGet("/api/characters/{characterId:guid}", async (
    Guid characterId,
    ICharacterRepository repository,
    CancellationToken cancellationToken) =>
{
    var character = await repository.GetByIdAsync(
        characterId, cancellationToken);

    return character is null
        ? Results.NotFound(new { error = "Character not found." })
        : Results.Ok(ToCharacterResponse(character));
})
.WithName("GetCharacterById");

app.MapPost("/api/characters/{characterId:guid}/references/face", async (
    Guid characterId,
    HttpRequest request,
    ICharacterRepository repository,
    ICharacterReferenceStorage storage,
    CancellationToken cancellationToken) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest(new
        {
            error = "Send a multipart/form-data request containing one file field named 'file'."
        });
    }

    var character = await repository.GetByIdAsync(
        characterId, cancellationToken);

    if (character is null)
        return Results.NotFound(new { error = "Character not found." });

    var form = await request.ReadFormAsync(cancellationToken);

    if (form.Files.Count != 1)
    {
        return Results.BadRequest(new
        {
            error = "Upload exactly one image using the 'file' field."
        });
    }

    var file = form.Files.GetFile("file");

    if (file is null || file.Length == 0)
        return Results.BadRequest(new { error = "An image file is required." });

    if (file.Length > MaxCharacterReferenceBytes)
    {
        return Results.BadRequest(new
        {
            error = "The image must not exceed 5 MB."
        });
    }

    // Buffer only after the multipart parser has applied its configured limit.
    await using var imageBuffer = new MemoryStream();
    await file.CopyToAsync(imageBuffer, cancellationToken);

    if (imageBuffer.Length == 0 ||
        imageBuffer.Length > MaxCharacterReferenceBytes)
    {
        return Results.BadRequest(new
        {
            error = "The image must be non-empty and no larger than 5 MB."
        });
    }

    var imageBytes = imageBuffer.ToArray();

    var isPng = imageBytes.AsSpan().StartsWith(
        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

    var isJpeg = imageBytes.Length >= 3 &&
        imageBytes[0] == 0xFF &&
        imageBytes[1] == 0xD8 &&
        imageBytes[2] == 0xFF;

    if (!isPng && !isJpeg)
    {
        return Results.BadRequest(new
        {
            error = "Only JPEG and PNG images are supported."
        });
    }

    var extension = isPng ? ".png" : ".jpg";
    var contentType = isPng ? "image/png" : "image/jpeg";
    var fileName = $"{Guid.NewGuid():N}{extension}";

    var description = form["description"].ToString().Trim();
    if (description.Length > 300)
    {
        return Results.BadRequest(new
        {
            error = "Description must not exceed 300 characters."
        });
    }

    await using var uploadStream = new MemoryStream(imageBytes);

    await storage.SaveAsync(
        characterId,
        fileName,
        uploadStream,
        cancellationToken);

    var reference = new CharacterReference(
        CharacterReferenceType.Face,
        fileName,
        string.IsNullOrWhiteSpace(description) ? null : description);

    bool added;

    try
    {
        added = await repository.AddReferenceAsync(
            characterId, reference, cancellationToken);
    }
    catch
    {
        // Preserve the original exception if cleanup itself fails.
        try
        {
            await storage.DeleteAsync(
                characterId, fileName, CancellationToken.None);
        }
        catch
        {
            // Best-effort cleanup.
        }

        throw;
    }

    if (!added)
    {
        // Best-effort cleanup; preserve the intended not-found response.
        try
        {
            await storage.DeleteAsync(
                characterId, fileName, CancellationToken.None);
        }
        catch
        {
            // Cleanup failure must not mask the not-found result.
        }

        return Results.NotFound(new { error = "Character not found." });
    }

    return Results.Created(
        $"/api/characters/{characterId}/references/{reference.Id}/content",
        new
        {
            reference.Id,
            Type = reference.Type.ToString(),
            reference.Description,
            ContentUrl = $"/api/characters/{characterId}/references/{reference.Id}/content",
            ContentType = contentType
        });
})
.WithName("UploadCharacterFaceReference");

app.MapGet(
    "/api/characters/{characterId:guid}/references/{referenceId:guid}/content",
    async (
        Guid characterId,
        Guid referenceId,
        ICharacterRepository repository,
        ICharacterReferenceStorage storage,
        CancellationToken cancellationToken) =>
    {
        var character = await repository.GetByIdAsync(
            characterId, cancellationToken);

        if (character is null)
            return Results.NotFound(new { error = "Character not found." });

        var reference = character.References.FirstOrDefault(
            item => item.Id == referenceId);

        if (reference is null ||
            string.IsNullOrWhiteSpace(reference.StorageLocation) ||
            reference.StorageLocation !=
                Path.GetFileName(reference.StorageLocation))
        {
            return Results.NotFound(new { error = "Reference image not found." });
        }

        var fileName = reference.StorageLocation;
        var isPng = fileName.EndsWith(
            ".png", StringComparison.OrdinalIgnoreCase);
        var isJpeg = fileName.EndsWith(
            ".jpg", StringComparison.OrdinalIgnoreCase);

        if (!isPng && !isJpeg)
            return Results.NotFound(new { error = "Reference image not found." });

        var stream = await storage.OpenReadAsync(
            characterId, fileName, cancellationToken);

        if (stream is null)
            return Results.NotFound(new { error = "Reference image file not found." });

        return Results.File(
            stream,
            isPng ? "image/png" : "image/jpeg");
    })
.WithName("GetCharacterReferenceContent");
app.MapPut("/api/characters/{characterId:guid}", async (
    Guid characterId,
    UpdateCharacterRequest body,
    ICharacterRepository repository,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(body.Name))
        return Results.BadRequest(new { error = "Character name is required." });

    var character = await repository.GetByIdAsync(
        characterId, cancellationToken);

    if (character is null)
        return Results.NotFound(new { error = "Character not found." });

    var gender = CharacterGender.Unspecified;

    if (!string.IsNullOrWhiteSpace(body.Gender) &&
        !Enum.TryParse(body.Gender, true, out gender))
    {
        return Results.BadRequest(new
        {
            error = "Gender must be Unspecified, Male, Female, or NonBinary."
        });
    }

    CharacterPersonality? personality =
        string.IsNullOrWhiteSpace(body.PersonalityDescription)
            ? null
            : new CharacterPersonality(body.PersonalityDescription.Trim());

    CharacterVisualProfile? visualProfile = null;

    if (body.Appearance is not null ||
        body.Hair is not null ||
        body.TypicalClothing is not null ||
        body.VisualStyle is not null)
    {
        visualProfile = new CharacterVisualProfile(
            body.Appearance ?? string.Empty,
            body.Hair ?? string.Empty,
            body.TypicalClothing ?? string.Empty,
            body.VisualStyle ?? string.Empty);
    }

    character.UpdateProfile(
        body.Name.Trim(),
        gender,
        personality,
        visualProfile);

    try
    {
        await repository.UpdateAsync(character, cancellationToken);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { error = "Character not found." });
    }

    return Results.Ok(ToCharacterResponse(character));
})
.WithName("UpdateCharacter");
app.MapPost("/api/characters", async (
    CreateCharacterRequest body,
    ICharacterRepository repository,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(body.Name))
        return Results.BadRequest(new { error = "Character name is required." });

    var gender = CharacterGender.Unspecified;

    if (!string.IsNullOrWhiteSpace(body.Gender) &&
        !Enum.TryParse(body.Gender, true, out gender))
    {
        return Results.BadRequest(new
        {
            error = "Gender must be Unspecified, Male, Female, or NonBinary."
        });
    }

    var character = new Character(body.Name.Trim(), gender);

    if (!string.IsNullOrWhiteSpace(body.PersonalityDescription))
    {
        character.SetPersonality(
            new CharacterPersonality(body.PersonalityDescription.Trim()));
    }

    if (body.Appearance is not null ||
        body.Hair is not null ||
        body.TypicalClothing is not null ||
        body.VisualStyle is not null)
    {
        character.SetVisualProfile(new CharacterVisualProfile(
            body.Appearance ?? string.Empty,
            body.Hair ?? string.Empty,
            body.TypicalClothing ?? string.Empty,
            body.VisualStyle ?? string.Empty));
    }

    if (body.References is not null)
    {
        foreach (var item in body.References)
        {
            if (string.IsNullOrWhiteSpace(item.StorageLocation))
                return Results.BadRequest(new
                {
                    error = "Every reference requires a storage location."
                });

            if (!Enum.TryParse<CharacterReferenceType>(
                    item.Type, true, out var referenceType))
                return Results.BadRequest(new
                {
                    error = $"Unknown reference type: {item.Type}"
                });

            character.AddReference(new CharacterReference(
                referenceType,
                item.StorageLocation.Trim(),
                item.Description));
        }
    }

    await repository.AddAsync(character, cancellationToken);

    return Results.Created(
        $"/api/characters/{character.Id}",
        ToCharacterResponse(character));
})
.WithName("CreateCharacter");
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapPost("/api/dev/shot-test", async (
        ShotTestRequest body,
        IShotGenerator shotGenerator,
        CancellationToken cancellationToken) =>
    {
        var promptRequest = new CinematicPromptRequest(
            location: body.Location,
            action: body.Action,
            emotion: body.Emotion ?? string.Empty,
            camera: body.Camera ?? string.Empty,
            lighting: body.Lighting ?? string.Empty,
            visualStyle: body.VisualStyle
                ?? "Photorealistic cinematic film still",
            characters: body.Characters);

        var shotRequest = new ShotGenerationRequest(Guid.NewGuid(), promptRequest);

        var result = await shotGenerator.GenerateAsync(
            shotRequest,
            cancellationToken);

        return result.Succeeded
            ? Results.Ok(new
            {
                result.Succeeded,
                result.Prompt,
                ImageId = result.ImageResult.Id,
                result.ImageResult.OutputLocation
            })
            : Results.Problem(
                detail: result.ImageResult.ErrorMessage,
                statusCode: StatusCodes.Status502BadGateway);
    })
    .WithName("GenerateCinematicShot");
}

app.UseHttpsRedirection();

app.Run();

static CharacterResponseDto ToCharacterResponse(Character character) =>
    new(
        character.Id,
        character.Name,
        character.Gender.ToString(),
        character.Personality?.Description,
        character.VisualProfile is null ? null :
            new CharacterVisualProfileDto(
                character.VisualProfile.Appearance,
                character.VisualProfile.Hair,
                character.VisualProfile.TypicalClothing,
                character.VisualProfile.VisualStyle),
        character.References.Select(reference =>
            new CharacterReferenceDto(
                reference.Id,
                reference.Type.ToString(),
                reference.StorageLocation,
                reference.Description)).ToArray());
public sealed record ShotTestRequest(
    string Location,
    string Action,
    string? Emotion = null,
    string? Camera = null,
    string? Lighting = null,
    string? VisualStyle = null,
    string[]? Characters = null);

public sealed record CreateEpisodeRequest(string? Title);
public sealed record CreateShotRequest(double DurationSeconds, string? Action, string? Emotion = null, string? Camera = null, string? Lighting = null);
record CreateSceneRequest(string? Name, string? Location);

public sealed record CreateStoryRequest(
    string? Title,
    string? Description = null,
    string? Country = "Philippines",
    string? Region = "Metro Manila",
    string? Era = "Present Day",
    string? Language = "Taglish",
    string? Genre = "Family Drama",
    string? Tone = "Emotional",
    string? CulturalFlavor = "Filipino");


public sealed record UpdateCharacterRequest(
    string? Name,
    string? Gender = null,
    string? PersonalityDescription = null,
    string? Appearance = null,
    string? Hair = null,
    string? TypicalClothing = null,
    string? VisualStyle = null);
public sealed record CreateCharacterRequest(
    string? Name,
    string? Gender = null,
    string? PersonalityDescription = null,
    string? Appearance = null,
    string? Hair = null,
    string? TypicalClothing = null,
    string? VisualStyle = null,
    List<CreateCharacterReferenceRequest>? References = null);

public sealed record CreateCharacterReferenceRequest(
    string? Type,
    string? StorageLocation,
    string? Description = null);

public sealed record CharacterResponseDto(
    Guid Id,
    string Name,
    string Gender,
    string? PersonalityDescription,
    CharacterVisualProfileDto? VisualProfile,
    IReadOnlyList<CharacterReferenceDto> References);

public sealed record CharacterVisualProfileDto(
    string Appearance,
    string Hair,
    string TypicalClothing,
    string VisualStyle);

public sealed record CharacterReferenceDto(
    Guid Id,
    string Type,
    string StorageLocation,
    string? Description);
