namespace NoodKa.Application.AI.Video;

public interface IVideoGenerator
{
    Task<VideoGenerationResult> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default);
}