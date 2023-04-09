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
	/// <summary>
	/// This will be initialized in <see cref="OnLaunched"/>.
	/// </summary>
	private IContext? _context;

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
			_context = Engine.CreateContext();

			_context.Exited += Context_Exited;
			_context.Initialize();

			return;
		}
		catch (Exception ex)
		{
			_context?.Exit(new ExitEventArgs() { Reason = ExitReason.Error, Message = ex.ToString() });
		}
	}

	private void Context_Exited(object? sender, ExitEventArgs e)
	{
		if (_context is not null)
		{
			_context.Exited -= Context_Exited;
			_context = null;
		}

		if (e.Reason == ExitReason.User)
		{
			Exit();
		}
		else
		{
			new StartupExceptionWindow(e).Activate();
		}
	}

	private void Application_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
	{
		Logger.Error(e.Exception.ToString());
		_context?.Exit();
	}

	// Add when Windows App SDK supports the application exit event.
	// https://docs.microsoft.com/en-us/windows/winui/api/microsoft.ui.xaml.application?view=winui-3.0
	//private void Application_Exit(object sender, ExitEventArgs e)
	//{
	//	Logger.Information("Application exiting");
	//	_context.Quit();
	//	Logger.Information("Application disposed");
	//}
}
