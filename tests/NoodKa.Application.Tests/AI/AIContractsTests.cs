using NoodKa.Application.AI.Images;
using NoodKa.Application.AI.Video;
using NoodKa.Application.AI.Voice;

namespace NoodKa.Application.Tests.AI;

public sealed class AIContractsTests
{
    [Fact]
    public void ImageRequest_CreatesValidRequest()
    {
        var request = new ImageGenerationRequest(
            "Toffey standing in a kitchen.",
            "characters/toffey/face-01.jpg");

        Assert.Equal(
            "Toffey standing in a kitchen.",
            request.Prompt);

        Assert.Equal(
            "characters/toffey/face-01.jpg",
            request.ReferenceImageLocation);

        Assert.Equal(1024, request.Width);
        Assert.Equal(1024, request.Height);
    }

    [Fact]
    public void ImageResult_CanRepresentSuccess()
    {
        var result =
            ImageGenerationResult.Success(
                "generated/toffey-kitchen.png");

        Assert.True(result.Succeeded);
        Assert.Equal(
            "generated/toffey-kitchen.png",
            result.OutputLocation);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void VideoRequest_DefaultsToVerticalFormat()
    {
        var request = new VideoGenerationRequest(
            "Toffey slowly enters the kitchen.",
            "generated/toffey-kitchen.png",
            TimeSpan.FromSeconds(4));

        Assert.Equal(1080, request.Width);
        Assert.Equal(1920, request.Height);
        Assert.Equal(
            TimeSpan.FromSeconds(4),
            request.Duration);
    }

    [Fact]
    public void VideoResult_CanRepresentFailure()
    {
        var result =
            VideoGenerationResult.Failure(
                "Provider unavailable.");

        Assert.False(result.Succeeded);
        Assert.Null(result.OutputLocation);
        Assert.Equal(
            "Provider unavailable.",
            result.ErrorMessage);
    }

    [Fact]
    public void VoiceRequest_CanSpecifyEmotion()
    {
        var request = new VoiceGenerationRequest(
            "Toffey...",
            "jowa-an",
            "concerned");

        Assert.Equal("Toffey...", request.Text);
        Assert.Equal("jowa-an", request.VoiceId);
        Assert.Equal("concerned", request.Emotion);
    }

    [Fact]
    public void VoiceResult_CanRepresentSuccess()
    {
        var result =
            VoiceGenerationResult.Success(
                "audio/jowa-an-01.wav");

        Assert.True(result.Succeeded);
        Assert.Equal(
            "audio/jowa-an-01.wav",
            result.OutputLocation);
    }
}