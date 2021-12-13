using System;
using System.Collections.Generic;

namespace Whim.Core.Binds;

/// <summary>
/// The manager for bindings.
/// </summary>
public interface IKeybindManager : IEnumerable<KeyValuePair<IKeybind, KeybindHandler>>, IDisposable
{
	/// <summary>
	/// Number of keybinds stored by the manager.
	/// </summary>
	public int Count { get; }

	/// <summary>
	/// Initialize the keyboard hook.
	/// </summary>
	public void Initialize();

	/// <summary>
	/// Adds the keybind and handler to the manager. If the keybind already exists, the handler
	/// is overriden.
	/// </summary>
	/// <param name="keybind"></param>
	/// <param name="handler"></param>
	/// <param name="throwIfExists">When <see langword="true"/>, throws if the keybind already exists.</param>
	public void Add(IKeybind keybind, KeybindHandler handler, bool throwIfExists = false);

	/// <summary>
	/// Tries to remove the given keybind and associated handler.
	/// </summary>
	/// <param name="keybind"></param>
	/// <returns></returns>
	public bool Remove(IKeybind keybind);

	/// <summary>
	/// Clear all the keybinds and handlers.
	/// </summary>
	public void Clear();

	/// <summary>
	/// Tries to get the keybind handler by keybind.
	/// </summary>
	/// <param name="keybind"></param>
	/// <returns></returns>
	public KeybindHandler? TryGet(IKeybind keybind);

	public KeybindHandler? this[IKeybind keybind] { get; set; }
}
