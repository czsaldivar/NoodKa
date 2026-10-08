export type Story = {
  id: string
  title: string
  description: string
  country: string
  region: string
  era: string
  language: string
  genre: string
  tone: string
  culturalFlavor: string
}

export type StoryShot = {
  id: string
  sequence: number
  duration: number
  action: string
  emotion: string
  camera: string
  lighting: string
}

export type StoryScene = {
  id: string
  number: number
  name: string
  location: string
  shots: StoryShot[]
}

export type StoryEpisode = {
  id: string
  number: number
  title: string
  scenes: StoryScene[]
}

export type StoryDetails = Story & {
  episodes: StoryEpisode[]
}
