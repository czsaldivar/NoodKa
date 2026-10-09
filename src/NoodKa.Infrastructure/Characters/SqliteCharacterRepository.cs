using Microsoft.Data.Sqlite;
using NoodKa.Application.Characters;
using NoodKa.Domain.Characters;

namespace NoodKa.Infrastructure.Characters;

public sealed class SqliteCharacterRepository : ICharacterRepository
{
    private readonly string _connectionString;
    private readonly SemaphoreSlim _schemaLock = new(1, 1);
    private bool _schemaInitialized;

    public SqliteCharacterRepository(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
            throw new ArgumentException(
                "Database path is required.",
                nameof(databasePath));

        var fullPath = Path.GetFullPath(databasePath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = fullPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = true
        }.ToString();
    }

    public async Task AddAsync(
        Character character,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(character);

        if (character.Id == Guid.Empty)
            throw new ArgumentException(
                "Character ID cannot be empty.",
                nameof(character));

        await EnsureSchemaAsync(cancellationToken);

        await using var connection = await OpenConnectionAsync(
            cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(cancellationToken);

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = (SqliteTransaction)transaction;
            command.CommandText = """
                INSERT INTO Characters
                    (Id, Name, Gender, PersonalityDescription,
                     Appearance, Hair, TypicalClothing, VisualStyle)
                VALUES
                    ($id, $name, $gender, $personality,
                     $appearance, $hair, $clothing, $style);
                """;

            command.Parameters.AddWithValue("$id", character.Id.ToString("D"));
            command.Parameters.AddWithValue("$name", character.Name);
            command.Parameters.AddWithValue("$gender", (int)character.Gender);
            command.Parameters.AddWithValue(
                "$personality",
                (object?)character.Personality?.Description ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "$appearance",
                (object?)character.VisualProfile?.Appearance ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "$hair",
                (object?)character.VisualProfile?.Hair ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "$clothing",
                (object?)character.VisualProfile?.TypicalClothing ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "$style",
                (object?)character.VisualProfile?.VisualStyle ?? DBNull.Value);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var reference in character.References)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = (SqliteTransaction)transaction;
            command.CommandText = """
                INSERT INTO CharacterReferences
                    (Id, CharacterId, Type, StorageLocation, Description)
                VALUES
                    ($id, $characterId, $type, $location, $description);
                """;

            command.Parameters.AddWithValue("$id", reference.Id.ToString("D"));
            command.Parameters.AddWithValue(
                "$characterId",
                character.Id.ToString("D"));
            command.Parameters.AddWithValue("$type", (int)reference.Type);
            command.Parameters.AddWithValue(
                "$location",
                reference.StorageLocation);
            command.Parameters.AddWithValue(
                "$description",
                (object?)reference.Description ?? DBNull.Value);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Character>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureSchemaAsync(cancellationToken);

        var characters = new List<Character>();

        await using var connection = await OpenConnectionAsync(
            cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Name, Gender, PersonalityDescription,
                   Appearance, Hair, TypicalClothing, VisualStyle
            FROM Characters
            ORDER BY Name COLLATE NOCASE, Id;
            """;

        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
            characters.Add(ReadCharacter(reader));

        await reader.CloseAsync();

        foreach (var character in characters)
            await LoadReferencesAsync(
                connection,
                character,
                cancellationToken);

        return characters;
    }

    public async Task<Character?> GetByIdAsync(
        Guid characterId,
        CancellationToken cancellationToken = default)
    {
        if (characterId == Guid.Empty)
            return null;

        await EnsureSchemaAsync(cancellationToken);

        await using var connection = await OpenConnectionAsync(
            cancellationToken);

        Character? character;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT Id, Name, Gender, PersonalityDescription,
                       Appearance, Hair, TypicalClothing, VisualStyle
                FROM Characters
                WHERE Id = $id;
                """;
            command.Parameters.AddWithValue(
                "$id",
                characterId.ToString("D"));

            await using var reader = await command.ExecuteReaderAsync(
                cancellationToken);

            character = await reader.ReadAsync(cancellationToken)
                ? ReadCharacter(reader)
                : null;
        }

        if (character is not null)
            await LoadReferencesAsync(
                connection,
                character,
                cancellationToken);

        return character;
    }

    public async Task<bool> AddReferenceAsync(
        Guid characterId,
        CharacterReference reference,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reference);

        if (characterId == Guid.Empty)
            return false;

        await EnsureSchemaAsync(cancellationToken);

