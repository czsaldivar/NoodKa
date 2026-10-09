import type { StoryShot } from './storyTypes';

export type ShotCardProps = {
  shot: StoryShot;
  highlighted: boolean;
  generatingImageShotId: string | null;
  shotImages: Record<string, string>;
  imageErrors: Record<string, string>;
  imagePrompts: Record<string, string>;
  charactersForShotGeneration: Array<{ id: string; name: string }>;
  shotCharacterIds: Record<string, string>;
  selectShotCharacter: (shotId: string, characterId: string) => void;
  generateShotImage: (shotId: string) => Promise<void>;
  goToShotAsset: (shotId: string) => void;
};
