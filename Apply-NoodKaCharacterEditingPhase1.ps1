$projectRoot = Split-Path -Parent $PSScriptRoot
Set-Location $projectRoot

$ErrorActionPreference = "Stop"
Set-Location "$projectRoot"

$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backup = ".\backups\character-editing-phase1-$stamp"

$files = @(
    "src\NoodKa.Domain\Character.cs",
    "src\NoodKa.Application\Characters\ICharacterRepository.cs",
    "src\NoodKa.Infrastructure\Characters\SqliteCharacterRepository.cs",
    "src\NoodKa.Api\Program.cs"
)

# Read and validate all files before writing anything.
$paths = @{}
$text = @{}

foreach ($file in $files) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) {
        throw "Required file missing: $file"
    }

    $paths[$file] = (Resolve-Path -LiteralPath $file).Path
    $text[$file] = [System.IO.File]::ReadAllText($paths[$file])
}

$character = $text[$files[0]]
$interface = $text[$files[1]]
$repository = $text[$files[2]]
$program = $text[$files[3]]

# Prevent duplicate patching.
if ($character.Contains("UpdateProfile(") -or
    $interface.Contains("UpdateAsync(") -or
    $repository.Contains("UpdateAsync(") -or
    $program.Contains('app.MapPut("/api/characters/{characterId:guid}"') -or
    $program.Contains("UpdateCharacterRequest(")) {
    throw "An update implementation already exists. No source files were changed."
}

# Validate all insertion points.
$checks = @(
    @{ Name = "Character gender property"; Content = $character; Needle = "public CharacterGender Gender { get; }" },
    @{ Name = "Character personality method"; Content = $character; Needle = "    public void SetPersonality(CharacterPersonality personality)" },
    @{ Name = "Repository interface closing brace"; Content = $interface; Needle = "public interface ICharacterRepository" },
    @{ Name = "SQLite reader method"; Content = $repository; Needle = "    private static Character ReadCharacter(SqliteDataReader reader)" },
    @{ Name = "Character POST endpoint"; Content = $program; Needle = 'app.MapPost("/api/characters", async (' },
    @{ Name = "Character create DTO"; Content = $program; Needle = "public sealed record CreateCharacterRequest(" }
)

foreach ($check in $checks) {
    if (-not $check.Content.Contains($check.Needle)) {
        throw "Validation failed: $($check.Name) not found. No source files were changed."
    }
}

# 1. Domain model.
$character = $character.Replace(
    "public CharacterGender Gender { get; }",
    "public CharacterGender Gender { get; private set; }"
)

$method = @'
    public void UpdateProfile(
        string name,
        CharacterGender gender,
        CharacterPersonality? personality,
        CharacterVisualProfile? visualProfile)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Character name is required.",
                nameof(name));

        Name = name.Trim();
        Gender = gender;
        Personality = personality;
        VisualProfile = visualProfile;
    }

'@

$anchor = "    public void SetPersonality(CharacterPersonality personality)"
$character = $character.Replace($anchor, $method + $anchor)

# 2. Repository interface: insert before its final closing brace.
$interfaceEnd = $interface.TrimEnd()
$lastBrace = $interfaceEnd.LastIndexOf("}")

if ($lastBrace -lt 0) {
    throw "Cannot locate interface closing brace. No source files were changed."
}

$interface = $interfaceEnd.Insert(
    $lastBrace,
    @'

    Task UpdateAsync(
        Character character,
        CancellationToken cancellationToken = default);

'@
)

# 3. SQLite repository: update only the Characters row.
# CharacterReferences is deliberately untouched.
$anchor = "    private static Character ReadCharacter(SqliteDataReader reader)"

