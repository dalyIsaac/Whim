using System;

namespace Whim;

/// <summary>
/// Delegate which is called to filter a <see cref="IWindow"/>.
/// </summary>
public delegate bool Filter(IWindow window);

/// <summary>
/// Manages filters for <see cref="IWindow"/>s.
/// </summary>
public interface IFilterManager
{
	/// <summary>
	/// Add a filter.
	/// </summary>
	/// <param name="filter"></param>
	void Add(Filter filter);

	/// <summary>
	/// Clears all the filters.
	/// </summary>
	void Clear();

	/// <summary>
	/// Indicates whether the window should be ignored.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>
	/// <see langword="true"/> when the window should be ignored, otherwise
	/// <see langword="false"/>.
	/// </returns>
	bool ShouldBeIgnored(IWindow window);

	/// <summary>
	/// Ignores the window class. Case insensitive.
	/// </summary>
	/// <param name="windowClass"></param>
	[Obsolete("Use AddWindowClassFilter instead")]
	IFilterManager IgnoreWindowClass(string windowClass);

	/// <summary>
	/// Ignores the process name. Case insensitive.
	/// </summary>
	/// <param name="processName"></param>
	[Obsolete("Use AddProcessNameFilter instead")]
	IFilterManager IgnoreProcessName(string processName);

	/// <summary>
	/// Ignores the title. Case insensitive.
	/// </summary>
	/// <param name="title"></param>
	[Obsolete("Use AddTitleFilter instead")]
	IFilterManager IgnoreTitle(string title);

	/// <summary>
	/// Filter the title according to the regex pattern.
	/// </summary>
	/// <param name="match"></param>
	[Obsolete("Use AddTitleMatchFilter instead")]
	IFilterManager IgnoreTitleMatch(string match);

	/// <summary>
	/// Ignores the window class. Case insensitive.
	/// </summary>
	/// <param name="windowClass"></param>
	/// <returns></returns>
	IFilterManager AddWindowClassFilter(string windowClass);

	/// <summary>
	/// Ignores the process name - see <see cref="IWindow.ProcessName"/>. Case insensitive.
	/// </summary>
	/// <param name="processName"></param>
	/// <returns></returns>
	[Obsolete("Use AddProcessFileNameFilter instead")]
	IFilterManager AddProcessNameFilter(string processName);

	/// <summary>
	/// Ignores the process name - see <see cref="IWindow.ProcessFileName"/>. Case insensitive.
	/// </summary>
	/// <param name="processName"></param>
	/// <returns></returns>
	IFilterManager AddProcessFileNameFilter(string processName);

	/// <summary>
	/// Ignores the title. Case insensitive.
	/// </summary>
	/// <param name="title"></param>
	/// <returns></returns>
	IFilterManager AddTitleFilter(string title);

	/// <summary>
	/// Filter the title according to the regex pattern.
	/// </summary>
	/// <param name="match"></param>
	/// <returns></returns>
	IFilterManager AddTitleMatchFilter(string match);
}
