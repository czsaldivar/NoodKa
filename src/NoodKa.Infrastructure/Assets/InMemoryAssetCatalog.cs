using System.Collections.Concurrent;
using NoodKa.Application.Assets;

namespace NoodKa.Infrastructure.Assets;

public sealed class InMemoryAssetCatalog : IAssetCatalog
{
    private readonly ConcurrentDictionary<Guid, AssetDescriptor> _assets = new();

    public Task RegisterAsync(
        AssetDescriptor asset,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(asset);

        if (!_assets.TryAdd(asset.Id, asset))
        {
            throw new InvalidOperationException(
                $"An asset with ID '{asset.Id}' is already registered.");
        }

        return Task.CompletedTask;
    }

    public Task<AssetDescriptor?> GetByIdAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _assets.TryGetValue(assetId, out var asset);

        return Task.FromResult(asset);
    }

    public Task<IReadOnlyList<AssetDescriptor>> GetByOwnerAsync(
        AssetOwnerType ownerType,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<AssetDescriptor> assets = _assets.Values
            .Where(asset =>
                asset.OwnerType == ownerType &&
                asset.OwnerId == ownerId)
            .OrderBy(asset => asset.CreatedAtUtc)
            .ThenBy(asset => asset.Id)
            .ToArray();

        return Task.FromResult(assets);
    }

    public Task<IReadOnlyList<AssetDescriptor>> GetRecentAsync(
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

        IReadOnlyList<AssetDescriptor> assets = _assets.Values
            .OrderByDescending(asset => asset.CreatedAtUtc)
            .ThenByDescending(asset => asset.Id)
            .Take(take)
            .ToArray();

        return Task.FromResult(assets);
    }


}
