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
	internal TreeLayoutEngineWidget(IConfigContext config, IMonitor monitor, Microsoft.UI.Xaml.Window window)
	{
		UIElementExtensions.InitializeComponent(this, "Whim.TreeLayout.Bar", "TreeLayoutEngineWidget");
	}

	/// <summary>
	/// Create the tree layout engine bar component.
	/// </summary>
	public static BarComponent CreateComponent() {
		return new BarComponent((configContext, monitor, window) => new TreeLayoutEngineWidget(configContext, monitor, window));
	}
}
