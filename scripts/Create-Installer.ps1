<#
	.SYNOPSIS
	Creates the Whim installer.
#>

param (
	[Parameter(Mandatory = $true, Position = 0)]
	[ValidateSet('x64', 'arm64')]
	[string]$Architecture
)

$buildDir = "src\Whim.Runner\bin\${Architecture}\Release\net6.0-windows10.0.19041.0"
$version = (Get-Item "${buildDir}\Whim.Runner.exe").VersionInfo.ProductVersion
$installerName = "WhimInstaller-${Architecture}-${version}"

& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" .\whim-installer.iss `
	/DMyBuildDir="$buildDir" `
	/DMyOutputBaseFilename="$installerName" `
	/DMyVersion="$version"

return "bin\${installerName}.exe"
