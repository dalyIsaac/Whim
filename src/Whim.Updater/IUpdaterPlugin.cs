using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNext;
using Octokit;

namespace Whim.Updater;

internal record SavedUpdaterPluginState(string? SkippedReleaseTagName, DateTime? LastCheckedForUpdates);

/// <summary>
/// UpdaterPlugin checks for updates to Whim and installs them, according to the given <see cref="UpdaterConfig"/>.
/// </summary>
public interface IUpdaterPlugin : IPlugin, IDisposable
{
	/// <summary>
	/// The configuration for the updater.
	/// </summary>
	UpdaterConfig Config { get; }

	/// <summary>
	/// The latest release that the user has chosen to skip.
	/// </summary>
	string? SkippedReleaseTagName { get; }

	/// <summary>
	/// The date and time that the updater last checked for updates.
	/// </summary>
	DateTime? LastCheckedForUpdates { get; }

	/// <summary>
	/// Gets the releases in the current <see cref="ReleaseChannel"/> that have not been installed.
	/// </summary>
	/// <returns>
	/// The releases, sorted by semver in descending order.
	/// </returns>
	Task<List<ReleaseInfo>> GetNotInstalledReleases();

	/// <summary>
	/// Downloads the given release.
	/// </summary>
	/// <param name="release"></param>
	/// <returns>
	/// The path to the downloaded release.
	/// </returns>
	Task<Result<string>> DownloadRelease(Release release);

	/// <summary>
	/// Installs the downloaded release, if one is downloaded - see <see cref="DownloadRelease"/>.
	/// </summary>
	/// <returns></returns>
	Task InstallDownloadedRelease();

	/// <summary>
	/// Skips the given release.
	/// </summary>
	/// <param name="release"></param>
	void SkipRelease(Release release);

	/// <summary>
	/// Checks for updates. If there are updates, shows the updater window.
	/// </summary>
	Task CheckForUpdates();

	/// <summary>
	/// Close the updater window.
	/// </summary>
	void CloseUpdaterWindow();
}
