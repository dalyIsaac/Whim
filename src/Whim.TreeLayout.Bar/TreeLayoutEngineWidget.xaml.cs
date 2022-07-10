using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Whim.Bar;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Whim.TreeLayout.Bar;

/// <summary>
/// Interaction logic for TreeLayoutEngineWidget.
/// </summary>
public sealed partial class TreeLayoutEngineWidget : UserControl
{
	/// <summary>
	/// The tree layout engine widget.
	/// </summary>
	public TreeLayoutEngineWidgetViewModel ViewModel { get; }

	internal TreeLayoutEngineWidget(IConfigContext configContext, IMonitor monitor, Microsoft.UI.Xaml.Window window)
	{
		ViewModel = new TreeLayoutEngineWidgetViewModel(configContext, monitor);
		window.Closed += Window_Closed;
		UIElementExtensions.InitializeComponent(this, "Whim.TreeLayout.Bar", "TreeLayoutEngineWidget");
	}

	private void Window_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
	{
		ViewModel.Dispose();
	}

	/// <summary>
	/// Create the tree layout engine bar component.
	/// </summary>
	public static BarComponent CreateComponent()
	{
		return new BarComponent((configContext, monitor, window) => new TreeLayoutEngineWidget(configContext, monitor, window));
	}
}
