using System;

namespace Whim.Core.Window;

/// <summary>
/// The manager for <see cref="IWindow"/>s.
/// </summary>
public interface IWindowManager : ICommandable, IDisposable
{
	/// <summary>
	/// Initialize the windows event hooks.
	/// </summary>
	/// <returns>
	/// <see langword="true"/> when the hooks have all been registered successfully, otherwise
	/// <see langword="false"/>
	/// </returns>
	public bool Initialize();

	/// <summary>
	/// Event for when a window is registered.
	/// </summary>
	public event WindowRegisterEventHandler WindowRegistered;
}