$updateMethod = @'
    public async Task UpdateAsync(
        Character character,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(character);

        await EnsureSchemaAsync(cancellationToken);

        await using var connection = await OpenConnectionAsync(
            cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE Characters
            SET Name = $name,
                Gender = $gender,
                PersonalityDescription = $personality,
                Appearance = $appearance,
                Hair = $hair,
                TypicalClothing = $clothing,
                VisualStyle = $style
            WHERE Id = $id;
            """;

        command.Parameters.AddWithValue("$id", character.Id.ToString("D"));
        command.Parameters.AddWithValue("$name", character.Name);
        command.Parameters.AddWithValue("$gender", (int)character.Gender);
        command.Parameters.AddWithValue(
            "$personality",
            (object?)character.Personality?.Description ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$appearance",
            (object?)character.VisualProfile?.Appearance ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$hair",
            (object?)character.VisualProfile?.Hair ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$clothing",
            (object?)character.VisualProfile?.TypicalClothing ?? DBNull.Value);
        command.Parameters.AddWithValue(
            "$style",
            (object?)character.VisualProfile?.VisualStyle ?? DBNull.Value);

        var rows = await command.ExecuteNonQueryAsync(cancellationToken);

        if (rows == 0)
            throw new KeyNotFoundException(
                $"Character '{character.Id}' was not found.");
    }

'@

$repository = $repository.Replace($anchor, $updateMethod + $anchor)

# 4. API endpoint: insert before the existing POST endpoint.
$anchor = 'app.MapPost("/api/characters", async ('

$putEndpoint = @'
app.MapPut("/api/characters/{characterId:guid}", async (
    Guid characterId,
    UpdateCharacterRequest body,
    ICharacterRepository repository,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(body.Name))
        return Results.BadRequest(new { error = "Character name is required." });

    var character = await repository.GetByIdAsync(
        characterId, cancellationToken);

    if (character is null)
        return Results.NotFound(new { error = "Character not found." });

    var gender = CharacterGender.Unspecified;

    if (!string.IsNullOrWhiteSpace(body.Gender) &&
        !Enum.TryParse(body.Gender, true, out gender))
    {
        return Results.BadRequest(new
        {
            error = "Gender must be Unspecified, Male, Female, or NonBinary."
        });
    }

    CharacterPersonality? personality =
        string.IsNullOrWhiteSpace(body.PersonalityDescription)
            ? null
            : new CharacterPersonality(body.PersonalityDescription.Trim());

    CharacterVisualProfile? visualProfile = null;

    if (body.Appearance is not null ||
        body.Hair is not null ||
        body.TypicalClothing is not null ||
        body.VisualStyle is not null)
    {
        visualProfile = new CharacterVisualProfile(
            body.Appearance ?? string.Empty,
            body.Hair ?? string.Empty,
            body.TypicalClothing ?? string.Empty,
            body.VisualStyle ?? string.Empty);
    }

    character.UpdateProfile(
        body.Name.Trim(),
        gender,
        personality,
        visualProfile);

    try
    {
        await repository.UpdateAsync(character, cancellationToken);
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound(new { error = "Character not found." });
    }

    return Results.Ok(ToCharacterResponse(character));
})
.WithName("UpdateCharacter");

'@

$program = $program.Replace($anchor, $putEndpoint + $anchor)

# 5. Request DTO: insert before the existing create DTO.
$anchor = "public sealed record CreateCharacterRequest("

$updateDto = @'
public sealed record UpdateCharacterRequest(
    string? Name,
    string? Gender = null,
    string? PersonalityDescription = null,
    string? Appearance = null,
    string? Hair = null,
    string? TypicalClothing = null,
    string? VisualStyle = null);

'@

$program = $program.Replace($anchor, $updateDto + $anchor)

# Final in-memory validation.
if (-not $character.Contains("public void UpdateProfile(") -or
    -not $character.Contains("Gender { get; private set; }") -or
    -not $interface.Contains("Task UpdateAsync(") -or
    -not $repository.Contains("public async Task UpdateAsync(") -or
    -not $program.Contains('app.MapPut("/api/characters/{characterId:guid}"') -or
    -not $program.Contains("public sealed record UpdateCharacterRequest(")) {
    throw "Final validation failed. No source files were changed."
}

# Backup all affected files before writing.
New-Item -ItemType Directory -Path $backup -Force | Out-Null

foreach ($file in $files) {
    $destination = Join-Path $backup $file
    New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force |
        Out-Null
    Copy-Item -LiteralPath $file -Destination $destination
}

Write-Host "Backup created: $backup" -ForegroundColor Green

# Write the four validated files.
$updated = @{
    $files[0] = $character
    $files[1] = $interface
    $files[2] = $repository
    $files[3] = $program
}

foreach ($file in $files) {
    [System.IO.File]::WriteAllText(
        $paths[$file],
        $updated[$file],
        [System.Text.UTF8Encoding]::new($false)
    )
}

Write-Host "Backend source changes applied." -ForegroundColor Green

# Build first; only test if build succeeds.
dotnet build .\NoodKa.slnx --no-restore

if ($LASTEXITCODE -ne 0) {
    throw "Build failed. Backup: $backup"
}

dotnet test .\NoodKa.slnx --no-build --no-restore

if ($LASTEXITCODE -ne 0) {
    throw "Tests failed. Backup: $backup"
}

Write-Host "SUCCESS: backend build and tests passed." -ForegroundColor Green
Write-Host "Backup: $backup" -ForegroundColor Yellow