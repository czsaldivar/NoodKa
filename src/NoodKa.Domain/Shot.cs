namespace NoodKa.Domain.Stories;

public sealed class Shot
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public int Sequence { get; }

    public TimeSpan Duration { get; }

    public string Action { get; }

    public string Emotion { get; }

    public string Camera { get; }

    public string Lighting { get; }

    public Shot(
        int sequence,
        TimeSpan duration,
        string action,
        string emotion,
        string camera,
        string lighting)
    {
        if (sequence <= 0)
            throw new ArgumentOutOfRangeException(nameof(sequence));

        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration));

        Sequence = sequence;
        Duration = duration;
        Action = action ?? string.Empty;
        Emotion = emotion ?? string.Empty;
        Camera = camera ?? string.Empty;
        Lighting = lighting ?? string.Empty;
    }
}