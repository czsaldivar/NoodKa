import { useEffect, useMemo, useState } from 'react'
// NOODKA_STORY_MANAGEMENT_V1
import './App.css'
import StoryManagement from './components/stories/StoryManagement'
import type { Story, StoryDetails } from './components/stories/storyTypes'

type Asset = {
  id: string
  ownerType: number | string
  ownerId: string
  type: number | string
  contentType: string
  createdAtUtc: string
  contentUrl: string
  metadataUrl: string
  storyTitle?: string | null
  episodeTitle?: string | null
  episodeNumber?: number | null
  sceneId?: string | null
  sceneNumber?: number | null
  sceneLocation?: string | null
  shotSequence?: number | null
  shotAction?: string | null
  shotEmotion?: string | null
  shotCamera?: string | null
  shotLighting?: string | null}
function getOwnerLabel(value: number | string): string {
  const normalized = String(value).trim().toLowerCase()

  if (normalized === '0' || normalized === 'story') return 'Story'
  if (normalized === '1' || normalized === 'episode') return 'Episode'
  if (normalized === '2' || normalized === 'scene') return 'Scene'
  if (normalized === '3' || normalized === 'shot') return 'Shot'
  if (normalized === '4' || normalized === 'character') return 'Character'

  return 'Asset'
}

function getAssetTitle(asset: Asset): string {
  const action = asset.shotAction?.trim()

  if (action) {
    const compactAction = action.replace(/\s+/g, ' ')
    return compactAction.length > 68
      ? `${compactAction.slice(0, 65).trimEnd()}...`
      : compactAction
  }

  if (asset.storyTitle?.trim() && asset.shotSequence != null) {
    return `${asset.storyTitle.trim()} - Shot ${asset.shotSequence}`
  }

  if (asset.sceneLocation?.trim()) {
    return asset.sceneLocation.trim()
  }

  const owner = getOwnerLabel(asset.ownerType)

  // A Shot-owned asset without story metadata may refer to a missing Shot.
  if (owner === 'Shot') {
    return 'Unlinked Shot Asset'
  }

  const date = new Date(asset.createdAtUtc)

  if (Number.isNaN(date.getTime())) {
    return `${owner} image`
  }

  return `${owner} - ${date.toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  })}`
}

function getAssetContext(asset: Asset): string {
  return [
    asset.storyTitle?.trim(),
    asset.episodeTitle?.trim(),
    asset.sceneLocation?.trim(),
  ].filter(Boolean).join(' / ')
}

type Character = {
  id: string
  name: string
  gender: string
  personalityDescription: string | null
  visualProfile: {
    appearance: string | null
    hair: string | null
    typicalClothing: string | null
    visualStyle: string | null
  } | null
  references: Array<{
    id: string
    type: string
    storageLocation: string
    description: string | null
  }>
}

type CharacterForm = {
  name: string
  gender: string
  personalityDescription: string
  appearance: string
  hair: string
  typicalClothing: string
  visualStyle: string
}

