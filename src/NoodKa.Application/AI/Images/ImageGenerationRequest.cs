namespace NoodKa.Application.AI.Images;

public sealed class ImageGenerationRequest
{
    public string Prompt { get; }

    public string? ReferenceImageLocation { get; }

    public int Width { get; }

    public int Height { get; }

    public ImageGenerationRequest(
        string prompt,
        string? referenceImageLocation = null,
        int width = 1024,
        int height = 1024)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException(
                "Prompt is required.",
                nameof(prompt));

        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));

        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        Prompt = prompt;
        ReferenceImageLocation = referenceImageLocation;
        Width = width;
        Height = height;
    }
}