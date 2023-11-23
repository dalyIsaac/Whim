using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whim;

internal class FilterManager : IFilterManager
{
	#region Filters for specific properties
	private readonly HashSet<string> _ignoreWindowClasses = new();
	private readonly HashSet<string> _ignoreProcessNames = new();
	private readonly HashSet<string> _ignoreTitles = new();
	#endregion

	/// <summary>
	/// Generic filter for windows.
	/// </summary>
	private readonly List<Filter> _filters = new();

	public void Add(Filter filter)
	{
		_filters.Add(filter);
	}

	public void Clear()
	{
		Logger.Debug($"Clearing filters");
		_ignoreWindowClasses.Clear();
		_ignoreProcessNames.Clear();
		_ignoreTitles.Clear();
		_filters.Clear();
	}

	public bool ShouldBeIgnored(IWindow window)
	{
		return _ignoreWindowClasses.Contains(window.WindowClass.ToLower())
			|| (window.ProcessName is string processName && _ignoreProcessNames.Contains(processName.ToLower()))
			|| _ignoreTitles.Contains(window.Title.ToLower())
			|| _filters.Any(f => f(window));
	}

	public IFilterManager AddWindowClassFilter(string windowClass)
	{
		_ignoreWindowClasses.Add(windowClass.ToLower());
		return this;
	}

	public IFilterManager IgnoreWindowClass(string windowClass) => AddWindowClassFilter(windowClass);

	public IFilterManager AddProcessNameFilter(string processName)
	{
		_ignoreProcessNames.Add(processName.ToLower());
		return this;
	}

	public IFilterManager IgnoreProcessName(string processName) => AddProcessNameFilter(processName);

	public IFilterManager AddTitleFilter(string title)
	{
		_ignoreTitles.Add(title.ToLower());
		return this;
	}

	public IFilterManager IgnoreTitle(string title) => AddTitleFilter(title);

	public IFilterManager AddTitleMatchFilter(string title)
	{
		Regex regex = new(title);
		_filters.Add(window => regex.IsMatch(window.Title));
		return this;
	}

	public IFilterManager IgnoreTitleMatch(string title) => AddTitleMatchFilter(title);
}
