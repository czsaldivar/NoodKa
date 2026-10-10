using Microsoft.Data.Sqlite;
using NoodKa.Application.Stories;
using NoodKa.Domain.Stories;

namespace NoodKa.Infrastructure.Stories;

public sealed class SqliteStoryRepository : IStoryRepository
{
    private readonly string _connectionString;

    public SqliteStoryRepository(string databasePath)
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
            ForeignKeys = true
        }.ToString();
    }

    public async Task AddAsync(
        Story story,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(story);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        using var transaction = connection.BeginTransaction();

        try
        {
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = """
                    INSERT INTO Stories
                        (Id, Title, Description,
                         Country, Region, Era, Language,
                         Genre, Tone, CulturalFlavor)
                    VALUES
                        ($id, $title, $description,
                         $country, $region, $era, $language,
                         $genre, $tone, $culturalFlavor);
                    """;

                command.Parameters.AddWithValue("$id", story.Id.ToString());
                command.Parameters.AddWithValue("$title", story.Title);
                command.Parameters.AddWithValue("$description", story.Description);
                command.Parameters.AddWithValue("$country", story.Country);
                command.Parameters.AddWithValue("$region", story.Region);
                command.Parameters.AddWithValue("$era", story.Era);
                command.Parameters.AddWithValue("$language", story.Language);
                command.Parameters.AddWithValue("$genre", story.Genre);
                command.Parameters.AddWithValue("$tone", story.Tone);
                command.Parameters.AddWithValue("$culturalFlavor", story.CulturalFlavor);

                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            foreach (var episode in story.Episodes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = """
                        INSERT INTO Episodes (Id, StoryId, Number, Title)
                        VALUES ($id, $storyId, $number, $title);
                        """;

                    command.Parameters.AddWithValue("$id", episode.Id.ToString());
                    command.Parameters.AddWithValue("$storyId", story.Id.ToString());
                    command.Parameters.AddWithValue("$number", episode.Number);
                    command.Parameters.AddWithValue("$title", episode.Title);

                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                foreach (var scene in episode.Scenes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = """
                            INSERT INTO Scenes (Id, EpisodeId, Number, Location)
                            VALUES ($id, $episodeId, $number, $location);
                            """;

                        command.Parameters.AddWithValue("$id", scene.Id.ToString());
                        command.Parameters.AddWithValue("$episodeId", episode.Id.ToString());
                        command.Parameters.AddWithValue("$number", scene.Number);
                        command.Parameters.AddWithValue("$location", scene.Location);

                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }

                    foreach (var shot in scene.Shots)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await using var command = connection.CreateCommand();
                        command.Transaction = transaction;
                        command.CommandText = """
                            INSERT INTO Shots
                                (Id, SceneId, Sequence, DurationTicks,
                                 Action, Emotion, Camera, Lighting)
                            VALUES
                                ($id, $sceneId, $sequence, $durationTicks,
                                 $action, $emotion, $camera, $lighting);
                            """;

                        command.Parameters.AddWithValue("$id", shot.Id.ToString());
                        command.Parameters.AddWithValue("$sceneId", scene.Id.ToString());
                        command.Parameters.AddWithValue("$sequence", shot.Sequence);
                        command.Parameters.AddWithValue("$durationTicks", shot.Duration.Ticks);
                        command.Parameters.AddWithValue("$action", shot.Action);
                        command.Parameters.AddWithValue("$emotion", shot.Emotion);
                        command.Parameters.AddWithValue("$camera", shot.Camera);
                        command.Parameters.AddWithValue("$lighting", shot.Lighting);

                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            transaction.Commit();
        }
        catch (SqliteException ex)
            when (ex.SqliteErrorCode == 19 &&
                  (ex.SqliteExtendedErrorCode == 1555 ||
                   ex.SqliteExtendedErrorCode == 2067))
        {
            throw new InvalidOperationException(
                $"A story or one of its child records conflicts with an existing ID or unique key. Story ID: '{story.Id}'.",
                ex);
        }
    }

    public async Task<Episode?> CreateEpisodeAsync(
        Guid storyId,
        string title,
        CancellationToken cancellationToken = default)
    {
        if (storyId == Guid.Empty)
            throw new ArgumentException("Story ID is required.", nameof(storyId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Episode title is required.", nameof(title));

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        // Acquire the write lock before calculating the next episode number.
        using var transaction = connection.BeginTransaction(deferred: false);

        await using (var storyCommand = connection.CreateCommand())
        {
            storyCommand.Transaction = transaction;
            storyCommand.CommandText =
                "SELECT 1 FROM Stories WHERE Id = $storyId LIMIT 1;";
            storyCommand.Parameters.AddWithValue("$storyId", storyId.ToString());

            var exists = await storyCommand.ExecuteScalarAsync(cancellationToken);
            if (exists is null)
            {
                transaction.Rollback();
                return null;
            }
        }

        int nextNumber;
        await using (var numberCommand = connection.CreateCommand())
        {
            numberCommand.Transaction = transaction;
            numberCommand.CommandText = """
                SELECT COALESCE(MAX(Number), 0) + 1
                FROM Episodes
                WHERE StoryId = $storyId;
                """;
            numberCommand.Parameters.AddWithValue("$storyId", storyId.ToString());

            nextNumber = Convert.ToInt32(
                await numberCommand.ExecuteScalarAsync(cancellationToken));
        }

        var episode = new Episode(nextNumber, title.Trim());

        await using (var insertCommand = connection.CreateCommand())
        {
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = """
                INSERT INTO Episodes (Id, StoryId, Number, Title)
                VALUES ($id, $storyId, $number, $title);
                """;
            insertCommand.Parameters.AddWithValue("$id", episode.Id.ToString());
            insertCommand.Parameters.AddWithValue("$storyId", storyId.ToString());
            insertCommand.Parameters.AddWithValue("$number", episode.Number);
            insertCommand.Parameters.AddWithValue("$title", episode.Title);

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        transaction.Commit();

        return episode;
    }
    public async Task<Shot?> CreateShotAsync(
        Guid sceneId,
        TimeSpan duration,
        string action,
        string emotion,
        string camera,
        string lighting,
        CancellationToken cancellationToken = default)
    {
        if (sceneId == Guid.Empty)
            throw new ArgumentException("Scene ID is required.", nameof(sceneId));

        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Shot action is required.", nameof(action));

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        using var transaction = connection.BeginTransaction(deferred: false);

        await using (var sceneCommand = connection.CreateCommand())
        {
            sceneCommand.Transaction = transaction;
            sceneCommand.CommandText =
                "SELECT 1 FROM Scenes WHERE Id = $sceneId LIMIT 1;";
            sceneCommand.Parameters.AddWithValue("$sceneId", sceneId.ToString());

            var exists = await sceneCommand.ExecuteScalarAsync(cancellationToken);
            if (exists is null)
            {
                transaction.Rollback();
                return null;
            }
        }

        int nextSequence;
        await using (var sequenceCommand = connection.CreateCommand())
        {
            sequenceCommand.Transaction = transaction;
            sequenceCommand.CommandText = """
                SELECT COALESCE(MAX(Sequence), 0) + 1
                FROM Shots
                WHERE SceneId = $sceneId;
                """;
            sequenceCommand.Parameters.AddWithValue("$sceneId", sceneId.ToString());

            nextSequence = Convert.ToInt32(
                await sequenceCommand.ExecuteScalarAsync(cancellationToken));
        }

        var shot = new Shot(
            nextSequence,
            duration,
            action.Trim(),
            emotion?.Trim() ?? string.Empty,
            camera?.Trim() ?? string.Empty,
            lighting?.Trim() ?? string.Empty);

        await using (var insertCommand = connection.CreateCommand())
        {
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = """
                INSERT INTO Shots
                    (Id, SceneId, Sequence, DurationTicks,
                     Action, Emotion, Camera, Lighting)
                VALUES
                    ($id, $sceneId, $sequence, $durationTicks,
                     $action, $emotion, $camera, $lighting);
                """;
            insertCommand.Parameters.AddWithValue("$id", shot.Id.ToString());
            insertCommand.Parameters.AddWithValue("$sceneId", sceneId.ToString());
            insertCommand.Parameters.AddWithValue("$sequence", shot.Sequence);
            insertCommand.Parameters.AddWithValue("$durationTicks", shot.Duration.Ticks);
            insertCommand.Parameters.AddWithValue("$action", shot.Action);
            insertCommand.Parameters.AddWithValue("$emotion", shot.Emotion);
            insertCommand.Parameters.AddWithValue("$camera", shot.Camera);
            insertCommand.Parameters.AddWithValue("$lighting", shot.Lighting);

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        transaction.Commit();

        return shot;
    }
    public async Task<bool> SetShotCharacterIdsAsync(
        Guid shotId,
        IReadOnlyList<Guid> characterIds,
        CancellationToken cancellationToken = default)
    {
        if (shotId == Guid.Empty)
            throw new ArgumentException("Shot ID is required.", nameof(shotId));

        ArgumentNullException.ThrowIfNull(characterIds);

        if (characterIds.Any(id => id == Guid.Empty))
            throw new ArgumentException("Character IDs cannot contain an empty ID.", nameof(characterIds));

        if (characterIds.Distinct().Count() != characterIds.Count)
            throw new ArgumentException("Character IDs cannot contain duplicates.", nameof(characterIds));

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        using var transaction = connection.BeginTransaction(deferred: false);

        await using (var check = connection.CreateCommand())
        {
            check.Transaction = transaction;
            check.CommandText = "SELECT 1 FROM Shots WHERE Id = $id LIMIT 1;";
            check.Parameters.AddWithValue("$id", shotId.ToString());

            if (await check.ExecuteScalarAsync(cancellationToken) is null)
            {
                transaction.Rollback();
                return false;
            }
        }

        await using (var delete = connection.CreateCommand())
        {
            delete.Transaction = transaction;
            delete.CommandText = "DELETE FROM ShotCharacters WHERE ShotId = $id;";
            delete.Parameters.AddWithValue("$id", shotId.ToString());
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        for (var i = 0; i < characterIds.Count; i++)
        {
            await using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO ShotCharacters (ShotId, CharacterId, Sequence)
                VALUES ($shotId, $characterId, $sequence);
                """;
            insert.Parameters.AddWithValue("$shotId", shotId.ToString());
            insert.Parameters.AddWithValue("$characterId", characterIds[i].ToString());
            insert.Parameters.AddWithValue("$sequence", i + 1);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        transaction.Commit();
        return true;
    }
    public async Task<Scene?> CreateSceneAsync(
        Guid episodeId,
        string name,
        string location,
        CancellationToken cancellationToken = default)
    {
        if (episodeId == Guid.Empty)
            throw new ArgumentException("Episode ID is required.", nameof(episodeId));

        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Scene location is required.", nameof(location));

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        // Acquire the write lock before calculating the next scene number.
        using var transaction = connection.BeginTransaction(deferred: false);

        await using (var episodeCommand = connection.CreateCommand())
        {
            episodeCommand.Transaction = transaction;
            episodeCommand.CommandText =
                "SELECT 1 FROM Episodes WHERE Id = $episodeId LIMIT 1;";
            episodeCommand.Parameters.AddWithValue("$episodeId", episodeId.ToString());

            var exists = await episodeCommand.ExecuteScalarAsync(cancellationToken);
            if (exists is null)
            {
                transaction.Rollback();
                return null;
            }
        }

        int nextNumber;
        await using (var numberCommand = connection.CreateCommand())
        {
            numberCommand.Transaction = transaction;
            numberCommand.CommandText = """
                SELECT COALESCE(MAX(Number), 0) + 1
                FROM Scenes
                WHERE EpisodeId = $episodeId;
                """;
            numberCommand.Parameters.AddWithValue("$episodeId", episodeId.ToString());

            nextNumber = Convert.ToInt32(
                await numberCommand.ExecuteScalarAsync(cancellationToken));
        }

        var scene = new Scene(
            nextNumber,
            name.Trim(),
            location.Trim());

        await using (var insertCommand = connection.CreateCommand())
        {
            insertCommand.Transaction = transaction;
            insertCommand.CommandText = """
                INSERT INTO Scenes (Id, EpisodeId, Number, Location)
                VALUES ($id, $episodeId, $number, $location);
                """;
            insertCommand.Parameters.AddWithValue("$id", scene.Id.ToString());
            insertCommand.Parameters.AddWithValue("$episodeId", episodeId.ToString());
            insertCommand.Parameters.AddWithValue("$number", scene.Number);
            insertCommand.Parameters.AddWithValue("$location", scene.Location);

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        cancellationToken.ThrowIfCancellationRequested();
        transaction.Commit();

        return scene;
    }
    public async Task<IReadOnlyList<Story>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await EnsureSchemaAsync(connection, cancellationToken);

        var stories = new List<Story>();

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT
                    Id,
                    Title,
                    Description,
                    Country,
                    Region,
                    Era,
                    Language,
                    Genre,
                    Tone,
                    CulturalFlavor
                FROM Stories
                ORDER BY rowid DESC;
                """;

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                stories.Add(new Story(
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? "Philippines" : reader.GetString(3),
                    reader.IsDBNull(4) ? "Metro Manila" : reader.GetString(4),
                    reader.IsDBNull(5) ? "Present Day" : reader.GetString(5),
                    reader.IsDBNull(6) ? "Taglish" : reader.GetString(6),
                    reader.IsDBNull(7) ? "Family Drama" : reader.GetString(7),
                    reader.IsDBNull(8) ? "Emotional" : reader.GetString(8),
                    reader.IsDBNull(9) ? "Filipino" : reader.GetString(9))
                {
                    Id = Guid.Parse(reader.GetString(0))
                });
            }
        }

        foreach (var story in stories)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var episode in await LoadEpisodesAsync(
                         connection, story.Id, cancellationToken))
            {
                story.AddEpisode(episode);
            }
        }

        return stories;
    }

    private static async Task<List<Episode>> LoadEpisodesAsync(
        SqliteConnection connection,
        Guid storyId,
        CancellationToken cancellationToken)
    {
        var episodes = new List<Episode>();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Number, Title
            FROM Episodes
            WHERE StoryId = $storyId
            ORDER BY Number, rowid;
            """;
        command.Parameters.AddWithValue("$storyId", storyId.ToString());

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            episodes.Add(new Episode(reader.GetInt32(1), reader.GetString(2))
            {
                Id = Guid.Parse(reader.GetString(0))
            });
        }

        foreach (var episode in episodes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var scene in await LoadScenesAsync(
                         connection, episode.Id, cancellationToken))
            {
                episode.AddScene(scene);
            }
        }

        return episodes;
    }

    private static async Task<List<Scene>> LoadScenesAsync(
        SqliteConnection connection,
        Guid episodeId,
        CancellationToken cancellationToken)
    {
        var scenes = new List<Scene>();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Number, Location
            FROM Scenes
            WHERE EpisodeId = $episodeId
            ORDER BY Number, rowid;
            """;
        command.Parameters.AddWithValue("$episodeId", episodeId.ToString());

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            scenes.Add(new Scene(reader.GetInt32(1), reader.GetString(2))
            {
                Id = Guid.Parse(reader.GetString(0))
            });
        }

        foreach (var scene in scenes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var shot in await LoadShotsAsync(
                         connection, scene.Id, cancellationToken))
            {
                scene.AddShot(shot);
            }
        }

        return scenes;
    }

    private static async Task<List<Shot>> LoadShotsAsync(
        SqliteConnection connection,
        Guid sceneId,
        CancellationToken cancellationToken)
    {
        var shots = new List<Shot>();

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT Id, Sequence, DurationTicks, Action, Emotion, Camera, Lighting
                FROM Shots
                WHERE SceneId = $sceneId
                ORDER BY Sequence, rowid;
                """;
            command.Parameters.AddWithValue("$sceneId", sceneId.ToString());

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                shots.Add(new Shot(
                    reader.GetInt32(1),
                    TimeSpan.FromTicks(reader.GetInt64(2)),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetString(6))
                {
                    Id = Guid.Parse(reader.GetString(0))
                });
            }
        }

        foreach (var shot in shots)
        {
            var characterIds = new List<Guid>();

            await using var characterCommand = connection.CreateCommand();
            characterCommand.CommandText = """
                SELECT CharacterId
                FROM ShotCharacters
                WHERE ShotId = $shotId
                ORDER BY Sequence;
                """;
            characterCommand.Parameters.AddWithValue("$shotId", shot.Id.ToString());

            await using var characterReader =
                await characterCommand.ExecuteReaderAsync(cancellationToken);

            while (await characterReader.ReadAsync(cancellationToken))
                characterIds.Add(Guid.Parse(characterReader.GetString(0)));

            shot.SetCharacterIds(characterIds);
        }

        return shots;
    }
    private static async Task EnsureSchemaAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Stories (
                Id TEXT NOT NULL PRIMARY KEY,
                Title TEXT NOT NULL,
                Description TEXT NOT NULL,
                Country TEXT NOT NULL DEFAULT 'Philippines',
                Region TEXT NOT NULL DEFAULT 'Metro Manila',
                Era TEXT NOT NULL DEFAULT 'Present Day',
                Language TEXT NOT NULL DEFAULT 'Taglish',
                Genre TEXT NOT NULL DEFAULT 'Family Drama',
                Tone TEXT NOT NULL DEFAULT 'Emotional',
                CulturalFlavor TEXT NOT NULL DEFAULT 'Filipino'
            );

            CREATE TABLE IF NOT EXISTS Episodes (
                Id TEXT NOT NULL PRIMARY KEY,
                StoryId TEXT NOT NULL,
                Number INTEGER NOT NULL,
                Title TEXT NOT NULL,
                FOREIGN KEY (StoryId) REFERENCES Stories(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Scenes (
                Id TEXT NOT NULL PRIMARY KEY,
                EpisodeId TEXT NOT NULL,
                Number INTEGER NOT NULL,
                Location TEXT NOT NULL,
                FOREIGN KEY (EpisodeId) REFERENCES Episodes(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Shots (
                Id TEXT NOT NULL PRIMARY KEY,
                SceneId TEXT NOT NULL,
                Sequence INTEGER NOT NULL,
                DurationTicks INTEGER NOT NULL,
                Action TEXT NOT NULL,
                Emotion TEXT NOT NULL,
                Camera TEXT NOT NULL,
                Lighting TEXT NOT NULL,
                FOREIGN KEY (SceneId) REFERENCES Scenes(Id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS ShotCharacters (
                ShotId TEXT NOT NULL,
                CharacterId TEXT NOT NULL,
                Sequence INTEGER NOT NULL,
                PRIMARY KEY (ShotId, CharacterId),
                FOREIGN KEY (ShotId) REFERENCES Shots(Id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS IX_ShotCharacters_ShotId
                ON ShotCharacters(ShotId);
            CREATE INDEX IF NOT EXISTS IX_Episodes_StoryId
                ON Episodes(StoryId);

            CREATE INDEX IF NOT EXISTS IX_Scenes_EpisodeId
                ON Scenes(EpisodeId);

            CREATE INDEX IF NOT EXISTS IX_Shots_SceneId
                ON Shots(SceneId);
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);

        await EnsureStoryDnaColumnsAsync(connection, cancellationToken);
        await EnsureSceneNameColumnAsync(connection, cancellationToken);
    }

    private static async Task EnsureSceneNameColumnAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var existingColumns = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA table_info(Scenes);";

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                existingColumns.Add(reader.GetString(1));
            }
        }

        if (!existingColumns.Contains("Name"))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                ALTER TABLE Scenes
                ADD COLUMN Name TEXT NOT NULL DEFAULT '';
                """;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        // Give existing scenes a deterministic fallback name.
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                UPDATE Scenes
                SET Name = 'Scene ' || Number
                WHERE Name IS NULL OR TRIM(Name) = '';
                """;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
    private static async Task EnsureStoryDnaColumnsAsync(
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        var existingColumns = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA table_info(Stories);";

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                existingColumns.Add(reader.GetString(1));
            }
        }

        var columns = new (string Name, string Definition)[]
        {
            ("Country", "TEXT NOT NULL DEFAULT 'Philippines'"),
            ("Region", "TEXT NOT NULL DEFAULT 'Metro Manila'"),
            ("Era", "TEXT NOT NULL DEFAULT 'Present Day'"),
            ("Language", "TEXT NOT NULL DEFAULT 'Taglish'"),
            ("Genre", "TEXT NOT NULL DEFAULT 'Family Drama'"),
            ("Tone", "TEXT NOT NULL DEFAULT 'Emotional'"),
            ("CulturalFlavor", "TEXT NOT NULL DEFAULT 'Filipino'")
        };

        foreach (var column in columns)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (existingColumns.Contains(column.Name))
                continue;

            await using var alterCommand = connection.CreateCommand();

            alterCommand.CommandText =
                $"ALTER TABLE Stories ADD COLUMN {column.Name} {column.Definition};";

            await alterCommand.ExecuteNonQueryAsync(cancellationToken);

            existingColumns.Add(column.Name);
        }
    }
}
