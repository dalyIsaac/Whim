<#
	.SYNOPSIS
	Creates the Whim installer.
#>

param (
	[Parameter(Mandatory = $true, Position = 0)]
	[ValidateSet('x64', 'arm64')]
	[string]$Architecture
)

$buildDir = "src\Whim.Runner\bin\${Architecture}\Release\net8.0-windows10.0.19041.0"
$version = (Get-Item "${buildDir}\Whim.Runner.exe").VersionInfo.ProductVersion
$installerName = "WhimInstaller-${Architecture}-${version}"

$output = & "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" .\whim-installer.iss `
	/DMyBuildDir="$buildDir" `
	/DMyOutputBaseFilename="$installerName" `
	/DMyVersion="$version"

$path = $output.Split("\n")[-1]

# Test if the installer exists.
if (Test-Path $path) {
	return $path
}
else {
	Write-Host "Failed to create installer"
	Write-Host $output
	exit 1
}
