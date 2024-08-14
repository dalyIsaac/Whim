using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Windows.AppNotifications;

namespace Whim.Updater;

/// <inheritdoc />
public class UpdaterPlugin : IUpdaterPlugin
{
	private readonly IContext _ctx;
	private readonly ReleaseManager _releaseManager;

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
	}

	private void OnOpenChangelogNotificationReceived(AppNotificationActivatedEventArgs args)
	{
		Logger.Debug("Opening GitHub");
		Process.Start(new ProcessStartInfo(ReleaseManager.ChangelogUrl) { UseShellExecute = true });
	}

	private void OnDeferUpdateNotificationReceived(AppNotificationActivatedEventArgs args) =>
		Logger.Debug("Deferring update");

	private void OnSkipUpdateNotificationReceived(AppNotificationActivatedEventArgs args) => SkipRelease();

	/// <inheritdoc />
	public void PostInitialize()
	{
#if !DEBUG
		CheckForUpdates(false).ConfigureAwait(true);
#endif

		_timer.Elapsed += Timer_Elapsed;
		_timer.Start();
	}

	private async void Timer_Elapsed(object? sender, ElapsedEventArgs e) =>
		await CheckForUpdates().ConfigureAwait(true);

	/// <inheritdoc />
	public void SkipRelease(string? tagName = null)
	{
		SkippedReleaseTagName = tagName ?? _releaseManager.NextRelease?.TagName;
		_ctx.Store.Dispatch(new SaveStateTransform());
	}

	/// <inheritdoc />
	public Task CheckForUpdates(bool notifyIfNoUpdates = true) => _releaseManager.CheckForUpdates(notifyIfNoUpdates);

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
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_timer.Elapsed -= Timer_Elapsed;
				_timer.Dispose();
				_ctx.NotificationManager.Unregister(OPEN_CHANGELOG_NOTIFICATION_ID);
				_ctx.NotificationManager.Unregister(DEFER_UPDATE_NOTIFICATION_ID);
				_ctx.NotificationManager.Unregister(SKIP_UPDATE_NOTIFICATION_ID);
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
