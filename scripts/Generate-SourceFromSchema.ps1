Write-Host "Generating source from schema..."

$schemaPath = ".\src\Whim.Json\Config\schema.json"
$outputPath = ".\src\Whim.Json\Generated"

Remove-Item -Path $outputPath -Recurse -Force
New-Item $outputPath -ItemType Directory | Out-Null

dotnet tool run generatejsonschematypes `
    $schemaPath `
    --rootNamespace Whim.Json `
    --useSchema Draft7 `
    --outputPath $outputPath
