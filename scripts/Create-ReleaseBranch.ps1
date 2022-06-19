<#
    .SYNOPSIS
    Bumps the version and creates a release branch.

    .EXAMPLE
    PS> .\scripts\Create-ReleaseBranch.ps1
#>

$repository = $env:GITHUB_REPOSITORY

if ($null -eq $repository) {
    $url = gh repo view --json url -q ".url"
    $repository = $url.Replace("https://github.com/", "")
}

$status = (git status --porcelain)
if ($null -ne $status) {
    throw "Git working directory is dirty. Please commit or stash changes before proceeding."
}

$version = (.\scripts\Get-WhimVersion.ps1) + 1
$bumpVersionBranch = "bump/v${version}"

# Create the branch.
git checkout -b $bumpVersionBranch

if (0 -ne $LastExitCode) {
    Write-Error "Failed to create branch $bumpVersionBranch"
    exit 1
}

# Check for set-version.
if (!(Get-Command setversion -ErrorAction SilentlyContinue)) {
    $proceed = Read-Host "dotnet-setversion not found. Install now? (y/N)"
    if ($proceed -cne "y") {
        Write-Error -Message "dotnet-setversion not found. Aborting."
        exit 1
    }

    dotnet tool install -g dotnet-setversion
}

# Bump the version.
setversion -r $version

$proceed = Read-Host "Commit and push on branch ${bumpVersionBranch}? (y/N)"
if ($proceed -cne "y") {
    Write-Error -Message "Aborting."
    exit 1
}

# Commit the changes.
git add .
git commit -m "Bump version to $version" -S

# Push the branch.
git push -u origin $bumpVersionBranch

$proceed = Read-Host "Create pull request? (y/N)"
if ($proceed -cne "y") {
    Write-Error -Message "Aborting."
    exit 1
}

# Create a new pull request.
$prUrl = gh pr create `
    --reviewer "dalyIsaac" `
    --title "Bump Whim version to $version" `
    --body "Bump Whim version to $version" `
    --label "whim version"

# Checkout main.
git checkout main

# Wait for the pull request to be merged.
$isMerged = $false

Write-Host "Waiting for pull request to be merged"
do {
    $isMergedUser = Read-Host "Pull request is merged? (y/N)"

    if ($isMergedUser -ceq "y") {
        $isMerged = (gh pr view $prUrl --json mergedAt -q ".mergedAt") -ne ""

        if ($isMerged) {
            Write-Host "Pull request is merged"
            break
        }

        Write-Host "You lie"
    }

    Start-Sleep -Seconds 10
} until (
    $isMerged
)

git fetch
git pull

# Create a release branch.
$releaseBranch = "release/v$version"
git checkout -b $releaseBranch
git push -u origin $releaseBranch
