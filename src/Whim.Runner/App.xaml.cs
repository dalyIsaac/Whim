using Microsoft.UI.Xaml;
using System;
using System.Reflection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Whim.Runner;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
	/// <summary>
	/// This will be initialized in <see cref="OnLaunched"/>.
	/// </summary>
	private IConfigContext? _configContext;

	/// <summary>
	/// Initializes the Whim application.
	/// </summary>
	public App()
	{
		Logger.Debug("Starting application...");

		//Exit += Application_Exit;
		UnhandledException += Application_UnhandledException;

		InitializeComponent();
	}

	/// <inheritdoc/>
	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		StartWhim();
	}

	private void StartWhim()
	{
		try
		{
			_configContext = Engine.CreateConfigContext(Assembly.GetAssembly(typeof(Program)));

			_configContext.Exited += ConfigContext_Exited;
			_configContext.Initialize();

			return;
		}
		catch (Exception ex)
		{
			_configContext?.Exit(new ExitEventArgs(ExitReason.Error, ex.ToString()));
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
		_configContext?.Exit();
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
