namespace NoodKa.Application.AI.Video;

public sealed class VideoGenerationResult
{
    public Guid Id { get; }

    public bool Succeeded { get; }

    public string? OutputLocation { get; }

    public string? ErrorMessage { get; }

    private VideoGenerationResult(
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

    public static VideoGenerationResult Success(
        string outputLocation)
    {
        if (string.IsNullOrWhiteSpace(outputLocation))
            throw new ArgumentException(
                "Output location is required.",
                nameof(outputLocation));

        return new VideoGenerationResult(
            Guid.NewGuid(),
            true,
            outputLocation,
            null);
    }

    public static VideoGenerationResult Failure(
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException(
                "Error message is required.",
                nameof(errorMessage));

        return new VideoGenerationResult(
            Guid.NewGuid(),
            false,
            null,
            errorMessage);
    }
}