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
		if (!whim.Initialize())
		{
			Logger.Fatal("Initialization failed");
			return;
		}

		App application = new(whim);
		application.InitializeComponent();
		application.Run();
	}
}
