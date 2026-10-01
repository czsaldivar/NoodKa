namespace NoodKa.Application.AI.Video;

public sealed class VideoGenerationRequest
{
    public string Prompt { get; }

    public string ImageLocation { get; }

    public TimeSpan Duration { get; }

    public int Width { get; }

    public int Height { get; }

    public VideoGenerationRequest(
        string prompt,
        string imageLocation,
        TimeSpan duration,
        int width = 1080,
        int height = 1920)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException(
                "Prompt is required.",
                nameof(prompt));

        if (string.IsNullOrWhiteSpace(imageLocation))
            throw new ArgumentException(
                "Image location is required.",
                nameof(imageLocation));

        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(
                nameof(duration));

        Width = width;
        Height = height;
        Prompt = prompt;
        ImageLocation = imageLocation;
        Duration = duration;
    }
}