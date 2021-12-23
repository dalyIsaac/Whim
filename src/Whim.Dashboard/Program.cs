using System;
using Whim.Core;

namespace Whim.Dashboard;

public class Program
{
	[STAThread]
	public static void Main()
	{
		WhimManager whim = new(CreateConfigContext());
		try
		{
			whim.Initialize();
		}
		catch (Exception e)
		{
			Logger.Fatal(e.ToString());
			whim.Dispose();
			return;
		}

		App application = new(whim);
		application.InitializeComponent();
		application.Run();
	}

	private static ConfigContext CreateConfigContext()
	{
		ConfigContext configContext = new();

		for (int i = 0; i < 4; i++)
		{
			Workspace workspace = new(configContext, i.ToString(), new ColumnLayoutEngine());
			configContext.WorkspaceManager.Add(workspace);
		}

		return configContext;
	}
}
