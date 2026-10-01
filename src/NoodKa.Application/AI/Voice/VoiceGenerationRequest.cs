namespace NoodKa.Application.AI.Voice;

public sealed class VoiceGenerationRequest
{
    public string Text { get; }

    public string VoiceId { get; }

    public string? Emotion { get; }

    public VoiceGenerationRequest(
        string text,
        string voiceId,
        string? emotion = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException(
                "Text is required.",
                nameof(text));

        if (string.IsNullOrWhiteSpace(voiceId))
            throw new ArgumentException(
                "Voice ID is required.",
                nameof(voiceId));

        Text = text;
        VoiceId = voiceId;
        Emotion = emotion;
    }
}