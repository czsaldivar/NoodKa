using NoodKa.Application.Prompts;

namespace NoodKa.Application.Shots;

public sealed class ShotGenerationRequest
{
    public CinematicPromptRequest PromptRequest { get; }

    public string? ReferenceImageLocation { get; }

    public ShotGenerationRequest(
        CinematicPromptRequest promptRequest,
        string? referenceImageLocation = null)
    {
        ArgumentNullException.ThrowIfNull(promptRequest);

        PromptRequest = promptRequest;
        ReferenceImageLocation = referenceImageLocation;
    }
}