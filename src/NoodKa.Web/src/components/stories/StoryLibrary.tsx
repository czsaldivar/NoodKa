import type { Story } from './storyTypes';

type StoryLibraryProps = {
  stories: Story[];
  storiesLoading: boolean;
  selectedStoryId: string | null;
  selectStory: (storyId: string) => void;
};

export default function StoryLibrary({
  stories,
  storiesLoading,
  selectedStoryId,
  selectStory,
}: StoryLibraryProps) {
  return (
    <section className="story-list-panel">
      <div className="story-list-heading">
        <div>
          <div className="story-panel-kicker">YOUR LIBRARY</div>
          <h2>Saved stories</h2>
        </div>
        <span className="toolbar-count">
          {stories.length} {stories.length === 1 ? 'story' : 'stories'}
        </span>
      </div>

      {storiesLoading ? (
        <div className="story-empty">
          <div className="loader" />
          <p>Loading your stories...</p>
        </div>
      ) : stories.length === 0 ? (
        <div className="story-empty">
          <div className="empty-icon">N</div>
          <h3>Your next story starts here.</h3>
          <p>
            Create your first story using the form. Saved stories
            will appear here.
          </p>
        </div>
      ) : (
        <div className="story-list">
          {stories.map((story) => (
            <article
              className="story-card"
              key={story.id}
              role="button"
              tabIndex={0}
              aria-pressed={selectedStoryId === story.id}
              onClick={() => selectStory(story.id)}
              onKeyDown={(event) => {
                if (event.key === 'Enter' || event.key === ' ') {
                  event.preventDefault();
                  selectStory(story.id);
                }
              }}
              style={{
                cursor: 'pointer',
                outline: selectedStoryId === story.id
                  ? '1px solid var(--amber)'
                  : 'none',
                outlineOffset: '4px',
              }}
            >
              <div className="story-card-mark">N.</div>
              <div className="story-card-content">
                <h3>{story.title}</h3>
                <p>{story.description || 'No description added yet.'}</p>
                <span className="story-card-id">{story.id}</span>
              </div>
              <span className="asset-badge">
                {selectedStoryId === story.id ? 'Selected' : 'Saved'}
              </span>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
