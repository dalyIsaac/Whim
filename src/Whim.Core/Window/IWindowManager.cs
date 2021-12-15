using System;

namespace Whim.Core;

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
	public void Initialize();

	/// <summary>
	/// Event for when a window is registered by the <see cref="IWindowManager"/>.
	/// </summary>
	public event WindowRegisterEventHandler WindowRegistered;
}
