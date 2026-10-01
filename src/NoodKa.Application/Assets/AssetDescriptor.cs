namespace NoodKa.Application.Assets;

public sealed record AssetDescriptor
{
    public Guid Id { get; init; }
    public AssetOwnerType OwnerType { get; init; }
    public Guid OwnerId { get; init; }
    public AssetType Type { get; init; }
    public string StorageKey { get; init; }
    public string ContentType { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }

    public AssetDescriptor(
        Guid id,
        AssetOwnerType ownerType,
        Guid ownerId,
        AssetType type,
        string storageKey,
        string contentType,
        DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Asset ID is required.", nameof(id));

        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner ID is required.", nameof(ownerId));

        if (string.IsNullOrWhiteSpace(storageKey))
            throw new ArgumentException("Storage key is required.", nameof(storageKey));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type is required.", nameof(contentType));

        if (createdAtUtc == default)
            throw new ArgumentException("Creation timestamp is required.", nameof(createdAtUtc));

        Id = id;
        OwnerType = ownerType;
        OwnerId = ownerId;
        Type = type;
        StorageKey = storageKey;
        ContentType = contentType;
        CreatedAtUtc = createdAtUtc;
    }
}
