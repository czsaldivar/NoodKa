using Microsoft.Data.Sqlite;
using NoodKa.Domain.Stories;
using NoodKa.Infrastructure.Stories;

namespace NoodKa.Infrastructure.Tests;

public sealed class SqliteStoryRepositoryTests
{
    [Fact]
    public async Task Story_PersistsAcrossRepositoryInstances()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "stories.db");
        var id = Guid.NewGuid();

        try
        {
            var first = new SqliteStoryRepository(databasePath);
            var story = new Story("Pilot", "The opening episode.")
            {
                Id = id
            };

            await first.AddAsync(story);

            var second = new SqliteStoryRepository(databasePath);
            var stories = await second.GetAllAsync();

            var retrieved = Assert.Single(stories);
            Assert.Equal(id, retrieved.Id);
            Assert.Equal("Pilot", retrieved.Title);
            Assert.Equal("The opening episode.", retrieved.Description);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task FullHierarchy_PersistsAcrossRepositoryInstances()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "stories.db");

        try
        {
            var story = new Story("The Last Sunrise", "A cinematic drama.");
            var episode = new Episode(1, "The Blackout");
            var scene = new Scene(1, "Rooftop at sunset");
            var shot = new Shot(
                1,
                TimeSpan.FromSeconds(7.5),
                "A lone survivor watches the darkened city.",
                "Hope mixed with fear",
                "Slow cinematic push-in",
                "Warm sunset rim light");

            scene.AddShot(shot);
            episode.AddScene(scene);
            story.AddEpisode(episode);

            await new SqliteStoryRepository(databasePath).AddAsync(story);

            var stories = await new SqliteStoryRepository(databasePath).GetAllAsync();
            var retrievedStory = Assert.Single(stories);
            var retrievedEpisode = Assert.Single(retrievedStory.Episodes);
            var retrievedScene = Assert.Single(retrievedEpisode.Scenes);
            var retrievedShot = Assert.Single(retrievedScene.Shots);

            Assert.Equal(story.Id, retrievedStory.Id);
            Assert.Equal(episode.Id, retrievedEpisode.Id);
            Assert.Equal(scene.Id, retrievedScene.Id);
            Assert.Equal(shot.Id, retrievedShot.Id);

            Assert.Equal("The Last Sunrise", retrievedStory.Title);
            Assert.Equal("The Blackout", retrievedEpisode.Title);
            Assert.Equal("Rooftop at sunset", retrievedScene.Location);
            Assert.Equal(TimeSpan.FromSeconds(7.5), retrievedShot.Duration);
            Assert.Equal("A lone survivor watches the darkened city.", retrievedShot.Action);
            Assert.Equal("Hope mixed with fear", retrievedShot.Emotion);
            Assert.Equal("Slow cinematic push-in", retrievedShot.Camera);
            Assert.Equal("Warm sunset rim light", retrievedShot.Lighting);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task AddAsync_DuplicateId_ThrowsInvalidOperationException()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "stories.db");

        try
        {
            var repository = new SqliteStoryRepository(databasePath);
            var id = Guid.NewGuid();

            await repository.AddAsync(new Story("First", "")
            {
                Id = id
            });

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repository.AddAsync(new Story("Duplicate", "")
                {
                    Id = id
                }));
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task GetAllAsync_CanceledToken_ThrowsOperationCanceledException()
    {
        var directory = CreateTestDirectory();

        try
        {
            var repository = new SqliteStoryRepository(
                Path.Combine(directory, "stories.db"));

            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => repository.GetAllAsync(cancellation.Token));
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var directory = CreateTestDirectory();

        try
        {
            var repository = new SqliteStoryRepository(
                Path.Combine(directory, "stories.db"));

            var stories = await repository.GetAllAsync();

            Assert.Empty(stories);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task CreateEpisodeAsync_CreatesAndPersistsSequentialEpisodes()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "stories.db");

        try
        {
            var repository = new SqliteStoryRepository(databasePath);
            var story = new Story("The Last Sunrise", "A cinematic drama.");
            await repository.AddAsync(story);

            var first = await repository.CreateEpisodeAsync(
                story.Id, "  The Blackout  ");
            var second = await repository.CreateEpisodeAsync(
                story.Id, "First Light");

            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Equal(1, first.Number);
            Assert.Equal("The Blackout", first.Title);
            Assert.Equal(2, second.Number);

            var reloaded = await new SqliteStoryRepository(databasePath)
                .GetAllAsync();
            var savedStory = Assert.Single(reloaded);
            Assert.Equal(2, savedStory.Episodes.Count);
            Assert.Contains(savedStory.Episodes, episode =>
                episode.Id == first.Id &&
                episode.Number == 1 &&
                episode.Title == "The Blackout");
            Assert.Contains(savedStory.Episodes, episode =>
                episode.Id == second.Id &&
                episode.Number == 2 &&
                episode.Title == "First Light");
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task CreateEpisodeAsync_MissingStory_ReturnsNull()
    {
        var directory = CreateTestDirectory();

        try
        {
            var repository = new SqliteStoryRepository(
                Path.Combine(directory, "stories.db"));

            var result = await repository.CreateEpisodeAsync(
                Guid.NewGuid(), "The Beginning");

            Assert.Null(result);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateEpisodeAsync_BlankTitle_ThrowsArgumentException(
        string title)
    {
        var directory = CreateTestDirectory();

        try
        {
            var repository = new SqliteStoryRepository(
                Path.Combine(directory, "stories.db"));

            await Assert.ThrowsAsync<ArgumentException>(
                () => repository.CreateEpisodeAsync(Guid.NewGuid(), title));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }
    [Fact]
    public async Task CreateSceneAsync_CreatesAndPersistsSequentialScenes()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "stories.db");

        try
        {
            var repository = new SqliteStoryRepository(databasePath);
            var story = new Story("The Last Sunrise", "A cinematic drama.");
            await repository.AddAsync(story);

            var episode = await repository.CreateEpisodeAsync(
                story.Id, "The Blackout");

            Assert.NotNull(episode);

            var first = await repository.CreateSceneAsync(
                episode.Id, "Living Room", "  INT. LIVING ROOM - NIGHT  ");
            var second = await repository.CreateSceneAsync(
                episode.Id, "Rooftop", "EXT. ROOFTOP - DAWN");

            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Equal(1, first.Number);
            Assert.Equal("INT. LIVING ROOM - NIGHT", first.Location);
            Assert.Equal(2, second.Number);

            var reloaded = await new SqliteStoryRepository(databasePath)
                .GetAllAsync();

            var savedStory = Assert.Single(reloaded);
            var savedEpisode = Assert.Single(savedStory.Episodes);

            Assert.Equal(2, savedEpisode.Scenes.Count);
            Assert.Contains(savedEpisode.Scenes, scene =>
                scene.Id == first.Id &&
                scene.Number == 1 &&
                scene.Location == "INT. LIVING ROOM - NIGHT");
            Assert.Contains(savedEpisode.Scenes, scene =>
                scene.Id == second.Id &&
                scene.Number == 2 &&
                scene.Location == "EXT. ROOFTOP - DAWN");
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task CreateSceneAsync_MissingEpisode_ReturnsNull()
    {
        var directory = CreateTestDirectory();

        try
        {
            var repository = new SqliteStoryRepository(
                Path.Combine(directory, "stories.db"));

            var result = await repository.CreateSceneAsync(
                Guid.NewGuid(), "Kitchen", "INT. KITCHEN - DAY");

            Assert.Null(result);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateSceneAsync_BlankLocation_ThrowsArgumentException(
        string location)
    {
        var directory = CreateTestDirectory();

        try
        {
            var repository = new SqliteStoryRepository(
                Path.Combine(directory, "stories.db"));

            await Assert.ThrowsAsync<ArgumentException>(
                () => repository.CreateSceneAsync(Guid.NewGuid(), "Test Scene", location));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task CreateShotAsync_CreatesAndPersistsSequentialShots()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "stories.db");

        try
        {
            var repository = new SqliteStoryRepository(databasePath);
            var story = new Story("The Last Sunrise", "A cinematic drama.");
            await repository.AddAsync(story);

            var episode = await repository.CreateEpisodeAsync(
                story.Id, "The Blackout");
            Assert.NotNull(episode);

            var scene = await repository.CreateSceneAsync(
                episode.Id, "Kitchen", "INT. KITCHEN - NIGHT");
            Assert.NotNull(scene);

            var first = await repository.CreateShotAsync(
                scene.Id,
                TimeSpan.FromSeconds(5),
                "A survivor turns toward the window.",
                "Uneasy anticipation",
                "Slow push-in",
                "Low-key blue lighting", title: "Rain on the Windshield");

            var second = await repository.CreateShotAsync(
                scene.Id,
                TimeSpan.FromSeconds(7.5),
                "A distant light flickers on.",
                "Renewed hope",
                "Gentle dolly-out",
                "Warm practical light", title: "A Light in the Distance");

            Assert.NotNull(first);
            Assert.NotNull(second);
            Assert.Equal(1, first.Sequence);
            Assert.Equal(2, second.Sequence);

            var reloaded = await new SqliteStoryRepository(databasePath)
                .GetAllAsync();

            var savedStory = Assert.Single(reloaded);
            var savedEpisode = Assert.Single(savedStory.Episodes);
            var savedScene = Assert.Single(savedEpisode.Scenes);
            var savedShots = savedScene.Shots.OrderBy(x => x.Sequence).ToList();

            Assert.Equal(2, savedShots.Count);
            Assert.Equal(first.Id, savedShots[0].Id);
            Assert.Equal(second.Id, savedShots[1].Id);

            Assert.Equal("Rain on the Windshield", savedShots[0].Title);
            Assert.Equal("A Light in the Distance", savedShots[1].Title);
            Assert.Equal(TimeSpan.FromSeconds(5), savedShots[0].Duration);
            Assert.Equal(
                "A survivor turns toward the window.",
                savedShots[0].Action);
            Assert.Equal("Uneasy anticipation", savedShots[0].Emotion);
            Assert.Equal("Slow push-in", savedShots[0].Camera);
            Assert.Equal("Low-key blue lighting", savedShots[0].Lighting);

            Assert.Equal(TimeSpan.FromSeconds(7.5), savedShots[1].Duration);
            Assert.Equal(
                "A distant light flickers on.",
                savedShots[1].Action);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task GetAllAsync_LegacyShotsTable_AddsTitleAndPreservesExistingTitles()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "stories.db");
        var storyId = Guid.NewGuid();
        var episodeId = Guid.NewGuid();
        var sceneId = Guid.NewGuid();
        var shotId = Guid.NewGuid();

        try
        {
            // Create the legacy schema without Shots.Title.
            await using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = """
                    CREATE TABLE Stories (
                        Id TEXT NOT NULL PRIMARY KEY,
                        Title TEXT NOT NULL,
                        Description TEXT NOT NULL
                    );
                    CREATE TABLE Episodes (
                        Id TEXT NOT NULL PRIMARY KEY,
                        StoryId TEXT NOT NULL,
                        Number INTEGER NOT NULL,
                        Title TEXT NOT NULL
                    );
                    CREATE TABLE Scenes (
                        Id TEXT NOT NULL PRIMARY KEY,
                        EpisodeId TEXT NOT NULL,
                        Number INTEGER NOT NULL,
                        Location TEXT NOT NULL
                    );
                    CREATE TABLE Shots (
                        Id TEXT NOT NULL PRIMARY KEY,
                        SceneId TEXT NOT NULL,
                        Sequence INTEGER NOT NULL,
                        DurationTicks INTEGER NOT NULL,
                        Action TEXT NOT NULL,
                        Emotion TEXT NOT NULL,
                        Camera TEXT NOT NULL,
                        Lighting TEXT NOT NULL
                    );
                    INSERT INTO Stories (Id, Title, Description)
                    VALUES ($storyId, 'Legacy Story', 'Migration test');
                    INSERT INTO Episodes (Id, StoryId, Number, Title)
                    VALUES ($episodeId, $storyId, 1, 'Episode 1');
                    INSERT INTO Scenes (Id, EpisodeId, Number, Location)
                    VALUES ($sceneId, $episodeId, 1, 'Kitchen');
                    INSERT INTO Shots
                        (Id, SceneId, Sequence, DurationTicks, Action, Emotion, Camera, Lighting)
                    VALUES
                        ($shotId, $sceneId, 1, 50000000,
                         'A character looks outside', 'Thoughtful', 'Medium shot', 'Soft daylight');
                    """;
                command.Parameters.AddWithValue("$storyId", storyId.ToString());
                command.Parameters.AddWithValue("$episodeId", episodeId.ToString());
                command.Parameters.AddWithValue("$sceneId", sceneId.ToString());
                command.Parameters.AddWithValue("$shotId", shotId.ToString());
                await command.ExecuteNonQueryAsync();
            }

            var firstLoad = await new SqliteStoryRepository(databasePath).GetAllAsync();
            var firstShot = Assert.Single(
                Assert.Single(Assert.Single(Assert.Single(firstLoad).Episodes).Scenes).Shots);

            Assert.Equal(shotId, firstShot.Id);
            Assert.Equal("Shot 1", firstShot.Title);

            // A later schema initialization must not overwrite a meaningful title.
            await using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "UPDATE Shots SET Title = $title WHERE Id = $id;";
                command.Parameters.AddWithValue("$title", "The Unexpected Call");
                command.Parameters.AddWithValue("$id", shotId.ToString());
                Assert.Equal(1, await command.ExecuteNonQueryAsync());
            }

            var secondLoad = await new SqliteStoryRepository(databasePath).GetAllAsync();
            var secondShot = Assert.Single(
                Assert.Single(Assert.Single(Assert.Single(secondLoad).Episodes).Scenes).Shots);

            Assert.Equal("The Unexpected Call", secondShot.Title);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task SetShotCharacterIds_PersistsMultipleCharacterIds()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "stories.db");

        try
        {
            var repository = new SqliteStoryRepository(databasePath);
            var story = new Story("Multi-character test", "Persistence test.");
            await repository.AddAsync(story);

            var episode = await repository.CreateEpisodeAsync(story.Id, "Episode 1");
            Assert.NotNull(episode);

            var scene = await repository.CreateSceneAsync(
                episode.Id, "Family Room", "INT. FAMILY ROOM - EVENING");
            Assert.NotNull(scene);

            var shot = await repository.CreateShotAsync(
                scene.Id,
                TimeSpan.FromSeconds(5),
                "The family gathers for prayer.",
                "Warm and peaceful",
                "Medium shot",
                "Soft evening light");
            Assert.NotNull(shot);

            var firstCharacterId = Guid.NewGuid();
            var secondCharacterId = Guid.NewGuid();

            var saved = await repository.SetShotCharacterIdsAsync(
                shot.Id,
                new[] { firstCharacterId, secondCharacterId });

            Assert.True(saved);

            var reloaded = await new SqliteStoryRepository(databasePath).GetAllAsync();
            var reloadedShot = Assert.Single(
                Assert.Single(Assert.Single(Assert.Single(reloaded).Episodes).Scenes).Shots);

            Assert.Equal(
                new[] { firstCharacterId, secondCharacterId },
                reloadedShot.CharacterIds);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }
    [Fact]
    public async Task CreateShotAsync_MissingScene_ReturnsNull()
    {
        var directory = CreateTestDirectory();

        try
        {
            var repository = new SqliteStoryRepository(
                Path.Combine(directory, "stories.db"));

            var result = await repository.CreateShotAsync(
                Guid.NewGuid(),
                TimeSpan.FromSeconds(5),
                "A character looks outside.",
                "Curious",
                "Static wide shot",
                "Soft window light");

            Assert.Null(result);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateShotAsync_InvalidDuration_Throws(
        double seconds)
    {
        var directory = CreateTestDirectory();

        try
        {
            var repository = new SqliteStoryRepository(
                Path.Combine(directory, "stories.db"));

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => repository.CreateShotAsync(
                    Guid.NewGuid(),
                    TimeSpan.FromSeconds(seconds),
                    "A character looks outside.",
                    "Curious",
                    "Static wide shot",
                    "Soft window light"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateShotAsync_BlankAction_ThrowsArgumentException(
        string action)
    {
        var directory = CreateTestDirectory();

        try
        {
            var repository = new SqliteStoryRepository(
                Path.Combine(directory, "stories.db"));

            await Assert.ThrowsAsync<ArgumentException>(
                () => repository.CreateShotAsync(
                    Guid.NewGuid(),
                    TimeSpan.FromSeconds(5),
                    action,
                    "Curious",
                    "Static wide shot",
                    "Soft window light"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }
    private static string CreateTestDirectory()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "NoodKa-StoryTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);
        return directory;
    }
}
