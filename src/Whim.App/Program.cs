using System;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Whim.Dashboard;

namespace Whim.App;

public class Program
{
	[STAThread]
	public static void Main()
	{
		_ = new App(CreateConfigContext());
	}

	private static ConfigContext CreateConfigContext()
	{
		ConfigContext configContext = new();

		// Add workspaces
		for (int i = 0; i < 4; i++)
		{
			Workspace workspace = new(configContext, i.ToString(), new ColumnLayoutEngine());
			configContext.WorkspaceManager.Add(workspace);
		}

		// Add plugins
		DashboardPlugin dashboardPlugin = new(configContext);

		configContext.PluginManager.RegisterPlugin(dashboardPlugin);
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_F12), (args) => dashboardPlugin.Toggle());

		return configContext;
	}
}
