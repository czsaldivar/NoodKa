namespace NoodKa.Application.Prompts;

public interface ICinematicPromptBuilder
{
    CinematicPromptResult Build(
        CinematicPromptRequest request);
}