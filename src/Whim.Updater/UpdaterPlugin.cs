using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Octokit;

namespace Whim.Updater;

/// <inheritdoc />
public class UpdaterPlugin : IUpdaterPlugin
{
	private const string Owner = "dalyIsaac";
	private const string Repository = "Whim";

	/// <summary>
	/// Notification ID for showing the updater window.
	/// </summary>
	private string SHOW_WINDOW_NOTIFICATION_ID => $"{Name}.show_window";

	/// <summary>
	/// Notification ID for not performing an update.
	/// </summary>
	private string CANCEL_NOTIFICATION_ID => $"{Name}.cancel";

	private readonly IContext _context;
	private readonly Version _currentVersion;
	private readonly UpdaterConfig _config;

	private UpdaterWindow? _updaterWindow;
	private readonly string _architecture = RuntimeInformation.ProcessArchitecture.ToString().ToLower();

	/// <summary>
	/// The release that the user has chosen to skip.
	/// </summary>
	private string? _skippedReleaseTagName;

	private readonly Timer _timer = new();

	private List<ReleaseInfo> _notInstalledReleases = new();
	private bool _disposedValue;

	/// <inheritdoc />
	public DateTime? LastCheckedForUpdates { get; private set; }

	/// <inheritdoc />
	public string Name => "whim.updater";

	/// <inheritdoc />
	public IPluginCommands PluginCommands => new UpdaterCommands(this);

	/// <summary>
	/// Initializes a new instance of <see cref="UpdaterPlugin"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="config"></param>
	public UpdaterPlugin(IContext context, UpdaterConfig config)
	{
		_context = context;
		_config = config;

#if DEBUG
		_currentVersion = Version.Parse("v0.1.263-alpha+bc5c56c4")!;

		if (_currentVersion == null)
		{
			throw new Exception("Failed to parse version");
		}
#else
		_currentVersion = new Version(Assembly.GetExecutingAssembly().GetName().Version!.ToString())!;
#endif

		_timer = config.UpdateFrequency.GetTimer();
	}

	private IGitHubClient CreateGitHubClient() => new GitHubClient(new ProductHeaderValue(Name));

	/// <inheritdoc />
	public void PreInitialize()
	{
		_context.NotificationManager.Register(SHOW_WINDOW_NOTIFICATION_ID, HandleOnShowWindowNotification);
		_context.NotificationManager.Register(CANCEL_NOTIFICATION_ID, HandleOnCancelNotification);
	}

	private void HandleOnShowWindowNotification(AppNotificationActivatedEventArgs args)
	{
		Logger.Debug("Showing update window");

		_context
			.NativeManager
			.TryEnqueue(async () =>
			{
				_updaterWindow = new UpdaterWindow(plugin: this, null);
				await _updaterWindow.Activate(_notInstalledReleases).ConfigureAwait(true);
			});
	}

	private void HandleOnCancelNotification(AppNotificationActivatedEventArgs args)
	{
		Logger.Debug("User cancelled update");
	}

	/// <inheritdoc />
	public void PostInitialize()
	{
#if !DEBUG
		CheckForUpdates().ConfigureAwait(true);
#endif

		_timer.Elapsed += Timer_Elapsed;
		_timer.Start();
	}

	private async void Timer_Elapsed(object? sender, ElapsedEventArgs e) =>
		await CheckForUpdates().ConfigureAwait(true);

	/// <inheritdoc />
	public void SkipRelease(Release release)
	{
		_skippedReleaseTagName = release.TagName;
	}

	/// <inheritdoc />
	public async Task CheckForUpdates()
	{
		Logger.Debug("Checking for updates...");

		_notInstalledReleases = await GetNotInstalledReleases().ConfigureAwait(true);
		if (_notInstalledReleases.Count == 0)
		{
			Logger.Debug("No updates found");
			return;
		}

		ReleaseInfo lastRelease = _notInstalledReleases[0];
		if (_skippedReleaseTagName == lastRelease.Release.TagName)
		{
			Logger.Debug($"Skipping release {lastRelease.Release.TagName}");
			return;
		}

		Logger.Debug($"Found {lastRelease.Release.TagName}");

		AppNotification notification = new AppNotificationBuilder()
			.AddArgument(INotificationManager.NotificationIdKey, SHOW_WINDOW_NOTIFICATION_ID)
			.AddText("Update available!")
			.AddText(lastRelease.Release.TagName)
			.AddButton(
				new AppNotificationButton("Not now").AddArgument(
					INotificationManager.NotificationIdKey,
					CANCEL_NOTIFICATION_ID
				)
			)
			.AddButton(
				new AppNotificationButton("Open changelog").AddArgument(
					INotificationManager.NotificationIdKey,
					SHOW_WINDOW_NOTIFICATION_ID
				)
			)
			.BuildNotification();

		_context.NotificationManager.SendToastNotification(notification);
	}

