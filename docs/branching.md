# Branching

The main development branch is `main`.

## Releases

Releases reside within dedicated release branches, named with the [semantic version number](https://semver.org/). For example, `v3.1.2`.

When a release branch is made, a new commit is made to the main development branch updating the source code to the new version. Then, a release branch is made off the main development branch. When the branch is ready to be released, it is tagged and released through GitHub releases.

The final commit in a release branch is tagged with the version number. Any further changes must be made in a further release.

This release process is codified in [`scripts/Create-ReleaseBranch.ps1`](../scripts/Create-ReleaseBranch.ps1).

![branching strategy](branching.svg)

## Pull Requests

Pull requests must contain a description of the changes made and an explanation of why the changes are necessary.

Pull requests must update [`CHANGELOG.md`](../CHANGELOG.md) with a user-oriented description of the changes.
