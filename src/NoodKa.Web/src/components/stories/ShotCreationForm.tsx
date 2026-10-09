import type { ShotCreationFormProps } from './shotCreationFormTypes';

export default function ShotCreationForm({
  sceneId,
  shotDurations,
  shotActions,
  shotEmotions,
  shotCameras,
  shotLighting,
  setShotDurations,
  setShotActions,
  setShotEmotions,
  setShotCameras,
  setShotLighting,
  creatingShotId,
  shotErrors,
  shotSuccesses,
  createShot,
}: ShotCreationFormProps) {
  return (
    <form
      className="story-form"
      onSubmit={(event) => createShot(event, sceneId)}
      style={{
        marginTop: '12px',
        marginBottom: '14px',
        padding: '12px',
        border: '1px solid var(--border)',
        borderRadius: '10px',
        display: 'grid',
        gap: '10px',
      }}
    >
      <strong>Add a shot</strong>

      <label htmlFor={`shot-duration-${sceneId}`}>
        Duration (seconds)
      </label>
      <input
        id={`shot-duration-${sceneId}`}
        type="number"
        min="0.1"
        step="0.1"
        required
        value={shotDurations[sceneId] ?? '5'}
        onChange={(event) =>
          setShotDurations((current) => ({
            ...current,
            [sceneId]: event.target.value,
          }))
        }
      />

      <label htmlFor={`shot-action-${sceneId}`}>
        Shot action
      </label>
      <textarea
        id={`shot-action-${sceneId}`}
        required
        rows={2}
        placeholder="What happens in this shot?"
        value={shotActions[sceneId] ?? ''}
        onChange={(event) =>
          setShotActions((current) => ({
            ...current,
            [sceneId]: event.target.value,
          }))
        }
      />

      <label htmlFor={`shot-emotion-${sceneId}`}>
        Emotion (optional)
      </label>
      <input
        id={`shot-emotion-${sceneId}`}
        type="text"
        placeholder="e.g. anxious, hopeful"
        value={shotEmotions[sceneId] ?? ''}
        onChange={(event) =>
          setShotEmotions((current) => ({
            ...current,
            [sceneId]: event.target.value,
          }))
        }
      />

      <label htmlFor={`shot-camera-${sceneId}`}>
        Camera (optional)
      </label>
      <input
        id={`shot-camera-${sceneId}`}
        type="text"
        placeholder="e.g. close-up, slow dolly-in"
        value={shotCameras[sceneId] ?? ''}
        onChange={(event) =>
          setShotCameras((current) => ({
            ...current,
            [sceneId]: event.target.value,
          }))
        }
      />

      <label htmlFor={`shot-lighting-${sceneId}`}>
        Lighting (optional)
      </label>
      <input
        id={`shot-lighting-${sceneId}`}
        type="text"
        placeholder="e.g. warm practical light"
        value={shotLighting[sceneId] ?? ''}
        onChange={(event) =>
          setShotLighting((current) => ({
            ...current,
            [sceneId]: event.target.value,
          }))
        }
      />

      <button
        type="submit"
        className="story-submit"
        disabled={
          creatingShotId !== null ||
          !(shotActions[sceneId] ?? '').trim() ||
          !(Number(shotDurations[sceneId] ?? '5') > 0)
        }
      >
        {creatingShotId === sceneId ? 'Saving shot...' : 'Save Shot'}
      </button>

      {shotErrors[sceneId] && (
        <p className="notice error-notice" role="alert">
          {shotErrors[sceneId]}
        </p>
      )}
      {shotSuccesses[sceneId] && (
        <p className="notice" role="status">
          {shotSuccesses[sceneId]}
        </p>
      )}
    </form>
  );
}
