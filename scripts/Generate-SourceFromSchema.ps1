Write-Host "Generating source from schema..."

$schemaPath = ".\src\Whim.Yaml\schema.json"
$outputPath = ".\src\Whim.Yaml\Generated"

New-Item $outputPath -ItemType Directory | Out-Null

dotnet tool run generatejsonschematypes `
    $schemaPath `
    --rootNamespace Whim.Yaml `
    --useSchema Draft7 `
    --outputPath $outputPath
