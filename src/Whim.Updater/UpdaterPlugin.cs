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
using Microsoft.UI.Dispatching;
using Octokit;

namespace Whim.Updater;

/// <inheritdoc />
public class UpdaterPlugin : IUpdaterPlugin
{
	private const string Owner = "dalyIsaac";
	private const string Repository = "Whim";

	private readonly IContext _context;
	private readonly Version _currentVersion;
	private readonly UpdaterConfig _config;

	// TODO: This doesn't need to exist all the time.
	private readonly IGitHubClient _client;
	private UpdaterWindow? _updaterWindow;
	private readonly Architecture _architecture = RuntimeInformation.ProcessArchitecture;

	/// <summary>
	/// The release that the user has chosen to skip.
	/// </summary>
	private string? _skippedReleaseTagName;

	private readonly Timer _timer = new();
	private bool _disposedValue;

	/// <inheritdoc />
	public DateTime? LastCheckedForUpdates { get; private set; }

	/// <inheritdoc />
	public string Name => "Whim.Updater";

	// TODO
	/// <inheritdoc />
	public IPluginCommands PluginCommands => new PluginCommands(Name);

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
		_currentVersion = new Version("v0.1.263-alpha+bc5c56c4");
#else
		_currentVersion = new Version(Assembly.GetExecutingAssembly().GetName().Version!.ToString())!;
#endif

		_client = new GitHubClient(new ProductHeaderValue(Name));

		_timer = config.UpdateFrequency.GetTimer();
		// TODO: Timer subscribe
	}

	/// <inheritdoc />
	public void PreInitialize()
	{
		// TODO
		// TODO: Also unsubscribe
		// _timer.Elapsed +=
	}

	/// <inheritdoc />
	public void PostInitialize()
	{
		// TODO: Destroy the window
		DateTime now = DateTime.Now;
		_updaterWindow = new UpdaterWindow(this, null);
		DispatcherQueue
			.GetForCurrentThread()
			.TryEnqueue(async () =>
			{
				await _updaterWindow.Activate(DateTime.Now, await GetNotInstalledReleases());
				// _updaterWindow.Activate(new List<ReleaseInfo>());
			});
	}

	/// <inheritdoc />
	public void SkipRelease(Release release)
	{
		_skippedReleaseTagName = release.TagName;
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
		LastCheckedForUpdates = DateTime.Now;

		IReadOnlyList<Release> releases = await _client
			.Repository
			.Release
			.GetAll(Owner, Repository, new ApiOptions() { PageSize = 100 })
			.ConfigureAwait(false);

		// Sort the releases by semver
		List<ReleaseInfo> sortedReleases = new();
		foreach (Release r in releases)
		{
			ReleaseInfo info;
			try
			{
				info = new ReleaseInfo(r, new Version(r.TagName), releases.Count > 1);
			}
			catch (Exception ex)
			{
				// TODO: Don't throw, since early releases had an incorrect format.
				Logger.Debug($"Invalid release tag: {r.TagName}: {ex}");
				continue;
			}

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
		IReadOnlyList<ReleaseAsset> assets = await _client
			.Repository
			.Release
			.GetAllAssets(Owner, Repository, release.Id)
			.ConfigureAwait(false);

		string assetNameStart = $"WhimInstaller-{_architecture}-{release}";

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
		Process.Start(tempPath);

		// Exit Whim while the installer runs.
		_context.Exit(new ExitEventArgs() { Reason = ExitReason.Update });
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
				_timer.Dispose();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
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
