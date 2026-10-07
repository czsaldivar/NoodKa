$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location $projectRoot

$ErrorActionPreference = "Stop"
Set-Location "$projectRoot"

$appPath = ".\src\NoodKa.Web\src\App.tsx"
$cssPath = ".\src\NoodKa.Web\src\App.css"

if (!(Test-Path $appPath) -or !(Test-Path $cssPath)) {
    throw "Required frontend files were not found. No changes made."
}

$app = [System.IO.File]::ReadAllText((Resolve-Path $appPath))
$css = [System.IO.File]::ReadAllText((Resolve-Path $cssPath))

function Assert-Contains([string]$Text, [string]$Needle, [string]$Description) {
    if (!$Text.Contains($Needle)) {
        throw "Verification failed: $Description. No source files have been written."
    }
}

function Replace-Once(
    [string]$Text,
    [string]$Old,
    [string]$New,
    [string]$Description
) {
    $first = $Text.IndexOf($Old, [StringComparison]::Ordinal)
    if ($first -lt 0) {
        throw "Patch anchor not found: $Description. No source files have been written."
    }

    if ($Text.IndexOf($Old, $first + $Old.Length, [StringComparison]::Ordinal) -ge 0) {
        throw "Patch anchor is ambiguous: $Description. No source files have been written."
    }

    return $Text.Substring(0, $first) + $New + $Text.Substring($first + $Old.Length)
}

# Verify the current implementation before making in-memory changes.
Assert-Contains $app 'const [selectedCharacterId, setSelectedCharacterId]' "selected character state"
Assert-Contains $app 'async function handleCreateCharacter' "character creation handler"
Assert-Contains $app '<form className="character-form" onSubmit={handleCreateCharacter}>' "character form"
Assert-Contains $app '<div className="character-profile-topbar">' "profile toolbar"
Assert-Contains $app 'className="character-profile-eyebrow">CHARACTER PROFILE</span>' "profile label"
Assert-Contains $app 'const response = await fetch(' "fetch implementation"

$api = [System.IO.File]::ReadAllText((Resolve-Path ".\src\NoodKa.Api\Program.cs"))
Assert-Contains $api '"/api/characters/{characterId:guid}"' "character update API endpoint"

# 1. Add edit state alongside the existing character state.
$app = Replace-Once $app `
    "  const [creatingCharacter, setCreatingCharacter] = useState(false)" `
    "  const [creatingCharacter, setCreatingCharacter] = useState(false)`r`n  const [editingCharacterId, setEditingCharacterId] = useState<string | null>(null)" `
    "editing state"

# 2. Replace the create handler with a shared create/update handler.
$handlerStart = $app.IndexOf("async function handleCreateCharacter", [StringComparison]::Ordinal)
if ($handlerStart -lt 0) { throw "Create handler not found. No source files have been written." }

$handlerStart = $app.LastIndexOf("  ", $handlerStart, [StringComparison]::Ordinal)
$returnMarker = "  return ("
$handlerEnd = $app.IndexOf($returnMarker, $handlerStart, [StringComparison]::Ordinal)

if ($handlerStart -lt 0 -or $handlerEnd -lt 0) {
    throw "Could not safely locate handler boundaries. No source files have been written."
}

$oldHandler = $app.Substring($handlerStart, $handlerEnd - $handlerStart)
if (!$oldHandler.Contains("method: 'POST'") -or !$oldHandler.Contains("setCreatingCharacter(false)")) {
    throw "Handler content differs from the expected version. No source files have been written."
}

$newHandler = @'
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

'@

$app = $app.Substring(0, $handlerStart) + $newHandler + $app.Substring($handlerEnd)

# 3. Switch the form to the shared handler and dynamic heading.
$app = Replace-Once $app `
    '<h2>Create a character</h2>' `
    '<h2>{editingCharacterId ? "Edit character" : "Create a character"}</h2>' `
    "dynamic character form heading"

$app = Replace-Once $app `
    '<form className="character-form" onSubmit={handleCreateCharacter}>' `
    '<form className="character-form" onSubmit={handleSaveCharacter}>' `
    "shared character form handler"

$app = Replace-Once $app `
    "{creatingCharacter ? 'Saving character...' : 'Save character'}" `
    "{creatingCharacter ? 'Saving character...' : editingCharacterId ? 'Save changes' : 'Save character'}" `
    "dynamic save button label"


# Insert cancel button immediately after the submit button.
$submitBlock = @'
                  <button className="primary-button character-submit" type="submit" disabled={creatingCharacter}>
                    {creatingCharacter ? 'Saving character...' : editingCharacterId ? 'Save changes' : 'Save character'}
                  </button>
'@

$cancelBlock = @'
                  {editingCharacterId && (
                    <button
                      className="character-profile-switch"
                      type="button"
                      onClick={handleCancelCharacterEdit}
                      disabled={creatingCharacter}
                    >
                      Cancel editing
                    </button>
                  )}
'@

$app = Replace-Once $app $submitBlock ($submitBlock + $cancelBlock) "cancel editing button"

# 4. Add an Edit Character button to the profile toolbar.
$oldToolbar = @'
                <span className="character-profile-eyebrow">CHARACTER PROFILE</span>
'@

$newToolbar = @'
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
'@

$app = Replace-Once $app $oldToolbar $newToolbar "profile edit button"

# 5. Add small layout styling for the profile actions.
$css = Replace-Once $css `
    '.character-profile-back {' `
    @'
.character-profile-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 12px;
}
.character-profile-back {
'@ `
    "profile action layout CSS"

# Validate the final in-memory source before any file writes.
Assert-Contains $app 'async function handleSaveCharacter' "shared save handler"
Assert-Contains $app 'method: isEditing ? ''PUT'' : ''POST''' "PUT/POST branching"
Assert-Contains $app 'onClick={() => handleEditCharacter(selectedCharacter)}' "profile edit action"
Assert-Contains $app 'onClick={handleCancelCharacterEdit}' "cancel action"
Assert-Contains $app 'onSubmit={handleSaveCharacter}' "form submit wiring"
Assert-Contains $css '.character-profile-actions {' "profile action CSS"

# Create backups only after all source checks pass.
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupDir = Join-Path (Get-Location) "backups\character-editing-phase2-$timestamp"
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null

Copy-Item $appPath (Join-Path $backupDir "App.tsx")
Copy-Item $cssPath (Join-Path $backupDir "App.css")

# Write both files only after all validation and backups succeed.
[System.IO.File]::WriteAllText((Resolve-Path $appPath), $app, [System.Text.UTF8Encoding]::new($false))
[System.IO.File]::WriteAllText((Resolve-Path $cssPath), $css, [System.Text.UTF8Encoding]::new($false))

Write-Host "`nCharacter Editing Phase 2 patch applied." -ForegroundColor Green
Write-Host "Backups: $backupDir"
Write-Host "Next: run the frontend build and backend tests."