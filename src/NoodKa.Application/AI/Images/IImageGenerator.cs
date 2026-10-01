namespace NoodKa.Application.AI.Images;

public interface IImageGenerator
{
    Task<ImageGenerationResult> GenerateAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);
}