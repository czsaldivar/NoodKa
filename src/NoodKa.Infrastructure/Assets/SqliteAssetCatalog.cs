using System.Globalization;
using Microsoft.Data.Sqlite;
using NoodKa.Application.Assets;

namespace NoodKa.Infrastructure.Assets;

public sealed class SqliteAssetCatalog : IAssetCatalog
{
    private readonly string _connectionString;

    public SqliteAssetCatalog(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        var fullPath = Path.GetFullPath(databasePath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true
        }.ToString();

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Assets (
                Id TEXT NOT NULL PRIMARY KEY,
                OwnerType TEXT NOT NULL,
                OwnerId TEXT NOT NULL,
                Type TEXT NOT NULL,
                StorageKey TEXT NOT NULL,
                ContentType TEXT NOT NULL,
                CreatedAtUtc TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS IX_Assets_Owner
            ON Assets (OwnerType, OwnerId, CreatedAtUtc, Id);
            """;

        command.ExecuteNonQuery();
    }

    public async Task RegisterAsync(
        AssetDescriptor asset,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(asset);
        cancellationToken.ThrowIfCancellationRequested();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Assets (
                Id, OwnerType, OwnerId, Type,
                StorageKey, ContentType, CreatedAtUtc
            )
            VALUES (
                $id, $ownerType, $ownerId, $type,
                $storageKey, $contentType, $createdAtUtc
            );
            """;

        command.Parameters.AddWithValue("$id", asset.Id.ToString("D"));
        command.Parameters.AddWithValue("$ownerType", asset.OwnerType.ToString());
        command.Parameters.AddWithValue("$ownerId", asset.OwnerId.ToString("D"));
        command.Parameters.AddWithValue("$type", asset.Type.ToString());
        command.Parameters.AddWithValue("$storageKey", asset.StorageKey);
        command.Parameters.AddWithValue("$contentType", asset.ContentType);
        command.Parameters.AddWithValue(
            "$createdAtUtc",
            asset.CreatedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqliteException exception)
            when (exception.SqliteExtendedErrorCode == 1555)
        {
            throw new InvalidOperationException(
                $"An asset with ID '{asset.Id}' is already registered.",
                exception);
        }
    }

    public async Task<AssetDescriptor?> GetByIdAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, OwnerType, OwnerId, Type,
                   StorageKey, ContentType, CreatedAtUtc
            FROM Assets
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", assetId.ToString("D"));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadAsset(reader);
    }

    public async Task<IReadOnlyList<AssetDescriptor>> GetByOwnerAsync(
        AssetOwnerType ownerType,
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, OwnerType, OwnerId, Type,
                   StorageKey, ContentType, CreatedAtUtc
            FROM Assets
            WHERE OwnerType = $ownerType AND OwnerId = $ownerId
            ORDER BY CreatedAtUtc, Id;
            """;

        command.Parameters.AddWithValue("$ownerType", ownerType.ToString());
        command.Parameters.AddWithValue("$ownerId", ownerId.ToString("D"));

        var assets = new List<AssetDescriptor>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            assets.Add(ReadAsset(reader));
        }

        return assets;
    }

    public async Task<IReadOnlyList<AssetDescriptor>> GetRecentAsync(
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);
        cancellationToken.ThrowIfCancellationRequested();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, OwnerType, OwnerId, Type,
                   StorageKey, ContentType, CreatedAtUtc
            FROM Assets
            ORDER BY CreatedAtUtc DESC, Id DESC
            LIMIT $take;
            """;
        command.Parameters.AddWithValue("$take", take);

        var assets = new List<AssetDescriptor>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            assets.Add(ReadAsset(reader));
        }

        return assets;
    }




    private static AssetDescriptor ReadAsset(SqliteDataReader reader)
    {
        return new AssetDescriptor(
            Guid.Parse(reader.GetString(0)),
            Enum.Parse<AssetOwnerType>(reader.GetString(1)),
            Guid.Parse(reader.GetString(2)),
            Enum.Parse<AssetType>(reader.GetString(3)),
            reader.GetString(4),
            reader.GetString(5),
            DateTimeOffset.Parse(
                reader.GetString(6),
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind));
    }
}
