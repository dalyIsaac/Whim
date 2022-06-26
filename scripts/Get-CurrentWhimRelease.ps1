<#
    .SYNOPSIS
    Returns the current Whim release version for the given channel.
    This does not return the leading 'v' in the version string.

    .EXAMPLE
    PS> $currentRelease = .\scripts\Get-CurrentWhimRelease -Channel "alpha"
#>

param (
	[Parameter(Mandatory = $true, Position = 0)]
	[ValidateSet("alpha", "beta", "stable")]
	[string]$Channel
)

$releases = gh release list
if ($null -eq $releases) {
    $major, $minor, $patch = .\scripts\Get-WhimVersion.ps1
    $commit = (git rev-parse HEAD).Substring(0, 8)
    return "${major}.${minor}.${patch}-${channel}.${commit}"
}

$latestRelease = $releases | Select-String -Pattern $Channel -SimpleMatch | Select-Object -First 1
return $latestRelease.ToString().Split("`t")[0].Replace("v", "")
