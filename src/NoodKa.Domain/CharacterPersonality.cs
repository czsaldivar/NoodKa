namespace NoodKa.Domain.Characters;

public sealed class CharacterPersonality
{
    public string Description { get; private set; }

    public CharacterPersonality(string description)
    {
        Description = description ?? string.Empty;
    }
}