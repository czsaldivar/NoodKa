$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location $projectRoot

$ErrorActionPreference = 'Stop'
Set-Location "$projectRoot"

$apiPath = ".\src\NoodKa.Api\Program.cs"
$webPath = ".\src\NoodKa.Web\src\App.tsx"

$api = [System.IO.File]::ReadAllText((Resolve-Path $apiPath))
$web = [System.IO.File]::ReadAllText((Resolve-Path $webPath))

if ($api.Contains('/api/shots/{shotId:guid}/generate-image') -or
    $web.Contains('NOODKA_SHOT_IMAGE_GENERATION_V1')) {
    throw "Patch marker already exists. No files changed."
}

$apiLines = [System.Collections.Generic.List[string]]::new()
[System.IO.File]::ReadAllLines((Resolve-Path $apiPath)) | ForEach-Object { $apiLines.Add($_) }

$webLines = [System.Collections.Generic.List[string]]::new()
[System.IO.File]::ReadAllLines((Resolve-Path $webPath)) | ForEach-Object { $webLines.Add($_) }

$apiAnchor = -1
for ($i = 0; $i -lt $apiLines.Count; $i++) {
    if ($apiLines[$i].Trim() -eq 'if (app.Environment.IsDevelopment())') {
        $apiAnchor = $i
        break
    }
}

$stateAnchor = -1
$handlerAnchor = -1
$descriptionAnchor = -1

for ($i = 0; $i -lt $webLines.Count; $i++) {
    if ($webLines[$i].Contains('const [shotSuccesses, setShotSuccesses]')) { $stateAnchor = $i }
    if ($webLines[$i].Contains('const filteredAssets = useMemo(() => {')) { $handlerAnchor = $i }
    if ($webLines[$i].Contains('].filter(Boolean).join(')) { $descriptionAnchor = $i }
}

if ($apiAnchor -lt 0 -or $stateAnchor -lt 0 -or $handlerAnchor -lt 0 -or $descriptionAnchor -lt 0) {
    throw "An expected insertion point was not found. No files changed."
}

$closingDiv = -1
$closingLi = -1
for ($i = $descriptionAnchor + 1; $i -lt [Math]::Min($descriptionAnchor + 6, $webLines.Count); $i++) {
    if ($webLines[$i].Trim() -eq '</div>') { $closingDiv = $i }
    if ($closingDiv -ge 0 -and $webLines[$i].Trim() -eq '</li>') {
        $closingLi = $i
        break
    }
}
if ($closingDiv -lt 0 -or $closingLi -lt 0) {
    throw "Could not safely identify the end of the shot description. No files changed."
}

$endpoint = @(
'app.MapPost("/api/shots/{shotId:guid}/generate-image", async ('
'    Guid shotId,'
'    IStoryRepository storyRepository,'
'    IShotGenerator shotGenerator,'
'    CancellationToken cancellationToken) =>'
'{'
'    var stories = await storyRepository.GetAllAsync(cancellationToken);'
'    var match = stories'
'        .SelectMany(story => story.Episodes)'
'        .SelectMany(episode => episode.Scenes)'
'        .SelectMany(scene => scene.Shots.Select(shot => new { Scene = scene, Shot = shot }))'
'        .FirstOrDefault(item => item.Shot.Id == shotId);'
''
'    if (match is null)'
'        return Results.NotFound(new { error = "Shot not found." });'
''
'    var promptRequest = new CinematicPromptRequest('
'        location: match.Scene.Location,'
'        action: match.Shot.Action,'
'        emotion: match.Shot.Emotion,'
'        camera: match.Shot.Camera,'
'        lighting: match.Shot.Lighting,'
'        visualStyle: "Photorealistic cinematic film still");'
''
'    var result = await shotGenerator.GenerateAsync('
'        new ShotGenerationRequest(match.Shot.Id, promptRequest), cancellationToken);'
''
'    if (!result.Succeeded)'
'        return Results.Json(new { error = result.ImageResult.ErrorMessage ?? "Image generation failed.", prompt = result.Prompt }, statusCode: StatusCodes.Status502BadGateway);'
''
'    return Results.Ok(new { shotId = match.Shot.Id, prompt = result.Prompt, imageId = result.ImageResult.Id, imageUrl = $"/api/assets/{result.ImageResult.Id}" });'
'}).WithName("GenerateSavedShotImage");'
''
)

$state = @(
'  // NOODKA_SHOT_IMAGE_GENERATION_V1'
'  const [generatingImageShotId, setGeneratingImageShotId] = useState<string | null>(null)'
'  const [shotImages, setShotImages] = useState<Record<string, string>>({})'
'  const [imageErrors, setImageErrors] = useState<Record<string, string>>({})'
'  const [imagePrompts, setImagePrompts] = useState<Record<string, string>>({})'
''
)

