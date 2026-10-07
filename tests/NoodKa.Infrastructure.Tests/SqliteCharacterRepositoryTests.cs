using Microsoft.Data.Sqlite;
using NoodKa.Domain.Characters;
using NoodKa.Infrastructure.Characters;

namespace NoodKa.Infrastructure.Tests;

public sealed class SqliteCharacterRepositoryTests
{
    [Fact]
    public async Task CharacterProfile_PersistsAcrossRepositoryInstances()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "characters.db");

        try
        {
            var character = new Character("Toffey", CharacterGender.Male)
            {
                Id = Guid.NewGuid()
            };

            character.SetPersonality(
                new CharacterPersonality(
                    "Dramatic, easily startled, and overthinks simple problems."));

            character.SetVisualProfile(
                new CharacterVisualProfile(
                    "Filipino man with expressive eyes.",
                    "Short dark hair.",
                    "Casual home clothing.",
                    "Cinematic realistic Filipino drama."));

            await new SqliteCharacterRepository(databasePath)
                .AddAsync(character);

            var repository = new SqliteCharacterRepository(databasePath);
            var retrieved = await repository.GetByIdAsync(character.Id);

            Assert.NotNull(retrieved);
            Assert.Equal(character.Id, retrieved.Id);
            Assert.Equal("Toffey", retrieved.Name);
            Assert.Equal(CharacterGender.Male, retrieved.Gender);
            Assert.Equal(
                "Dramatic, easily startled, and overthinks simple problems.",
                retrieved.Personality?.Description);

            Assert.NotNull(retrieved.VisualProfile);
            Assert.Equal(
                "Filipino man with expressive eyes.",
                retrieved.VisualProfile.Appearance);
            Assert.Equal("Short dark hair.", retrieved.VisualProfile.Hair);
            Assert.Equal(
                "Casual home clothing.",
                retrieved.VisualProfile.TypicalClothing);
            Assert.Equal(
                "Cinematic realistic Filipino drama.",
                retrieved.VisualProfile.VisualStyle);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task CharacterReferences_PersistAcrossRepositoryInstances()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "characters.db");

        try
        {
            var character = new Character("Toffey", CharacterGender.Male);
            var reference = new CharacterReference(
                CharacterReferenceType.Face,
                "characters/toffey/face.png",
                "Primary face reference.");

            character.AddReference(reference);

            await new SqliteCharacterRepository(databasePath)
                .AddAsync(character);

            var retrieved = await new SqliteCharacterRepository(databasePath)
                .GetByIdAsync(character.Id);

            Assert.NotNull(retrieved);

            var savedReference = Assert.Single(retrieved.References);
            Assert.Equal(reference.Id, savedReference.Id);
            Assert.Equal(CharacterReferenceType.Face, savedReference.Type);
            Assert.Equal(
                "characters/toffey/face.png",
                savedReference.StorageLocation);
            Assert.Equal(
                "Primary face reference.",
                savedReference.Description);
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMultipleCharactersWithoutOverwriting()
    {
        var directory = CreateTestDirectory();
        var databasePath = Path.Combine(directory, "characters.db");

        try
        {
            var repository = new SqliteCharacterRepository(databasePath);
            var toffey = new Character("Toffey", CharacterGender.Male);
            var joAnn = new Character("Jo-Ann", CharacterGender.Female);

            await repository.AddAsync(toffey);
            await repository.AddAsync(joAnn);

            var characters = await new SqliteCharacterRepository(databasePath)
                .GetAllAsync();

            Assert.Equal(2, characters.Count);
            Assert.Contains(characters, character =>
                character.Id == toffey.Id &&
                character.Name == "Toffey");
            Assert.Contains(characters, character =>
                character.Id == joAnn.Id &&
                character.Name == "Jo-Ann");
        }
        finally
        {
            SqliteConnection.ClearAllPools();

            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task GetByIdAsync_MissingCharacter_ReturnsNull()
    {
        var directory = CreateTestDirectory();

        try
        {
            var repository = new SqliteCharacterRepository(
                Path.Combine(directory, "characters.db"));

            var retrieved = await repository.GetByIdAsync(Guid.NewGuid());

            Assert.Null(retrieved);
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
            "NoodKa-CharacterTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);
        return directory;
    }
}
