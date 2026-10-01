namespace NoodKa.Domain.Stories;

public sealed class Scene
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public int Number { get; }

    public string Location { get; }

    private readonly List<Shot> _shots = [];

    public IReadOnlyCollection<Shot> Shots => _shots;

    public Scene(int number, string location)
    {
        if (number <= 0)
            throw new ArgumentOutOfRangeException(nameof(number));

        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Scene location is required.", nameof(location));

        Number = number;
        Location = location;
    }

    public void AddShot(Shot shot)
    {
        ArgumentNullException.ThrowIfNull(shot);
        _shots.Add(shot);
    }
}