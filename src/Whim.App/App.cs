using System.Windows;

namespace Whim.App;

public class App : Application
{
	private readonly IConfigContext _configContext;

	internal App(IConfigContext configContext)
	{
		Logger.Debug("Starting application...");

		_configContext = configContext;
		Exit += Application_Exit;

		_configContext.Initialize();
		Run();
	}

	private void Application_Exit(object sender, ExitEventArgs e)
	{
		Logger.Information("Application exiting");
		_configContext.Quit();
		Logger.Information("Application disposed");
	}
}