        await using var connection = await OpenConnectionAsync(
            cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO CharacterReferences
                (Id, CharacterId, Type, StorageLocation, Description)
            SELECT
                $id, $characterId, $type, $location, $description
            WHERE EXISTS (
                SELECT 1 FROM Characters WHERE Id = $characterId
            );
            """;

        command.Parameters.AddWithValue("$id", reference.Id.ToString("D"));
        command.Parameters.AddWithValue(
            "$characterId", characterId.ToString("D"));
        command.Parameters.AddWithValue("$type", (int)reference.Type);
        command.Parameters.AddWithValue("$location", reference.StorageLocation);
        command.Parameters.AddWithValue(
            "$description", (object?)reference.Description ?? DBNull.Value);

        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task UpdateAsync(
        Character character,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(character);

        await EnsureSchemaAsync(cancellationToken);

        await using var connection = await OpenConnectionAsync(
            cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE Characters
            SET Name = $name,
                Gender = $gender,
                PersonalityDescription = $personality,
                Appearance = $appearance,
                Hair = $hair,
                TypicalClothing = $clothing,
                VisualStyle = $style
            WHERE Id = $id;
            """;

        command.Parameters.AddWithValue("$id", character.Id.ToString("D"));
        command.Parameters.AddWithValue("$name", character.Name);
        command.Parameters.AddWithValue("$gender", (int)character.Gender);
        command.Parameters.AddWithValue(
            "$personality",
            (object?)character.Personality?.Description ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$appearance",
            (object?)character.VisualProfile?.Appearance ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$hair",
            (object?)character.VisualProfile?.Hair ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$clothing",
            (object?)character.VisualProfile?.TypicalClothing ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$style",
            (object?)character.VisualProfile?.VisualStyle ?? DBNull.Value);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);

        if (rows == 0)
            throw new KeyNotFoundException(
                $"Character '{character.Id}' was not found.");
    }
    private static Character ReadCharacter(SqliteDataReader reader)
    {
        var id = Guid.Parse(reader.GetString(0));
        var name = reader.GetString(1);
        var gender = (CharacterGender)reader.GetInt32(2);

        var character = new Character(name, gender)
        {
            Id = id
        };

        if (!reader.IsDBNull(3))
        {
            character.SetPersonality(
                new CharacterPersonality(reader.GetString(3)));
        }

        if (!reader.IsDBNull(4) ||
            !reader.IsDBNull(5) ||
            !reader.IsDBNull(6) ||
            !reader.IsDBNull(7))
        {
            character.SetVisualProfile(
                new CharacterVisualProfile(
                    reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    reader.IsDBNull(7) ? string.Empty : reader.GetString(7)));
        }

        return character;
    }

    private static async Task LoadReferencesAsync(
        SqliteConnection connection,
        Character character,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Type, StorageLocation, Description
            FROM CharacterReferences
            WHERE CharacterId = $characterId
            ORDER BY rowid;
            """;
        command.Parameters.AddWithValue(
            "$characterId",
            character.Id.ToString("D"));

        await using var reader = await command.ExecuteReaderAsync(
            cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var reference = new CharacterReference(
                (CharacterReferenceType)reader.GetInt32(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3))
            {
                Id = Guid.Parse(reader.GetString(0))
            };

            character.AddReference(reference);
        }
    }

    private async Task EnsureSchemaAsync(
        CancellationToken cancellationToken)
    {
        if (_schemaInitialized)
            return;

        await _schemaLock.WaitAsync(cancellationToken);

        try
        {
            if (_schemaInitialized)
                return;

            await using var connection = await OpenConnectionAsync(
                cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE IF NOT EXISTS Characters
                (
                    Id TEXT NOT NULL PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Gender INTEGER NOT NULL,
                    PersonalityDescription TEXT NULL,
                    Appearance TEXT NULL,
                    Hair TEXT NULL,
                    TypicalClothing TEXT NULL,
                    VisualStyle TEXT NULL
                );

                CREATE TABLE IF NOT EXISTS CharacterReferences
                (
                    Id TEXT NOT NULL PRIMARY KEY,
                    CharacterId TEXT NOT NULL,
                    Type INTEGER NOT NULL,
                    StorageLocation TEXT NOT NULL,
                    Description TEXT NULL,
                    FOREIGN KEY (CharacterId)
                        REFERENCES Characters(Id)
                        ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS IX_Characters_Name
                    ON Characters(Name);

                CREATE INDEX IF NOT EXISTS IX_CharacterReferences_CharacterId
                    ON CharacterReferences(CharacterId);
                """;

            await command.ExecuteNonQueryAsync(cancellationToken);
            _schemaInitialized = true;
        }
        finally
        {
            _schemaLock.Release();
        }
    }

    private async Task<SqliteConnection> OpenConnectionAsync(
        CancellationToken cancellationToken)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        await command.ExecuteNonQueryAsync(cancellationToken);

        return connection;
    }
}
