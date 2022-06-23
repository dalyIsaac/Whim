<#
    .SYNOPSIS
    Triggers a stable release on GitHub.

    .EXAMPLE
    PS> .\scripts\Create-StableRelease.ps1
#>

# Verify that we're on a release branch.
$releaseBranch = git rev-parse --abbrev-ref HEAD

if (!$releaseBranch.StartsWith("release/v")) {
    throw "You must be on a release branch to create a stable release."
}

$nextRelease = .\scripts\Get-NextWhimRelease.ps1 -Channel stable -VersionBump none
$nextRelease = "v${nextRelease}"

# Verify there are no tags matching $nextRelease.
git fetch --tags
$existingTags = git tag -l $nextRelease
if ($null -ne $existingTags) {
    throw "There are already tags matching the release version ${nextRelease}."
}

$proceed = Read-Host "Are you sure you want to create a stable release for ${nextRelease}? (y/N)"
if ($proceed -cne "y") {
    throw "Stable release creation cancelled."
}

# Create and push the tag.
git tag $nextRelease
git push origin $nextRelease
