using System;
using Whim.Core.ConfigContext;
using Whim.Core.Logging;

namespace Whim;

public class Program
{
	[STAThread]
	public static void Main()
	{
		WhimManager whim = new(new ConfigContext());
		try
		{
			whim.Initialize();
		}
		catch (Exception e)
		{
			Logger.Fatal(e.Message);
			whim.Dispose();
			return;
		}

		App application = new(whim);
		application.InitializeComponent();
		application.Run();
	}
}
