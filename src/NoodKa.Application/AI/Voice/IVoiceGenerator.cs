namespace NoodKa.Application.AI.Voice;

public interface IVoiceGenerator
{
    Task<VoiceGenerationResult> GenerateAsync(
        VoiceGenerationRequest request,
        CancellationToken cancellationToken = default);
}