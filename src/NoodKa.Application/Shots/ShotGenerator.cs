using NoodKa.Application.AI.Images;
using NoodKa.Application.Assets;
using NoodKa.Application.Prompts;

namespace NoodKa.Application.Shots;

public sealed class ShotGenerator : IShotGenerator
{
    private readonly ICinematicPromptBuilder _promptBuilder;
    private readonly IImageGenerator _imageGenerator;
    private readonly IAssetCatalog _assetCatalog;

    public ShotGenerator(
        ICinematicPromptBuilder promptBuilder,
        IImageGenerator imageGenerator,
        IAssetCatalog assetCatalog)
    {
        ArgumentNullException.ThrowIfNull(promptBuilder);
        ArgumentNullException.ThrowIfNull(imageGenerator);
        ArgumentNullException.ThrowIfNull(assetCatalog);

        _promptBuilder = promptBuilder;
        _imageGenerator = imageGenerator;
        _assetCatalog = assetCatalog;
    }

    public async Task<ShotGenerationResult> GenerateAsync(
        ShotGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var promptResult = _promptBuilder.Build(request.PromptRequest);

        var imageRequest = new ImageGenerationRequest(
            promptResult.Prompt,
            request.ReferenceImageLocation,
            referenceImageBytes: request.ReferenceImageBytes,
            referenceImageFileName: request.ReferenceImageFileName,
            referenceImages: request.ReferenceImages.Count > 0 &&
                request.ReferenceImageBytes is null
                    ? request.ReferenceImages
                    : null);

        var imageResult = await _imageGenerator.GenerateAsync(
            imageRequest,
            cancellationToken);

        if (!imageResult.Succeeded)
        {
            return ShotGenerationResult.Failure(
                promptResult.Prompt,
                imageResult);
        }

        if (!string.IsNullOrWhiteSpace(imageResult.StorageKey))
        {
            var asset = new AssetDescriptor(
                id: imageResult.Id,
                ownerType: AssetOwnerType.Shot,
                ownerId: request.ShotId,
                type: AssetType.Image,
                storageKey: imageResult.StorageKey,
                contentType: "image/png",
                createdAtUtc: DateTimeOffset.UtcNow);

            await _assetCatalog.RegisterAsync(
                asset,
                cancellationToken);
        }

        return ShotGenerationResult.Success(
            promptResult.Prompt,
            imageResult);
    }
}
