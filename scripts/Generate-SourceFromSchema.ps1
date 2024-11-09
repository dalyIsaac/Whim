Write-Host "Generating source from schema..."

$schemaPath = ".\src\Whim.Yaml\schema.json"
$outputPath = ".\src\Whim.Yaml\Generated"
$metadataPath = "$outputPath\metadata.json"

$schemaHash = (Get-FileHash $schemaPath -Algorithm SHA256).Hash

function Test-Regenerate {
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
    if ($metadata.schemaHash -ne $schemaHash) {
        Write-Host "Schema has changed since last generation, regenerating..."
        Write-Host "Old hash: $($metadata.schemaHash)"
        Write-Host "New hash: $schemaHash"
        return $true
    }

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
    @{ schemaHash = $schemaHash } | ConvertTo-Json | Set-Content $metadataPath
}
