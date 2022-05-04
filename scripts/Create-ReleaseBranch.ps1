<#
	.SYNOPSIS
	Create a release commit, branch, and push it to the remote.

	.DESCRIPTION
	This command will:
	1. Verify that the repository is clean.
	2. Assert that the current branch is the main branch.
	3. Calculate the next version number.
	4. Verify that there are no commits or tags containing the version number.
	5. Update the version number in the Whim source code.
	6. Create a release commit, and push the commit to the remote.
	7. Create a new branch for the release, and push it to the remote.

	.PARAMETER $BumpType
	The type of bump to create a release for.
	This can be either "major", "minor", or "patch".

	.EXAMPLE
	PS> ./Create-ReleaseBranch.ps1 major

#>
param (
	[Parameter(Mandatory = $true, Position = 0)]
	[string]$BumpType
)

Assert-GitMainBranch
Assert-GitClean
$nextVersion = Get-NextVersion($BumpType)
Assert-GitVersion($nextVersion)
Set-Version($nextVersion)
Add-BumpCommit
Add-ReleaseBranch($nextVersion)

<#
	.SYNOPSIS
	Assert that the current branch is the main branch.
#>
function Assert-GitMainBranch() {
	$branch = (git rev-parse --abbrev-ref HEAD)

	if ($branch -ne "main") {
		throw "Not on the main branch: $branch"
	}
}

<#
	.SYNOPSIS
	Verify that the codebase is clean.
#>
function Assert-GitClean() {
	$status = (git status --porcelain)

	if ($null -ne $status) {
		Write-Output $status
		throw "Git status is not clean:"
	}
}

<#
	.SYNOPSIS
	Bumps the version number in the project file.
#>
function Get-NextVersion() {
	param (
		[Parameter(Mandatory = $true)]
		[string]$BumpType
	)

	if (($BumpType -ne "patch") -and ($BumpType -ne "minor") -and ($BumpType -ne "major")) {
		Write-Error "BumpType must be one of: patch, minor, major"
		exit 1
	}

	# Get the current version.
	$xml = [Xml] (Get-Content .\src\Whim.Runner\Whim.Runner.csproj)
	$version = $xml.Project.PropertyGroup.Version

	$versionParts = $version.Split(".")

	# Verify that the version is in the correct format.
	if ($versionParts.Length -ne 3) {
		Write-Error "Version must be in the format: major.minor.patch"
		exit 1
	}

	$majorVersion = [int]$versionParts[0]
	$minorVersion = [int]$versionParts[1]
	$patchVersion = [int]$versionParts[2]

	# Increment the version.
	if ($BumpType -eq "patch") {
		$patchVersion++
	}
	elseif ($BumpType -eq "minor") {
		$minorVersion++
		$patchVersion = 0
	}
	elseif ($BumpType -eq "major") {
		$majorVersion++
		$minorVersion = 0
		$patchVersion = 0
	}

	$nextVersion = $majorVersion.ToString() + "." + $minorVersion.ToString() + "." + $patchVersion.ToString()

	# Check with the user that the next version is correct.
	Write-Host "The current version is $version"
	Write-Host "The next version will be $nextVersion"
	$proceed = Read-Host "Is this correct? (y/n): "

	if ($proceed -ne "y") {
		Write-Error "Aborting"
		exit 1
	}

	return $nextVersion
}

<#
	.SYNOPSIS
	Ensure no branch or tag exists named $nextVersion.
#>
function Assert-GitVersion() {
	param (
		[Parameter(Mandatory = $true)]
		[String]
		$nextVersion
	)

	git fetch

	# Verify there is no branch on the remote named $nextVersion.
	$branches = git branch -r
	if ($branches.Contains($nextVersion)) {
		Write-Error "A branch on the remote containing the string $nextVersion already exists"
		exit 1
	}

	# Verify there is no branch locally named $nextVersion.
	$branches = git branch
	if ($branches.Contains($nextVersion)) {
		Write-Error "A branch locally containing the string $nextVersion already exists"
		exit 1
	}

	# Verify that there is no tag named $nextVersion.
	$tags = git tag
	if ($tags.Contains($nextVersion)) {
		Write-Error "A tag containing the string $nextVersion already exists"
		exit 1
	}
}

<#
	.SYNOPSIS
	Sets the version number of Whim.
#>
function Set-Version() {
	param (
		[Parameter(Mandatory = $true)]
		[string]$version
	)

	# Check for set-version.
	if (!(Get-Command setversion -ErrorAction SilentlyContinue)) {
		$proceed = Read-Host "dotnet-setversion not found. Install now? (Y/n): "
		if ([string]::IsNullOrEmpty($proceed)) {
			$proceed = "Y"
		}

		if ($proceed -eq "Y") {
			dotnet tool install -g dotnet-setversion
		}
		else {
			Write-Error -Message "dotnet-setversion not found. Aborting."
			exit 1
		}

		setversion -r $Version
	}
}

<#
	.SYNOPSIS
	Creates and pushes a commit with the current changes..
#>
function Add-BumpCommit() {
	param (
		[Parameter(Mandatory = $true)]
		[String]
		$nextVersion
	)

	git add .
	git commit -m "Bumped version to $nextVersion" -S

	# Ask the user if they want to push.
	$proceed = Read-Host "Push to remote? (y/n): "
	if ($proceed -ne "y") {
		Write-Error "Aborting"
		exit 1
	}

	git push
}

<#
	.SYNOPSIS
	Creates and pushes a release branch.
#>
function Add-ReleaseBranch() {
	param (
		[Parameter(Mandatory = $true)]
		[String]
		$nextVersion
	)

	git checkout -b $nextVersion

	# Ask the user if they want to push.
	$proceed = Read-Host "Push to remote? (y/n): "
	if ($proceed -ne "y") {
		Write-Error "Aborting"
		exit 1
	}

	git push --set-upstream origin $nextVersion
}
