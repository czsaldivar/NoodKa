import type { Dispatch, FormEvent, SetStateAction } from 'react';

export type SceneCreationFormProps = {
  episodeId: string;
  sceneName: string;
  sceneLocation: string;
  setSceneNames: Dispatch<SetStateAction<Record<string, string>>>;
  setSceneLocations: Dispatch<SetStateAction<Record<string, string>>>;
  creatingSceneId: string | null;
  sceneError?: string;
  sceneSuccess?: string;
  createScene: (
    event: FormEvent<HTMLFormElement>,
    episodeId: string,
  ) => Promise<void>;
};
