using System;

namespace Whim;

/// <summary>
/// This is the core of Whim. <br/>
///
/// <c>IConfigContext</c> consists of managers which contain and control Whim's state, and thus
/// functionality. <br/>
///
/// <c>IConfigContext</c> also contains other associated state and functionality, like the
/// <see cref="Logger"/>.
/// </summary>
public interface IConfigContext
{
	/// <summary>
	/// Whim's <see cref="Logger"/> instances.
	/// </summary>
	public Logger Logger { get; }

	/// <summary>
	/// Whim's <see cref="IWorkspaceManager"/> instances.
	/// </summary>
	public IWorkspaceManager WorkspaceManager { get; }

	/// <summary>
	/// Whim's <see cref="IWindowManager"/> instances.
	/// </summary>
	public IWindowManager WindowManager { get; }

	/// <summary>
	/// Whim's <see cref="IMonitorManager"/> instances.
	/// </summary>
	public IMonitorManager MonitorManager { get; }

	/// <summary>
	/// Whim's <see cref="IRouterManager"/> instances.
	/// </summary>
	public IRouterManager RouterManager { get; }

	/// <summary>
	/// Whim's <see cref="IFilterManager"/> instances.
	/// </summary>
	public IFilterManager FilterManager { get; }

	/// <summary>
	/// Whim's <see cref="ICommandManager"/> instances.
	/// </summary>
	public ICommandManager CommandManager { get; }

	/// <summary>
	/// Whim's <see cref="IPluginManager"/> instances.
	/// </summary>
	public IPluginManager PluginManager { get; }

	/// <summary>
	/// Manager for interacting with native Windows config.
	/// </summary>
	public INativeManager NativeManager { get; }

	/// <summary>
	/// This will be called by the Whim Runner.
	/// You likely won't need to call it yourself.
	/// </summary>
	/// <exception cref="ConfigLoaderException">
	/// Thrown if the user's config could not be loaded.
	/// </exception>
	public void Initialize();

	/// <summary>
	/// This event is fired when the config context is shutting down.
	/// </summary>
	public event EventHandler<ExitEventArgs>? Exiting;

	/// <summary>
	/// This event is fired after the config context has been shut down.
	/// </summary>
	public event EventHandler<ExitEventArgs>? Exited;

	/// <summary>
	/// This is called to shutdown the config context.
	/// </summary>
	/// <param name="args">
	/// The shutdown event arguments. If this is not provided, we assume
	/// <see cref="ExitReason.User"/>.
	/// </param>
	public void Exit(ExitEventArgs? args = null);
}
