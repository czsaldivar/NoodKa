import { useState } from 'react';
import type { Story, StoryDetails } from './storyTypes';

type StoryExplorerProps = {
  stories: Story[];
  storiesLoading: boolean;
  selectedStoryId: string | null;
  storyDetails: StoryDetails | null;
  selectStory: (storyId: string) => void;
  onSelectNode: (type: 'story' | 'episode' | 'scene' | 'shot', id: string) => void;
};

export default function StoryExplorer({
  stories,
  storiesLoading,
  selectedStoryId,
  storyDetails,
  selectStory,
  onSelectNode,
}: StoryExplorerProps) {
  const [expandedStories, setExpandedStories] = useState<string[]>([]);
  const [expandedEpisodes, setExpandedEpisodes] = useState<string[]>([]);
  const [expandedScenes, setExpandedScenes] = useState<string[]>([]);
  const [selectedNode, setSelectedNode] = useState('');

  const toggle = (
    id: string,
    current: string[],
    update: (next: string[]) => void,
  ) => {
    update(current.includes(id)
      ? current.filter((item) => item !== id)
      : [...current, id]);
  };

  const activeStory = stories.find((story) => story.id === selectedStoryId);
  const activeDetails = storyDetails?.id === selectedStoryId
    ? storyDetails
    : null;

  return (
    <aside className="story-explorer" aria-label="Story Explorer">
      <div className="story-explorer-header">
        <div>
          <div className="story-panel-kicker">WORKSPACE</div>
          <h2>Story Explorer</h2>
        </div>
        <span className="story-explorer-count">{stories.length}</span>
      </div>

      <p className="story-explorer-hint">
        Expand a story to browse its episodes, scenes, and shots.
      </p>

      {storiesLoading ? (
        <p className="story-explorer-empty">Loading stories...</p>
      ) : stories.length === 0 ? (
        <p className="story-explorer-empty">Your stories will appear here.</p>
      ) : (
        <nav className="story-tree" aria-label="Saved story hierarchy">
          {stories.map((story) => {
            const isActive = story.id === selectedStoryId;
            const isExpanded = expandedStories.includes(story.id) || isActive;
            const details = isActive ? activeDetails : null;

            return (
              <div className="story-tree-group" key={story.id}>
                <div className={`story-tree-row ${isActive && (!selectedNode || selectedNode === story.id) ? 'is-selected' : ''}`}>
                  <button
                    type="button"
                    className="story-tree-toggle"
                    aria-label={`${isExpanded ? 'Collapse' : 'Expand'} ${story.title}`}
                    aria-expanded={isExpanded}
                    onClick={() => {
                      toggle(story.id, expandedStories, setExpandedStories);
                    }}
                  >
                    {isExpanded ? 'v' : '>'}
                  </button>
                  <button
                    type="button"
                    className="story-tree-label"
                    onClick={() => {
                      setSelectedNode(story.id);
                      selectStory(story.id);
                      setExpandedStories((current) =>
                        current.includes(story.id) ? current : [...current, story.id]);
                      onSelectNode('story', story.id);
                    }}
                  >
                    <span className="story-tree-kind">STORY</span>
                    <span className="story-tree-name">{story.title}</span>
                  </button>
                </div>

                {isExpanded && isActive && (
                  <div className="story-tree-children">
                    {!details ? (
                      <p className="story-tree-muted">Loading structure...</p>
                    ) : details.episodes.length === 0 ? (
                      <p className="story-tree-muted">No episodes yet.</p>
                    ) : details.episodes.map((episode) => {
                      const episodeExpanded = expandedEpisodes.includes(episode.id);
                      const episodeSelected = selectedNode === episode.id;

                      return (
                        <div key={episode.id}>
                          <div className={`story-tree-row ${episodeSelected ? 'is-selected' : ''}`}>
                            <button
                              type="button"
                              className="story-tree-toggle"
                              aria-label={`${episodeExpanded ? 'Collapse' : 'Expand'} ${episode.title}`}
                              aria-expanded={episodeExpanded}
                              onClick={() => toggle(episode.id, expandedEpisodes, setExpandedEpisodes)}
                            >
                              {episodeExpanded ? 'v' : '>'}
                            </button>
                            <button
                              type="button"
                              className="story-tree-label"
                              onClick={() => {
                                setSelectedNode(episode.id);
                                setExpandedEpisodes((current) =>
                                  current.includes(episode.id) ? current : [...current, episode.id]);
                                onSelectNode('episode', episode.id);
                              }}
                            >
                              <span className="story-tree-kind">EPISODE {episode.number}</span>
                              <span className="story-tree-name">{episode.title}</span>
                            </button>
                          </div>

                          {episodeExpanded && (
                            <div className="story-tree-children">
                              {episode.scenes.length === 0 ? (
                                <p className="story-tree-muted">No scenes yet.</p>
                              ) : episode.scenes.map((scene) => {
                                const sceneExpanded = expandedScenes.includes(scene.id);
                                const sceneSelected = selectedNode === scene.id;

                                return (
                                  <div key={scene.id}>
                                    <div className={`story-tree-row ${sceneSelected ? 'is-selected' : ''}`}>
                                      <button
                                        type="button"
                                        className="story-tree-toggle"
                                        aria-label={`${sceneExpanded ? 'Collapse' : 'Expand'} ${scene.name}`}
                                        aria-expanded={sceneExpanded}
                                        onClick={() => toggle(scene.id, expandedScenes, setExpandedScenes)}
                                      >
                                        {sceneExpanded ? 'v' : '>'}
                                      </button>
                                      <button
                                        type="button"
                                        className="story-tree-label"
                                        onClick={() => {
                                          setSelectedNode(scene.id);
                                          setExpandedEpisodes((current) =>
                                            current.includes(episode.id) ? current : [...current, episode.id]);
                                          setExpandedScenes((current) =>
                                            current.includes(scene.id) ? current : [...current, scene.id]);
                                          onSelectNode('scene', scene.id);
                                        }}
                                      >
                                        <span className="story-tree-kind">SCENE {scene.number}</span>
                                        <span className="story-tree-name">{scene.name}</span>
                                      </button>
                                    </div>

                                    {sceneExpanded && (
                                      <div className="story-tree-children">
                                        {scene.shots.length === 0 ? (
                                          <p className="story-tree-muted">No shots yet.</p>
                                        ) : scene.shots.map((shot) => (
                                          <button
                                            type="button"
                                            key={shot.id}
                                            className={`story-tree-shot ${selectedNode === shot.id ? 'is-selected' : ''}`}
                                            onClick={() => {
                                              setSelectedNode(shot.id);
                                              setExpandedEpisodes((current) =>
                                                current.includes(episode.id) ? current : [...current, episode.id]);
                                              setExpandedScenes((current) =>
                                                current.includes(scene.id) ? current : [...current, scene.id]);
                                              onSelectNode('shot', shot.id);
                                            }}
                                          >
                                            <span className="story-tree-shot-mark">SHOT {shot.sequence}</span>
                                            <span className="story-tree-name">{shot.title?.trim() || `Shot ${shot.sequence}`}</span>
                                          </button>
                                        ))}
                                      </div>
                                    )}
                                  </div>
                                );
                              })}
                            </div>
                          )}
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
        </nav>
      )}

      {activeStory && (
        <div className="story-explorer-footer">
          <span className="story-tree-kind">CURRENT STORY</span>
          <strong>{activeStory.title}</strong>
        </div>
      )}
    </aside>
  );
}