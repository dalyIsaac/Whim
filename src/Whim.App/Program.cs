using System;
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
		configContext.PluginManager.RegisterPlugin(new DashboardPlugin(configContext));

		return configContext;
	}
}
