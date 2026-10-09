import type { ComponentProps, Dispatch, SetStateAction } from 'react';
import type { StoryEpisode } from './storyTypes';
import SceneCreationForm from './SceneCreationForm';
import ShotCreationForm from './ShotCreationForm';
import ShotCard from './ShotCard';
import type { SceneCreationFormProps } from './sceneCreationFormTypes';
import type { ShotCreationFormProps } from './shotCreationFormTypes';

type EpisodeCardProps = {
  episode: StoryEpisode;
  sceneForm: {
    sceneNames: Record<string, string>;
    sceneLocations: Record<string, string>;
    setSceneNames: Dispatch<SetStateAction<Record<string, string>>>;
    setSceneLocations: Dispatch<SetStateAction<Record<string, string>>>;
    creatingSceneId: string | null;
    sceneErrors: Record<string, string>;
    sceneSuccesses: Record<string, string>;
    createScene: SceneCreationFormProps['createScene'];
  };
  shotForm: Omit<ShotCreationFormProps, 'sceneId'>;
  shotCard: Omit<ComponentProps<typeof ShotCard>, 'shot' | 'highlighted'> & {
    highlightedShotId: string | null;
  };
};

export default function EpisodeCard({
  episode,
  sceneForm,
  shotForm,
  shotCard,
}: EpisodeCardProps) {
  const { highlightedShotId, ...shotCardProps } = shotCard;

  return (
    <article
      style={{
        padding: '18px',
        border: '1px solid var(--border)',
        borderRadius: '4px',
      }}
    >
      <div className="story-panel-kicker">
        EPISODE {episode.number}
      </div>

      <h3 style={{ margin: '8px 0 14px', fontWeight: 500 }}>
        {episode.title || `Episode ${episode.number}`}
      </h3>

      <SceneCreationForm
        episodeId={episode.id}
        sceneName={sceneForm.sceneNames[episode.id] ?? ''}
        sceneLocation={sceneForm.sceneLocations[episode.id] ?? ''}
        setSceneNames={sceneForm.setSceneNames}
        setSceneLocations={sceneForm.setSceneLocations}
        creatingSceneId={sceneForm.creatingSceneId}
        sceneError={sceneForm.sceneErrors[episode.id]}
        sceneSuccess={sceneForm.sceneSuccesses[episode.id]}
        createScene={sceneForm.createScene}
      />

      {episode.scenes.length === 0 ? (
        <p className="story-panel-description">
          No scenes have been added to this episode yet.
        </p>
      ) : (
        <div style={{ display: 'grid', gap: '14px' }}>
          {episode.scenes.map((scene) => (
            <section
              key={scene.id}
              style={{
                paddingLeft: '14px',
                borderLeft: '2px solid var(--amber)',
              }}
            >
              <strong>
                Scene {scene.number}: {scene.name}
              </strong>

              <div
                className="story-panel-description"
                style={{ marginTop: '4px' }}
              >
                {scene.location}
              </div>

              <ShotCreationForm
                sceneId={scene.id}
                {...shotForm}
              />

              {scene.shots.length === 0 ? (
                <p className="story-panel-description">
                  No shots in this scene yet.
                </p>
              ) : (
                <ol
                  style={{
                    paddingLeft: '20px',
                    lineHeight: 1.8,
                  }}
                >
                  {scene.shots.map((shot) => (
                    <ShotCard
                      key={shot.id}
                      shot={shot}
                      highlighted={highlightedShotId === shot.id}
                      {...shotCardProps}
                    />
                  ))}
                </ol>
              )}
            </section>
          ))}
        </div>
      )}
    </article>
  );
}
