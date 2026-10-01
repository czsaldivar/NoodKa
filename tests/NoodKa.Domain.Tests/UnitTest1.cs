using NoodKa.Domain.Stories;

namespace NoodKa.Domain.Tests;

public class StoryTests
{
    [Fact]
    public void Story_CanContain_Episode_Scene_And_Shot()
    {
        var story = new Story(
            "The Last Choknut",
            "Toffey and Jowa-an search for the missing Choknut.");

        var episode = new Episode(
            1,
            "The Disappearance");

        var scene = new Scene(
            1,
            "Kitchen");

        var shot = new Shot(
            1,
            TimeSpan.FromSeconds(4),
            "Jowa-an enters the kitchen and notices something missing.",
            "Concerned",
            "Medium shot with slow push-in",
            "Warm morning light");

        scene.AddShot(shot);
        episode.AddScene(scene);
        story.AddEpisode(episode);

        Assert.Single(story.Episodes);
        Assert.Single(episode.Scenes);
        Assert.Single(scene.Shots);

        Assert.Equal("The Last Choknut", story.Title);
        Assert.Equal("The Disappearance", episode.Title);
        Assert.Equal("Kitchen", scene.Location);
        Assert.Equal(4, shot.Duration.TotalSeconds);
    }
}