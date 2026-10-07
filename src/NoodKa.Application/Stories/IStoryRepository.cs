using NoodKa.Domain.Stories;

namespace NoodKa.Application.Stories;

public interface IStoryRepository
{
    Task AddAsync(
        Story story,
        CancellationToken cancellationToken = default);

    Task<Episode?> CreateEpisodeAsync(
        Guid storyId,
        string title,
        CancellationToken cancellationToken = default);

    Task<Shot?> CreateShotAsync(
        Guid sceneId,
        TimeSpan duration,
        string action,
        string emotion,
        string camera,
        string lighting,
        CancellationToken cancellationToken = default);

    Task<Scene?> CreateSceneAsync(
        Guid episodeId,
        string name,
        string location,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Story>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
