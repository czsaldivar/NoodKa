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

    public IReadOnlyList<Guid> CharacterIds { get; private set; } = Array.Empty<Guid>();

    public void SetCharacterIds(IEnumerable<Guid> characterIds)
    {
        ArgumentNullException.ThrowIfNull(characterIds);
        var ids = characterIds.ToArray();

        if (ids.Any(id => id == Guid.Empty))
            throw new ArgumentException("Character IDs cannot contain an empty ID.", nameof(characterIds));

        if (ids.Distinct().Count() != ids.Length)
            throw new ArgumentException("Character IDs cannot contain duplicates.", nameof(characterIds));

        CharacterIds = Array.AsReadOnly(ids);
    }
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