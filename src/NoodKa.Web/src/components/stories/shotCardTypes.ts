import type { StoryShot } from './storyTypes';

export type ShotCardProps = {
  shot: StoryShot;
  highlighted: boolean;
  generatingImageShotId: string | null;
  shotImages: Record<string, string>;
  imageErrors: Record<string, string>;
  imagePrompts: Record<string, string>;
  generateShotImage: (shotId: string) => Promise<void>;
  goToShotAsset: (shotId: string) => void;
};
