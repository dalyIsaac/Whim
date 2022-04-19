using Microsoft.UI.Xaml;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Whim.Runner;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
	private readonly IConfigContext _configContext;
	private Exception? _startupException;

	/// <summary>
	/// Initializes the Whim application.
	/// </summary>
	/// <param name="configContext">The Whim config context.</param>
	/// <param name="startupException">An exception encountered during startup.</param>
	public App(IConfigContext configContext, Exception? startupException = null)
	{
		Logger.Debug("Starting application...");

		//Exit += Application_Exit;

		InitializeComponent();

		_configContext = configContext;
		_startupException = startupException;
	}

	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		if (_startupException == null)
		{
			try
			{
				_configContext.Initialize();
				return;
			}
			catch (Exception ex)
			{
				_startupException = ex;
			}
		}

		// If we get to here, there's been an error somewhere during startup.
		_configContext.Quit();
		new StartupExceptionWindow(_startupException!).Activate();
	}

	// Add when Windows App SDK supports the application exit event.
	// https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.application?view=winui-3.0
	//private void Application_Exit(object sender, ExitEventArgs e)
	//{
	//	Logger.Information("Application exiting");
	//	_configContext.Quit();
	//	Logger.Information("Application disposed");
	//}
}
