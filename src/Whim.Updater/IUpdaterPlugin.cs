using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace Whim.Updater;

internal record SavedUpdaterPluginState(string? SkippedReleaseTagName, DateTime? LastCheckedForUpdates);

/// <summary>
/// UpdaterPlugin checks for updates to Whim and installs them, according to the given <see cref="UpdaterConfig"/>.
/// </summary>
public interface IUpdaterPlugin : IPlugin, IDisposable
{
	/// <summary>
	/// The date and time that the updater last checked for updates.
	/// </summary>
	DateTime? LastCheckedForUpdates { get; }

	/// <summary>
	/// Gets the releases in the current <see cref="ReleaseChannel"/> that have not been installed.
	/// </summary>
	/// <returns></returns>
	Task<List<ReleaseInfo>> GetNotInstalledReleases();

	/// <summary>
	/// Downloads and installs the given release.
	/// </summary>
	/// <param name="release"></param>
	/// <returns></returns>
	Task InstallRelease(Release release);

	/// <summary>
	/// Skips the given release.
	/// </summary>
	/// <param name="release"></param>
	void SkipRelease(Release release);
}
