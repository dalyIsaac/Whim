using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Markdig;

namespace Whim.Updater;

internal class UpdaterWindowViewModel : INotifyPropertyChanged
{
	private const string _htmlTemplate =
		@"
<!DOCTYPE html>
<html>
	<head>
		<title>Whim Updater</title>
		<meta charset=""utf-8"" />
		<link name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
	</head>

	<style>
		html {
			font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
		}

		:root {
			--border-color: rgb(216. 222, 228);
			font-size: 14px;
		}

		a {
			color: rgb(9, 105, 218);
		}

		@media (prefers-color-scheme: dark) {
			html {
				background: black;
				color: white;
			}

			:root {
				--border-color: rgb(33, 38, 45);
			}

			a {
				color: rgb(47, 129, 247);
			}
		}

		h1, h2, h3 {
			margin: 0;
			margin-top: .2em;
			padding-bottom: .3em;
			border-bottom: 1px solid var(--border-color);
		}

		h1 {
			margin-top: 1em;
		}
	</style>

	<body>
		<!-- CONTENT -->
	</body>
</html>
";

	public event PropertyChangedEventHandler? PropertyChanged;

	private List<ReleaseInfo> _releases = new();

	private string _lastCheckedForUpdates;
	public string LastCheckedForUpdates
	{
		get => _lastCheckedForUpdates;
		private set
		{
			_lastCheckedForUpdates = value;
			OnPropertyChanged();
		}
	}

	public int SkippedReleases => _releases.Count;

	private string _releaseNotes = string.Empty;

	public string ReleaseNotes
	{
		get => _releaseNotes;
		private set
		{
			_releaseNotes = value;
			OnPropertyChanged();
		}
	}

	public ReleaseInfo? LastRelease => _releases.Count > 0 ? _releases[0] : null;

	public SkipReleaseCommand SkipReleaseCommand { get; }

	public InstallReleaseCommand InstallReleaseCommand { get; }

	public CloseUpdaterWindowCommand CloseUpdaterWindowCommand { get; }

	public UpdaterWindowViewModel(IUpdaterPlugin plugin, DateTime? lastCheckedForUpdates)
	{
		SkipReleaseCommand = new SkipReleaseCommand(plugin, this);
		InstallReleaseCommand = new InstallReleaseCommand(plugin, this);
		CloseUpdaterWindowCommand = new CloseUpdaterWindowCommand(plugin);

		_lastCheckedForUpdates = lastCheckedForUpdates?.ToString() ?? "Never";
		ReleaseNotes = "<html><body><h1>Loading...</h1></body></html>";
	}

	public void Update(DateTime lastCheckedForUpdates, List<ReleaseInfo> releases)
	{
		LastCheckedForUpdates = lastCheckedForUpdates.ToString();
		_releases = releases;
		ReleaseNotes = GetReleaseNotes(releases);
	}

	private static string GetReleaseNotes(List<ReleaseInfo> releases)
	{
		MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
			.UseAutoLinks()
			.Use<GitHubUserProfileExtension>()
			.Build();

		StringBuilder contentBuilder = new();
		foreach (ReleaseInfo r in releases)
		{
			contentBuilder.Append($"<h1>{r.Release.Name}</h1>");
			string body = Markdown.ToHtml(r.Release.Body, pipeline);

			contentBuilder.Append(body);
		}

		return _htmlTemplate.Replace("<!-- CONTENT -->", contentBuilder.ToString());
	}

	private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
