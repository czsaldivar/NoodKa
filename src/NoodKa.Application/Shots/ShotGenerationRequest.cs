using NoodKa.Application.Prompts;

namespace NoodKa.Application.Shots;

public sealed class ShotGenerationRequest
{
    public Guid ShotId { get; }

    public CinematicPromptRequest PromptRequest { get; }

    public string? ReferenceImageLocation { get; }

    public ShotGenerationRequest(
        Guid shotId,
        CinematicPromptRequest promptRequest,
        string? referenceImageLocation = null)
    {
        if (shotId == Guid.Empty)
            throw new ArgumentException("Shot ID cannot be empty.", nameof(shotId));

        ArgumentNullException.ThrowIfNull(promptRequest);

        ShotId = shotId;
        PromptRequest = promptRequest;
        ReferenceImageLocation = referenceImageLocation;
    }
}
