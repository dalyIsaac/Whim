using System;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Microsoft.Windows.AppNotifications;
using Windows.ApplicationModel.Core;

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
		ProcessLaunchActivationArgs();
	}

	private void StartWhim()
	{
		try
		{
			_context = Engine.CreateContext();

			_context.Exited += Context_Exited;
			_context.Initialize();
		}
		catch (Exception ex)
		{
			_context?.Exit(new ExitEventArgs() { Reason = ExitReason.Error, Message = ex.ToString() });
		}
	}

	private void ProcessLaunchActivationArgs()
	{
		// NOTE: AppInstance is ambiguous between
		// Microsoft.Windows.AppLifecycle.AppInstance and
		// Windows.ApplicationModel.AppInstance
		AppInstance currentInstance = AppInstance.GetCurrent();
		if (!currentInstance.IsCurrent)
		{
			return;
		}

		// AppInstance.GetActivatedEventArgs will report the correct ActivationKind,
		// even in WinUI's OnLaunched.
		AppActivationArguments activationArgs = currentInstance.GetActivatedEventArgs();
		if (activationArgs == null)
		{
			return;
		}

		ExtendedActivationKind extendedKind = activationArgs.Kind;
		if (extendedKind != ExtendedActivationKind.AppNotification)
		{
			return;
		}

		AppNotificationActivatedEventArgs notificationActivatedEventArgs = (AppNotificationActivatedEventArgs)
			activationArgs.Data;
		_context?.NotificationManager.ProcessLaunchActivationArgs(notificationActivatedEventArgs);
	}

	private void Context_Exited(object? sender, ExitEventArgs e)
	{
		if (_context is not null)
		{
			_context.Exited -= Context_Exited;
			_context = null;
		}

		if (e.Reason == ExitReason.User || e.Reason == ExitReason.Update)
		{
			Exit();
		}
		else if (e.Reason == ExitReason.Restart)
		{
			Restart();
		}
		else
		{
			new ExceptionWindow(this, e).Activate();
		}
	}

	private void Restart()
	{
		AppRestartFailureReason reason = AppInstance.Restart("");

		switch (reason)
		{
			case AppRestartFailureReason.RestartPending:
				Logger.Debug("Another restart is currently pending.");
				break;
			case AppRestartFailureReason.InvalidUser:
				Logger.Fatal("Current user is not signed in or not a valid user.");
				break;
			case AppRestartFailureReason.Other:
				Logger.Fatal("Failure restarting.");
				break;
		}

		Exit();
	}

	private void Application_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
	{
		Logger.Error(e.Exception.ToString());
		e.Handled = true;

		_context?.HandleUncaughtException("App", e.Exception);

#if DEBUG
		_context?.Exit();
#endif
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
