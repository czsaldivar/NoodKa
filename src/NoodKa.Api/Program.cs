using NoodKa.Application.AI.Images;
using NoodKa.Application.Assets;
using NoodKa.Application.Prompts;
using NoodKa.Application.Shots;
using NoodKa.Infrastructure.AI.OpenAI;
using NoodKa.Infrastructure.Assets;

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

// AI image generation
builder.Services.AddScoped<IImageGenerator, OpenAIImageGenerator>();

// Cinematic shot generation pipeline
builder.Services.AddScoped<
    ICinematicPromptBuilder,
    CinematicPromptBuilder>();

builder.Services.AddScoped<IShotGenerator, ShotGenerator>();

var app = builder.Build();

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

        var shotRequest = new ShotGenerationRequest(
            promptRequest);

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

public sealed record ShotTestRequest(
    string Location,
    string Action,
    string? Emotion = null,
    string? Camera = null,
    string? Lighting = null,
    string? VisualStyle = null,
    string[]? Characters = null);
