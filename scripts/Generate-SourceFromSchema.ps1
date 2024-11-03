$ErrorActionPreference = "Stop"

Write-Host "Generating source from schema..."

$schemaPath = ".\src\Whim.Yaml\schema.json"
$outputPath = ".\src\Whim.Yaml\Generated"
$metadataPath = "$outputPath\metadata.json"
$now = Get-Date

$paths = @(
    ".\src\Whim.Yaml\Whim.Yaml.csproj",
    ".\src\Whim.Yaml\schema.json"
)
$yamlCodeHash = $paths | ForEach-Object { Get-FileHash $_ } | Out-String

function Test-Regenerate {
    param (
        [string]$schemaPath = ".\src\Whim.Yaml\schema.json",
        [string]$outputPath = ".\src\Whim.Yaml\Generated"
    )

    if (!(Test-Path $outputPath)) {
        Write-Host "Output directory does not exist, generating..."
        New-Item $outputPath -ItemType Directory
        return $true
    }

    Write-Host "Output directory exists..."

    if ((Test-Path $schemaPath) -eq $false) {
        Write-Host "Schema file does not exist, skipping..."
        return $false
    }

    $metadata = Get-Content $metadataPath | ConvertFrom-Json
    if ($metadata.yamlCodeHash -ne $yamlCodeHash) {
        Write-Host "YAML schema has changed since last generation, regenerating..."
        return $true
    }

    $schemaLastWriteTime = (Get-Item $schemaPath).LastWriteTime
    if ($metadata.lastWriteTime -lt $schemaLastWriteTime) {
        Write-Host "Schema has changed since last generation time, regenerating..."
        return $true
    }

    Write-Host "Schema has not changed since last generation, skipping..."
    return $false
}

if (!(Test-Regenerate -schemaPath $schemaPath -outputPath $outputPath)) {
    Write-Host "Skipping generation..."
    return
}

dotnet tool run generatejsonschematypes `
    $schemaPath `
    --rootNamespace Whim.Yaml `
    --useSchema Draft7 `
    --outputPath $outputPath

# If not in CI, write metadata file
if ($LASTEXITCODE -eq 0 -and $null -eq $env:CI) {
    Write-Host "Writing metadata file..."
    @{ yamlCodeHash = $yamlCodeHash; lastWriteTime = $now } | ConvertTo-Json | Set-Content $metadataPath
}
