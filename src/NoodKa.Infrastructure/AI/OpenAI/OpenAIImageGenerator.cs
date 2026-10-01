using Microsoft.Extensions.Configuration;
using NoodKa.Application.AI.Images;
using NoodKa.Application.Assets;
using OpenAI.Images;

namespace NoodKa.Infrastructure.AI.OpenAI;

public sealed class OpenAIImageGenerator : IImageGenerator
{
    private readonly ImageClient _client;
    private readonly IAssetStorage _assetStorage;

    public OpenAIImageGenerator(
        IConfiguration configuration,
        IAssetStorage assetStorage)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(assetStorage);

        var apiKey = configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is not configured.");
        }

        _client = new ImageClient(
            model: "gpt-image-2",
            apiKey: apiKey);

        _assetStorage = assetStorage;
    }

    public async Task<ImageGenerationResult> GenerateAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            if (request.Width != 1024 || request.Height != 1024)
            {
                return ImageGenerationResult.Failure(
                    $"Round 7 currently supports only 1024x1024 images. " +
                    $"Requested: {request.Width}x{request.Height}");
            }

            var imageOptions = new ImageGenerationOptions
            {
                Size = GeneratedImageSize.W1024xH1024
            };

            var result = await _client.GenerateImageAsync(
                request.Prompt,
                imageOptions,
                cancellationToken);

            var image = result.Value;

            if (image.ImageBytes is null)
            {
                return ImageGenerationResult.Failure(
                    "OpenAI returned no image data.");
            }

            var relativePath =
                $"generated-images/{Guid.NewGuid():N}.png";

            using var imageStream = image.ImageBytes.ToStream();

            var outputPath = await _assetStorage.SaveAsync(
                relativePath,
                imageStream,
                cancellationToken);

            return ImageGenerationResult.Success(outputPath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ImageGenerationResult.Failure(
                $"Image generation or storage failed: {ex.Message}");
        }
    }
}
