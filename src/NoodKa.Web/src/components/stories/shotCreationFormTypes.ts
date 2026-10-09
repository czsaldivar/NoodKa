import type { Dispatch, FormEvent, SetStateAction } from 'react';

export type ShotCreationFormProps = {
  sceneId: string;
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
  creatingShotId: string | null;
  shotErrors: Record<string, string>;
  shotSuccesses: Record<string, string>;
  createShot: (
    event: FormEvent<HTMLFormElement>,
    sceneId: string,
  ) => Promise<void>;
};
