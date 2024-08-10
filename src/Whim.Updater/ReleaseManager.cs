using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DotNext;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Octokit;

namespace Whim.Updater;

internal record DownloadedRelease(string Path, Release Release);

/// <summary>
/// Searches for, downloads, and installs updates for Whim.
/// </summary>
internal class ReleaseManager
{
	private const string Owner = "dalyIsaac";
	private const string Repository = "Whim";

	private readonly IContext _ctx;
	private readonly UpdaterPlugin _plugin;

	private readonly string _architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLower();

	/// <summary>
	/// The downloaded release, if one is downloaded.
	/// </summary>
	private DownloadedRelease? _downloadedRelease;

	/// <summary>
	/// The next release to install.
	/// </summary>
	public Release? NextRelease => NotInstalledReleases.FirstOrDefault()?.Release;

	/// <summary>
	/// The current version of Whim.
	/// </summary>
	public Version CurrentVersion { get; }

	private GitHubClient? _gitHubClient;

	/// <summary>
	/// Lazy-loaded GitHub client.
	/// </summary>
	private GitHubClient GitHubClient
	{
		get
		{
			_gitHubClient ??= new GitHubClient(new ProductHeaderValue(_plugin.Name));
			return _gitHubClient;
		}
	}

	public ReleaseManager(IContext context, UpdaterPlugin plugin)
	{
		_ctx = context;
		_plugin = plugin;
		CurrentVersion = Version.Parse(_ctx.NativeManager.GetWhimVersion())!;
	}

	/// <summary>
	/// The releases after the currently installed version. Sorted by semver in descending order.
	/// </summary>
	public List<ReleaseInfo> NotInstalledReleases { get; private set; } = [];

	/// <summary>
	/// Checks for updates. If updates are found, a notification is shown, and the notifications are stored in
	/// <see cref="NotInstalledReleases"/>.
	/// </summary>
	/// <returns></returns>
	public async Task CheckForUpdates()
	{
		Logger.Debug("Checking for updates...");

		NotInstalledReleases = await GetNotInstalledReleases().ConfigureAwait(true);
		if (NotInstalledReleases.Count == 0)
		{
			Logger.Debug("No updates found");
			return;
		}

		ReleaseInfo lastRelease = NotInstalledReleases[0];
		if (_plugin.SkippedReleaseTagName == lastRelease.Release.TagName)
		{
			Logger.Debug($"Skipping release {lastRelease.Release.TagName}");
			return;
		}

		Logger.Debug($"Found {lastRelease.Release.TagName}");

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
	}

	public async Task<Result<string>> DownloadRelease(Release release)
	{
		Logger.Debug($"Downloading release {release.TagName}");

		// Get the release asset to download.
		string assetNameStart = $"WhimInstaller-{_architecture}";

		ReleaseAsset? asset = null;
		foreach (ReleaseAsset a in release.Assets)
		{
			if (a.Name.StartsWith(assetNameStart) && a.Name.EndsWith(".exe"))
			{
				asset = a;
				break;
			}
		}

		if (asset == null)
		{
			return Result.FromException<string>(new WhimException($"No asset found for release {release}"));
		}

		string tempPath = Path.Combine(Path.GetTempPath(), asset.Name);
		Uri requestUri = new(asset.BrowserDownloadUrl);

		// Create the progress notification.
		AppNotificationProgressData progress =
			new(0) { Title = $"Downloading {release.TagName}", Status = "Downloading..." };

		AppNotification downloadNotification = new AppNotificationBuilder()
			.AddText("Downloading update...")
			.AddProgressBar(
				new AppNotificationProgressBar().BindTitle().BindStatus().BindValue().BindValueStringOverride()
			)
			.AddArgument(INotificationManager.NotificationIdKey, _plugin.SKIP_UPDATE_NOTIFICATION_ID)
			.BuildNotification();

		downloadNotification.Progress = progress;
		_ctx.NotificationManager.SendToastNotification(downloadNotification);

		// Download the asset.
		try
		{
			await _ctx.NativeManager.DownloadFileAsync(requestUri, tempPath).ConfigureAwait(false);
			_downloadedRelease = new DownloadedRelease(tempPath, release);
			progress.Status = "Download complete";
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to download release: {ex}");
			return Result.FromException<string>(ex);
		}

		await _ctx.NotificationManager.ClearToastNotification(downloadNotification.Id).ConfigureAwait(false);

		// Create the installation notification.
		AppNotification installNotification = new AppNotificationBuilder()
			.AddText("Install update now?")
			.AddText(release.TagName)
			.AddButton(
				new AppNotificationButton("Install").AddArgument(
					INotificationManager.NotificationIdKey,
					_plugin.INSTALL_NOTIFICATION_ID
				)
			)
			.AddButton(
				new AppNotificationButton("Not now").AddArgument(
					INotificationManager.NotificationIdKey,
					_plugin.CANCEL_INSTALL_NOTIFICATION_ID
				)
			)
			.BuildNotification();

		_ctx.NotificationManager.SendToastNotification(installNotification);

		return Result.FromValue(tempPath);
	}

	public async Task InstallDownloadedRelease()
	{
		if (_downloadedRelease == null)
		{
			Logger.Error("No downloaded release to install");
			return;
		}

		Logger.Debug($"Installing release {_downloadedRelease.Release.TagName} from path {_downloadedRelease.Path}");

		try
		{
			await _ctx.NativeManager.RunFileAsync(_downloadedRelease.Path).ConfigureAwait(false);
			_ctx.NativeManager.TryEnqueue(() => _ctx.Exit(new ExitEventArgs() { Reason = ExitReason.Update }));
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to run installer: {ex}");
			return;
		}
	}

	public async Task<List<ReleaseInfo>> GetNotInstalledReleases()
	{
		Logger.Debug("Getting not installed releases");

		_plugin.LastCheckedForUpdates = DateTime.Now;
		_ctx.Store.Dispatch(new SaveStateTransform());

		IReadOnlyList<Release> releases = await GitHubClient
			.Repository.Release.GetAll(Owner, Repository, new ApiOptions() { PageSize = 100 })
			.ConfigureAwait(false);

		// Sort the releases by semver
		List<ReleaseInfo> sortedReleases = [];
		foreach (Release r in releases)
		{
			Version? version = Version.Parse(r.TagName);
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

			if (CurrentVersion == null || info.Version.IsNewerVersion(CurrentVersion))
			{
				sortedReleases.Add(info);
			}
		}

		// Sort the releases by semver in descending order.
		sortedReleases.Sort((a, b) => a.Version.IsNewerVersion(b.Version) ? -1 : 1);

		Logger.Debug($"Found {sortedReleases.Count} releases");

		return sortedReleases;
	}
}
