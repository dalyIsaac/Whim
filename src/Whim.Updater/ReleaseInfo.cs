using Octokit;

namespace Whim.Updater;

/// <summary>
/// A Whim release and its corresponding version.
/// </summary>
/// <param name="Release">The GitHub release.</param>
/// <param name="Version">The parsed version.</param>
public record ReleaseInfo(Release Release, Version Version);
