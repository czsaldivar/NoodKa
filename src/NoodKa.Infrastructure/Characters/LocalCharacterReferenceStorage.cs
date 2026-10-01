using NoodKa.Application.Characters;

namespace NoodKa.Infrastructure.Characters;

public sealed class LocalCharacterReferenceStorage
    : ICharacterReferenceStorage
{
    private readonly string _rootPath;

    public LocalCharacterReferenceStorage(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentException(
                "Root path is required.",
                nameof(rootPath));

        _rootPath = Path.GetFullPath(rootPath);

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(
        Guid characterId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        ValidateFileName(fileName);

        var characterDirectory =
            GetCharacterDirectory(characterId);

        Directory.CreateDirectory(characterDirectory);

        var filePath =
            Path.Combine(characterDirectory, fileName);

        await using var fileStream =
            new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

        await content.CopyToAsync(
            fileStream,
            cancellationToken);

        return filePath;
    }

    public Task<Stream?> OpenReadAsync(
        Guid characterId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ValidateFileName(fileName);

        var filePath =
            Path.Combine(
                GetCharacterDirectory(characterId),
                fileName);

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
        Guid characterId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ValidateFileName(fileName);

        var filePath =
            Path.Combine(
                GetCharacterDirectory(characterId),
                fileName);

        return Task.FromResult(
            File.Exists(filePath));
    }

    public Task DeleteAsync(
        Guid characterId,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ValidateFileName(fileName);

        var filePath =
            Path.Combine(
                GetCharacterDirectory(characterId),
                fileName);

        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    private string GetCharacterDirectory(Guid characterId)
    {
        if (characterId == Guid.Empty)
            throw new ArgumentException(
                "Character ID cannot be empty.",
                nameof(characterId));

        return Path.Combine(
            _rootPath,
            "characters",
            characterId.ToString("N"));
    }

    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException(
                "File name is required.",
                nameof(fileName));

        if (fileName != Path.GetFileName(fileName))
            throw new ArgumentException(
                "File name must not contain directory separators.",
                nameof(fileName));
    }
}