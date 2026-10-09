import type { StoryCreationFormProps } from './storyCreationFormTypes';

export default function StoryCreationForm({
  form,
  creatingStory,
  createStory,
}: StoryCreationFormProps) {
  const {
    storyTitle,
    storyDescription,
    storyCountry,
    storyRegion,
    storyEra,
    storyLanguage,
    storyGenre,
    storyTone,
    storyCulturalFlavor,
    setStoryTitle,
    setStoryDescription,
    setStoryCountry,
    setStoryRegion,
    setStoryEra,
    setStoryLanguage,
    setStoryGenre,
    setStoryTone,
    setStoryCulturalFlavor,
  } = form;

  return (
    <section className="story-form-panel">
      <div className="story-panel-kicker">NEW PROJECT</div>
      <h2>Create a story</h2>
      <p className="story-panel-description">
        Capture the premise now. Episodes and scenes can be added in
        a later development milestone.
      </p>

      <form
        className="story-form"
        onSubmit={(event) => {
          event.preventDefault()
          void createStory()
        }}
      >
        <label htmlFor="story-title">STORY TITLE</label>
        <input
          id="story-title"
          value={storyTitle}
          onChange={(event) => setStoryTitle(event.target.value)}
          placeholder="e.g. The Last Sunrise"
          maxLength={160}
          required
        />

        <label htmlFor="story-description">PREMISE / DESCRIPTION</label>
        <textarea
          id="story-description"
          value={storyDescription}
          onChange={(event) => setStoryDescription(event.target.value)}
          placeholder="What is this story about?"
          rows={5}
          maxLength={4000}
        />

        <div
          style={{
            marginTop: '8px',
            paddingTop: '18px',
            borderTop: '1px solid var(--border)',
          }}
        >
          <div
            style={{
              fontSize: '11px',
              fontWeight: 700,
              letterSpacing: '0.14em',
              marginBottom: '6px',
            }}
          >
            STORY DNA
          </div>

          <p
            className="story-panel-description"
            style={{ marginBottom: '16px' }}
          >
            Define the world, culture, language, and emotional identity
            of your story.
          </p>

          <div
            style={{
              display: 'grid',
              gridTemplateColumns: 'repeat(2, minmax(0, 1fr))',
              gap: '14px',
            }}
          >
            <label htmlFor="story-country">
              COUNTRY
              <input
                id="story-country"
                value={storyCountry}
                onChange={(event) => setStoryCountry(event.target.value)}
                placeholder="Philippines"
                maxLength={100}
              />
            </label>

            <label htmlFor="story-region">
              REGION
              <input
                id="story-region"
                value={storyRegion}
                onChange={(event) => setStoryRegion(event.target.value)}
                placeholder="Metro Manila"
                maxLength={100}
              />
            </label>

            <label htmlFor="story-era">
              ERA
              <input
                id="story-era"
                value={storyEra}
                onChange={(event) => setStoryEra(event.target.value)}
                placeholder="Present Day"
                maxLength={100}
              />
            </label>

            <label htmlFor="story-language">
              LANGUAGE
              <select
                id="story-language"
                value={storyLanguage}
                onChange={(event) => setStoryLanguage(event.target.value)}
              >
                <option value="Taglish">Taglish</option>
                <option value="Filipino">Filipino</option>
                <option value="English">English</option>
                <option value="Cebuano">Cebuano</option>
                <option value="Ilocano">Ilocano</option>
                <option value="Other">Other</option>
              </select>
            </label>

            <label htmlFor="story-genre">
              GENRE
              <select
                id="story-genre"
                value={storyGenre}
                onChange={(event) => setStoryGenre(event.target.value)}
              >
                <option value="Family Drama">Family Drama</option>
                <option value="Romance">Romance</option>
                <option value="Comedy">Comedy</option>
                <option value="Comedy Drama">Comedy Drama</option>
                <option value="Thriller">Thriller</option>
                <option value="Mystery">Mystery</option>
                <option value="Action">Action</option>
                <option value="Horror">Horror</option>
                <option value="Slice of Life">Slice of Life</option>
                <option value="Other">Other</option>
              </select>
            </label>

            <label htmlFor="story-tone">
              TONE
              <select
                id="story-tone"
                value={storyTone}
                onChange={(event) => setStoryTone(event.target.value)}
              >
                <option value="Emotional">Emotional</option>
                <option value="Heartwarming">Heartwarming</option>
                <option value="Funny">Funny</option>
                <option value="Dark">Dark</option>
                <option value="Suspenseful">Suspenseful</option>
                <option value="Hopeful">Hopeful</option>
                <option value="Bittersweet">Bittersweet</option>
                <option value="Intense">Intense</option>
                <option value="Other">Other</option>
              </select>
            </label>

            <label
              htmlFor="story-cultural-flavor"
              style={{ gridColumn: '1 / -1' }}
            >
              CULTURAL FLAVOR
              <input
                id="story-cultural-flavor"
                value={storyCulturalFlavor}
                onChange={(event) =>
                  setStoryCulturalFlavor(event.target.value)
                }
                placeholder="Filipino"
                maxLength={160}
              />
            </label>
          </div>
        </div>

        <button
          className="primary-button story-submit"
          type="submit"
          disabled={creatingStory || !storyTitle.trim()}
        >
          {creatingStory ? 'Saving story...' : 'Create story'}
        </button>
      </form>
    </section>
  );
}
