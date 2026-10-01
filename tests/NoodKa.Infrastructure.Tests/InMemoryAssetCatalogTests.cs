using NoodKa.Application.Assets;
using NoodKa.Infrastructure.Assets;

namespace NoodKa.Infrastructure.Tests;

public sealed class InMemoryAssetCatalogTests
{
    private readonly InMemoryAssetCatalog _catalog = new();

    private static AssetDescriptor CreateAsset(
        Guid? id = null,
        AssetOwnerType ownerType = AssetOwnerType.Shot,
        Guid? ownerId = null,
        string storageKey = "generated-images/test.png",
        DateTimeOffset? createdAtUtc = null)
    {
        return new AssetDescriptor(
            id ?? Guid.NewGuid(),
            ownerType,
            ownerId ?? Guid.NewGuid(),
            AssetType.Image,
            storageKey,
            "image/png",
            createdAtUtc ?? DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_StoresAssetForLookupById()
    {
        var asset = CreateAsset();

        await _catalog.RegisterAsync(asset);

        var result = await _catalog.GetByIdAsync(asset.Id);

        Assert.Equal(asset, result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForUnknownId()
    {
        var result = await _catalog.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_RejectsDuplicateAssetId()
    {
        var id = Guid.NewGuid();

        await _catalog.RegisterAsync(CreateAsset(id: id));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _catalog.RegisterAsync(CreateAsset(id: id)));
    }

    [Fact]
    public async Task GetByOwnerAsync_ReturnsOnlyAssetsForMatchingOwner()
    {
        var ownerId = Guid.NewGuid();
        var first = CreateAsset(
            ownerType: AssetOwnerType.Shot,
            ownerId: ownerId,
            createdAtUtc: DateTimeOffset.Parse("2026-01-01T00:00:00Z"));
        var second = CreateAsset(
            ownerType: AssetOwnerType.Shot,
            ownerId: ownerId,
            createdAtUtc: DateTimeOffset.Parse("2026-01-02T00:00:00Z"));
        var otherOwner = CreateAsset(
            ownerType: AssetOwnerType.Shot,
            ownerId: Guid.NewGuid());
        var otherType = CreateAsset(
            ownerType: AssetOwnerType.Scene,
            ownerId: ownerId);

        await _catalog.RegisterAsync(first);
        await _catalog.RegisterAsync(second);
        await _catalog.RegisterAsync(otherOwner);
        await _catalog.RegisterAsync(otherType);

        var results = await _catalog.GetByOwnerAsync(
            AssetOwnerType.Shot,
            ownerId);

        Assert.Collection(
            results,
            item => Assert.Equal(first.Id, item.Id),
            item => Assert.Equal(second.Id, item.Id));
    }

    [Fact]
    public async Task GetByOwnerAsync_ReturnsEmptyListWhenNoAssetsMatch()
    {
        var results = await _catalog.GetByOwnerAsync(
            AssetOwnerType.Character,
            Guid.NewGuid());

        Assert.Empty(results);
    }

    [Fact]
    public async Task RegisterAsync_ThrowsWhenCancellationRequested()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _catalog.RegisterAsync(
                CreateAsset(),
                cancellation.Token));
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsWhenCancellationRequested()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _catalog.GetByIdAsync(
                Guid.NewGuid(),
                cancellation.Token));
    }

    [Fact]
    public async Task GetByOwnerAsync_ThrowsWhenCancellationRequested()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _catalog.GetByOwnerAsync(
                AssetOwnerType.Shot,
                Guid.NewGuid(),
                cancellation.Token));
    }
}
