namespace NoodKa.Application.Assets;

public interface IAssetStorage
{
    Task<string> SaveAsync(
        string relativePath,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        string relativePath,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string relativePath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken = default);
}
