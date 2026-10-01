namespace NoodKa.Domain.Stories;

public sealed class Story
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Title { get; private set; }

    public string Description { get; private set; }

    private readonly List<Episode> _episodes = [];

    public IReadOnlyCollection<Episode> Episodes => _episodes;

    public Story(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Story title is required.", nameof(title));

        Title = title;
        Description = description ?? string.Empty;
    }

    public void AddEpisode(Episode episode)
    {
        ArgumentNullException.ThrowIfNull(episode);
        _episodes.Add(episode);
    }
}