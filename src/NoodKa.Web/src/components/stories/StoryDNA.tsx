import type { StoryDetails } from './storyTypes';

type StoryDNAProps = {
  story: StoryDetails;
};

export default function StoryDNA({ story }: StoryDNAProps) {
  return (
    <section
      style={{
        marginBottom: '22px',
        padding: '18px',
        border: '1px solid var(--border)',
        borderRadius: '8px',
      }}
    >
      <div className="story-panel-kicker">STORY DNA</div>

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
          gap: '12px 18px',
          marginTop: '14px',
        }}
      >
        <div>
          <strong>Country</strong>
          <div className="story-panel-description">
            {story.country}
          </div>
        </div>

        <div>
          <strong>Region</strong>
          <div className="story-panel-description">
            {story.region}
          </div>
        </div>

        <div>
          <strong>Era</strong>
          <div className="story-panel-description">
            {story.era}
          </div>
        </div>

        <div>
          <strong>Language</strong>
          <div className="story-panel-description">
            {story.language}
          </div>
        </div>

        <div>
          <strong>Genre</strong>
          <div className="story-panel-description">
            {story.genre}
          </div>
        </div>

        <div>
          <strong>Tone</strong>
          <div className="story-panel-description">
            {story.tone}
          </div>
        </div>

        <div style={{ gridColumn: '1 / -1' }}>
          <strong>Cultural Flavor</strong>
          <div className="story-panel-description">
            {story.culturalFlavor}
          </div>
        </div>
      </div>
    </section>
  );
}
