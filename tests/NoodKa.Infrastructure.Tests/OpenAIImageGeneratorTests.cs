using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using NoodKa.Application.AI.Images;
using NoodKa.Application.Assets;
using NoodKa.Infrastructure.AI.OpenAI;

namespace NoodKa.Infrastructure.Tests;

public sealed class OpenAIImageGeneratorTests
{
    private const string TestApiKey = "test-key-not-a-real-secret";

    [Fact]
    public async Task GenerateAsync_WithReferenceImage_SendsEditRequestAndStoresImage()
    {
        var generatedBytes = "generated-image-bytes"u8.ToArray();
        var handler = new StubHttpMessageHandler(async request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(
                "https://api.openai.com/v1/images/edits",
                request.RequestUri!.ToString());
            Assert.Equal(
                "Bearer",
                request.Headers.Authorization!.Scheme);
            Assert.Equal(TestApiKey, request.Headers.Authorization.Parameter);

            var multipart = Assert.IsType<MultipartFormDataContent>(
                request.Content);

            Assert.Equal("gpt-image-2", await ReadFieldAsync(multipart, "model"));
            Assert.Equal("cinematic portrait", await ReadFieldAsync(multipart, "prompt"));
            Assert.Equal("1024x1024", await ReadFieldAsync(multipart, "size"));
            Assert.Equal("png", await ReadFieldAsync(multipart, "output_format"));

            var imagePart = multipart.FirstOrDefault(part =>
                part.Headers.ContentDisposition?.Name?.Trim('"') == "image[]");

            Assert.NotNull(imagePart);
            Assert.Equal(
                "face.png",
                imagePart!.Headers.ContentDisposition!.FileName!.Trim('"'));
            Assert.Equal(
                "image/png",
                imagePart.Headers.ContentType!.MediaType);

            var uploadedBytes = await imagePart.ReadAsByteArrayAsync();
            Assert.Equal("face-reference"u8.ToArray(), uploadedBytes);

            var base64 = Convert.ToBase64String(generatedBytes);
            return JsonResponse(
                HttpStatusCode.OK,
                $$"""{"data":[{"b64_json":"{{base64}}"}]}""");
        });

        var storage = new FakeAssetStorage();
        using var httpClient = new HttpClient(handler);
        var generator = CreateGenerator(storage, httpClient);

        var result = await generator.GenerateAsync(
            new ImageGenerationRequest(
                "cinematic portrait",
                referenceImageBytes: "face-reference"u8.ToArray(),
                referenceImageFileName: "face.png"));

        Assert.True(result.Succeeded, result.ErrorMessage);
        Assert.NotNull(result.StorageKey);
        Assert.StartsWith("generated-images/", result.StorageKey);
        Assert.True(storage.SavedContent.TryGetValue(
            result.StorageKey!, out var savedBytes));
        Assert.Equal(generatedBytes, savedBytes);
        Assert.Equal(1, handler.RequestCount);
    }

    [Fact]
    public async Task GenerateAsync_WhenApiReturnsError_ReturnsFailure()
    {
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(JsonResponse(
                HttpStatusCode.BadRequest,
                """{"error":{"message":"Invalid reference image"}}""")));

        var storage = new FakeAssetStorage();
        using var httpClient = new HttpClient(handler);
        var generator = CreateGenerator(storage, httpClient);

        var result = await generator.GenerateAsync(
            new ImageGenerationRequest(
                "cinematic portrait",
                referenceImageBytes: "face-reference"u8.ToArray(),
                referenceImageFileName: "face.png"));

        Assert.False(result.Succeeded);
        Assert.Contains("Invalid reference image", result.ErrorMessage);
        Assert.Empty(storage.SavedContent);
    }

    [Fact]
    public async Task GenerateAsync_WhenResponseHasNoImage_ReturnsFailure()
    {
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(JsonResponse(
                HttpStatusCode.OK,
                """{"data":[]}""")));

        var storage = new FakeAssetStorage();
        using var httpClient = new HttpClient(handler);
        var generator = CreateGenerator(storage, httpClient);

        var result = await generator.GenerateAsync(
            new ImageGenerationRequest(
                "cinematic portrait",
                referenceImageBytes: "face-reference"u8.ToArray(),
                referenceImageFileName: "face.png"));

        Assert.False(result.Succeeded);
        Assert.Contains("did not contain image data", result.ErrorMessage);
        Assert.Empty(storage.SavedContent);
    }

    [Fact]
    public async Task GenerateAsync_WithUnsupportedReferenceExtension_ReturnsFailure()
    {
        var handler = new StubHttpMessageHandler(_ =>
            throw new InvalidOperationException(
                "HTTP should not be called for an unsupported extension."));

        var storage = new FakeAssetStorage();
        using var httpClient = new HttpClient(handler);
        var generator = CreateGenerator(storage, httpClient);

        var result = await generator.GenerateAsync(
            new ImageGenerationRequest(
                "cinematic portrait",
                referenceImageBytes: "face-reference"u8.ToArray(),
                referenceImageFileName: "face.gif"));

        Assert.False(result.Succeeded);
        Assert.Contains("PNG or JPEG", result.ErrorMessage);
        Assert.Equal(0, handler.RequestCount);
    }

    private static OpenAIImageGenerator CreateGenerator(
        IAssetStorage storage,
        HttpClient httpClient)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = TestApiKey
            })
            .Build();

        return new OpenAIImageGenerator(
            configuration,
            storage,
            httpClient);
    }

    private static async Task<string?> ReadFieldAsync(
        MultipartFormDataContent multipart,
        string name)
    {
        var part = multipart.FirstOrDefault(content =>
            content.Headers.ContentDisposition?.Name?.Trim('"') == name);

        return part is null ? null : await part.ReadAsStringAsync();
    }

    private static HttpResponseMessage JsonResponse(
        HttpStatusCode statusCode,
        string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json")
        };
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory)
        : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            return responseFactory(request);
        }
    }

    private sealed class FakeAssetStorage : IAssetStorage
    {
        public Dictionary<string, byte[]> SavedContent { get; } = new();

        public async Task<string> SaveAsync(
            string relativePath,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            using var memory = new MemoryStream();
            await content.CopyToAsync(memory, cancellationToken);
            SavedContent[relativePath] = memory.ToArray();
            return Path.Combine(Path.GetTempPath(), relativePath);
        }

        public Task<Stream?> OpenReadAsync(
            string relativePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!SavedContent.TryGetValue(relativePath, out var bytes))
                return Task.FromResult<Stream?>(null);

            return Task.FromResult<Stream?>(
                new MemoryStream(bytes, writable: false));
        }

        public Task<bool> ExistsAsync(
            string relativePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(SavedContent.ContainsKey(relativePath));
        }

        public Task DeleteAsync(
            string relativePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SavedContent.Remove(relativePath);
            return Task.CompletedTask;
        }
    }
}
