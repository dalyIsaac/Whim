using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Whim.Runner;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
	private static TaskbarIcon? TrayIcon { get; private set; }
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
		UnhandledException += Application_UnhandledException;

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

	private void InitializeTrayIcon()
	{
		XamlUICommand? exitApplicationCommand = (XamlUICommand)Resources["ExitApplicationCommand"];
		exitApplicationCommand.ExecuteRequested += ExitApplicationCommand_ExecuteRequested;

		TrayIcon = (TaskbarIcon)Resources["TrayIcon"];
		TrayIcon.ForceCreate();
	}

	private void ExitApplicationCommand_ExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args)
	{
		_configContext.Quit();
		Exit();
	}

	private void Application_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
	{
		Logger.Error(e.Exception.ToString());
		_configContext.Quit();
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
