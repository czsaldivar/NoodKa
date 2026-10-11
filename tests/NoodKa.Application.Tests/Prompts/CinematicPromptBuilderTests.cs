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
    public void Build_IncludesCharacterVisualDescriptionWhenProvided()
    {
        var builder = new CinematicPromptBuilder();

        var request = new CinematicPromptRequest(
            location: "a Filipino home kitchen",
            action: "opens a cabinet",
            emotion: "concerned",
            camera: "medium close-up",
            lighting: "warm morning light",
            visualStyle: "realistic Filipino drama",
            characters: ["Jowa-an"],
            characterVisualDescription: "Appearance: oval face; Hair: long black hair; Typical clothing: blue blouse");

        var result = builder.Build(request);

        Assert.Contains("Jowa-an", result.Prompt);
        Assert.Contains("Character appearance reference", result.Prompt);
        Assert.Contains("long black hair", result.Prompt);
        Assert.Contains("blue blouse", result.Prompt);
    }

    [Fact]
    public void Build_EmphasizesIdentityPreservationForMultipleCharacters()
    {
        var builder = new CinematicPromptBuilder();

        var request = new CinematicPromptRequest(
            location: "BYU-Hawaii campus",
            action: "Tinn calls Zhay-Zhay",
            emotion: "happy and homesick",
            camera: "medium close-up",
            lighting: "soft late afternoon light",
            visualStyle: "photorealistic cinematic film still",
            characters: ["Tinn", "Zhay-Zhay"],
            characterVisualDescription: "Tinn: long dark hair; Zhay-Zhay: shoulder-length dark hair");

        var result = builder.Build(request);

        Assert.Contains("Tinn and Zhay-Zhay", result.Prompt);
        Assert.Contains("Identity preservation", result.Prompt);
        Assert.Contains("supplied character reference images", result.Prompt);
        Assert.Contains("Keep the characters visually distinct", result.Prompt);
        Assert.Contains("Do not merge, swap", result.Prompt);
        Assert.Contains("Tinn: long dark hair", result.Prompt);
    }

    [Fact]
    public void Build_PreservesRequestedPhoneScreenComposition()
    {
        var builder = new CinematicPromptBuilder();

        var request = new CinematicPromptRequest(
            location: "BYU-Hawaii campus bench",
            action: "Tinn talks to Zhay-Zhay, who appears on the phone screen",
            emotion: "joyful and affectionate",
            camera: "medium close-up with the phone screen visible",
            lighting: "soft late afternoon light",
            visualStyle: "photorealistic cinematic film still",
            characters: ["Tinn", "Zhay-Zhay"]);

        var result = builder.Build(request);

        Assert.Contains("requested placement", result.Prompt);
        Assert.Contains("phone screen", result.Prompt);
        Assert.Contains("within that surface", result.Prompt);
    }
    [Fact]
    public void Build_EmphasizesShotContinuityWithoutBlockingExplicitChanges()
    {
        var builder = new CinematicPromptBuilder();

        var request = new CinematicPromptRequest(
            location: "a warm Filipino home kitchen",
            action: "The sisters continue preparing snacks in the same scene",
            emotion: "happy and relaxed",
            camera: "medium two-shot",
            lighting: "warm morning light",
            visualStyle: "photorealistic cinematic film still",
            characters: ["Tinn", "Zhay-Zhay"]);

        var result = builder.Build(request);

        Assert.Contains("Shot continuity", result.Prompt);
        Assert.Contains("same continuous scene and time", result.Prompt);
        Assert.Contains("clothing, accessories, hairstyle", result.Prompt);
        Assert.Contains("environmental details", result.Prompt);
        Assert.Contains("explicitly requested changes", result.Prompt);
        Assert.Contains("rather than forcing continuity", result.Prompt);
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
