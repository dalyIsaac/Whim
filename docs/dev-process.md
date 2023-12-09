# Development Process

The main development branch is `main`.

## Releases

Whim has the following release channels:

- `alpha`: The latest changes in `main`.
- `beta`: The latest changes in a release branch `release/v*`.
- `stable`: A release of the latest stable version, in a release branch.

The release format is compatible with [Semantic Versioning 2.0](https://semver.org/spec/v2.0.0.html). The format is `MAJOR.MINOR.PATCH-CHANNEL+COMMIT`. For example, `0.5.1-beta+3b8c8aa`.

- `PATCH` is the number of commits since the last bump commit.
- `COMMIT` is the first 8 characters of the commit hash.

Scripts will generally return the release string with no leading `v`. Builds will similarly have no leading `v`. However, **GitHub releases** (and thus tags) **will have a leading `v`**.

### Alpha Releases

`alpha` releases are created by making a commit to `main`, typically via a squashed pull request. This will run [`release.yml`](#releaseyml).

### Beta Releases

`beta` releases are created by making a commit to a release branch. This will run [`release.yml`](#releaseyml).

`beta` release branches are created by running [`scripts\Create-ReleaseBranch.ps1`](#create-releasebranchps1).

### Stable Releases

`stable` releases are running [`scripts\Create-StableRelease.ps1`](#create-stablereleaseps1) locally.

## Automating Releases

### `release.yml`

`release.yml` will:

1. Get the release version.
2. Build the release artifacts.
3. Create the release notes.
4. Create the release.

### `Create-ReleaseBranch.ps1`

`Create-ReleaseBranch.ps1` will:

1. Create a branch to bump the version.
2. Bump the version and push the commit.
3. Create a pull request to `main`.
4. Checkout `main`.
5. Wait for user input checking that the pull request is merged.
6. Create a release branch.
7. Push the release branch, which will trigger [`release.yml`](#releaseyml).

### `Create-StableRelease.ps1`

`Create-StableRelease.ps1` will:

1. Get the release version.
2. Push a lightweight tag, which will trigger [`release.yml`](#releaseyml)

### `Get-NextWhimRelease.ps1`

`Get-NextWhimRelease.ps1` will return the next release version given the parameters:

- `channel`
- `versionBump` (`major`, `minor`, `patch`, `none`)

## `Get-CurrentWhimRelease.ps1`

`Get-CurrentWhimRelease.ps1` will return the current release version given the specified channel.
