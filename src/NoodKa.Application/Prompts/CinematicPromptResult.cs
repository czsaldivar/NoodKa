namespace NoodKa.Application.Prompts;

public sealed class CinematicPromptResult
{
    public string Prompt { get; }

    public CinematicPromptResult(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException(
                "Prompt is required.",
                nameof(prompt));

        Prompt = prompt;
    }
}