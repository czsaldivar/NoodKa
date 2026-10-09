import type { Dispatch, FormEvent, SetStateAction } from 'react';
import type { Story, StoryDetails } from './storyTypes';
import StoryCreationForm from './StoryCreationForm';
import StoryLibrary from './StoryLibrary';
import StoryDNA from './StoryDNA';
import type { StoryCreationFormProps } from './storyCreationFormTypes';

type StoryManagementProps = {
  data: {
    stories: Story[];
    storiesLoading: boolean;
    storiesError: string;
    selectedStoryId: string | null;
    storyDetails: StoryDetails | null;
    detailsLoading: boolean;
    detailsError: string;
    highlightedShotId: string | null;
    creatingStory: boolean;
    creatingEpisode: boolean;
    creatingSceneId: string | null;
    creatingShotId: string | null;
    generatingImageShotId: string | null;
    episodeError: string;
    episodeSuccess: string;
    sceneErrors: Record<string, string>;
    sceneSuccesses: Record<string, string>;
    shotErrors: Record<string, string>;
    shotSuccesses: Record<string, string>;
    shotImages: Record<string, string>;
    imageErrors: Record<string, string>;
    imagePrompts: Record<string, string>;
  };

  forms: {
    story: StoryCreationFormProps['form'];

    episode: {
      episodeTitle: string;
      setEpisodeTitle: (value: string) => void;
      setEpisodeError: (value: string) => void;
      setEpisodeSuccess: (value: string) => void;
    };

    scene: {
      sceneNames: Record<string, string>;
      sceneLocations: Record<string, string>;
      setSceneNames: Dispatch<SetStateAction<Record<string, string>>>;
      setSceneLocations: Dispatch<SetStateAction<Record<string, string>>>;
    };

    shot: {
      shotDurations: Record<string, string>;
      shotActions: Record<string, string>;
      shotEmotions: Record<string, string>;
      shotCameras: Record<string, string>;
      shotLighting: Record<string, string>;
      setShotDurations: Dispatch<SetStateAction<Record<string, string>>>;
      setShotActions: Dispatch<SetStateAction<Record<string, string>>>;
      setShotEmotions: Dispatch<SetStateAction<Record<string, string>>>;
      setShotCameras: Dispatch<SetStateAction<Record<string, string>>>;
      setShotLighting: Dispatch<SetStateAction<Record<string, string>>>;
    };
  };

  actions: {
    createStory: () => Promise<void>;
    createEpisode: (event: FormEvent<HTMLFormElement>) => Promise<void>;
    createScene: (
      event: FormEvent<HTMLFormElement>,
      episodeId: string,
    ) => Promise<void>;
    createShot: (
      event: FormEvent<HTMLFormElement>,
      sceneId: string,
    ) => Promise<void>;
    generateShotImage: (shotId: string) => Promise<void>;
    goToShotAsset: (shotId: string) => void;
    selectStory: (storyId: string) => void;
  };
};

