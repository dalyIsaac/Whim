using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;

namespace Whim.Updater;

public sealed partial class UpdaterWindow : Window
{
	private readonly IUpdaterPlugin _plugin;

	internal ObservableCollection<ReleaseInfo> Releases { get; } = new();

	public UpdaterWindow(IUpdaterPlugin plugin)
	{
		_plugin = plugin;
		UIElementExtensions.InitializeComponent(this, "Whim.Updater", "UpdaterWindow");
	}

	public void Activate(IEnumerable<ReleaseInfo> releases)
	{
		Releases.Clear();
		foreach (ReleaseInfo release in releases)
		{
			Releases.Add(release);
		}

		Activate();
	}
}
