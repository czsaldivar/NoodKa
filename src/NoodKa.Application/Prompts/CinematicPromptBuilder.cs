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
            prompt.Append(string.Join(" and ", request.Characters));
            prompt.Append(". ");
        }

        if (request.Characters.Count > 0)
        {
            prompt.Append(
                "Identity preservation: use the supplied character reference images "
                + "to preserve each selected character's recognizable facial features, "
                + "hairstyle, and overall appearance. Keep the characters visually "
                + "distinct. Do not merge, swap, or unintentionally duplicate their identities. ");
        }

        if (!string.IsNullOrWhiteSpace(request.CharacterVisualDescription))
        {
            prompt.Append(
                $"Character appearance reference: {request.CharacterVisualDescription.Trim().TrimEnd('.')}. ");
        }

        prompt.Append(
            $"The scene takes place in {request.Location}. ");

        prompt.Append(
            request.Action.TrimEnd('.') + ". ");

        prompt.Append(
            "Composition fidelity: follow the described action and requested placement "
            + "of every character. If a character is specified as appearing on a phone "
            + "screen, photograph, reflection, or other framed surface, keep that character "
            + "within that surface unless the action explicitly says otherwise. ");

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
