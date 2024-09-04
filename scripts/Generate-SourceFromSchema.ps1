pwd
# $schemaPath = ..\src\Whim.Json\Config\schema.json
# $outputPath = ..\src\Whim.Json\Generated

# Get-ChildItem $outputPath -Recurse | Remove-Item -Recurse -Force -Confirm:$false

# dotnet tool run generatejsonschematypes `
#     $schemaPath `
#     --rootNamespace Whim.Json `
#     --useSchema Draft7 `
#     --outputPath $outputPath
