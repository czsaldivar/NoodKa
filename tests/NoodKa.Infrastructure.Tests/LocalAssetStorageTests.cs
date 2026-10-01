using NoodKa.Infrastructure.Assets;

namespace NoodKa.Infrastructure.Tests;

public sealed class LocalAssetStorageTests : IDisposable
{
    private readonly string _rootPath;
    private readonly LocalAssetStorage _storage;

    public LocalAssetStorageTests()
    {
        _rootPath = Path.Combine(
            Path.GetTempPath(),
            "NoodKa-AssetTests",
            Guid.NewGuid().ToString("N"));

        _storage = new LocalAssetStorage(_rootPath);
    }

    [Fact]
    public async Task SaveAsync_StoresFileAndReturnsFullPath()
    {
        var content = new MemoryStream(
            "NoodKa test image"u8.ToArray());

        var path = await _storage.SaveAsync(
            "generated-images/test.png",
            content);

        Assert.True(File.Exists(path));
        Assert.Equal(
            Path.GetFullPath(Path.Combine(
                _rootPath,
                "generated-images",
                "test.png")),
            path);

        Assert.Equal(
            "NoodKa test image",
            await File.ReadAllTextAsync(path));
    }

    [Fact]
    public async Task OpenReadAsync_ReturnsSavedContent()
    {
        const string expected = "NoodKa asset content";

        await using var input = new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(expected));

        await _storage.SaveAsync("test.txt", input);

        await using var output =
            await _storage.OpenReadAsync("test.txt");

        Assert.NotNull(output);

        using var reader = new StreamReader(output!);
        Assert.Equal(expected, await reader.ReadToEndAsync());
    }

    [Fact]
    public async Task ExistsAsync_ReportsWhetherFileExists()
    {
        Assert.False(await _storage.ExistsAsync("test.txt"));

        await using var content = new MemoryStream(
            "content"u8.ToArray());

        await _storage.SaveAsync("test.txt", content);

        Assert.True(await _storage.ExistsAsync("test.txt"));
    }

    [Fact]
    public async Task DeleteAsync_RemovesExistingFile()
    {
        await using var content = new MemoryStream(
            "content"u8.ToArray());

        await _storage.SaveAsync("test.txt", content);

        await _storage.DeleteAsync("test.txt");

        Assert.False(await _storage.ExistsAsync("test.txt"));
    }

    [Fact]
    public async Task OpenReadAsync_ReturnsNullForMissingFile()
    {
        var result = await _storage.OpenReadAsync("missing.txt");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("../outside.txt")]
    [InlineData("..\\outside.txt")]
    [InlineData("nested/../../outside.txt")]
    public async Task SaveAsync_RejectsPathsOutsideAssetRoot(
        string relativePath)
    {
        await using var content = new MemoryStream(
            "content"u8.ToArray());

        await Assert.ThrowsAsync<ArgumentException>(
            () => _storage.SaveAsync(relativePath, content));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootPath))
        {
            Directory.Delete(_rootPath, recursive: true);
        }
    }
}
