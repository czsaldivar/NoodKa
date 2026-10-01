using NoodKa.Application.Assets;

namespace NoodKa.Infrastructure.Assets;

public sealed class LocalAssetStorage : IAssetStorage
{
    private readonly string _rootPath;

    public LocalAssetStorage(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentException(
                "Root path is required.",
                nameof(rootPath));

        _rootPath = Path.GetFullPath(rootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(
        string relativePath,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var filePath = GetSafePath(relativePath);
        var directory = Path.GetDirectoryName(filePath)!;

        Directory.CreateDirectory(directory);

        await using var fileStream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await content.CopyToAsync(fileStream, cancellationToken);

        return filePath;
    }

    public Task<Stream?> OpenReadAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var filePath = GetSafePath(relativePath);

        if (!File.Exists(filePath))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            useAsync: true);

        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> ExistsAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(
            File.Exists(GetSafePath(relativePath)));
    }

    public Task DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var filePath = GetSafePath(relativePath);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private string GetSafePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException(
                "Relative path is required.",
                nameof(relativePath));

        if (Path.IsPathRooted(relativePath))
            throw new ArgumentException(
                "Rooted paths are not allowed.",
                nameof(relativePath));

        var normalizedPath = relativePath.Replace(
            Path.AltDirectorySeparatorChar,
            Path.DirectorySeparatorChar);

        var fullPath = Path.GetFullPath(
            Path.Combine(_rootPath, normalizedPath));

        var relativeToRoot = Path.GetRelativePath(
            _rootPath,
            fullPath);

        if (relativeToRoot == ".." ||
            relativeToRoot.StartsWith(
                ".." + Path.DirectorySeparatorChar,
                StringComparison.Ordinal) ||
            Path.IsPathRooted(relativeToRoot))
        {
            throw new ArgumentException(
                "Path must remain inside the asset root.",
                nameof(relativePath));
        }

        return fullPath;
    }
}
