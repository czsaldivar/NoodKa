using NoodKa.Domain.Characters;

namespace NoodKa.Domain.Tests.Characters;

public class CharacterTests
{
    [Fact]
    public void Character_CanBeCreated()
    {
        var toffey = new Character(
            "Toffey",
            CharacterGender.Male);

        Assert.NotEqual(Guid.Empty, toffey.Id);
        Assert.Equal("Toffey", toffey.Name);
        Assert.Equal(CharacterGender.Male, toffey.Gender);
    }

    [Fact]
    public void Character_CanHavePersonality()
    {
        var toffey = new Character(
            "Toffey",
            CharacterGender.Male);

        var personality = new CharacterPersonality(
            "Dramatic, easily startled, and tends to overthink simple problems.");

        toffey.SetPersonality(personality);

        Assert.NotNull(toffey.Personality);

        Assert.Contains(
            "overthink",
            toffey.Personality!.Description);
    }

    [Fact]
    public void Character_CanHaveVisualProfile()
    {
        var jowaAn = new Character(
            "Jowa-an",
            CharacterGender.Female);

        var visualProfile = new CharacterVisualProfile(
            "Filipina woman with a warm and approachable appearance.",
            "Dark shoulder-length hair.",
            "Casual elegant home clothing.",
            "Cinematic realistic Filipino drama.");

        jowaAn.SetVisualProfile(visualProfile);

        Assert.NotNull(jowaAn.VisualProfile);

        Assert.Contains(
            "Filipina",
            jowaAn.VisualProfile!.Appearance);
    }

    [Fact]
    public void Character_CanHaveReferenceImages()
    {
        var toffey = new Character(
            "Toffey",
            CharacterGender.Male);

        var reference = new CharacterReference(
            CharacterReferenceType.Face,
            "characters/toffey/face-01.jpg",
            "Primary face reference.");

        toffey.AddReference(reference);

        Assert.Single(toffey.References);

        Assert.Equal(
            CharacterReferenceType.Face,
            toffey.References.First().Type);

        Assert.Equal(
            "characters/toffey/face-01.jpg",
            toffey.References.First().StorageLocation);
    }
}