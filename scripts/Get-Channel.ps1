<#
    .SYNOPSIS
    Gets the current channel for the running GitHub Actions workflow.
    This will not work if you are not running a GitHub Actions workflow.
#>

$channel = $null
$isPrerelease = $true

if ($env:GITHUB_REF -eq "refs/heads/main") {
    $channel = "alpha"
}
elseif ($env:GITHUB_REF.StartsWith("refs/heads/release/")) {
    $channel = "beta"
}
elseif ($env:GITHUB_REF.StartsWith("refs/tags/")) {
    $channel = "stable"
    $isPrerelease = $false
}
else {
    throw "Unsupported ref: $env:GITHUB_REF"
}

$isPrerelease = $isPrerelease.ToString().ToLower()
"channel=${channel}" >> $env:GITHUB_ENV
"isPrerelease=${isPrerelease}" >> $env:GITHUB_ENV

return $channel, $isPrerelease
