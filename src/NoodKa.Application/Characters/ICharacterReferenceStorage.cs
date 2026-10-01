namespace NoodKa.Application.Characters;

public interface ICharacterReferenceStorage
{
    Task<string> SaveAsync(
        Guid characterId,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        Guid characterId,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid characterId,
        string fileName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid characterId,
        string fileName,
        CancellationToken cancellationToken = default);
}