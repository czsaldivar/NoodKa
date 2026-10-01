namespace NoodKa.Application.AI.Images;

public sealed class ImageGenerationResult
{
    public Guid Id { get; }

    public bool Succeeded { get; }

    public string? OutputLocation { get; }

    public string? ErrorMessage { get; }

    private ImageGenerationResult(
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

    public static ImageGenerationResult Success(
        string outputLocation)
    {
        if (string.IsNullOrWhiteSpace(outputLocation))
            throw new ArgumentException(
                "Output location is required.",
                nameof(outputLocation));

        return new ImageGenerationResult(
            Guid.NewGuid(),
            true,
            outputLocation,
            null);
    }

    public static ImageGenerationResult Failure(
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException(
                "Error message is required.",
                nameof(errorMessage));

        return new ImageGenerationResult(
            Guid.NewGuid(),
            false,
            null,
            errorMessage);
    }
}