function App() {
  const [assets, setAssets] = useState<Asset[]>([])
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [pendingAssetNavigation, setPendingAssetNavigation] = useState<{ shotId: string } | null>(null)
  const [search, setSearch] = useState('')
  const [assetFilter, setAssetFilter] = useState<'all' | 'linked' | 'unlinked'>('all')
  const [isPreviewOpen, setIsPreviewOpen] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [activeView, setActiveView] = useState<'assets' | 'stories' | 'characters'>('assets')

  const [characters, setCharacters] = useState<Character[]>([])
  const [selectedCharacterId, setSelectedCharacterId] = useState<string | null>(null)
  const [charactersLoading, setCharactersLoading] = useState(false)
  const selectedCharacter = characters.find((character) => character.id === selectedCharacterId) ?? null
  const [charactersError, setCharactersError] = useState('')
  const [creatingCharacter, setCreatingCharacter] = useState(false)
  const [editingCharacterId, setEditingCharacterId] = useState<string | null>(null)
  const [characterForm, setCharacterForm] = useState<CharacterForm>({
    name: '',
    gender: 'Unspecified',
    personalityDescription: '',
    appearance: '',
    hair: '',
    typicalClothing: '',
    visualStyle: 'Cinematic realistic Filipino drama.',
  })

  const [stories, setStories] = useState<Story[]>([])
  const [storiesLoading, setStoriesLoading] = useState(true)
  const [storiesError, setStoriesError] = useState('')
  const [storyTitle, setStoryTitle] = useState('')
  const [storyDescription, setStoryDescription] = useState('')

  const [storyCountry, setStoryCountry] = useState('Philippines')
  const [storyRegion, setStoryRegion] = useState('Metro Manila')
  const [storyEra, setStoryEra] = useState('Present Day')
  const [storyLanguage, setStoryLanguage] = useState('Taglish')
  const [storyGenre, setStoryGenre] = useState('Family Drama')
  const [storyTone, setStoryTone] = useState('Emotional')
  const [storyCulturalFlavor, setStoryCulturalFlavor] = useState('Filipino')

  const [creatingStory, setCreatingStory] = useState(false)
  const [selectedStoryId, setSelectedStoryId] = useState<string | null>(null)
  const [storyDetails, setStoryDetails] = useState<StoryDetails | null>(null)
  const [detailsLoading, setDetailsLoading] = useState(false)
  const [detailsError, setDetailsError] = useState('')
  const [pendingShotNavigation, setPendingShotNavigation] = useState<{ storyId: string; shotId: string } | null>(null)
  const [highlightedShotId, setHighlightedShotId] = useState<string | null>(null)
  const [shotNavigationBusy, setShotNavigationBusy] = useState(false)
  const [shotNavigationError, setShotNavigationError] = useState('')
  const [episodeTitle, setEpisodeTitle] = useState('')
  const [creatingEpisode, setCreatingEpisode] = useState(false)
  const [episodeError, setEpisodeError] = useState('')
  const [episodeSuccess, setEpisodeSuccess] = useState('')
  const [sceneNames, setSceneNames] = useState<Record<string, string>>({})
  const [sceneLocations, setSceneLocations] = useState<Record<string, string>>({})
  const [creatingSceneId, setCreatingSceneId] = useState<string | null>(null)
  const [sceneErrors, setSceneErrors] = useState<Record<string, string>>({})
  const [sceneSuccesses, setSceneSuccesses] = useState<Record<string, string>>({})
  // NOODKA_SHOT_CREATION_V1
  const [shotDurations, setShotDurations] = useState<Record<string, string>>({})
  const [shotActions, setShotActions] = useState<Record<string, string>>({})
  const [shotEmotions, setShotEmotions] = useState<Record<string, string>>({})
  const [shotCameras, setShotCameras] = useState<Record<string, string>>({})
  const [shotLighting, setShotLighting] = useState<Record<string, string>>({})
  const [creatingShotId, setCreatingShotId] = useState<string | null>(null)
  const [shotErrors, setShotErrors] = useState<Record<string, string>>({})
  const [shotSuccesses, setShotSuccesses] = useState<Record<string, string>>({})
  // NOODKA_SHOT_IMAGE_GENERATION_V1
  const [generatingImageShotId, setGeneratingImageShotId] = useState<string | null>(null)
  const [shotImages, setShotImages] = useState<Record<string, string>>({})
  const [imageErrors, setImageErrors] = useState<Record<string, string>>({})
  const [imagePrompts, setImagePrompts] = useState<Record<string, string>>({})




  useEffect(() => {
    const controller = new AbortController()

    async function loadAssets() {
      try {
        setLoading(true)
        setError('')

        const response = await fetch('/api/assets/recent?take=50', {
          signal: controller.signal,
        })

        if (!response.ok) {
          throw new Error(`Asset API returned HTTP ${response.status}`)
        }

        const payload: unknown = await response.json()
        let items: unknown[] = []

        if (Array.isArray(payload)) {
          items = payload
        } else if (
          payload !== null &&
          typeof payload === 'object' &&
          'value' in payload &&
          Array.isArray(payload.value)
        ) {
          items = payload.value
        }

        const validAssets = items.filter(
          (item): item is Asset =>
            item !== null &&
            typeof item === 'object' &&
            'id' in item &&
            typeof item.id === 'string' &&
            'contentUrl' in item &&
            typeof item.contentUrl === 'string',
        )

        setAssets(validAssets)
        setSelectedId((current) =>
          validAssets.some((asset) => asset.id === current)
            ? current
            : validAssets[0]?.id ?? null,
        )
      } catch (cause) {
        if (cause instanceof Error && cause.name === 'AbortError') return

        setError(
          cause instanceof Error
            ? cause.message
            : 'Unable to load your asset library.',
        )
      } finally {
        if (!controller.signal.aborted) setLoading(false)
      }
    }

    void loadAssets()
    return () => controller.abort()
  }, [])

  useEffect(() => {
    const controller = new AbortController()

    async function loadStories() {
      try {
        setStoriesLoading(true)
        setStoriesError('')

        const response = await fetch('/api/stories', {
          signal: controller.signal,
        })

        if (!response.ok) {
          throw new Error(`Stories API returned HTTP ${response.status}`)
        }

        const payload: unknown = await response.json()

        if (!Array.isArray(payload)) {
          throw new Error('The Stories API returned an unexpected response.')
        }

        const validStories = payload.filter(
          (item): item is Story =>
            item !== null &&
            typeof item === 'object' &&
            'id' in item &&
            typeof item.id === 'string' &&
            'title' in item &&
            typeof item.title === 'string' &&
            'description' in item &&
            typeof item.description === 'string' &&
            'country' in item &&
            typeof item.country === 'string' &&
            'region' in item &&
            typeof item.region === 'string' &&
            'era' in item &&
            typeof item.era === 'string' &&
            'language' in item &&
            typeof item.language === 'string' &&
            'genre' in item &&
            typeof item.genre === 'string' &&
            'tone' in item &&
            typeof item.tone === 'string' &&
            'culturalFlavor' in item &&
            typeof item.culturalFlavor === 'string',
        )

        setStories(validStories)
        setSelectedStoryId((current) =>
          current && validStories.some((story) => story.id === current)
            ? current
            : validStories[0]?.id ?? null,
        )
      } catch (cause) {
        if (cause instanceof Error && cause.name === 'AbortError') return

        setStoriesError(
          cause instanceof Error
            ? cause.message
            : 'Unable to load your stories.',
        )
      } finally {
        if (!controller.signal.aborted) setStoriesLoading(false)
      }
    }

    void loadStories()
    return () => controller.abort()
  }, [])

  useEffect(() => {
    if (!selectedStoryId) {
      setStoryDetails(null)
      setDetailsError('')
      setDetailsLoading(false)
      return
    }

    const controller = new AbortController()

    async function loadStoryDetails() {
      setDetailsLoading(true)
      setDetailsError('')
      setStoryDetails(null)

      try {
        const response = await fetch(
          `/api/stories/${selectedStoryId}`,
          { signal: controller.signal },
        )

        if (!response.ok) {
          throw new Error(`Story details API returned HTTP ${response.status}`)
        }

        const payload = (await response.json()) as StoryDetails

        if (!payload || typeof payload.id !== 'string') {
          throw new Error('The Story Details API returned an unexpected response.')
        }

        setStoryDetails({
          ...payload,
          episodes: Array.isArray(payload.episodes) ? payload.episodes : [],
        })
      } catch (cause) {
        if (cause instanceof Error && cause.name === 'AbortError') return

        setDetailsError(
          cause instanceof Error
            ? cause.message
            : 'Unable to load story details.',
        )
      } finally {
        if (!controller.signal.aborted) setDetailsLoading(false)
      }
    }

    void loadStoryDetails()
    return () => controller.abort()
  }, [selectedStoryId])

  useEffect(() => {
    if (!pendingShotNavigation) return
    if (selectedStoryId !== pendingShotNavigation.storyId) return
    if (detailsLoading || !storyDetails) return

    const target = pendingShotNavigation
    const shotExists = storyDetails.episodes.some((episode) =>
      episode.scenes.some((scene) =>
        scene.shots.some((shot) => shot.id === target.shotId),
      ),
    )

    if (!shotExists) {
      setShotNavigationError('The selected shot could not be found in this story.')
      setPendingShotNavigation(null)
      return
    }

    setActiveView('stories')
    setHighlightedShotId(target.shotId)
    setPendingShotNavigation(null)

    window.requestAnimationFrame(() => {
      document.getElementById('shot-' + target.shotId)?.scrollIntoView({
        behavior: 'smooth',
        block: 'center',
      })
    })
  }, [pendingShotNavigation, selectedStoryId, detailsLoading, storyDetails])

  async function createStory() {
    const title = storyTitle.trim()
    if (!title || creatingStory) return

    try {
      setCreatingStory(true)
      setStoriesError('')

      const response = await fetch('/api/stories', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          title,
          description: storyDescription.trim(),
          country: storyCountry.trim(),
          region: storyRegion.trim(),
          era: storyEra.trim(),
          language: storyLanguage.trim(),
          genre: storyGenre.trim(),
          tone: storyTone.trim(),
          culturalFlavor: storyCulturalFlavor.trim(),
        }),
      })

      if (!response.ok) {
        let message = `Stories API returned HTTP ${response.status}`

        try {
          const problem: unknown = await response.json()
          if (
            problem !== null &&
            typeof problem === 'object' &&
            'error' in problem &&
            typeof problem.error === 'string'
          ) {
            message = problem.error
          }
        } catch {
          // Keep the HTTP status message if the response has no JSON body.
        }

        throw new Error(message)
      }

      const created = (await response.json()) as Story

      setStories((current) => [
        created,
        ...current.filter((story) => story.id !== created.id),
      ])
      setSelectedStoryId(created.id)
      setStoryTitle('')
      setStoryDescription('')
      setStoryCountry('Philippines')
      setStoryRegion('Metro Manila')
      setStoryEra('Present Day')
      setStoryLanguage('Taglish')
      setStoryGenre('Family Drama')
      setStoryTone('Emotional')
      setStoryCulturalFlavor('Filipino')
    } catch (cause) {
      setStoriesError(
        cause instanceof Error ? cause.message : 'Unable to create the story.',
      )
    } finally {
      setCreatingStory(false)
    }
  }

  async function createEpisode(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const title = episodeTitle.trim()
    if (!selectedStoryId || !title || creatingEpisode) return

    try {
      setCreatingEpisode(true)
      setEpisodeError('')
      setEpisodeSuccess('')

      const response = await fetch(
        `/api/stories/${selectedStoryId}/episodes`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ title }),
        },
      )

      if (!response.ok) {
        let message = `Episode API returned HTTP ${response.status}`

        try {
          const problem: unknown = await response.json()
          if (
            problem !== null &&
            typeof problem === 'object' &&
            'error' in problem &&
            typeof problem.error === 'string'
          ) {
            message = problem.error
          }
        } catch {
          // Keep the HTTP status message.
        }

        throw new Error(message)
      }

      const created = (await response.json()) as {
        id: string
        number: number
        title: string
      }

      const detailsResponse = await fetch(
        `/api/stories/${selectedStoryId}`,
      )

      if (!detailsResponse.ok) {
        throw new Error(
          'Episode was saved, but the story structure could not be refreshed.',
        )
      }

      const refreshed = (await detailsResponse.json()) as StoryDetails

      setStoryDetails({
        ...refreshed,
        episodes: Array.isArray(refreshed.episodes)
          ? refreshed.episodes
          : [],
      })
      setEpisodeTitle('')
      setEpisodeSuccess(
        `Episode ${created.number}: ${created.title} saved successfully.`,
      )
    } catch (cause) {
      setEpisodeError(
        cause instanceof Error
          ? cause.message
          : 'Unable to create the episode.',
      )
    } finally {
      setCreatingEpisode(false)
    }
  }

  async function createScene(
    event: React.FormEvent<HTMLFormElement>,
    episodeId: string,
  ) {
    event.preventDefault()

    const name = (sceneNames[episodeId] ?? '').trim()
    const location = (sceneLocations[episodeId] ?? '').trim()
    if (!name || !location || creatingSceneId) return

    try {
      setCreatingSceneId(episodeId)
      setSceneErrors((current) => ({ ...current, [episodeId]: '' }))
      setSceneSuccesses((current) => ({ ...current, [episodeId]: '' }))

      const response = await fetch(`/api/episodes/${episodeId}/scenes`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, location }),
      })

      if (!response.ok) {
        let message = `Scene API returned HTTP ${response.status}`
        try {
          const problem: unknown = await response.json()
          if (
            problem !== null &&
            typeof problem === 'object' &&
            'error' in problem &&
            typeof problem.error === 'string'
          ) {
            message = problem.error
          }
        } catch {
          // Keep the HTTP status message.
        }
        throw new Error(message)
      }

      const created = (await response.json()) as {
        id: string
        number: number
        location: string
      }

      if (!selectedStoryId) {
        throw new Error('Scene saved, but no story is selected for refreshing.')
      }

      const detailsResponse = await fetch(`/api/stories/${selectedStoryId}`)
      if (!detailsResponse.ok) {
        throw new Error(
          'Scene was saved, but the story structure could not be refreshed.',
        )
      }

      const refreshed = (await detailsResponse.json()) as StoryDetails
      setStoryDetails({
        ...refreshed,
        episodes: Array.isArray(refreshed.episodes)
          ? refreshed.episodes
          : [],
      })
      setSceneNames((current) => ({ ...current, [episodeId]: '' }))
      setSceneLocations((current) => ({ ...current, [episodeId]: '' }))
      setSceneSuccesses((current) => ({
        ...current,
        [episodeId]: `Scene ${created.number}: ${created.location} saved successfully.`,
      }))
    } catch (cause) {
      setSceneErrors((current) => ({
        ...current,
        [episodeId]:
          cause instanceof Error ? cause.message : 'Unable to create the scene.',
      }))
    } finally {
      setCreatingSceneId(null)
    }
  }

  async function createShot(
    event: React.FormEvent<HTMLFormElement>,
    sceneId: string,
  ) {
    event.preventDefault()

    const durationSeconds = Number(shotDurations[sceneId] ?? '5')
    const action = (shotActions[sceneId] ?? '').trim()
    const emotion = (shotEmotions[sceneId] ?? '').trim()
    const camera = (shotCameras[sceneId] ?? '').trim()
    const lighting = (shotLighting[sceneId] ?? '').trim()

    if (
      !Number.isFinite(durationSeconds) ||
      durationSeconds <= 0 ||
      !action ||
      creatingShotId
    ) {
      setShotErrors((current) => ({
        ...current,
        [sceneId]: 'Enter a positive duration and describe the shot action.',
      }))
      return
    }

    try {
      setCreatingShotId(sceneId)
      setShotErrors((current) => ({ ...current, [sceneId]: '' }))
      setShotSuccesses((current) => ({ ...current, [sceneId]: '' }))

      const response = await fetch(`/api/scenes/${sceneId}/shots`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          durationSeconds,
          action,
          emotion,
          camera,
          lighting,
        }),
      })

      if (!response.ok) {
        let message = `Shot API returned HTTP ${response.status}`
        try {
          const problem: unknown = await response.json()
          if (
            problem !== null &&
            typeof problem === 'object' &&
            'error' in problem &&
            typeof problem.error === 'string'
          ) {
            message = problem.error
          }
        } catch {
          // Keep the HTTP status message.
        }
        throw new Error(message)
      }

      const created = (await response.json()) as {
        id: string
        sequence: number
        duration: number
        action: string
      }

      if (!selectedStoryId) {
        throw new Error('Shot saved, but no story is selected for refreshing.')
      }

      const detailsResponse = await fetch(`/api/stories/${selectedStoryId}`)
      if (!detailsResponse.ok) {
        throw new Error(
          'Shot was saved, but the story structure could not be refreshed.',
        )
      }

      const refreshed = (await detailsResponse.json()) as StoryDetails
      setStoryDetails({
        ...refreshed,
        episodes: Array.isArray(refreshed.episodes)
          ? refreshed.episodes
          : [],
      })

      setShotActions((current) => ({ ...current, [sceneId]: '' }))
      setShotEmotions((current) => ({ ...current, [sceneId]: '' }))
      setShotCameras((current) => ({ ...current, [sceneId]: '' }))
      setShotLighting((current) => ({ ...current, [sceneId]: '' }))
      setShotSuccesses((current) => ({
        ...current,
        [sceneId]: `Shot ${created.sequence} saved successfully.`,
      }))
    } catch (cause) {
      setShotErrors((current) => ({
        ...current,
        [sceneId]:
          cause instanceof Error ? cause.message : 'Unable to create the shot.',
      }))
    } finally {
      setCreatingShotId(null)
    }
  }

  async function generateShotImage(shotId: string) {
    if (generatingImageShotId) return
    setGeneratingImageShotId(shotId)
    setImageErrors((current) => ({ ...current, [shotId]: "" }))
    try {
      const response = await fetch(`/api/shots/${shotId}/generate-image`, { method: "POST" })
      const payload: { error?: string; imageUrl?: string; prompt?: string } = await response.json()
      if (!response.ok) throw new Error(payload.error ?? `Image generation failed (HTTP ${response.status}).`)
      if (!payload.imageUrl) throw new Error("The API did not return an image URL.")
      setShotImages((current) => ({ ...current, [shotId]: payload.imageUrl! }))
      setImagePrompts((current) => ({ ...current, [shotId]: payload.prompt ?? "" }))
      const assetsResponse = await fetch("/api/assets/recent?take=50")
      if (assetsResponse.ok) setAssets((await assetsResponse.json()) as Asset[])
    } catch (cause) {
      setImageErrors((current) => ({ ...current, [shotId]: cause instanceof Error ? cause.message : "Unable to generate the shot image." }))
    } finally {
      setGeneratingImageShotId(null)
    }
  }


  const filteredAssets = useMemo(() => {
    const query = search.trim().toLowerCase()

    return assets.filter((asset) => {
      const ownerLabel = getOwnerLabel(asset.ownerType)
      const hasStoryContext = Boolean(
        asset.storyTitle || asset.episodeTitle || asset.sceneLocation,
      )
      const isUnlinkedShot = ownerLabel === "Shot" && !hasStoryContext

      if (assetFilter === "linked" && !hasStoryContext) return false
      if (assetFilter === "unlinked" && !isUnlinkedShot) return false
      if (!query) return true

      const searchableValues = [
        asset.id, asset.contentType, asset.ownerId, ownerLabel,
        asset.storyTitle, asset.episodeTitle, asset.sceneLocation,
        asset.shotAction, asset.shotEmotion, asset.shotCamera,
        asset.shotLighting, asset.shotSequence?.toString(),
        asset.episodeNumber?.toString(), asset.sceneNumber?.toString(),
      ]

      return searchableValues.some((value) =>
        (value ?? "").toLowerCase().includes(query),
      )
    })
  }, [assets, search, assetFilter])

  const selectedAsset =
    filteredAssets.find((asset) => asset.id === selectedId) ??
    filteredAssets[0] ??
    null

  useEffect(() => {
    if (!pendingAssetNavigation) return

    const matchingAsset = assets.find((asset) =>
      getOwnerLabel(asset.ownerType) === 'Shot' &&
      asset.ownerId === pendingAssetNavigation.shotId
    )

    if (!matchingAsset) return

    setSearch('')
    setAssetFilter('all')
    setSelectedId(matchingAsset.id)
    setPendingAssetNavigation(null)
    setActiveView('assets')
  }, [pendingAssetNavigation, assets])

  function goToShotAsset(shotId: string) {
    const matchingAsset = assets.find((asset) =>
      getOwnerLabel(asset.ownerType) === 'Shot' &&
      asset.ownerId === shotId
    )

    if (matchingAsset) {
      setSearch('')
      setAssetFilter('all')
      setSelectedId(matchingAsset.id)
      setPendingAssetNavigation(null)
      setActiveView('assets')
      return
    }

    setPendingAssetNavigation({ shotId })
    setActiveView('assets')
    void refreshAssetsForShotNavigation(shotId)
  }

  async function refreshAssetsForShotNavigation(shotId: string) {
    try {
      const response = await fetch('/api/assets/recent?take=50')
      if (!response.ok) throw new Error(`Asset lookup failed (HTTP ${response.status}).`)

      const refreshedAssets = await response.json() as Asset[]
      setAssets(refreshedAssets)

      const matchingAsset = refreshedAssets.find((asset) =>
        getOwnerLabel(asset.ownerType) === 'Shot' &&
        asset.ownerId === shotId
      )

      if (matchingAsset) {
        setSearch('')
        setAssetFilter('all')
        setSelectedId(matchingAsset.id)
        setPendingAssetNavigation(null)
      } else {
        setPendingAssetNavigation(null)
        setError('No registered image asset was found for this shot in the recent asset list.')
      }
    } catch (cause) {
      setPendingAssetNavigation(null)
      setError(cause instanceof Error ? cause.message : 'Unable to find the shot image asset.')
    }
  }

  async function goToAssetShot(asset: Asset) {
    if (getOwnerLabel(asset.ownerType) !== 'Shot') return
    if (!asset.storyTitle || !asset.shotSequence) return
    if (shotNavigationBusy) return

    setShotNavigationBusy(true)
    setShotNavigationError('')
    setHighlightedShotId(null)

    try {
      const matchingTitle = asset.storyTitle.trim().toLowerCase()
      const orderedStories = [
        ...stories.filter((story) => story.title.trim().toLowerCase() === matchingTitle),
        ...stories.filter((story) => story.title.trim().toLowerCase() !== matchingTitle),
      ]

      for (const story of orderedStories) {
        const response = await fetch('/api/stories/' + story.id)
        if (!response.ok) continue

        const payload = await response.json() as StoryDetails
        const episodes = Array.isArray(payload.episodes) ? payload.episodes : []
        const shotExists = episodes.some((episode) =>
          episode.scenes?.some((scene) =>
            scene.shots?.some((shot) => shot.id === asset.ownerId),
          ),
        )

        if (!shotExists) continue

        setStoryDetails({
          ...payload,
          episodes,
        })
        setDetailsError('')
        setDetailsLoading(false)
        setPendingShotNavigation({ storyId: story.id, shotId: asset.ownerId })
        setSelectedStoryId(story.id)
        setActiveView('stories')
        return
      }

      setShotNavigationError('This asset is not linked to a shot in any available story.')
    } catch (cause) {
      setShotNavigationError(
        cause instanceof Error ? cause.message : 'Unable to locate the linked shot.',
      )
    } finally {
      setShotNavigationBusy(false)
    }
  }

  function formatDate(value: string) {
    const date = new Date(value)
    return Number.isNaN(date.getTime())
      ? 'Unknown date'
      : date.toLocaleString(undefined, {
          year: 'numeric',
          month: 'short',
          day: 'numeric',
          hour: '2-digit',
          minute: '2-digit',
        })
  }

  // NOODKA_CHARACTER_MANAGEMENT_V1
  useEffect(() => {
    if (activeView !== 'characters') return

    const controller = new AbortController()

    async function loadCharacters() {
      try {
        setCharactersLoading(true)
        setCharactersError('')

        const response = await fetch('/api/characters', {
          signal: controller.signal,
        })
        if (!response.ok) {
          throw new Error(`Characters API returned HTTP ${response.status}`)
        }

        const payload: unknown = await response.json()
        const rows = Array.isArray(payload)
          ? payload
          : payload && typeof payload === 'object' && 'value' in payload
            ? (payload as { value: unknown }).value
            : []

        const validCharacters: Character[] = Array.isArray(rows)
          ? rows.filter((item: unknown): item is Character => {
              if (!item || typeof item !== 'object') return false
              const candidate = item as Partial<Character>
              return typeof candidate.id === 'string' &&
                typeof candidate.name === 'string'
            })
          : []

        setCharacters(validCharacters)
      } catch (cause) {
        if (cause instanceof Error && cause.name === 'AbortError') return
        setCharactersError(
          cause instanceof Error ? cause.message : 'Unable to load characters.',
        )
      } finally {
        if (!controller.signal.aborted) setCharactersLoading(false)
      }
    }

    void loadCharacters()
    return () => controller.abort()
  }, [activeView])

  function resetCharacterForm() {
    setCharacterForm({
      name: '',
      gender: 'Unspecified',
      personalityDescription: '',
      appearance: '',
      hair: '',
      typicalClothing: '',
      visualStyle: 'Cinematic realistic Filipino drama.',
    })
  }

  function handleEditCharacter(character: Character) {
    setEditingCharacterId(character.id)
    setCharacterForm({
      name: character.name,
      gender: character.gender || 'Unspecified',
      personalityDescription: character.personalityDescription ?? '',
      appearance: character.visualProfile?.appearance ?? '',
      hair: character.visualProfile?.hair ?? '',
      typicalClothing: character.visualProfile?.typicalClothing ?? '',
      visualStyle: character.visualProfile?.visualStyle ?? 'Cinematic realistic Filipino drama.',
    })
    setCharactersError('')
    setSelectedCharacterId(null)
  }

  function handleCancelCharacterEdit() {
    setEditingCharacterId(null)
    setCharactersError('')
    resetCharacterForm()
  }

  async function handleSaveCharacter(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const name = characterForm.name.trim()

    if (!name) {
      setCharactersError('Please enter a character name.')
      return
    }

    const editingId = editingCharacterId
    const isEditing = editingId !== null
    const endpoint = isEditing
      ? `/api/characters/${encodeURIComponent(editingId)}`
      : '/api/characters'

    try {
      setCreatingCharacter(true)
      setCharactersError('')

      const response = await fetch(endpoint, {
        method: isEditing ? 'PUT' : 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name,
          gender: characterForm.gender,
          personalityDescription: characterForm.personalityDescription.trim() || null,
          appearance: characterForm.appearance.trim() || null,
          hair: characterForm.hair.trim() || null,
          typicalClothing: characterForm.typicalClothing.trim() || null,
          visualStyle: characterForm.visualStyle.trim() || null,
        }),
      })

      if (!response.ok) {
        const message = await response.text()
        throw new Error(message || `Character ${isEditing ? 'update' : 'creation'} failed (HTTP ${response.status})`)
      }

      const saved = (await response.json()) as Character
      setCharacters((current) => isEditing
        ? current.map((character) => character.id === saved.id ? saved : character)
        : [saved, ...current.filter((character) => character.id !== saved.id)],
      )

      setEditingCharacterId(null)
      resetCharacterForm()
    } catch (cause) {
      setCharactersError(
        cause instanceof Error
          ? cause.message
          : `Unable to ${isEditing ? 'update' : 'create'} character.`,
      )
    } finally {
      setCreatingCharacter(false)
    }
  }
  return (
    <div className="studio">
      <aside className="sidebar">
        <a className="brand" href="#" aria-label="NoodKa Studio home">
          <span className="brand-mark">N</span>
          <span className="brand-name">
            NOODKA<span>STUDIO</span>
          </span>
        </a>

        <div className="side-label">WORKSPACE</div>
        <button className={activeView === 'assets' ? 'nav-item active' : 'nav-item'} onClick={() => setActiveView('assets')} type="button">
          <span className="nav-icon">[A]</span>
          Asset library
          <span className="nav-count">{assets.length}</span>
        </button>
        <button
          className="nav-item"
          type="button"
          onClick={() => setSearch('')}
        >
          <span className="nav-icon">[+]</span>
          All creations
        </button>
        <button
          className={activeView === 'stories' ? 'nav-item active' : 'nav-item'}
          type="button"
          onClick={() => setActiveView('stories')}
        >
          <span className="nav-icon">[S]</span>
          Story management
          <span className="nav-count">{stories.length}</span>
        </button>

        <button
          className={activeView === 'characters' ? 'nav-item active' : 'nav-item'}
          type="button"
          onClick={() => setActiveView('characters')}
        >
          <span className="nav-icon">[C]</span>
          Character library
          <span className="nav-count">{characters.length}</span>
        </button>

        <div className="sidebar-bottom">
          <div className="system-indicator">
            <span className="status-dot" />
            <div>
              <strong>LOCAL WORKSPACE</strong>
              <span>Studio environment</span>
            </div>
          </div>
          <div className="sidebar-version">NOODKA STUDIO | 0.1.0</div>
        </div>
      </aside>

      <main className="main">
        <header className="topbar">
          <div className="breadcrumbs">
            Workspace <span>/</span> <strong>{activeView === 'assets' ? 'Asset library' : activeView === 'stories' ? 'Story management' : 'Character library'}</strong>
          </div>
          <div className="topbar-status">
            <span className="status-dot" />
            API workspace
          </div>
        </header>

        {activeView === 'assets' ? (
          <>
        <section className="page-heading">
          <div>
            <div className="eyebrow">
              <span className="eyebrow-line" />
              YOUR CREATIVE WORKSPACE
            </div>
            <h1>Asset <span>Library.</span></h1>
            <p className="page-description">
              Every frame starts somewhere. Your generated visuals live here.
            </p>
          </div>
          <div className="asset-total">
            <span>LIBRARY ASSETS</span>
                    <strong>{loading ? 'Loading assets...' : `${assets.length} assets`}</strong>
          </div>
        </section>

        <section className="library-toolbar">
          <div className="section-title">
            <span className="section-indicator" />
            <h2>{assetFilter === "all" ? "ALL ASSETS" : assetFilter === "linked" ? "LINKED ASSETS" : "UNLINKED ASSETS"}</h2>
            <span className="toolbar-count">
              {filteredAssets.length} {filteredAssets.length === 1 ? "item" : "items"}
            </span>
          </div>
          <div className="asset-toolbar-controls">
            <div className="asset-filter-group" aria-label="Filter assets">
              {(["all", "linked", "unlinked"] as const).map((filter) => (
                <button key={filter} type="button"
                  className={`asset-filter${assetFilter === filter ? " active" : ""}`}
                  aria-pressed={assetFilter === filter}
                  onClick={() => setAssetFilter(filter)}>
                  {filter === "all" ? "All" : filter === "linked" ? "Linked" : "Unlinked"}
                </button>
              ))}
            </div>
            <label className="search-box">
              <span aria-hidden="true">Search</span>
              <input aria-label="Search assets"
                placeholder="Search stories, shots, IDs..."
                value={search}
                onChange={(event) => setSearch(event.target.value)}
              />
              <kbd>/</kbd>
            </label>
          </div>
        </section>

        {error && (
          <div className="notice error-notice" role="alert">
            <strong>Could not load assets</strong>
            <span>{error}</span>
            <span className="notice-hint">
              Check that the API is running at localhost:5106 and reload this page.
            </span>
          </div>
        )}

        {loading ? (
          <div className="empty-state">
            <div className="loader" />
            <h3>Opening your library</h3>
            <p>Fetching assets from your local workspace...</p>
          </div>
        ) : !error && filteredAssets.length === 0 ? (
          <div className="empty-state">
                <div className="empty-icon">N</div>
            <h3>{assets.length ? 'No matching assets' : 'Your canvas awaits.'}</h3>
            <p>
              {assets.length
                ? 'Try another search term.'
                : 'Generated images will appear here when they are registered in your asset catalog.'}
            </p>
            {search && (
              <button className="secondary-button" onClick={() => setSearch('')}>
                Clear search
              </button>
            )}
          </div>
        ) : (
          <div className="workspace-grid">
            <section className="gallery" aria-label="Asset gallery">
              {filteredAssets.map((asset, index) => (
                <button
                  className={`asset-card ${selectedAsset?.id === asset.id ? 'selected' : ''}`}
                  key={asset.id}
                  onClick={() => setSelectedId(asset.id)}
                  type="button"
                  aria-pressed={selectedAsset?.id === asset.id}
                >
                  <div className="asset-image">
                    <img
                      src={asset.contentUrl}
                      alt={`Generated asset ${asset.id}`}
                      loading={index < 4 ? 'eager' : 'lazy'}
                    />
                    <span className="image-type">
                      {asset.contentType || 'IMAGE'}
                    </span>
                    <span className="image-open" aria-hidden="true">&gt;</span>
                  </div>
                  <div className="asset-card-info">
                    <div className="asset-card-name">
                      <span className="file-icon" aria-hidden="true">N</span>
                      <span>{getAssetTitle(asset)}</span>
                    </div>
                    <span className="asset-card-date">
                      {formatDate(asset.createdAtUtc)}
                    </span>
                    {getAssetContext(asset) && (
                      <span className="asset-card-context" title={getAssetContext(asset)}>
                        {getAssetContext(asset)}
                      </span>
                    )}
                  </div>
                  <div className="asset-card-id">{asset.id}</div>
                </button>
              ))}
            </section>

            {selectedAsset && (
              <aside className="inspector">
                <div className="inspector-heading">
                  <div>
                    <span className="inspector-kicker">ASSET DETAILS</span>
                    <h2>Inspector</h2>
                  </div>
                  <span className="inspector-symbol" aria-hidden="true">&#9670;</span>
                </div>

                <button
                  type="button"
                  className="inspector-preview inspector-preview-button"
                  onClick={() => setIsPreviewOpen(true)}
                  aria-label="Enlarge selected asset preview"
                >
                  <img
                    src={selectedAsset.contentUrl}
                    alt="Selected asset preview"
                  />
                </button>

                {isPreviewOpen && (
                  <div
                    className="asset-lightbox"
                    role="presentation"
                    onClick={() => setIsPreviewOpen(false)}
                  >
                    <section
                      className="asset-lightbox-panel"
                      role="dialog"
                      aria-modal="true"
                      aria-label="Expanded asset preview"
                      onClick={(event) => event.stopPropagation()}
                    >
                      <header className="asset-lightbox-header">
                        <div className="asset-lightbox-title">
                          <strong>{getAssetTitle(selectedAsset)}</strong>
                          {getAssetContext(selectedAsset) && (
                            <span>{getAssetContext(selectedAsset)}</span>
                          )}
                        </div>
                        <button
                          type="button"
                          className="asset-lightbox-close"
                          onClick={() => setIsPreviewOpen(false)}
                          aria-label="Close enlarged preview"
                        >
                          &times;
                        </button>
                      </header>
                      <img
                        className="asset-lightbox-image"
                        src={selectedAsset.contentUrl}
                        alt={getAssetTitle(selectedAsset)}
                      />
                    </section>
                  </div>
                )}

                <div className="inspector-name">
                  <span className="mini-label">ASSET NAME</span>
                  <strong>{getAssetTitle(selectedAsset)}</strong>
                  <span className="asset-badge">Registered asset</span>
                </div>

                {getAssetContext(selectedAsset) && (
                  <div className="metadata-section">
                    <div className="metadata-heading">STORY CONTEXT</div>
                    <MetadataRow label="Story" value={selectedAsset.storyTitle ?? ''} />
                    <MetadataRow
                      label="Episode"
                      value={selectedAsset.episodeTitle
                        ? `${selectedAsset.episodeNumber != null ? `Episode ${selectedAsset.episodeNumber}: ` : ''}${selectedAsset.episodeTitle}`
                        : ''}
                    />
                    <MetadataRow
                      label="Scene"
                      value={selectedAsset.sceneLocation
                        ? `${selectedAsset.sceneNumber != null ? `Scene ${selectedAsset.sceneNumber}: ` : ''}${selectedAsset.sceneLocation}`
                        : ''}
                    />
                    <MetadataRow
                      label="Shot"
                      value={selectedAsset.shotSequence != null ? `Shot ${selectedAsset.shotSequence}` : ''}
                    />
                    <MetadataRow label="Action" value={selectedAsset.shotAction ?? ''} />
                    <MetadataRow label="Emotion" value={selectedAsset.shotEmotion ?? ''} />
                    <MetadataRow label="Camera" value={selectedAsset.shotCamera ?? ''} />
                    <MetadataRow label="Lighting" value={selectedAsset.shotLighting ?? ''} />
                  </div>
                )}
                <div className="metadata-section">
                  <div className="metadata-heading">METADATA</div>
                  <MetadataRow label="Asset ID" value={selectedAsset.id} />
                  <MetadataRow label="Owner ID" value={selectedAsset.ownerId} />
                  <MetadataRow label="Owner type" value={getOwnerLabel(selectedAsset.ownerType)} />
                  <MetadataRow label="Asset type" value={String(selectedAsset.type)} />
                  <MetadataRow label="Format" value={selectedAsset.contentType} />
                  <MetadataRow
                    label="Created"
                    value={formatDate(selectedAsset.createdAtUtc)}
                  />
                </div>

                <div className="inspector-actions">
                  {getOwnerLabel(selectedAsset.ownerType) === 'Shot' &&
                    selectedAsset.storyTitle &&
                    selectedAsset.shotSequence != null && (
                      <button
                        type="button"
                        className="secondary-button"
                        disabled={shotNavigationBusy}
                        onClick={() => void goToAssetShot(selectedAsset)}
                      >
                        {shotNavigationBusy ? 'Finding shot...' : 'Go to Shot'}
                      </button>
                    )}
                  {shotNavigationError && (
                    <p className="notice error-notice" role="alert">
                      {shotNavigationError}
                    </p>
                  )}
                  <a
                    className="primary-button"
                    href={selectedAsset.contentUrl}
                    target="_blank"
                    rel="noreferrer"
                  >
                    Open full image <span aria-hidden="true">&gt;</span>
                  </a>
                  <a
                    className="secondary-button"
                    href={selectedAsset.metadataUrl}
                    target="_blank"
                    rel="noreferrer"
                  >
                    View raw metadata
                  </a>
                </div>
              </aside>
            )}
          </div>
        )}

          </>
        ) : activeView === 'stories' ? (
          <StoryManagement
            data={{
              stories,
              storiesLoading,
              storiesError,
              selectedStoryId,
              storyDetails,
              detailsLoading,
              detailsError,
              highlightedShotId,
              creatingStory,
              creatingEpisode,
              creatingSceneId,
              creatingShotId,
              generatingImageShotId,
              episodeError,
              episodeSuccess,
              sceneErrors,
              sceneSuccesses,
              shotErrors,
              shotSuccesses,
              shotImages,
              imageErrors,
              imagePrompts,
            }}
            forms={{
              story: {
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
              },
              episode: {
                episodeTitle,
                setEpisodeTitle,
                setEpisodeError,
                setEpisodeSuccess,
              },
              scene: {
                sceneNames,
                sceneLocations,
                setSceneNames,
                setSceneLocations,
              },
              shot: {
                shotDurations,
                shotActions,
                shotEmotions,
                shotCameras,
                shotLighting,
                setShotDurations,
                setShotActions,
                setShotEmotions,
                setShotCameras,
                setShotLighting,
              },
            }}
            actions={{
              createStory,
              createEpisode,
              createScene,
              createShot,
              generateShotImage,
              goToShotAsset,
              selectStory: setSelectedStoryId,
            }}
          />
        ) : (
          <section className="characters-view">
            <div className="page-heading">
              <div>
                <div className="eyebrow">
                  <span className="eyebrow-line" />
                  CHARACTER WORKSPACE
                </div>
                <h1>Character <em>library.</em></h1>
                <p className="page-description">
                  Create reusable character profiles for your NoodKa stories.
                </p>
              </div>
              <div className="story-count">
                <strong>{characters.length}</strong>
                <span>SAVED CHARACTERS</span>
              </div>
            </div>

            {charactersError && (
              <div className="story-error" role="alert">{charactersError}</div>
            )}

            <div className="character-layout">
              <section className="story-form-panel character-form-panel">
                <div className="panel-heading">
                  <div>
                    <span className="panel-kicker">CHARACTER DESIGN</span>
                    <h2>{editingCharacterId ? "Edit character" : "Create a character"}</h2>
                  </div>
                </div>

                <form className="character-form" onSubmit={handleSaveCharacter}>
                  <label>
                    Character name *
                    <input
                      required
                      maxLength={120}
                      value={characterForm.name}
                      onChange={(event) => setCharacterForm((current) => ({
                        ...current, name: event.target.value,
                      }))}
                      placeholder="e.g. Toffey"
                    />
                  </label>

                  <label>
                    Gender
                    <select
                      value={characterForm.gender}
                      onChange={(event) => setCharacterForm((current) => ({
                        ...current, gender: event.target.value,
                      }))}
                    >
                      <option value="Unspecified">Unspecified</option>
                      <option value="Male">Male</option>
                      <option value="Female">Female</option>
                      <option value="NonBinary">Non-binary</option>
                    </select>
                  </label>

                  <label>
                    Personality
                    <textarea
                      rows={3}
                      value={characterForm.personalityDescription}
                      onChange={(event) => setCharacterForm((current) => ({
                        ...current, personalityDescription: event.target.value,
                      }))}
                      placeholder="Personality, mannerisms, and behavior..."
                    />
                  </label>

                  <label>
                    Physical appearance
                    <textarea
                      rows={2}
                      value={characterForm.appearance}
                      onChange={(event) => setCharacterForm((current) => ({
                        ...current, appearance: event.target.value,
                      }))}
                      placeholder="Face, build, distinguishing features..."
                    />
                  </label>

                  <label>
                    Hair
                    <input
                      value={characterForm.hair}
                      onChange={(event) => setCharacterForm((current) => ({
                        ...current, hair: event.target.value,
                      }))}
                      placeholder="Hair style and color"
                    />
                  </label>

                  <label>
                    Typical clothing
                    <input
                      value={characterForm.typicalClothing}
                      onChange={(event) => setCharacterForm((current) => ({
                        ...current, typicalClothing: event.target.value,
                      }))}
                      placeholder="Usual wardrobe"
                    />
                  </label>

                  <label>
                    Visual style
                    <input
                      value={characterForm.visualStyle}
                      onChange={(event) => setCharacterForm((current) => ({
                        ...current, visualStyle: event.target.value,
                      }))}
                      placeholder="Cinematic realistic Filipino drama"
                    />
                  </label>

                  <button className="primary-button character-submit" type="submit" disabled={creatingCharacter}>
                    {creatingCharacter ? 'Saving character...' : editingCharacterId ? 'Save changes' : 'Save character'}
                  </button>                  {editingCharacterId && (
                    <button
                      className="character-profile-switch"
                      type="button"
                      onClick={handleCancelCharacterEdit}
                      disabled={creatingCharacter}
                    >
                      Cancel editing
                    </button>
                  )}
                </form>
              </section>

              <section className="story-list-panel character-list-panel">
                <div className="panel-heading">
                  <div>
                    <span className="panel-kicker">YOUR CAST</span>
                    <h2>Saved characters</h2>
                  </div>
                  <span className="story-count">{characters.length}</span>
                </div>

                {charactersLoading ? (
                  <div className="story-empty">Loading characters...</div>
                ) : characters.length === 0 ? (
                  <div className="story-empty">
                    <span className="empty-mark">[C]</span>
                    <h3>Your cast starts here.</h3>
                    <p>Create a character profile to build your reusable cast.</p>
                  </div>
                ) : (
                  <div className="character-list">
                    {characters.map((character) => (
                      <article
                  className="character-card character-card-selectable"
                  key={character.id}
                  role="button"
                  tabIndex={0}
                  aria-label={`View ${character.name} profile`}
                  onClick={() => setSelectedCharacterId(character.id)}
                  onKeyDown={(event) => {
                    if (event.key === "Enter" || event.key === " ") {
                      event.preventDefault()
                      setSelectedCharacterId(character.id)
                    }
                  }}
                >
                        <div className="character-avatar" aria-hidden="true">
                          {character.name.trim().charAt(0).toUpperCase()}
                        </div>
                        <div className="character-card-content">
                          <h3>{character.name}</h3>
                          <span className="character-gender">{character.gender}</span>
                          {character.personalityDescription && (
                            <p>{character.personalityDescription}</p>
                          )}
                          {character.visualProfile?.appearance && (
                            <p><strong>Appearance:</strong> {character.visualProfile.appearance}</p>
                          )}
                          {character.visualProfile?.hair && (
                            <p><strong>Hair:</strong> {character.visualProfile.hair}</p>
                          )}
                          {character.visualProfile?.typicalClothing && (
                            <p><strong>Clothing:</strong> {character.visualProfile.typicalClothing}</p>
                          )}
                          {character.visualProfile?.visualStyle && (
                            <p><strong>Style:</strong> {character.visualProfile.visualStyle}</p>
                          )}
                          <span className="character-reference-count">
                            {character.references?.length ?? 0} reference images
                          </span>
                        </div>
                      </article>
                    ))}
                  </div>
                )}
              </section>
            </div>
          </section>
        )}

        {/* NOODKA_CHARACTER_PROFILE_V1 */}
        {activeView === "characters" && selectedCharacter && (
          <div
            className="character-profile-backdrop"
            onMouseDown={(event) => {
              if (event.target === event.currentTarget) {
                setSelectedCharacterId(null)
              }
            }}
          >
            <section
              className="character-profile-panel"
              role="dialog"
              aria-modal="true"
              aria-labelledby="character-profile-title"
            >
              <div className="character-profile-topbar">
                <button
                  type="button"
                  className="character-profile-back"
                  onClick={() => setSelectedCharacterId(null)}
                >
                  &larr;
                </button>
                <div className="character-profile-actions">
                  <span className="character-profile-eyebrow">CHARACTER PROFILE</span>
                  <button
                    type="button"
                    className="character-profile-switch"
                    onClick={() => handleEditCharacter(selectedCharacter)}
                  >
                    Edit Character
                  </button>
                </div>
              </div>

              <div className="character-profile-heading">
                <div className="character-profile-avatar" aria-hidden="true">
                  {selectedCharacter.name.trim().charAt(0).toUpperCase()}
                </div>
                <div>
                  <h2 id="character-profile-title">{selectedCharacter.name}</h2>
                  <span className="character-gender">{selectedCharacter.gender}</span>
                </div>
              </div>

              <div className="character-profile-grid">
                <article className="character-profile-section">
                  <h3>Personality</h3>
                  <p>{selectedCharacter.personalityDescription || "No personality description added yet."}</p>
                </article>

                <article className="character-profile-section">
                  <h3>Appearance</h3>
                  <p>{selectedCharacter.visualProfile?.appearance || "No appearance details added yet."}</p>
                </article>

                <article className="character-profile-section">
                  <h3>Hair</h3>
                  <p>{selectedCharacter.visualProfile?.hair || "No hair details added yet."}</p>
                </article>

                <article className="character-profile-section">
                  <h3>Typical Clothing</h3>
                  <p>{selectedCharacter.visualProfile?.typicalClothing || "No clothing details added yet."}</p>
                </article>

                <article className="character-profile-section character-profile-section-wide">
                  <h3>Visual Style</h3>
                  <p>{selectedCharacter.visualProfile?.visualStyle || "No visual style defined yet."}</p>
                </article>

                <article className="character-profile-section character-profile-section-wide">
                  <div className="character-profile-reference-heading">
                    <h3>Reference Images</h3>
                    <span>{selectedCharacter.references?.length ?? 0}</span>
                  </div>
                  {selectedCharacter.references?.length ? (
                    <ul className="character-profile-reference-list">
                      {selectedCharacter.references.map((reference) => (
                        <li key={reference.id}>
                          <strong>{reference.type}</strong>
                          {reference.description && <p>{reference.description}</p>}
                          <span>{reference.storageLocation}</span>
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <p>No reference images have been added yet.</p>
                  )}
                </article>
              </div>

              <div className="character-profile-switcher">
                <span>Switch character</span>
                <div>
                  {characters.map((character) => (
                    <button
                      key={character.id}
                      type="button"
                      className={character.id === selectedCharacter.id
                        ? "character-profile-switch active"
                        : "character-profile-switch"}
                      aria-pressed={character.id === selectedCharacter.id}
                      onClick={() => setSelectedCharacterId(character.id)}
                    >
                      {character.name}
                    </button>
                  ))}
                </div>
              </div>
            </section>
          </div>
        )}
        <footer className="page-footer">
          <span>NOODKA STUDIO <span className="footer-separator">/</span> {activeView === 'assets' ? 'ASSET LIBRARY' : activeView === 'stories' ? 'STORY MANAGEMENT' : 'CHARACTER LIBRARY'}</span>
          <span>BUILT FOR STORIES THAT STAY.</span>
        </footer>
      </main>
    </div>
  )
}

function MetadataRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="metadata-row">
      <span>{label}</span>
      <span className="metadata-value" title={value}>{value || '-'}</span>
    </div>
  )
}
export default App
