using System;

namespace Whim.Core;

using Filter = Func<IWindow, bool>;

public interface IFilterManager
{
	/// <summary>
	/// Add a filter.
	/// </summary>
	/// <param name="filter"></param>
	public void Add(Filter filter);

	/// <summary>
	/// Clears all the filters.
	/// </summary>
	/// <param name="clearDefaults">Indicates whether the default filters should be retained.</param>
	public void Clear(bool clearDefaults = false);

	/// <summary>
	/// Indicates whether the window should be ignored.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>
	/// <see langword="true"/> when the window should be ignored, otherwise
	/// <see langword="false"/>.
	/// </returns>
	public bool FilterWindow(IWindow window);

	/// <summary>
	/// Ignores the window class. Case insensitive.
	/// </summary>
	/// <param name="windowClass"></param>
	public IFilterManager IgnoreWindowClass(string windowClass);

	/// <summary>
	/// Ignores the process name. Case insensitive.
	/// </summary>
	/// <param name="processName"></param>
	public IFilterManager IgnoreProcessName(string processName);

	/// <summary>
	/// Ignores the title. Case insensitive.
	/// </summary>
	/// <param name="title"></param>
	public IFilterManager IgnoreTitle(string title);

	/// <summary>
	/// Filter the title according to the regex pattern.
	/// </summary>
	/// <param name="match"></param>
	public IFilterManager IgnoreTitleMatch(string match);
}