	/// <inheritdoc />
	public void CloseUpdaterWindow()
	{
		_updaterWindow?.Close();
		_updaterWindow = null;
	}

	/// <inheritdoc />
	public void LoadState(JsonElement state)
	{
		try
		{
			SavedUpdaterPluginState savedState = JsonSerializer.Deserialize<SavedUpdaterPluginState>(
				state.GetRawText()
			)!;
			_skippedReleaseTagName = savedState.SkippedReleaseTagName;
			LastCheckedForUpdates = savedState.LastCheckedForUpdates;
		}
		catch (Exception e)
		{
			Logger.Error($"Failed to deserialize saved state: {e}");
		}
	}

	/// <inheritdoc />
	public JsonElement? SaveState()
	{
		if (_skippedReleaseTagName == null)
		{
			return null;
		}

		return JsonSerializer.SerializeToElement(
			new SavedUpdaterPluginState(_skippedReleaseTagName, LastCheckedForUpdates)
		);
	}

	/// <inheritdoc />
	public async Task<List<ReleaseInfo>> GetNotInstalledReleases()
	{
		Logger.Debug("Getting not installed releases");
		LastCheckedForUpdates = DateTime.Now;

		IReadOnlyList<Release> releases = await CreateGitHubClient()
			.Repository
			.Release
			.GetAll(Owner, Repository, new ApiOptions() { PageSize = 100 })
			.ConfigureAwait(false);

		// Sort the releases by semver
		List<ReleaseInfo> sortedReleases = new();
		foreach (Release r in releases)
		{
			Version? version = Version.Parse(r.TagName);
			if (version == null)
			{
				Logger.Debug($"Invalid release tag: {r.TagName}");
				continue;
			}

			ReleaseInfo info = new(r, version);
			if (info.Version.ReleaseChannel != _config.ReleaseChannel)
			{
				Logger.Debug($"Ignoring release for channel {info.Version.ReleaseChannel}");
				continue;
			}

			if (info.Version.IsNewerVersion(_currentVersion))
			{
				sortedReleases.Add(info);
			}
		}
		sortedReleases.Sort((a, b) => a.Version.IsNewerVersion(b.Version) ? -1 : 1);

		Logger.Debug($"Found {sortedReleases.Count} releases");

		return sortedReleases;
	}

	/// <inheritdoc />
	public async Task InstallRelease(Release release)
	{
		// Get the release asset to install.
		IReadOnlyList<ReleaseAsset> assets = await CreateGitHubClient()
			.Repository
			.Release
			.GetAllAssets(Owner, Repository, release.Id)
			.ConfigureAwait(false);

		string assetNameStart = $"WhimInstaller-{_architecture}";

		ReleaseAsset? asset = assets.First(a => a.Name.StartsWith(assetNameStart) && a.Name.EndsWith(".exe"));
		if (asset == null)
		{
			Logger.Debug($"No asset found for release {release}");
			return;
		}

		string tempPath = Path.Combine(Path.GetTempPath(), asset.Name);
		Uri requestUri = new(asset.BrowserDownloadUrl);

		// Download the asset.
		try
		{
			await DownloadRelease(tempPath, requestUri).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to download release: {ex}");
			return;
		}

		// Run the installer.
		using Process process = new();
		process.StartInfo.FileName = tempPath;
		process.Start();
		await process.WaitForExitAsync().ConfigureAwait(false);

		if (process.ExitCode != 0)
		{
			Logger.Error($"Installer exited with code {process.ExitCode}");
			return;
		}

		// Exit Whim.
		_context.NativeManager.TryEnqueue(() => _context.Exit(new ExitEventArgs() { Reason = ExitReason.Update }));
	}

	private static async Task DownloadRelease(string tempPath, Uri requestUri)
	{
		using HttpClient httpClient = new();
		using HttpResponseMessage response = await httpClient.GetAsync(requestUri).ConfigureAwait(false);

		// Save the asset to a temporary file.
		using Stream streamToReadFrom = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
		using Stream streamToWriteTo = File.Open(tempPath, System.IO.FileMode.Create);
		await streamToReadFrom.CopyToAsync(streamToWriteTo).ConfigureAwait(false);
		streamToWriteTo.Close();
		streamToReadFrom.Close();
	}

	/// <inheritdoc />
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_timer.Elapsed -= Timer_Elapsed;
				_timer.Dispose();
				_updaterWindow?.Close();
				_updaterWindow = null;
			}

			_disposedValue = true;
		}
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
