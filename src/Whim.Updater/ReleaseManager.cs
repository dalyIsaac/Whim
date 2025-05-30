using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Octokit;

namespace Whim.Updater;

internal record DownloadedRelease(string Path, Release Release);

/// <summary>
/// Searches for, downloads, and installs updates for Whim.
/// </summary>
internal class ReleaseManager(IContext context, UpdaterPlugin plugin)
{
	private const string Owner = "dalyIsaac";
	private const string Repository = "Whim";

	public const string ChangelogUrl = $"https://github.com/{Owner}/{Repository}/releases/";

	private readonly IContext _ctx = context;
	private readonly UpdaterPlugin _plugin = plugin;

	/// <summary>
	/// The next release to install.
	/// </summary>
	public Release? NextRelease => NotInstalledReleases.FirstOrDefault()?.Release;

	private IGitHubClient? _gitHubClient;

	/// <summary>
	/// Lazy-loaded GitHub client.
	/// </summary>
	internal IGitHubClient GitHubClient
	{
		get
		{
			_gitHubClient ??= new GitHubClient(new ProductHeaderValue(_plugin.Name));
			return _gitHubClient;
		}
		init => _gitHubClient = value;
	}

	/// <summary>
	/// The releases after the currently installed version. Sorted by semver in descending order.
	/// </summary>
	public List<ReleaseInfo> NotInstalledReleases { get; private set; } = [];

	/// <summary>
	/// Checks for updates. If updates are found, a notification is shown, and the notifications are stored in
	/// <see cref="NotInstalledReleases"/>.
	/// </summary>
	/// <param name="notifyIfNoUpdates">
	/// Whether to show a notification if there are no updates.
	/// </param>
	/// <returns></returns>
	public async Task CheckForUpdates(bool notifyIfNoUpdates)
	{
		Logger.Debug("Checking for updates...");

		NotInstalledReleases = await GetNotInstalledReleases().ConfigureAwait(true);
		if (NotInstalledReleases.Count == 0)
		{
			Logger.Debug("No updates found");

			if (notifyIfNoUpdates)
			{
				_ctx.NativeManager.TryEnqueue(() =>
				{
					AppNotification notification = new AppNotificationBuilder()
						.AddText("No updates found.")
						.AddText("Current version: " + _ctx.NativeManager.GetWhimVersion())
						.BuildNotification();

					_ctx.NotificationManager.SendToastNotification(notification);
				});
			}
			return;
		}

		ReleaseInfo lastRelease = NotInstalledReleases[0];
		if (_plugin.SkippedReleaseTagName == lastRelease.Release.TagName)
		{
			Logger.Debug($"Skipping release {lastRelease.Release.TagName}");
			return;
		}

		Logger.Debug($"Found {lastRelease.Release.TagName}");

		_ctx.NativeManager.TryEnqueue(() =>
		{
			AppNotification notification = new AppNotificationBuilder()
				.AddArgument(INotificationManager.NotificationIdKey, _plugin.OPEN_CHANGELOG_NOTIFICATION_ID)
				.AddText("Update available!")
				.AddText(lastRelease.Release.TagName)
				.AddButton(
					new AppNotificationButton("Not now").AddArgument(
						INotificationManager.NotificationIdKey,
						_plugin.DEFER_UPDATE_NOTIFICATION_ID
					)
				)
				.AddButton(
					new AppNotificationButton("Skip").AddArgument(
						INotificationManager.NotificationIdKey,
						_plugin.SKIP_UPDATE_NOTIFICATION_ID
					)
				)
				.AddButton(
					new AppNotificationButton("Open changelog").AddArgument(
						INotificationManager.NotificationIdKey,
						_plugin.OPEN_CHANGELOG_NOTIFICATION_ID
					)
				)
				.BuildNotification();

			_ctx.NotificationManager.SendToastNotification(notification);
		});
	}

	/// <summary>
	/// Gets the releases in the current <see cref="ReleaseChannel"/> that have not been installed.
	/// </summary>
	/// <returns>
	/// The releases, sorted by semver in descending order.
	/// </returns>
	public async Task<List<ReleaseInfo>> GetNotInstalledReleases()
	{
		Logger.Debug("Getting not installed releases");

		_plugin.LastCheckedForUpdates = DateTime.Now;
		_ctx.Store.WhimDispatch(new SaveStateTransform());

		Version currentVersion = Version.ParseProductVersion(_ctx.NativeManager.GetWhimVersion())!;

		IReadOnlyList<Release> releases = await GitHubClient
			.Repository.Release.GetAll(Owner, Repository, new ApiOptions() { PageSize = 100 })
			.ConfigureAwait(false);

		// Sort the releases by semver
		List<ReleaseInfo> sortedReleases = [];
		foreach (Release r in releases)
		{
			Version? version = Version.ParseTag(r.TagName);
			if (version == null)
			{
				Logger.Debug($"Invalid release tag: {r.TagName}");
				continue;
			}

			ReleaseInfo info = new(r, version);
			if (info.Version.ReleaseChannel != _plugin.Config.ReleaseChannel)
			{
				Logger.Debug($"Ignoring release for channel {info.Version.ReleaseChannel}");
				continue;
			}

			if (currentVersion == null || info.Version.IsNewerVersion(currentVersion))
			{
				sortedReleases.Add(info);
			}
		}

		// Sort the releases by semver in descending order.
		sortedReleases.Sort(static (a, b) => a.Version.IsNewerVersion(b.Version) ? -1 : 1);

		Logger.Debug($"Found {sortedReleases.Count} releases");

		return sortedReleases;
	}
}
