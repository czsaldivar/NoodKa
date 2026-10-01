using NoodKa.Application.AI.Mock;
using NoodKa.Application.Prompts;
using NoodKa.Application.Shots;

namespace NoodKa.Application.Tests.Shots;

public sealed class ShotGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_BuildsPromptAndGeneratesImage()
    {
        var promptBuilder =
            new CinematicPromptBuilder();

        var imageGenerator =
            new MockImageGenerator();

        var shotGenerator =
            new ShotGenerator(
                promptBuilder,
                imageGenerator);

        var promptRequest =
            new CinematicPromptRequest(
                location: "a warm Filipino home kitchen",
                action:
                    "Jowa-an walks into the kitchen and notices something missing",
                emotion:
                    "shocked and concerned",
                camera:
                    "medium shot with a subtle push-in",
                lighting:
                    "warm morning light",
                visualStyle:
                    "realistic cinematic Filipino drama",
                characters:
                [
                    "Jowa-an"
                ]);

        var request =
            new ShotGenerationRequest(
                promptRequest,
                "characters/jowa-an/face-01.jpg");

        var result =
            await shotGenerator.GenerateAsync(request);

        Assert.True(result.Succeeded);

        Assert.NotNull(result.ImageResult);

        Assert.True(
            result.ImageResult.Succeeded);

        Assert.Equal(
            "mock://generated/image-001.png",
            result.ImageResult.OutputLocation);

        Assert.Contains(
            "Jowa-an",
            result.Prompt);

        Assert.Contains(
            "warm Filipino home kitchen",
            result.Prompt);

        Assert.NotNull(
            imageGenerator.LastRequest);

        Assert.Equal(
            result.Prompt,
            imageGenerator.LastRequest!.Prompt);

        Assert.Equal(
            "characters/jowa-an/face-01.jpg",
            imageGenerator.LastRequest.ReferenceImageLocation);
    }

    [Fact]
    public async Task GenerateAsync_ReturnsFailureWhenImageGenerationFails()
    {
        var promptBuilder =
            new CinematicPromptBuilder();

        var imageGenerator =
            new MockImageGenerator
            {
                ShouldFail = true
            };

        var shotGenerator =
            new ShotGenerator(
                promptBuilder,
                imageGenerator);

        var promptRequest =
            new CinematicPromptRequest(
                location: "kitchen",
                action:
                    "Jowa-an opens the cabinet",
                emotion:
                    "shocked",
                camera:
                    "close-up",
                lighting:
                    "warm lighting",
                visualStyle:
                    "cinematic comedy",
                characters:
                [
                    "Jowa-an"
                ]);

        var request =
            new ShotGenerationRequest(
                promptRequest);

        var result =
            await shotGenerator.GenerateAsync(request);

        Assert.False(result.Succeeded);

        Assert.False(
            result.ImageResult.Succeeded);

        Assert.Equal(
            "Mock image generation failed.",
            result.ImageResult.ErrorMessage);
    }

    [Fact]
    public async Task GenerateAsync_RequiresRequest()
    {
        var promptBuilder =
            new CinematicPromptBuilder();

        var imageGenerator =
            new MockImageGenerator();

        var shotGenerator =
            new ShotGenerator(
                promptBuilder,
                imageGenerator);

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => shotGenerator.GenerateAsync(null!));
    }
}
