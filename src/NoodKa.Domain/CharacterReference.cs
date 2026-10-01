namespace NoodKa.Domain.Characters;

public sealed class CharacterReference
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public CharacterReferenceType Type { get; }

    public string StorageLocation { get; }

    public string? Description { get; }

    public CharacterReference(
        CharacterReferenceType type,
        string storageLocation,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(storageLocation))
            throw new ArgumentException(
                "Storage location is required.",
                nameof(storageLocation));

        Type = type;
        StorageLocation = storageLocation;
        Description = description;
    }
}