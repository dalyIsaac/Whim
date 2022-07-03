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

	public FilterManager()
	{
		AddDefaultFilters(this);
	}

	public void Add(Filter filter)
	{
		_filters.Add(filter);
	}

	public void Clear(bool clearDefaults = false)
	{
		Logger.Debug($"Clearing filters. Defaults being cleared: {clearDefaults}");
		_ignoreWindowClasses.Clear();
		_ignoreProcessNames.Clear();
		_ignoreTitles.Clear();
		_filters.Clear();
		if (!clearDefaults)
		{
			AddDefaultFilters(this);
		}
	}

	public bool ShouldBeIgnored(IWindow window)
	{
		return _ignoreWindowClasses.Contains(window.WindowClass.ToLower())
			|| _ignoreProcessNames.Contains(window.ProcessName.ToLower())
			|| _ignoreTitles.Contains(window.Title.ToLower())
			|| _filters.Any(f => f(window));
	}

	public IFilterManager IgnoreWindowClass(string windowClass)
	{
		_ignoreWindowClasses.Add(windowClass.ToLower());
		return this;
	}

	public IFilterManager IgnoreProcessName(string processName)
	{
		_ignoreProcessNames.Add(processName.ToLower());
		return this;
	}

	public IFilterManager IgnoreTitle(string title)
	{
		_ignoreTitles.Add(title.ToLower());
		return this;
	}

	public IFilterManager IgnoreTitleMatch(string title)
	{
		Regex regex = new(title);
		_filters.Add(window => regex.IsMatch(window.Title));
		return this;
	}

	/// <summary>
	/// Populates the provided <see cref="IFilterManager"/> with the default
	/// filters.
	/// </summary>
	public static void AddDefaultFilters(IFilterManager router)
	{
		router.IgnoreProcessName("SearchUI.exe");
	}
}
