using System;

namespace Whim;

/// <summary>
/// A command is a function with a unique identifier.
/// </summary>
public interface ICommand
{
	/// <summary>
	/// The unique identifier.
	/// </summary>
	public string Identifier { get; }

	/// <summary>
	/// The title of the menu item. This is what is displayed in the menu for
	/// the user to select.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// The callback to execute when the menu item is selected. This can include
	/// triggering another menu to be shown, free form input, or to perform some
	/// action.
	/// </summary>
	public Action Callback { get; }

	/// <summary>
	/// A condition to determine if the menu item should be visible and active
	/// to Whim. If this is null, the menu item will always be accessible.
	/// </summary>
	public Action? Condition { get; }

	/// <summary>
	/// The keybind for the command. If this is null, the command will not be
	/// bound to a keybind.
	/// </summary>
	public IKeybind? Keybind { get; }
}
