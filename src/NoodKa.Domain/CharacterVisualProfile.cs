namespace NoodKa.Domain.Characters;

public sealed class CharacterVisualProfile
{
    public string Appearance { get; private set; }

    public string Hair { get; private set; }

    public string TypicalClothing { get; private set; }

    public string VisualStyle { get; private set; }

    public CharacterVisualProfile(
        string appearance,
        string hair,
        string typicalClothing,
        string visualStyle)
    {
        Appearance = appearance ?? string.Empty;
        Hair = hair ?? string.Empty;
        TypicalClothing = typicalClothing ?? string.Empty;
        VisualStyle = visualStyle ?? string.Empty;
    }
}