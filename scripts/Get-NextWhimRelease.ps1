<#
    .SYNOPSIS
    Returns the next Whim release version for the given channel, for the given git commit.
    This does not return the leading 'v' in the version string.

    .PARAMETER Channel
    The channel to use in the release tag string. Must be one of the following:

    .PARAMETER VersionBump
    The version bump to use in the release tag string. Must be one of the following:

    .EXAMPLE
    PS> $nextRelease = .\scripts\Get-NextWhumVersion.ps1 -Channel 'alpha' -VersionBump 'minor'
#>

param (
    [Parameter(Mandatory = $false, Position = 0)]
    [ValidateSet("alpha", "beta", "stable")]
    [string]$Channel = "alpha",

    [Parameter(Mandatory = $false, Position = 1)]
    [ValidateSet("patch", "minor", "major", "none")]
    [string]$VersionBump = "patch"
)

$major, $minor, $patch = .\scripts\Get-WhimVersion.ps1

$currentReleaseTag = ""

$releases = gh release list
if ($null -ne $releases) {
    $priorRelease = $releases | Select-String -Pattern "^v${major}.${minor}" | Select-Object -First 1

    if ($null -ne $priorRelease) {
        $priorRelease = $priorRelease.ToString()
        $currentReleaseTag = $priorRelease.Split("`t")[2]

        $patch = [int] $currentReleaseTag.Split("-").Split("+")[2]
    }
}

if ($versionBump -eq "major") {
    $major += 1
    $minor = 0
    $patch = 0
}
elseif ($versionBump -eq "minor") {
    $minor += 1
    $patch = 0
}
elseif ($versionBump -eq "patch") {
    $patch += 1
}

$commit = (git rev-parse HEAD).Substring(0, 8)

return "${major}.${minor}.${patch}-${channel}+${commit}"
