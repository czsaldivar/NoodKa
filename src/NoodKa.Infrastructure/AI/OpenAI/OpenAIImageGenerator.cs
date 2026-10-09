using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NoodKa.Application.AI.Images;
using NoodKa.Application.Assets;
using OpenAI.Images;

namespace NoodKa.Infrastructure.AI.OpenAI;

public sealed class OpenAIImageGenerator : IImageGenerator
{
    private readonly ImageClient _client;
    private readonly IAssetStorage _assetStorage;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAIImageGenerator(
        IConfiguration configuration,
        IAssetStorage assetStorage,
        HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(assetStorage);
        ArgumentNullException.ThrowIfNull(httpClient);

        _apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException(
                "OpenAI API key is not configured.");

        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException(
                "OpenAI API key is not configured.");

        _client = new ImageClient(
            model: "gpt-image-2",
            apiKey: _apiKey);

        _assetStorage = assetStorage;
        _httpClient = httpClient;
    }

    public async Task<ImageGenerationResult> GenerateAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            if (request.Width != 1024 || request.Height != 1024)
            {
                return ImageGenerationResult.Failure(
                    $"Round 7 currently supports only 1024x1024 images. " +
                    $"Requested: {request.Width}x{request.Height}");
            }

            byte[]? generatedBytes;

            if (request.ReferenceImageBytes is not null)
            {
                if (string.IsNullOrWhiteSpace(
                    request.ReferenceImageFileName))
                {
                    return ImageGenerationResult.Failure(
                        "A filename is required when a reference image is provided.");
                }

                generatedBytes = await GenerateFromReferenceAsync(
                    request,
                    cancellationToken);
            }
            else
            {
                var imageOptions = new ImageGenerationOptions
                {
                    Size = GeneratedImageSize.W1024xH1024
                };

                var result = await _client.GenerateImageAsync(
                    request.Prompt,
                    imageOptions,
                    cancellationToken);

                generatedBytes = result.Value.ImageBytes?.ToArray();
            }

            if (generatedBytes is not { Length: > 0 })
            {
                return ImageGenerationResult.Failure(
                    "OpenAI returned no image data.");
            }

            var relativePath =
                $"generated-images/{Guid.NewGuid():N}.png";

            using var imageStream = new MemoryStream(
                generatedBytes,
                writable: false);

            var outputPath = await _assetStorage.SaveAsync(
                relativePath,
                imageStream,
                cancellationToken);

            return ImageGenerationResult.Success(
                outputPath,
                relativePath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return ImageGenerationResult.Failure(
                $"Image generation or storage failed: {ex.Message}");
        }
    }

    private async Task<byte[]> GenerateFromReferenceAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(
            request.ReferenceImageFileName);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException(
                "The reference image filename is invalid.");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        var contentType = extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => throw new InvalidOperationException(
                "The reference image must be PNG or JPEG.")
        };

        using var form = new MultipartFormDataContent();

        form.Add(new StringContent("gpt-image-2"), "model");
        form.Add(new StringContent(request.Prompt), "prompt");
        form.Add(new StringContent("1024x1024"), "size");
        form.Add(new StringContent("png"), "output_format");

        var imageContent = new ByteArrayContent(
            request.ReferenceImageBytes!);

        imageContent.Headers.ContentType =
            new MediaTypeHeaderValue(contentType);

        form.Add(imageContent, "image[]", fileName);

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/images/edits")
        {
            Content = form
        };

        message.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"OpenAI image edit failed with HTTP " +
                $"{(int)response.StatusCode}: {GetErrorMessage(responseBody)}");
        }

        using var document = JsonDocument.Parse(responseBody);

        if (!document.RootElement.TryGetProperty(
                "data", out var data) ||
            data.ValueKind != JsonValueKind.Array ||
            data.GetArrayLength() == 0 ||
            !data[0].TryGetProperty("b64_json", out var encodedImage) ||
            encodedImage.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException(
                "OpenAI's image-edit response did not contain image data.");
        }

        var base64 = encodedImage.GetString();

        if (string.IsNullOrWhiteSpace(base64))
        {
            throw new InvalidOperationException(
                "OpenAI returned empty image data.");
        }

        try
        {
            return Convert.FromBase64String(base64);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                "OpenAI returned invalid base64 image data.", ex);
        }
    }

    private static string GetErrorMessage(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);

            if (document.RootElement.TryGetProperty(
                    "error", out var error) &&
                error.TryGetProperty("message", out var message) &&
                message.ValueKind == JsonValueKind.String)
            {
                return message.GetString() ?? "Unknown API error.";
            }
        }
        catch (JsonException)
        {
            // Use a bounded fallback for non-JSON error responses.
        }

        return string.IsNullOrWhiteSpace(responseBody)
            ? "The API returned an empty error response."
            : responseBody[..Math.Min(responseBody.Length, 500)];
    }
}
