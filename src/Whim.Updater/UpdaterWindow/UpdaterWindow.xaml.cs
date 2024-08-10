using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;

namespace Whim.Updater;

internal sealed partial class UpdaterWindow : Window
{
	private readonly IContext _ctx;
	private readonly IUpdaterPlugin _plugin;

	public UpdaterWindowViewModel ViewModel { get; }

	public UpdaterWindow(IContext ctx, IUpdaterPlugin plugin, DateTime? lastCheckedForUpdates)
	{
		_ctx = ctx;
		_plugin = plugin;
		ViewModel = new UpdaterWindowViewModel(_plugin);
		UIElementExtensions.InitializeComponent(this, "Whim.Updater", "UpdaterWindow/UpdaterWindow");

		Closed += UpdaterWindow_Closed;
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

		ViewModel.Update(releases);

		UpdaterWebView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
		UpdaterWebView.CoreWebView2.NavigateToString(ViewModel.ReleaseNotes);

		Activate();
	}

	private static void CoreWebView2_NavigationStarting(
		CoreWebView2 sender,
		CoreWebView2NavigationStartingEventArgs args
	)
	{
		if (args.IsUserInitiated)
		{
			args.Cancel = true;
			Process.Start(new ProcessStartInfo(args.Uri) { UseShellExecute = true });
		}
	}

	private void UpdaterWindow_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
	{
		UpdaterWebView.CoreWebView2.NavigationStarting -= CoreWebView2_NavigationStarting;
		UpdaterWebView.Close();

		_ctx.Store.Dispatch(new SaveStateTransform());
	}
}
