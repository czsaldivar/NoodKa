using NoodKa.Application.AI.Images;
using NoodKa.Application.AI.Mock;
using NoodKa.Application.Assets;
using NoodKa.Application.Prompts;
using NoodKa.Application.Shots;

namespace NoodKa.Application.Tests.Shots;

public sealed class ShotGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_BuildsPromptAndGeneratesImage()
    {
        var promptBuilder = new CinematicPromptBuilder();
        var imageGenerator = new MockImageGenerator();
        var catalog = new TestAssetCatalog();

        var shotGenerator = new ShotGenerator(
            promptBuilder,
            imageGenerator,
            catalog);

        var promptRequest = new CinematicPromptRequest(
            location: "a warm Filipino home kitchen",
            action: "Jowa-an walks into the kitchen and notices something missing",
            emotion: "shocked and concerned",
            camera: "medium shot with a subtle push-in",
            lighting: "warm morning light",
            visualStyle: "realistic cinematic Filipino drama",
            characters: ["Jowa-an"]);

        var shotId = Guid.NewGuid();
        var request = new ShotGenerationRequest(
            shotId,
            promptRequest,
            "characters/jowa-an/face-01.jpg");

        var result = await shotGenerator.GenerateAsync(request);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.ImageResult);
        Assert.True(result.ImageResult.Succeeded);
        Assert.Equal(
            "mock://generated/image-001.png",
            result.ImageResult.OutputLocation);
        Assert.Contains("Jowa-an", result.Prompt);
        Assert.Contains("warm Filipino home kitchen", result.Prompt);
        Assert.NotNull(imageGenerator.LastRequest);
        Assert.Equal(result.Prompt, imageGenerator.LastRequest!.Prompt);
        Assert.Equal(
            "characters/jowa-an/face-01.jpg",
            imageGenerator.LastRequest.ReferenceImageLocation);

        // Mock results have no storage key, so nothing is registered.
        Assert.Empty(catalog.Assets);
    }

    [Fact]
    public async Task GenerateAsync_RegistersGeneratedImageAgainstShot()
    {
        var shotId = Guid.NewGuid();
        var imageResult = ImageGenerationResult.Success(
            @"C:\NoodKa\assets\generated-images\image-001.png",
            "generated-images/image-001.png");

        var catalog = new TestAssetCatalog();
        var imageGenerator = new ConfigurableImageGenerator(imageResult);
        var shotGenerator = new ShotGenerator(
            new CinematicPromptBuilder(),
            imageGenerator,
            catalog);

        var request = new ShotGenerationRequest(
            shotId,
            new CinematicPromptRequest(
                location: "kitchen",
                action: "Jowa-an opens the cabinet",
                emotion: "curious",
                camera: "medium shot",
                lighting: "warm kitchen lighting",
                visualStyle: "cinematic Filipino drama"));

        var result = await shotGenerator.GenerateAsync(request);

        Assert.True(result.Succeeded);

        var asset = Assert.Single(catalog.Assets);
        Assert.Equal(imageResult.Id, asset.Id);
        Assert.Equal(AssetOwnerType.Shot, asset.OwnerType);
        Assert.Equal(shotId, asset.OwnerId);
        Assert.Equal(AssetType.Image, asset.Type);
        Assert.Equal("generated-images/image-001.png", asset.StorageKey);
        Assert.Equal("image/png", asset.ContentType);
    }

    [Fact]
    public async Task GenerateAsync_DoesNotRegisterImageWhenGenerationFails()
    {
        var catalog = new TestAssetCatalog();
        var shotGenerator = new ShotGenerator(
            new CinematicPromptBuilder(),
            new MockImageGenerator { ShouldFail = true },
            catalog);

        var request = new ShotGenerationRequest(
            Guid.NewGuid(),
            new CinematicPromptRequest(
                location: "kitchen",
                action: "Jowa-an opens the cabinet",
                emotion: "curious",
                camera: "medium shot",
                lighting: "warm kitchen lighting",
                visualStyle: "cinematic Filipino drama"));

        var result = await shotGenerator.GenerateAsync(request);

        Assert.False(result.Succeeded);
        Assert.False(result.ImageResult.Succeeded);
        Assert.Equal(
            "Mock image generation failed.",
            result.ImageResult.ErrorMessage);
        Assert.Empty(catalog.Assets);
    }

    [Fact]
    public async Task GenerateAsync_RequiresRequest()
    {
        var shotGenerator = new ShotGenerator(
            new CinematicPromptBuilder(),
            new MockImageGenerator(),
            new TestAssetCatalog());

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => shotGenerator.GenerateAsync(null!));
    }

    private sealed class TestAssetCatalog : IAssetCatalog
    {
        private readonly List<AssetDescriptor> _assets = [];

        public IReadOnlyList<AssetDescriptor> Assets => _assets;

        public Task RegisterAsync(
            AssetDescriptor asset,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _assets.Add(asset);
            return Task.CompletedTask;
        }

        public Task<AssetDescriptor?> GetByIdAsync(
            Guid assetId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(
                _assets.FirstOrDefault(asset => asset.Id == assetId));
        }

        public Task<IReadOnlyList<AssetDescriptor>> GetByOwnerAsync(
            AssetOwnerType ownerType,
            Guid ownerId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<AssetDescriptor> result = _assets
                .Where(asset =>
                    asset.OwnerType == ownerType &&
                    asset.OwnerId == ownerId)
                .ToArray();

            return Task.FromResult(result);
        }
    }

    private sealed class ConfigurableImageGenerator(
        ImageGenerationResult result) : IImageGenerator
    {
        public Task<ImageGenerationResult> GenerateAsync(
            ImageGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(result);
        }
    }
}
