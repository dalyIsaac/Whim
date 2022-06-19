$xml = [Xml] (Get-Content .\src\Whim.Runner\Whim.Runner.csproj)
return [int] $xml.Project.PropertyGroup[0].Version
