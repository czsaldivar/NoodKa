using NoodKa.Application.Prompts;

namespace NoodKa.Application.Shots;

public sealed class ShotGenerationRequest
{
    public Guid ShotId { get; }

    public CinematicPromptRequest PromptRequest { get; }

    public string? ReferenceImageLocation { get; }

    public byte[]? ReferenceImageBytes { get; }

    public string? ReferenceImageFileName { get; }

    public ShotGenerationRequest(
        Guid shotId,
        CinematicPromptRequest promptRequest,
        string? referenceImageLocation = null,
        byte[]? referenceImageBytes = null,
        string? referenceImageFileName = null)
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

        ShotId = shotId;
        PromptRequest = promptRequest;
        ReferenceImageLocation = referenceImageLocation;
        ReferenceImageBytes = referenceImageBytes;
        ReferenceImageFileName = referenceImageFileName;
    }
}
