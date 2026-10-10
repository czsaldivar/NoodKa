import type { ShotCardProps } from './shotCardTypes';

export default function ShotCard({
  shot,
  highlighted,
  generatingImageShotId,
  shotImages,
  imageErrors,
  imagePrompts,
  charactersForShotGeneration,
  shotCharacterIds,
  selectShotCharacter,
  generateShotImage,
  goToShotAsset,
}: ShotCardProps) {
  return (
    <li
      id={`shot-${shot.id}`}
      className={highlighted ? 'story-shot-target' : undefined}
      style={{ marginTop: '8px', scrollMarginTop: '100px' }}
    >
      <strong>Shot {shot.sequence}</strong>
      <span aria-hidden="true">&mdash;</span>

      <div className="story-panel-description">
        {[
          shot.emotion && `Emotion: ${shot.emotion}`,
          shot.camera && `Camera: ${shot.camera}`,
          shot.lighting && `Lighting: ${shot.lighting}`,
        ].filter(Boolean).join(' | ')}
      </div>

      <div
        style={{
          display: 'flex',
          flexWrap: 'wrap',
          gap: '10px',
          alignItems: 'center',
          marginTop: '10px',
        }}
      >
        <fieldset className="shot-character-picker" disabled={generatingImageShotId !== null}>
          <legend>Characters in this shot</legend>
          <p className="shot-character-help">
            Select every character who appears in this scene. Leave all unchecked for text-only generation.
          </p>
          {charactersForShotGeneration.length === 0 ? (
            <p className="shot-character-help">Upload a face reference for a character to make them available here.</p>
          ) : (
            <div className="shot-character-options">
              {charactersForShotGeneration.map((character) => {
                const selectedIds = shotCharacterIds[shot.id] ?? []
                const checked = selectedIds.includes(character.id)
                return (
                  <label className="shot-character-option" key={character.id}>
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={(event) => {
                        const nextIds = event.target.checked
                          ? [...selectedIds, character.id]
                          : selectedIds.filter((id) => id !== character.id)
                        selectShotCharacter(shot.id, nextIds)
                      }}
                    />
                    <span>{character.name}</span>
                  </label>
                )
              })}
            </div>
          )}
          <p className="shot-character-help">
            Selected: {(shotCharacterIds[shot.id] ?? []).length}
          </p>
        </fieldset>

        <button
          type="button"
          className="story-submit"
          disabled={generatingImageShotId !== null}
          onClick={() => generateShotImage(shot.id)}
        >
          {generatingImageShotId === shot.id
            ? 'Generating image...'
            : shotImages[shot.id]
              ? 'Regenerate Image'
              : 'Generate Image'}
        </button>

        {shotImages[shot.id] && (
          <a
            className="primary-button"
            href={shotImages[shot.id]}
            target="_blank"
            rel="noreferrer"
          >
            Open image
          </a>
        )}

        {shotImages[shot.id] && (
          <button
            type="button"
            className="secondary-button"
            onClick={() => goToShotAsset(shot.id)}
          >
            View in Asset Library
          </button>
        )}
      </div>

      {imageErrors[shot.id] && (
        <p className="notice error-notice" role="alert">
          {imageErrors[shot.id]}
        </p>
      )}

      {shotImages[shot.id] && (
        <div style={{ marginTop: '12px', maxWidth: '560px' }}>
          <img
            src={shotImages[shot.id]}
            alt={`Generated image for shot ${shot.sequence}`}
            style={{
              display: 'block',
              width: '100%',
              height: 'auto',
              borderRadius: '6px',
              border: '1px solid var(--border)',
            }}
          />

          {imagePrompts[shot.id] && (
            <details style={{ marginTop: '8px' }}>
              <summary>View cinematic prompt</summary>
              <p className="story-panel-description">
                {imagePrompts[shot.id]}
              </p>
            </details>
          )}
        </div>
      )}
    </li>
  );
}

