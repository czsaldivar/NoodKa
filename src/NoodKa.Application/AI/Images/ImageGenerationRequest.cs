namespace NoodKa.Application.AI.Images;

public sealed class ImageGenerationRequest
{
    public string Prompt { get; }

    public string? ReferenceImageLocation { get; }

    public byte[]? ReferenceImageBytes { get; }

    public string? ReferenceImageFileName { get; }

    public int Width { get; }

    public int Height { get; }

    public ImageGenerationRequest(
        string prompt,
        string? referenceImageLocation = null,
        int width = 1024,
        int height = 1024,
        byte[]? referenceImageBytes = null,
        string? referenceImageFileName = null)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException(
                "Prompt is required.",
                nameof(prompt));

        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        if (referenceImageBytes is { Length: 0 })
            throw new ArgumentException(
                "Reference image bytes cannot be empty.",
                nameof(referenceImageBytes));

        if ((referenceImageBytes is null) !=
            string.IsNullOrWhiteSpace(referenceImageFileName))
            throw new ArgumentException(
                "Reference image bytes and filename must be provided together.");

        Prompt = prompt;
        ReferenceImageLocation = referenceImageLocation;
        ReferenceImageBytes = referenceImageBytes;
        ReferenceImageFileName = referenceImageFileName;
        Width = width;
        Height = height;
    }
}
