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
/// <c>IContext</c> consists of managers which contain and control Whim's state, and
/// functionality. <br/>
///
/// <c>IContext</c> also contains other associated state and functionality, like the
/// <see cref="Logger"/>.
/// </summary>
public interface IContext
{
	/// <inheritdoc cref="Logger" />
	Logger Logger { get; }

	/// <inheritdoc cref="UncaughtExceptionHandling" />
	UncaughtExceptionHandling UncaughtExceptionHandling { get; set; }

	/// <inheritdoc cref="IResourceManager" />
	IResourceManager ResourceManager { get; }

	/// <inheritdoc cref="IButler"/>
	IButler Butler { get; }

	/// <inheritdoc cref="IWorkspaceManager" />
	IWorkspaceManager WorkspaceManager { get; }

	/// <inheritdoc cref="IWindowManager" />
	IWindowManager WindowManager { get; }

	/// <inheritdoc cref="IMonitorManager" />
	IMonitorManager MonitorManager { get; }

	/// <inheritdoc cref="IRouterManager" />
	IRouterManager RouterManager { get; }

	/// <inheritdoc cref="IFilterManager" />
	IFilterManager FilterManager { get; }

	/// <inheritdoc cref="ICommandManager" />
	ICommandManager CommandManager { get; }

	/// <inheritdoc cref="IKeybindManager" />
	IKeybindManager KeybindManager { get; }

	/// <inheritdoc cref="IPluginManager" />
	IPluginManager PluginManager { get; }

	/// <inheritdoc cref="INativeManager" />
	INativeManager NativeManager { get; }

	/// <inheritdoc cref="IFileManager" />
	IFileManager FileManager { get; }

	/// <inheritdoc cref="INotificationManager" />
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
