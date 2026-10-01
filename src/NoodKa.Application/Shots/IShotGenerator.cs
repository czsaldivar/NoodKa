namespace NoodKa.Application.Shots;

public interface IShotGenerator
{
    Task<ShotGenerationResult> GenerateAsync(
        ShotGenerationRequest request,
        CancellationToken cancellationToken = default);
}