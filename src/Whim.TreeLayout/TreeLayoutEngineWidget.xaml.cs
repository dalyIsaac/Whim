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
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Whim.TreeLayout.BarComponent;

/// <summary>
/// Interaction logic for TreeLayoutEngineWidget.xaml
/// </summary>
public sealed partial class TreeLayoutEngineWidget : UserControl
{
	internal TreeLayoutEngineWidget()
	{
		UIElementExtensions.InitializeComponent(this, "Whim.TreeLayout", "TreeLayoutEngineWidget");
	}

	public static BarComponent CreateComponent() => new BarComponent((configContext, monitor, window) => new TreeLayoutEngineWidget(configContext));
}
