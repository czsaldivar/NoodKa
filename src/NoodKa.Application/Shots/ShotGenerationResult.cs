using NoodKa.Application.AI.Images;

namespace NoodKa.Application.Shots;

public sealed class ShotGenerationResult
{
    public bool Succeeded { get; }

    public string Prompt { get; }

    public ImageGenerationResult ImageResult { get; }

    private ShotGenerationResult(
        bool succeeded,
        string prompt,
        ImageGenerationResult imageResult)
    {
        Succeeded = succeeded;
        Prompt = prompt;
        ImageResult = imageResult;
    }

    public static ShotGenerationResult Success(
        string prompt,
        ImageGenerationResult imageResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        ArgumentNullException.ThrowIfNull(imageResult);

        return new ShotGenerationResult(
            true,
            prompt,
            imageResult);
    }

    public static ShotGenerationResult Failure(
        string prompt,
        ImageGenerationResult imageResult)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);
        ArgumentNullException.ThrowIfNull(imageResult);

        return new ShotGenerationResult(
            false,
            prompt,
            imageResult);
    }
}