$handler = @(
'  async function generateShotImage(shotId: string) {'
'    if (generatingImageShotId) return'
'    setGeneratingImageShotId(shotId)'
'    setImageErrors((current) => ({ ...current, [shotId]: "" }))'
'    try {'
'      const response = await fetch(`/api/shots/${shotId}/generate-image`, { method: "POST" })'
'      const payload: { error?: string; imageUrl?: string; prompt?: string } = await response.json()'
'      if (!response.ok) throw new Error(payload.error ?? `Image generation failed (HTTP ${response.status}).`)'
'      if (!payload.imageUrl) throw new Error("The API did not return an image URL.")'
'      setShotImages((current) => ({ ...current, [shotId]: payload.imageUrl! }))'
'      setImagePrompts((current) => ({ ...current, [shotId]: payload.prompt ?? "" }))'
'      const assetsResponse = await fetch("/api/assets/recent?take=50")'
'      if (assetsResponse.ok) setAssets((await assetsResponse.json()) as Asset[])'
'    } catch (cause) {'
'      setImageErrors((current) => ({ ...current, [shotId]: cause instanceof Error ? cause.message : "Unable to generate the shot image." }))'
'    } finally {'
'      setGeneratingImageShotId(null)'
'    }'
'  }'
''
)

$ui = @(
'                                      <div style={{ display: "flex", flexWrap: "wrap", gap: "10px", alignItems: "center", marginTop: "10px" }}>'
'                                        <button type="button" className="story-submit" disabled={generatingImageShotId !== null} onClick={() => generateShotImage(shot.id)}>'
'                                          {generatingImageShotId === shot.id ? "Generating image..." : shotImages[shot.id] ? "Regenerate Image" : "Generate Image"}'
'                                        </button>'
'                                        {shotImages[shot.id] && <a className="primary-button" href={shotImages[shot.id]} target="_blank" rel="noreferrer">Open image</a>}'
'                                      </div>'
'                                      {imageErrors[shot.id] && <p className="notice error-notice" role="alert">{imageErrors[shot.id]}</p>}'
'                                      {shotImages[shot.id] && ('
'                                        <div style={{ marginTop: "12px", maxWidth: "560px" }}>'
'                                          <img src={shotImages[shot.id]} alt={`Generated image for shot ${shot.sequence}`} style={{ display: "block", width: "100%", height: "auto", borderRadius: "6px", border: "1px solid var(--border)" }} />'
'                                          {imagePrompts[shot.id] && <details style={{ marginTop: "8px" }}><summary>View cinematic prompt</summary><p className="story-panel-description">{imagePrompts[shot.id]}</p></details>}'
'                                        </div>'
'                                      )}'
''
)

# Apply insertions in memory only.
$apiLines.InsertRange($apiAnchor, [string[]]$endpoint)
$webLines.InsertRange($stateAnchor + 1, [string[]]$state)
$webLines.InsertRange($handlerAnchor + $state.Count + 1, [string[]]$handler)

# Re-find the shot description after earlier insertions.
$descriptionAnchor = -1
for ($i = 0; $i -lt $webLines.Count; $i++) {
    if ($webLines[$i].Contains('].filter(Boolean).join(')) { $descriptionAnchor = $i; break }
}
$closingDiv = -1
$closingLi = -1
for ($i = $descriptionAnchor + 1; $i -lt [Math]::Min($descriptionAnchor + 6, $webLines.Count); $i++) {
    if ($webLines[$i].Trim() -eq '</div>') { $closingDiv = $i }
    if ($closingDiv -ge 0 -and $webLines[$i].Trim() -eq '</li>') { $closingLi = $i; break }
}
if ($closingLi -lt 0) { throw "Shot UI insertion validation failed. No files changed." }

$webLines.InsertRange($closingLi, [string[]]$ui)

$apiUpdated = [string]::Join("`r`n", $apiLines) + "`r`n"
$webUpdated = [string]::Join("`r`n", $webLines) + "`r`n"

if (-not $apiUpdated.Contains('/api/shots/{shotId:guid}/generate-image') -or
    -not $webUpdated.Contains('NOODKA_SHOT_IMAGE_GENERATION_V1') -or
    -not $webUpdated.Contains('Generate Image')) {
    throw "Final validation failed. No files changed."
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
Copy-Item $apiPath "$apiPath.$stamp.bak"
Copy-Item $webPath "$webPath.$stamp.bak"

$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText((Resolve-Path $apiPath), $apiUpdated, $utf8)
[System.IO.File]::WriteAllText((Resolve-Path $webPath), $webUpdated, $utf8)

Write-Host "Patch applied. Backups: $stamp" -ForegroundColor Green
