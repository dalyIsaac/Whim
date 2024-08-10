using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using DotNext;
using Microsoft.Windows.AppNotifications;
using Octokit;

namespace Whim.Updater;

/// <inheritdoc />
public class UpdaterPlugin : IUpdaterPlugin
{
	private readonly IContext _ctx;
	private readonly ReleaseManager _releaseManager;

	private UpdaterWindow? _updaterWindow;

	/// <summary>
	/// Notification ID for showing the changelog window.
	/// </summary>
	internal string OPEN_CHANGELOG_NOTIFICATION_ID => $"{Name}.show_window";

	/// <summary>
	/// Notification ID for not performing an update right now.
	/// </summary>
	internal string DEFER_UPDATE_NOTIFICATION_ID => $"{Name}.defer";

	/// <summary>
	/// Notification ID for skipping an update.
	/// </summary>
	internal string SKIP_UPDATE_NOTIFICATION_ID => $"{Name}.cancel";

	/// <summary>
	/// Notification ID for installing an update.
	/// </summary>
	internal string INSTALL_NOTIFICATION_ID => $"{Name}.install";

	/// <summary>
	/// Notification ID for cancelling an update installation.
	/// </summary>
	internal string CANCEL_INSTALL_NOTIFICATION_ID => $"{Name}.cancel_install";

	/// <summary>
	/// The release that the user has chosen to skip.
	/// </summary>
	public string? SkippedReleaseTagName { get; private set; }

	private readonly Timer _timer = new();

	private bool _disposedValue;

	/// <inheritdoc />
	public UpdaterConfig Config { get; }

	/// <inheritdoc />
	public DateTime? LastCheckedForUpdates { get; internal set; }

	/// <summary>
	/// <c>whim.updater</c>
	/// </summary>
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
		_ctx = context;
		Config = config;
		_releaseManager = new ReleaseManager(context, this);

		if (config.UpdateFrequency.GetInterval() is double interval)
		{
			_timer.Interval = interval;
		}
		else
		{
			_timer.Enabled = false;
		}
	}

	/// <inheritdoc />
	public void PreInitialize()
	{
		_ctx.NotificationManager.Register(OPEN_CHANGELOG_NOTIFICATION_ID, OnOpenChangelogNotificationReceived);
		_ctx.NotificationManager.Register(DEFER_UPDATE_NOTIFICATION_ID, OnDeferUpdateNotificationReceived);
		_ctx.NotificationManager.Register(SKIP_UPDATE_NOTIFICATION_ID, OnSkipUpdateNotificationReceived);
		_ctx.NotificationManager.Register(INSTALL_NOTIFICATION_ID, OnInstallNotificationReceived);
	}

	private void OnOpenChangelogNotificationReceived(AppNotificationActivatedEventArgs args)
	{
		Logger.Debug("Showing update window");

		_ctx.NativeManager.TryEnqueue(async () =>
		{
			_updaterWindow = new UpdaterWindow(_ctx, this);
			await _updaterWindow.Activate(_releaseManager.NotInstalledReleases).ConfigureAwait(true);
		});
	}

	private void OnDeferUpdateNotificationReceived(AppNotificationActivatedEventArgs args) =>
		Logger.Debug("Deferring update");

	private void OnSkipUpdateNotificationReceived(AppNotificationActivatedEventArgs args) => SkipRelease();

	private void OnInstallNotificationReceived(AppNotificationActivatedEventArgs args) => InstallDownloadedRelease();

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
	public void SkipRelease(Release? release = null)
	{
		SkippedReleaseTagName = _releaseManager.NextRelease?.TagName;
		_ctx.Store.Dispatch(new SaveStateTransform());
	}

	/// <inheritdoc />
	public Task CheckForUpdates() => _releaseManager.CheckForUpdates();

	/// <inheritdoc />
	public void CloseUpdaterWindow()
	{
		_updaterWindow?.Close();
		_updaterWindow = null;
		_ctx.Store.Dispatch(new SaveStateTransform());
	}

	/// <inheritdoc />
	public void LoadState(JsonElement state)
	{
		try
		{
			SavedUpdaterPluginState savedState = JsonSerializer.Deserialize<SavedUpdaterPluginState>(
				state.GetRawText()
			)!;
			SkippedReleaseTagName = savedState.SkippedReleaseTagName;
			LastCheckedForUpdates = savedState.LastCheckedForUpdates;
		}
		catch (Exception e)
		{
			Logger.Error($"Failed to deserialize saved state: {e}");
		}
	}

	/// <inheritdoc />
	public JsonElement? SaveState() =>
		JsonSerializer.SerializeToElement(new SavedUpdaterPluginState(SkippedReleaseTagName, LastCheckedForUpdates));

	/// <inheritdoc />
	public Task<List<ReleaseInfo>> GetNotInstalledReleases() => _releaseManager.GetNotInstalledReleases();

	/// <inheritdoc />
	public Task<Result<string>> DownloadRelease(Release release) => _releaseManager.DownloadRelease(release);

	/// <inheritdoc />
	public Task InstallDownloadedRelease() => _releaseManager.InstallDownloadedRelease();

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
				_ctx.NotificationManager.Unregister(OPEN_CHANGELOG_NOTIFICATION_ID);
				_ctx.NotificationManager.Unregister(DEFER_UPDATE_NOTIFICATION_ID);
				_ctx.NotificationManager.Unregister(SKIP_UPDATE_NOTIFICATION_ID);
				_ctx.NotificationManager.Unregister(INSTALL_NOTIFICATION_ID);
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
