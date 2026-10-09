import type { SceneCreationFormProps } from './sceneCreationFormTypes';

export default function SceneCreationForm({
  episodeId,
  sceneName,
  sceneLocation,
  setSceneNames,
  setSceneLocations,
  creatingSceneId,
  sceneError,
  sceneSuccess,
  createScene,
}: SceneCreationFormProps) {
  return (
    <form
      className="story-form"
      onSubmit={(event) => createScene(event, episodeId)}
      style={{ marginBottom: '18px' }}
    >
      <label htmlFor={`scene-name-${episodeId}`}>
        New scene name
      </label>
      <input
        id={`scene-name-${episodeId}`}
        type="text"
        value={sceneName}
        onChange={(event) =>
          setSceneNames((current) => ({
            ...current,
            [episodeId]: event.target.value,
          }))
        }
        placeholder="e.g. The Letter"
        maxLength={200}
        required
      />

      <label htmlFor={`scene-location-${episodeId}`}>
        New scene location
      </label>
      <input
        id={`scene-location-${episodeId}`}
        type="text"
        value={sceneLocation}
        onChange={(event) =>
          setSceneLocations((current) => ({
            ...current,
            [episodeId]: event.target.value,
          }))
        }
        placeholder="e.g. INT. KITCHEN - NIGHT"
        maxLength={200}
        required
      />

      <button
        className="story-submit"
        type="submit"
        disabled={
          !sceneName.trim() ||
          !sceneLocation.trim() ||
          creatingSceneId !== null
        }
      >
        {creatingSceneId === episodeId
          ? 'Saving Scene...'
          : 'Create Scene'}
      </button>

      {sceneError && (
        <p className="notice error-notice" role="alert">
          {sceneError}
        </p>
      )}

      {sceneSuccess && (
        <p className="notice" role="status">
          {sceneSuccess}
        </p>
      )}
    </form>
  );
}
