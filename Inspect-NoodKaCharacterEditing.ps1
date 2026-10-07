$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location $projectRoot

$ErrorActionPreference = "Stop"
Set-Location "$projectRoot"

$appPath = ".\src\NoodKa.Web\src\App.tsx"
$source = Get-Content $appPath -Raw

$checks = [ordered]@{
    CharacterFormState = $source.Contains("const [characterForm, setCharacterForm]")
    CreateHandler = $source.Contains("async function handleCreateCharacter")
    CharacterFormSubmit = $source.Contains("onSubmit={handleCreateCharacter}")
    ProfileToolbar = $source.Contains('className="character-profile-topbar"')
    ProfileHeading = $source.Contains('id="character-profile-title"')
    CharacterProfileEndpoint = (Get-Content ".\src\NoodKa.Api\Program.cs" -Raw).Contains('"/api/characters/{characterId:guid}"')
}

$checks.GetEnumerator() | ForEach-Object {
    "{0}: {1}" -f $_.Key, $(if ($_.Value) { "FOUND" } else { "MISSING" })
}

if ($checks.Values -contains $false) {
    throw "Source verification failed. No files were modified."
}

Write-Host "`nInspection passed. No source files were modified." -ForegroundColor Green
