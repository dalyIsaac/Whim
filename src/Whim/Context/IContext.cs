using System;

namespace Whim;

/// <summary>
/// Ways to handle uncaught exceptions.
/// </summary>
public enum UncaughtExceptionHandling
{
	/// <summary>
	/// Log the error and continue.
	/// </summary>
	Log,

	/// <summary>
	/// Shutdown and show the user an error message.
	/// </summary>
	Shutdown
}

/// <summary>
/// This is the core of Whim. <br/>
///
/// <c>IContext</c> consists of managers which contain and control Whim's state, and thus
/// functionality. <br/>
///
/// <c>IContext</c> also contains other associated state and functionality, like the
/// <see cref="Logger"/>.
/// </summary>
public interface IContext
{
	/// <summary>
	/// Whim's <see cref="Logger"/> instances.
	/// </summary>
	Logger Logger { get; }

	/// <summary>
	/// How to handle uncaught exceptions. Defaults to <see cref="UncaughtExceptionHandling.Log"/>.
	/// </summary>
	UncaughtExceptionHandling UncaughtExceptionHandling { get; set; }

	/// <summary>
	/// Whim's <see cref="IWorkspaceManager"/> instances.
	/// </summary>
	IWorkspaceManager WorkspaceManager { get; }

	/// <summary>
	/// Whim's <see cref="IWindowManager"/> instances.
	/// </summary>
	IWindowManager WindowManager { get; }

	/// <summary>
	/// Whim's <see cref="IMonitorManager"/> instances.
	/// </summary>
	IMonitorManager MonitorManager { get; }

	/// <summary>
	/// Whim's <see cref="IRouterManager"/> instances.
	/// </summary>
	IRouterManager RouterManager { get; }

	/// <summary>
	/// Whim's <see cref="IFilterManager"/> instances.
	/// </summary>
	IFilterManager FilterManager { get; }

	/// <summary>
	/// Whim's <see cref="ICommand"/>s.
	/// </summary>
	ICommandManager CommandManager { get; }

	/// <summary>
	/// Whim's keybinds.
	/// </summary>
	IKeybindManager KeybindManager { get; }

	/// <summary>
	/// Whim's <see cref="IPluginManager"/> instances.
	/// </summary>
	IPluginManager PluginManager { get; }

	/// <summary>
	/// Manager for interacting with native Windows config.
	/// </summary>
	INativeManager NativeManager { get; }

	/// <summary>
	/// Manager to help interacting with the file system.
	/// </summary>
	IFileManager FileManager { get; }

	/// <summary>
	/// Manager for interacting with notifications.
	/// </summary>
	INotificationManager NotificationManager { get; }

	/// <summary>
	/// This will be called by the Whim Runner.
	/// You likely won't need to call it yourself.
	/// </summary>
	/// <exception cref="ConfigLoaderException">
	/// Thrown if the user's config could not be loaded.
	/// </exception>
	void Initialize();

	/// <summary>
	/// Handles an uncaught exception, according to <see cref="UncaughtExceptionHandling"/>.
	/// Place this in a <c>catch</c> block where re-entry can occur - for example, in a
	/// Win32 hook.
	/// </summary>
	/// <param name="procName"></param>
	/// <param name="exception"></param>
	void HandleUncaughtException(string procName, Exception exception);

	/// <summary>
	/// This event is fired when the context is shutting down.
	/// </summary>
	event EventHandler<ExitEventArgs>? Exiting;

	/// <summary>
	/// This event is fired after the context has been shut down.
	/// </summary>
	event EventHandler<ExitEventArgs>? Exited;

	/// <summary>
	/// This is called to shutdown the context.
	/// </summary>
	/// <param name="args">
	/// The shutdown event arguments. If this is not provided, we assume
	/// <see cref="ExitReason.User"/>.
	/// </param>
	void Exit(ExitEventArgs? args = null);
}
