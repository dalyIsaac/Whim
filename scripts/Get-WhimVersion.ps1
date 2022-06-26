$xml = [Xml] (Get-Content .\src\Whim.Runner\Whim.Runner.csproj)
$version = $xml.Project.PropertyGroup[0].Version.Split(".")

return [int] $version[0], [int] $version[1], [int] $version[2]
