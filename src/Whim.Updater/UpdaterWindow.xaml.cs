using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;

namespace Whim.Updater;

public sealed partial class UpdaterWindow : Window
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
	private readonly IUpdaterPlugin _plugin;

	// TODO: move into the view model
	public string LastCheckedForUpdates => _plugin.LastCheckedForUpdates?.ToString() ?? "Never";

	// TODO: number of releases, in the view model.

	public UpdaterWindow(IUpdaterPlugin plugin)
	{
		_plugin = plugin;
		UIElementExtensions.InitializeComponent(this, "Whim.Updater", "UpdaterWindow");
	}

	public async Task Activate(List<ReleaseInfo> releases)
	{
		if (releases.Count == 0)
		{
			return;
		}

		await UpdaterWebView.EnsureCoreWebView2Async();
		UpdaterWebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
		UpdaterWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
		UpdaterWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

		// TODO: Move into the view model.
		// TODO: Unsubscribe in dispose
		UpdaterWebView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;

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

		string html = _htmlTemplate.Replace("<!-- CONTENT -->", contentBuilder.ToString());

		UpdaterWebView.CoreWebView2.NavigateToString(html);

		Activate();
	}

	private void CoreWebView2_NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
	{
		if (args.IsUserInitiated)
		{
			args.Cancel = true;
			Process.Start(new ProcessStartInfo(args.Uri) { UseShellExecute = true });
		}
	}
}
