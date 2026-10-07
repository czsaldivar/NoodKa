using NoodKa.Domain.Characters;

namespace NoodKa.Application.Characters;

public interface ICharacterRepository
{
    Task AddAsync(
        Character character,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Character>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Character?> GetByIdAsync(
        Guid characterId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Character character,
        CancellationToken cancellationToken = default);
}