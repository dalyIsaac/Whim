using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Whim.Core.ConfigContext;
using Whim.Core.Logging;

namespace Whim;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	private readonly WhimManager _whimManager;
	private readonly MainWindow _mainWindow;

	internal App(WhimManager whim) : base()
	{
		Logger.Debug("App.ctor()");
		_whimManager = whim;
		_mainWindow = new MainWindow(_whimManager.ConfigContext);
	}

	private void Application_Startup(object sender, StartupEventArgs e)
	{
		Logger.Information("Application starting");
		_mainWindow.Show();
	}

	private void Application_Exit(object sender, ExitEventArgs e)
	{
		Logger.Information("Application exiting");
		_whimManager.Dispose();
		Logger.Information("Application disposed");
	}
}
