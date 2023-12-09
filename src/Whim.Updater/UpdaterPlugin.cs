using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Octokit;
using Serilog;
using Windows.System.UserProfile;

namespace Whim.Updater;

public class UpdaterPlugin : IUpdaterPlugin
{
	private const string Owner = "dalyIsaac";
	private const string Repository = "Whim";

	private readonly IContext _context;
	private readonly ReleaseInfo _currentRelease;
	private readonly UpdaterConfig _config;

	// TODO: This doesn't need to exist all the time.
	private readonly IGitHubClient _client;
	private readonly Architecture _architecture = RuntimeInformation.ProcessArchitecture;

	public string Name => "Whim.Updater";

	public UpdaterPlugin(IContext context, UpdaterConfig config)
	{
		_context = context;
		_config = config;
		_currentRelease = ReleaseInfo.Create(Assembly.GetExecutingAssembly().GetName().Version!.ToString())!;
		_client = new GitHubClient(new ProductHeaderValue(Name));
	}

	public async Task<ReleaseInfo?> GetProposedRelease()
	{
		IReadOnlyList<Release> releases = await _client
			.Repository
			.Release
			.GetAll(Owner, Repository)
			.ConfigureAwait(false);

		// Sort the releases by semver
		List<ReleaseInfo> sortedReleases = new();
		foreach (Release r in releases)
		{
			ReleaseInfo? info = ReleaseInfo.Create(r.TagName, r);
			if (info == null)
			{
				Logger.Debug($"Invalid release tag: {r.TagName}");
				continue;
			}

			if (info.IsNewerRelease(_currentRelease))
			{
				sortedReleases.Add(info);
			}
		}
		sortedReleases.Sort((a, b) => a.IsNewerRelease(b) ? -1 : 1);

		// TODO: Check pagination
		// Find the newest release that matches the current release channel
		ReleaseInfo? proposedRelease = null;
		foreach (ReleaseInfo r in sortedReleases)
		{
			if (r.Channel == _config.ReleaseChannel)
			{
				proposedRelease = r;
				break;
			}
		}

		if (proposedRelease == null)
		{
			Logger.Debug($"No release found for channel {_config.ReleaseChannel}");
			return null;
		}

		return proposedRelease;
	}

	public async Task<string> GetReleaseNotes(Release release)
	{
		string? releaseNotes = release.Body;
		if (string.IsNullOrWhiteSpace(releaseNotes))
		{
			Logger.Debug($"No release notes found for {release}");
			return string.Empty;
		}

		return releaseNotes;
	}

	public async Task<string> DownloadChangelog(Release release)
	{
		IReadOnlyList<RepositoryContent> pathContents = await _client
			.Repository
			.Content
			.GetAllContents(Owner, Repository, "CHANGELOG.md")
			.ConfigureAwait(false);

		if (pathContents.Count == 0)
		{
			Logger.Debug($"No changelog found for {release}");
			return string.Empty;
		}

		return pathContents[0].Content;
	}

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
}
