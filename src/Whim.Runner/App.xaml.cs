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

	/// <summary>
	/// An exception which occurred during startup.
	/// </summary>
	private readonly Exception? _startupException;

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

	/// <inheritdoc/>
	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		try
		{
			if (_startupException != null)
			{
				throw _startupException;
			}

			_configContext.Exited += ConfigContext_Exited;
			_configContext.Initialize();
			return;
		}
		catch (Exception ex)
		{
			_configContext.Exit(new ExitEventArgs(ExitReason.Error, ex.ToString()));
		}
	}

	private void ConfigContext_Exited(object? sender, ExitEventArgs e)
	{
		if (e.Reason == ExitReason.Error)
		{
			new StartupExceptionWindow(e).Activate();
		}
		else
		{
			Exit();
		}
	}

	private void Application_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
	{
		Logger.Error(e.Exception.ToString());
		_configContext.Exit();
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