export default function StoryManagement({
  data,
  forms,
  actions,
}: StoryManagementProps) {
  const {
    stories,
    storiesLoading,
    storiesError,
    selectedStoryId,
    storyDetails,
    detailsLoading,
    detailsError,
    highlightedShotId,
    creatingStory,
    creatingEpisode,
    creatingSceneId,
    creatingShotId,
    generatingImageShotId,
    episodeError,
    episodeSuccess,
    sceneErrors,
    sceneSuccesses,
    shotErrors,
    shotSuccesses,
    shotImages,
    imageErrors,
    imagePrompts,
  } = data;

  const {
    episodeTitle,
    setEpisodeTitle,
    setEpisodeError,
    setEpisodeSuccess,
  } = forms.episode;

  const {
    sceneNames,
    sceneLocations,
    setSceneNames,
    setSceneLocations,
  } = forms.scene;

  const {
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
  } = forms.shot;

  const {
    createStory,
    createEpisode,
    createScene,
    createShot,
    generateShotImage,
    goToShotAsset,
    selectStory: setSelectedStoryId,
  } = actions;

  return (
    <section className="stories-view">
      <div className="page-heading">
        <div>
          <div className="eyebrow">
            <span className="eyebrow-line" />
            YOUR STORY WORKSPACE
          </div>
          <h1>Story <span>Management.</span></h1>
          <p className="page-description">
            Start with an idea. Give your next cinematic story a home.
          </p>
        </div>
        <div className="asset-total">
          <span>SAVED STORIES</span>
          <strong>{stories.length}</strong>
        </div>
      </div>

      {storiesError && (
        <div className="notice error-notice" role="alert">
          <strong>Something needs attention</strong>
          <span>{storiesError}</span>
          <span className="notice-hint">
            Check that the NoodKa API is running, then try again.
          </span>
        </div>
      )}

      <div className="story-layout">
        <StoryCreationForm
          form={forms.story}
          creatingStory={creatingStory}
          createStory={createStory}
        />

        <StoryLibrary
          stories={stories}
          storiesLoading={storiesLoading}
          selectedStoryId={selectedStoryId}
          selectStory={setSelectedStoryId}
        />
      </div>

      <section
        aria-label="Selected story details"
        style={{
          marginTop: '4px',
          marginBottom: '38px',
          padding: '25px',
          border: '1px solid var(--border)',
          borderRadius: '6px',
          background: 'var(--panel)',
          minWidth: 0,
        }}
      >
        <div className="story-panel-kicker">STORY STRUCTURE</div>
        <h2 style={{
          margin: '10px 0',
          fontFamily: 'Georgia, serif',
          fontSize: '25px',
          fontWeight: 400,
          overflowWrap: 'anywhere',
        }}>
          {storyDetails?.title ?? 'Story details'}
        </h2>

        {selectedStoryId && storyDetails && (
          <form
            className="story-form"
            onSubmit={createEpisode}
            style={{ marginBottom: '22px' }}
          >
            <label htmlFor="episode-title">NEW EPISODE</label>
            <input
              id="episode-title"
              type="text"
              value={episodeTitle}
              onChange={(event) => {
                setEpisodeTitle(event.target.value)
                setEpisodeError('')
                setEpisodeSuccess('')
              }}
              placeholder="e.g. The Beginning"
              maxLength={200}
              required
              disabled={creatingEpisode}
            />
            <button
              className="story-submit"
              type="submit"
              disabled={!episodeTitle.trim() || creatingEpisode}
            >
              <span>
                {creatingEpisode ? 'Saving episode...' : 'Create episode'}
              </span>
            </button>

            {episodeError && (
              <div className="notice error-notice" role="alert">
                {episodeError}
              </div>
            )}
            {episodeSuccess && (
              <div className="notice" role="status">
                {episodeSuccess}
              </div>
            )}
          </form>
        )}

        {detailsLoading ? (
          <div className="story-empty">
            <div className="loader" />
              <p>Loading story structure...</p>
          </div>
        ) : detailsError ? (
          <div className="notice error-notice" role="alert">
            <strong>Could not load story details</strong>
            <span>{detailsError}</span>
          </div>
        ) : !selectedStoryId ? (
          <p className="story-panel-description">
            Select a saved story to explore its structure.
          </p>
        ) : !storyDetails ? (
          <p className="story-panel-description">
            Story details are not available yet.
          </p>
        ) : (
          <>
            <StoryDNA story={storyDetails} />

            {storyDetails.episodes.length === 0 ? (
              <div className="story-empty">
                <div className="empty-icon">N</div>
                <h3>Your story begins here.</h3>
                <p>
                  This story is saved. It has no episodes yet.
                </p>
              </div>
            ) : (
          <div style={{ display: 'grid', gap: '18px' }}>
            {storyDetails.episodes.map((episode) => (
              <article
                key={episode.id}
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

                <form
                  className="story-form"
                  onSubmit={(event) => createScene(event, episode.id)}
                  style={{ marginBottom: '18px' }}
                >
                  <label htmlFor={`scene-name-${episode.id}`}>
                    New scene name
                  </label>
                  <input
                    id={`scene-name-${episode.id}`}
                    type="text"
                    value={sceneNames[episode.id] ?? ''}
                    onChange={(event) =>
                      setSceneNames((current) => ({
                        ...current,
                        [episode.id]: event.target.value,
                      }))
                    }
                    placeholder="e.g. The Letter"
                    maxLength={200}
                    required
                  />
                  <label htmlFor={`scene-location-${episode.id}`}>
                    New scene location
                  </label>
                  <input
                    id={`scene-location-${episode.id}`}
                    type="text"
                    value={sceneLocations[episode.id] ?? ''}
                    onChange={(event) =>
                      setSceneLocations((current) => ({
                        ...current,
                        [episode.id]: event.target.value,
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
                      !((sceneNames[episode.id] ?? '').trim()) ||
                      !((sceneLocations[episode.id] ?? '').trim()) ||
                      creatingSceneId !== null
                    }
                  >
                    {creatingSceneId === episode.id
                      ? 'Saving Scene...'
                      : 'Create Scene'}
                  </button>
                  {sceneErrors[episode.id] && (
                    <p className="notice error-notice" role="alert">
                      {sceneErrors[episode.id]}
                    </p>
                  )}
                  {sceneSuccesses[episode.id] && (
                    <p className="notice" role="status">
                      {sceneSuccesses[episode.id]}
                    </p>
                  )}
                </form>

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
                        <div className="story-panel-description" style={{ marginTop: '4px' }}>{scene.location}</div>

                        <form
                          className="story-form"
                          onSubmit={(event) => createShot(event, scene.id)}
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

                          <label htmlFor={`shot-duration-${scene.id}`}>
                            Duration (seconds)
                          </label>
                          <input
                            id={`shot-duration-${scene.id}`}
                            type="number"
                            min="0.1"
                            step="0.1"
                            required
                            value={shotDurations[scene.id] ?? '5'}
                            onChange={(event) =>
                              setShotDurations((current) => ({
                                ...current,
                                [scene.id]: event.target.value,
                              }))
                            }
                          />

                          <label htmlFor={`shot-action-${scene.id}`}>
                            Shot action
                          </label>
                          <textarea
                            id={`shot-action-${scene.id}`}
                            required
                            rows={2}
                            placeholder="What happens in this shot?"
                            value={shotActions[scene.id] ?? ''}
                            onChange={(event) =>
                              setShotActions((current) => ({
                                ...current,
                                [scene.id]: event.target.value,
                              }))
                            }
                          />

                          <label htmlFor={`shot-emotion-${scene.id}`}>
                            Emotion (optional)
                          </label>
                          <input
                            id={`shot-emotion-${scene.id}`}
                            type="text"
                            placeholder="e.g. anxious, hopeful"
                            value={shotEmotions[scene.id] ?? ''}
                            onChange={(event) =>
                              setShotEmotions((current) => ({
                                ...current,
                                [scene.id]: event.target.value,
                              }))
                            }
                          />

                          <label htmlFor={`shot-camera-${scene.id}`}>
                            Camera (optional)
                          </label>
                          <input
                            id={`shot-camera-${scene.id}`}
                            type="text"
                            placeholder="e.g. close-up, slow dolly-in"
                            value={shotCameras[scene.id] ?? ''}
                            onChange={(event) =>
                              setShotCameras((current) => ({
                                ...current,
                                [scene.id]: event.target.value,
                              }))
                            }
                          />

                          <label htmlFor={`shot-lighting-${scene.id}`}>
                            Lighting (optional)
                          </label>
                          <input
                            id={`shot-lighting-${scene.id}`}
                            type="text"
                            placeholder="e.g. warm practical light"
                            value={shotLighting[scene.id] ?? ''}
                            onChange={(event) =>
                              setShotLighting((current) => ({
                                ...current,
                                [scene.id]: event.target.value,
                              }))
                            }
                          />

                          <button
                            type="submit"
                            className="story-submit"
                            disabled={
                              creatingShotId !== null ||
                              !(shotActions[scene.id] ?? '').trim() ||
                              !(Number(shotDurations[scene.id] ?? '5') > 0)
                            }
                          >
                            {creatingShotId === scene.id
                              ? 'Saving shot...'
                              : 'Save Shot'}
                          </button>

                          {shotErrors[scene.id] && (
                            <p className="notice error-notice" role="alert">
                              {shotErrors[scene.id]}
                            </p>
                          )}
                          {shotSuccesses[scene.id] && (
                            <p className="notice" role="status">
                              {shotSuccesses[scene.id]}
                            </p>
                          )}
                        </form>

                        {scene.shots.length === 0 ? (
                          <p className="story-panel-description">
                            No shots in this scene yet.
                          </p>
                        ) : (
                          <ol style={{
                            paddingLeft: '20px',
                            lineHeight: 1.8,
                          }}>
                            {scene.shots.map((shot) => (
                              <li id={`shot-${shot.id}`} key={shot.id} className={highlightedShotId === shot.id ? 'story-shot-target' : undefined} style={{ marginTop: '8px', scrollMarginTop: '100px' }}>
                                <strong>Shot {shot.sequence}</strong>
                                <span aria-hidden="true">&mdash;</span>
                                <div className="story-panel-description">
                                  {[
                                    shot.emotion && `Emotion: ${shot.emotion}`,
                                    shot.camera && `Camera: ${shot.camera}`,
                                    shot.lighting && `Lighting: ${shot.lighting}`,
                                  ].filter(Boolean).join(' | ')}
                                </div>                                      <div style={{ display: "flex", flexWrap: "wrap", gap: "10px", alignItems: "center", marginTop: "10px" }}>
                                  <button type="button" className="story-submit" disabled={generatingImageShotId !== null} onClick={() => generateShotImage(shot.id)}>
                                    {generatingImageShotId === shot.id ? "Generating image..." : shotImages[shot.id] ? "Regenerate Image" : "Generate Image"}
                                  </button>
                                  {shotImages[shot.id] && <a className="primary-button" href={shotImages[shot.id]} target="_blank" rel="noreferrer">Open image</a>}
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
                                {imageErrors[shot.id] && <p className="notice error-notice" role="alert">{imageErrors[shot.id]}</p>}
                                {shotImages[shot.id] && (
                                  <div style={{ marginTop: "12px", maxWidth: "560px" }}>
                                    <img src={shotImages[shot.id]} alt={`Generated image for shot ${shot.sequence}`} style={{ display: "block", width: "100%", height: "auto", borderRadius: "6px", border: "1px solid var(--border)" }} />
                                    {imagePrompts[shot.id] && <details style={{ marginTop: "8px" }}><summary>View cinematic prompt</summary><p className="story-panel-description">{imagePrompts[shot.id]}</p></details>}
                                  </div>
                                )}

                              </li>
                            ))}
                          </ol>
                        )}
                      </section>
                    ))}
                  </div>
                )}
              </article>
            ))}
          </div>
        )}
      </>
    )}
    </section>
  </section>
  );
}
