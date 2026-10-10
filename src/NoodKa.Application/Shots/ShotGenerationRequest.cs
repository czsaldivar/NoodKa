using NoodKa.Application.Prompts;
using NoodKa.Application.AI.Images;

namespace NoodKa.Application.Shots;

public sealed class ShotGenerationRequest
{
    public Guid ShotId { get; }

    public CinematicPromptRequest PromptRequest { get; }

    public string? ReferenceImageLocation { get; }

    public byte[]? ReferenceImageBytes { get; }

    public string? ReferenceImageFileName { get; }

    public IReadOnlyList<ImageReferenceInput> ReferenceImages { get; }

    public ShotGenerationRequest(
        Guid shotId,
        CinematicPromptRequest promptRequest,
        string? referenceImageLocation = null,
        byte[]? referenceImageBytes = null,
        string? referenceImageFileName = null,
        IReadOnlyList<ImageReferenceInput>? referenceImages = null)
    {
        if (shotId == Guid.Empty)
            throw new ArgumentException(
                "Shot ID cannot be empty.",
                nameof(shotId));

        ArgumentNullException.ThrowIfNull(promptRequest);

        if (referenceImageBytes is { Length: 0 })
            throw new ArgumentException(
                "Reference image bytes cannot be empty.",
                nameof(referenceImageBytes));

        if ((referenceImageBytes is null) !=
            string.IsNullOrWhiteSpace(referenceImageFileName))
            throw new ArgumentException(
                "Reference image bytes and filename must be provided together.");

        if (referenceImages is not null && referenceImageBytes is not null)
            throw new ArgumentException("Use either referenceImages or the legacy single-image arguments, not both.");

        ShotId = shotId;
        PromptRequest = promptRequest;
        ReferenceImageLocation = referenceImageLocation;
        ReferenceImageBytes = referenceImageBytes;
        ReferenceImageFileName = referenceImageFileName;

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
    }
}
