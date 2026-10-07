using Microsoft.Data.Sqlite;
using NoodKa.Application.Assets;
using NoodKa.Infrastructure.Assets;

namespace NoodKa.Infrastructure.Tests;

public sealed class SqliteAssetCatalogTests
{
    [Fact]
    public async Task AssetMetadata_PersistsAcrossCatalogInstances()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "assets.db");
        var asset = CreateAsset();

        try
        {
            var firstCatalog = new SqliteAssetCatalog(databasePath);
            await firstCatalog.RegisterAsync(asset);

            var secondCatalog = new SqliteAssetCatalog(databasePath);
            var retrieved = await secondCatalog.GetByIdAsync(asset.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(asset.Id, retrieved.Id);
            Assert.Equal(asset.OwnerType, retrieved.OwnerType);
            Assert.Equal(asset.OwnerId, retrieved.OwnerId);
            Assert.Equal(asset.Type, retrieved.Type);
            Assert.Equal(asset.StorageKey, retrieved.StorageKey);
            Assert.Equal(asset.ContentType, retrieved.ContentType);
            Assert.Equal(
                asset.CreatedAtUtc.ToUniversalTime(),
                retrieved.CreatedAtUtc.ToUniversalTime());
        }
        finally
        {
            SqliteCleanup(directory);
        }
    }

    [Fact]
    public async Task RegisterAsync_DuplicateId_ThrowsInvalidOperationException()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "assets.db");

        try
        {
            var catalog = new SqliteAssetCatalog(databasePath);
            var asset = CreateAsset();

            await catalog.RegisterAsync(asset);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => catalog.RegisterAsync(asset));
        }
        finally
        {
            SqliteCleanup(directory);
        }
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "assets.db");

        try
        {
            var catalog = new SqliteAssetCatalog(databasePath);

            Assert.Null(await catalog.GetByIdAsync(Guid.NewGuid()));
        }
        finally
        {
            SqliteCleanup(directory);
        }
    }

    [Fact]
    public async Task GetByOwnerAsync_FiltersAndOrdersAssets()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "assets.db");
        var ownerId = Guid.NewGuid();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();

        try
        {
            var catalog = new SqliteAssetCatalog(databasePath);

            var later = CreateAsset(
                id: secondId,
                ownerId: ownerId,
                createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(2));

            var earlier = CreateAsset(
                id: firstId,
                ownerId: ownerId,
                createdAtUtc: DateTimeOffset.UtcNow.AddMinutes(1));

            var otherOwner = CreateAsset(ownerId: Guid.NewGuid());

            await catalog.RegisterAsync(later);
            await catalog.RegisterAsync(otherOwner);
            await catalog.RegisterAsync(earlier);

            var results = await catalog.GetByOwnerAsync(
                AssetOwnerType.Shot,
                ownerId);

            Assert.Equal(2, results.Count);
            Assert.Equal(firstId, results[0].Id);
            Assert.Equal(secondId, results[1].Id);
        }
        finally
        {
            SqliteCleanup(directory);
        }
    }

    [Fact]
    public async Task Methods_CanceledToken_ThrowOperationCanceledException()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "assets.db");

        try
        {
            var catalog = new SqliteAssetCatalog(databasePath);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => catalog.RegisterAsync(CreateAsset(), cancellation.Token));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => catalog.GetByIdAsync(Guid.NewGuid(), cancellation.Token));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => catalog.GetByOwnerAsync(
                    AssetOwnerType.Shot,
                    Guid.NewGuid(),
                    cancellation.Token));
        }
        finally
        {
            SqliteCleanup(directory);
        }
    }

    private static AssetDescriptor CreateAsset(
        Guid? id = null,
        Guid? ownerId = null,
        DateTimeOffset? createdAtUtc = null)
    {
        return new AssetDescriptor(
            id ?? Guid.NewGuid(),
            AssetOwnerType.Shot,
            ownerId ?? Guid.NewGuid(),
            AssetType.Image,
            $"generated-images/{Guid.NewGuid():N}.png",
            "image/png",
            createdAtUtc ?? DateTimeOffset.UtcNow);
    }

    private static string CreateTestDirectory()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "NoodKa-SqliteTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void SqliteCleanup(string directory)
    {
        // Clear pooled connections before removing the temporary database.
        SqliteConnection.ClearAllPools();

        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
