namespace NoodKa.Domain.Stories;

public sealed class Episode
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public int Number { get; }

    public string Title { get; }

    private readonly List<Scene> _scenes = [];

    public IReadOnlyCollection<Scene> Scenes => _scenes;

    public Episode(int number, string title)
    {
        if (number <= 0)
            throw new ArgumentOutOfRangeException(nameof(number));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Episode title is required.", nameof(title));

        Number = number;
        Title = title;
    }

    public void AddScene(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        _scenes.Add(scene);
    }
}