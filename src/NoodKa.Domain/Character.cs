namespace NoodKa.Domain.Characters;

public sealed class Character
{
    private readonly List<CharacterReference> _references = [];

    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; private set; }

    public CharacterGender Gender { get; }

    public CharacterPersonality? Personality { get; private set; }

    public CharacterVisualProfile? VisualProfile { get; private set; }

    public IReadOnlyCollection<CharacterReference> References =>
        _references;

    public Character(
        string name,
        CharacterGender gender = CharacterGender.Unspecified)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Character name is required.",
                nameof(name));

        Name = name;
        Gender = gender;
    }

    public void SetPersonality(CharacterPersonality personality)
    {
        ArgumentNullException.ThrowIfNull(personality);

        Personality = personality;
    }

    public void SetVisualProfile(CharacterVisualProfile visualProfile)
    {
        ArgumentNullException.ThrowIfNull(visualProfile);

        VisualProfile = visualProfile;
    }

    public void AddReference(CharacterReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);

        _references.Add(reference);
    }
}