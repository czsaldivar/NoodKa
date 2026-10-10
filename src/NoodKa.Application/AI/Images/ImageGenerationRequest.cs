namespace NoodKa.Application.AI.Images;

public sealed class ImageReferenceInput
{
    public string? Location { get; }
    public byte[] Bytes { get; }
    public string FileName { get; }

    public ImageReferenceInput(string? location, byte[] bytes, string fileName)
    {
        if (bytes is null || bytes.Length == 0)
            throw new ArgumentException("Reference image bytes cannot be empty.", nameof(bytes));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Reference image filename is required.", nameof(fileName));

        Location = location;
        Bytes = bytes;
        FileName = fileName;
    }
}

public sealed class ImageGenerationRequest
{
    public string Prompt { get; }

    public string? ReferenceImageLocation { get; }

    public byte[]? ReferenceImageBytes { get; }

    public string? ReferenceImageFileName { get; }

    public IReadOnlyList<ImageReferenceInput> ReferenceImages { get; }

    public int Width { get; }

    public int Height { get; }

    public ImageGenerationRequest(
        string prompt,
        string? referenceImageLocation = null,
        int width = 1024,
        int height = 1024,
        byte[]? referenceImageBytes = null,
        string? referenceImageFileName = null,
        IReadOnlyList<ImageReferenceInput>? referenceImages = null)
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

        if (referenceImages is not null && referenceImageBytes is not null)
            throw new ArgumentException("Use either referenceImages or the legacy single-image arguments, not both.");

        ReferenceImages = referenceImages is not null
            ? referenceImages.ToArray()
            : referenceImageBytes is not null
                ? new[]
                {
                    new ImageReferenceInput(
                        referenceImageLocation,
                        referenceImageBytes,
                        referenceImageFileName!)
                }
                : Array.Empty<ImageReferenceInput>();

        if (ReferenceImages.Any(image => image is null))
            throw new ArgumentException("Reference image collection cannot contain null entries.", nameof(referenceImages));

        Width = width;
        Height = height;
    }
}
