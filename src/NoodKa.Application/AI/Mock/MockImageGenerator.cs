using NoodKa.Application.AI.Images;

namespace NoodKa.Application.AI.Mock;

public sealed class MockImageGenerator : IImageGenerator
{
    public ImageGenerationRequest? LastRequest { get; private set; }

    public bool ShouldFail { get; set; }

    public Task<ImageGenerationResult> GenerateAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        LastRequest = request;

        if (ShouldFail)
        {
            return Task.FromResult(
                ImageGenerationResult.Failure(
                    "Mock image generation failed."));
        }

        return Task.FromResult(
            ImageGenerationResult.Success(
                "mock://generated/image-001.png"));
    }
}