using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Whim.Updater;

public sealed partial class UpdaterWindow : Window
{
	private readonly IUpdaterPlugin _plugin;

	public ObservableCollection<ReleaseInfo> Releases { get; } = new();

	public UpdaterWindow(IUpdaterPlugin plugin)
	{
		_plugin = plugin;
		this.InitializeComponent();
	}

	public void Activate(List<ReleaseInfo> releases)
	{
		Releases.Clear();
		foreach (ReleaseInfo release in releases)
		{
			Releases.Add(release);
		}

		Show();
	}
}
