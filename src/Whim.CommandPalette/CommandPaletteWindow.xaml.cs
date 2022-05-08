using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Whim.CommandPalette;

public sealed partial class CommandPaletteWindow : Microsoft.UI.Xaml.Window
{
	private readonly IConfigContext _configContext;
	private readonly CommandPaletteConfig _commandPaletteConfig;
	private readonly IWindow _window;

	public CommandPaletteWindow(IConfigContext configContext, CommandPaletteConfig commandPaletteConfig)
	{
		_configContext = configContext;
		_commandPaletteConfig = commandPaletteConfig;
		this.InitializeBorderlessWindow("Whim.CommandPalette", "CommandPaletteWindow", _configContext);

		Title = CommandPaletteConfig.Title;
	}
}
