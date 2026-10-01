namespace NoodKa.Application.AI.Images;

public sealed class ImageGenerationResult
{
    public Guid Id { get; }

    public bool Succeeded { get; }

    public string? OutputLocation { get; }

    public string? StorageKey { get; }

    public string? ErrorMessage { get; }

    private ImageGenerationResult(
        Guid id,
        bool succeeded,
        string? outputLocation,
        string? storageKey,
        string? errorMessage)
    {
        Id = id;
        Succeeded = succeeded;
        OutputLocation = outputLocation;
        StorageKey = storageKey;
        ErrorMessage = errorMessage;
    }

    public static ImageGenerationResult Success(
        string outputLocation,
        string? storageKey = null)
    {
        if (string.IsNullOrWhiteSpace(outputLocation))
            throw new ArgumentException(
                "Output location is required.",
                nameof(outputLocation));

        if (storageKey is not null &&
            string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException(
                "Storage key cannot be blank.",
                nameof(storageKey));

        return new ImageGenerationResult(
            Guid.NewGuid(),
            true,
            outputLocation,
            storageKey,
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
            null,
            errorMessage);
    }
}
