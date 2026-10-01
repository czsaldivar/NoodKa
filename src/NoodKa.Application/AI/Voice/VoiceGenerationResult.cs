namespace NoodKa.Application.AI.Voice;

public sealed class VoiceGenerationResult
{
    public Guid Id { get; }

    public bool Succeeded { get; }

    public string? OutputLocation { get; }

    public string? ErrorMessage { get; }

    private VoiceGenerationResult(
        Guid id,
        bool succeeded,
        string? outputLocation,
        string? errorMessage)
    {
        Id = id;
        Succeeded = succeeded;
        OutputLocation = outputLocation;
        ErrorMessage = errorMessage;
    }

    public static VoiceGenerationResult Success(
        string outputLocation)
    {
        if (string.IsNullOrWhiteSpace(outputLocation))
            throw new ArgumentException(
                "Output location is required.",
                nameof(outputLocation));

        return new VoiceGenerationResult(
            Guid.NewGuid(),
            true,
            outputLocation,
            null);
    }

    public static VoiceGenerationResult Failure(
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException(
                "Error message is required.",
                nameof(errorMessage));

        return new VoiceGenerationResult(
            Guid.NewGuid(),
            false,
            null,
            errorMessage);
    }
}