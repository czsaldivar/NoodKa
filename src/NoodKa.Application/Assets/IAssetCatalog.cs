namespace NoodKa.Application.Assets;

public interface IAssetCatalog
{
    Task RegisterAsync(
        AssetDescriptor asset,
        CancellationToken cancellationToken = default);

    Task<AssetDescriptor?> GetByIdAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssetDescriptor>> GetByOwnerAsync(
        AssetOwnerType ownerType,
        Guid ownerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssetDescriptor>> GetRecentAsync(
        int take = 50,
        CancellationToken cancellationToken = default);
}
