using System;
using System.Threading.Tasks;

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
	/// Skips the release with the given tag name.
	/// </summary>
	/// <param name="tagName">
	/// The tag name of the release to skip. If null, skips the latest release.
	/// </param>
	void SkipRelease(string? tagName = null);

	/// <summary>
	/// Checks for updates. If there are updates, shows the updater window.
	/// </summary>
	Task CheckForUpdates();
}
