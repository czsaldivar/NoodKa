import type { Dispatch, FormEvent, SetStateAction } from 'react';
import type { Story, StoryDetails } from './storyTypes';
import StoryCreationForm from './StoryCreationForm';
import StoryLibrary from './StoryLibrary';
import StoryDNA from './StoryDNA';
import EpisodeCard from './EpisodeCard';
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
    charactersForShotGeneration: Array<{ id: string; name: string }>;
    shotCharacterIds: Record<string, string[]>;
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
    selectShotCharacter: (shotId: string, characterIds: string[]) => void;
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
    charactersForShotGeneration,
    shotCharacterIds,
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
    selectShotCharacter,
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
              <EpisodeCard
                key={episode.id}
                episode={episode}
                sceneForm={{
                  sceneNames,
                  sceneLocations,
                  setSceneNames,
                  setSceneLocations,
                  creatingSceneId,
                  sceneErrors,
                  sceneSuccesses,
                  createScene,
                }}
                shotForm={{
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
                }}
                shotCard={{
                  highlightedShotId,
                  generatingImageShotId,
                  shotImages,
                  imageErrors,
                  imagePrompts,
                  charactersForShotGeneration,
                  shotCharacterIds,
                  selectShotCharacter,
                  generateShotImage,
                  goToShotAsset,
                }}
              />
            ))}
          </div>
        )}
      </>
    )}
    </section>
  </section>
  );
}

