using NoodKa.Application.Prompts;

namespace NoodKa.Application.Tests.Prompts;

public sealed class CinematicPromptBuilderTests
{
    [Fact]
    public void Build_CreatesCinematicPrompt()
    {
        var builder =
            new CinematicPromptBuilder();

        var request =
            new CinematicPromptRequest(
                location: "a warm Filipino home kitchen",
                action: "Jowa-an opens a cabinet and notices something missing",
                emotion: "shocked and concerned",
                camera: "medium close-up with a slow push-in",
                lighting: "warm morning light",
                visualStyle: "realistic Filipino drama",
                characters:
                [
                    "Jowa-an"
                ]);

        var result =
            builder.Build(request);

        Assert.Contains(
            "Jowa-an",
            result.Prompt);

        Assert.Contains(
            "warm Filipino home kitchen",
            result.Prompt);

        Assert.Contains(
            "opens a cabinet",
            result.Prompt);

        Assert.Contains(
            "shocked and concerned",
            result.Prompt);

        Assert.Contains(
            "slow push-in",
            result.Prompt);

        Assert.Contains(
            "warm morning light",
            result.Prompt);
    }

    [Fact]
    public void Build_SupportsMultipleCharacters()
    {
        var builder =
            new CinematicPromptBuilder();

        var request =
            new CinematicPromptRequest(
                location: "living room",
                action: "Toffey looks toward Jowa-an",
                emotion: "confused",
                camera: "two-shot",
                lighting: "soft indoor lighting",
                visualStyle: "cinematic comedy",
                characters:
                [
                    "Toffey",
                    "Jowa-an"
                ]);

        var result =
            builder.Build(request);

        Assert.Contains(
            "Toffey and Jowa-an",
            result.Prompt);
    }

    [Fact]
    public void Build_RejectsMissingLocation()
    {
        Assert.Throws<ArgumentException>(
            () => new CinematicPromptRequest(
                location: "",
                action: "opens the cabinet",
                emotion: "shocked",
                camera: "close-up",
                lighting: "warm",
                visualStyle: "cinematic"));
    }

    [Fact]
    public void Build_RejectsMissingAction()
    {
        Assert.Throws<ArgumentException>(
            () => new CinematicPromptRequest(
                location: "kitchen",
                action: "",
                emotion: "shocked",
                camera: "close-up",
                lighting: "warm",
                visualStyle: "cinematic"));
    }
}