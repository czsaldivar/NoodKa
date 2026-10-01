using System.Text;

namespace NoodKa.Application.Prompts;

public sealed class CinematicPromptBuilder
    : ICinematicPromptBuilder
{
    public CinematicPromptResult Build(
        CinematicPromptRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var prompt = new StringBuilder();

        prompt.Append("Cinematic realistic shot ");

        if (request.Characters.Count > 0)
        {
            prompt.Append("featuring ");

            prompt.Append(
                string.Join(
                    " and ",
                    request.Characters));

            prompt.Append(". ");
        }

        prompt.Append(
            $"The scene takes place in {request.Location}. ");

        prompt.Append(
            request.Action.TrimEnd('.') + ". ");

        if (!string.IsNullOrWhiteSpace(request.Emotion))
        {
            prompt.Append(
                $"The character's emotion is {request.Emotion}. ");
        }

        if (!string.IsNullOrWhiteSpace(request.Camera))
        {
            prompt.Append(
                $"Camera: {request.Camera}. ");
        }

        if (!string.IsNullOrWhiteSpace(request.Lighting))
        {
            prompt.Append(
                $"Lighting: {request.Lighting}. ");
        }

        if (!string.IsNullOrWhiteSpace(request.VisualStyle))
        {
            prompt.Append(
                $"Visual style: {request.VisualStyle}. ");
        }

        prompt.Append(
            "Natural human movement, realistic facial expressions, "
            + "cinematic composition and high visual consistency.");

        return new CinematicPromptResult(
            prompt.ToString());
    }
}