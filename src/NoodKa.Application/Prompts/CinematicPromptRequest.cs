namespace NoodKa.Application.Prompts;

public sealed class CinematicPromptRequest
{
    public string Location { get; }

    public string Action { get; }

    public string Emotion { get; }

    public string Camera { get; }

    public string Lighting { get; }

    public string VisualStyle { get; }

    public IReadOnlyList<string> Characters { get; }

    public string CharacterVisualDescription { get; }

    public CinematicPromptRequest(
        string location,
        string action,
        string emotion,
        string camera,
        string lighting,
        string visualStyle,
        IEnumerable<string>? characters = null,
        string? characterVisualDescription = null)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException(
                "Location is required.",
                nameof(location));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException(
                "Action is required.",
                nameof(action));

        Location = location;
        Action = action;
        Emotion = emotion ?? string.Empty;
        Camera = camera ?? string.Empty;
        Lighting = lighting ?? string.Empty;
        VisualStyle = visualStyle ?? string.Empty;

        Characters =
            characters?.ToArray()
            ?? [];

        CharacterVisualDescription =
            characterVisualDescription?.Trim() ?? string.Empty;
    }
}