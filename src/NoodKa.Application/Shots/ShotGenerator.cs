using NoodKa.Application.AI.Images;
using NoodKa.Application.Prompts;

namespace NoodKa.Application.Shots;

public sealed class ShotGenerator : IShotGenerator
{
    private readonly ICinematicPromptBuilder _promptBuilder;
    private readonly IImageGenerator _imageGenerator;

    public ShotGenerator(
        ICinematicPromptBuilder promptBuilder,
        IImageGenerator imageGenerator)
    {
        ArgumentNullException.ThrowIfNull(promptBuilder);
        ArgumentNullException.ThrowIfNull(imageGenerator);

        _promptBuilder = promptBuilder;
        _imageGenerator = imageGenerator;
    }

    public async Task<ShotGenerationResult> GenerateAsync(
        ShotGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var promptResult =
            _promptBuilder.Build(
                request.PromptRequest);

        var imageRequest =
            new ImageGenerationRequest(
                promptResult.Prompt,
                request.ReferenceImageLocation);

        var imageResult =
            await _imageGenerator.GenerateAsync(
                imageRequest,
                cancellationToken);

        if (!imageResult.Succeeded)
        {
            return ShotGenerationResult.Failure(
                promptResult.Prompt,
                imageResult);
        }

        return ShotGenerationResult.Success(
            promptResult.Prompt,
            imageResult);
    }
}