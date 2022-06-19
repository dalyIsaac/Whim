<#
    .SYNOPSIS
    Returns the next release tag and the current release tag.

    .PARAMETER Channel
    The channel to use in the release tag string. Must be one of the following:
    - `'canary'`
    - `'beta'`
    - `'stable'`

    .EXAMPLE
    PS> $currentRelease, $nextRelease = .\scripts\Get-WhimRelease.ps1
#>

param (
    [Parameter()]
    [string]$Channel = "canary"
)

$channel = $Channel.ToLower()

if ($channel -ne "canary" -and $channel -ne "beta" -and $channel -ne "stable") {
    Write-Error "Channel must be one of canary, beta, or stable"
}

$version = .\scripts\Get-WhimVersion.ps1

$nextBuild = 0
$currentReleaseTag = ""

$releases = gh release list
if ($null -ne $releases) {
    $priorRelease = $releases | Select-String -Pattern "v${version}-${channel}" | Select-Object -First 1

    if ($null -ne $priorRelease) {
        $priorRelease = $priorRelease.ToString()
        $currentReleaseTag = $priorRelease.Split("`t")[2]

        if ($currentReleaseTag.StartsWith("v${version}")) {
            $priorBuild = $currentReleaseTag.Split(".")[1]
            $nextBuild = ([int] $priorBuild) + 1
        }
    }
}

$commit = (git rev-parse HEAD).Substring(0, 8)

return $currentReleaseTag, "v${version}-${Channel}.${nextBuild}.${commit}"

