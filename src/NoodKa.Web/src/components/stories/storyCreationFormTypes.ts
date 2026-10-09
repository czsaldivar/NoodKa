export type StoryCreationFormProps = {
  form: {
    storyTitle: string;
    storyDescription: string;
    storyCountry: string;
    storyRegion: string;
    storyEra: string;
    storyLanguage: string;
    storyGenre: string;
    storyTone: string;
    storyCulturalFlavor: string;
    setStoryTitle: (value: string) => void;
    setStoryDescription: (value: string) => void;
    setStoryCountry: (value: string) => void;
    setStoryRegion: (value: string) => void;
    setStoryEra: (value: string) => void;
    setStoryLanguage: (value: string) => void;
    setStoryGenre: (value: string) => void;
    setStoryTone: (value: string) => void;
    setStoryCulturalFlavor: (value: string) => void;
  };
  creatingStory: boolean;
  createStory: () => Promise<void>;
};
