namespace NoodKa.Domain.Stories;

public sealed class Story
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Title { get; private set; }

    public string Description { get; private set; }

    // Story DNA
    public string Country { get; private set; }
    public string Region { get; private set; }
    public string Era { get; private set; }
    public string Language { get; private set; }
    public string Genre { get; private set; }
    public string Tone { get; private set; }
    public string CulturalFlavor { get; private set; }

    private readonly List<Episode> _episodes = [];

    public IReadOnlyCollection<Episode> Episodes => _episodes;

    public Story(
        string title,
        string description,
        string country = "Philippines",
        string region = "Metro Manila",
        string era = "Present Day",
        string language = "Taglish",
        string genre = "Family Drama",
        string tone = "Emotional",
        string culturalFlavor = "Filipino")
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Story title is required.", nameof(title));

        Title = title;
        Description = description ?? string.Empty;

        Country = string.IsNullOrWhiteSpace(country) ? "Philippines" : country;
        Region = string.IsNullOrWhiteSpace(region) ? "Metro Manila" : region;
        Era = string.IsNullOrWhiteSpace(era) ? "Present Day" : era;
        Language = string.IsNullOrWhiteSpace(language) ? "Taglish" : language;
        Genre = string.IsNullOrWhiteSpace(genre) ? "Family Drama" : genre;
        Tone = string.IsNullOrWhiteSpace(tone) ? "Emotional" : tone;
        CulturalFlavor = string.IsNullOrWhiteSpace(culturalFlavor) ? "Filipino" : culturalFlavor;
    }

    public void AddEpisode(Episode episode)
    {
        ArgumentNullException.ThrowIfNull(episode);
        _episodes.Add(episode);
    }
}