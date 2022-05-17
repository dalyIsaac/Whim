using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Whim.CommandPalette;

public sealed partial class CommandPaletteWindow : Microsoft.UI.Xaml.Window
{
	private readonly IConfigContext _configContext;
	private readonly CommandPalettePlugin _plugin;

	private readonly IWindow _window;
	private IMonitor? _monitor;
	public bool IsVisible => _monitor != null;

	/// <summary>
	/// The current commands from which <see cref="Matches"/> is derived.
	/// </summary>
	private readonly List<CommandPaletteMatch> _allCommands = new();

	/// <summary>
	/// The matching commands which are shown to the user.
	/// </summary>
	private readonly ObservableCollection<CommandPaletteMatch> _matches = new();

	public CommandPaletteWindow(IConfigContext configContext, CommandPalettePlugin plugin)
	{
		_configContext = configContext;
		_plugin = plugin;
		_window = this.InitializeBorderlessWindow("Whim.CommandPalette", "CommandPaletteWindow", _configContext);

		Title = CommandPaletteConfig.Title;
		CommandListItems.ItemsSource = _matches;
	}

	public void Activate(IEnumerable<(ICommand, IKeybind?)>? items = null, IMonitor? monitor = null)
	{
		foreach ((ICommand command, IKeybind? keybind) in items ?? _configContext.CommandManager)
		{
			if (command.CanExecute())
			{
				_allCommands.Add(new CommandPaletteMatch(command, keybind));
			}
		}
		UpdateMatches();

		monitor ??= _configContext.MonitorManager.FocusedMonitor;
		if (monitor == _monitor)
		{
			return;
		}

		_monitor = monitor;

		int width = 800;
		int height = 800;

		ILocation<int> windowLocation = new Location(
			x: monitor.X + (monitor.Width / 2) - (width / 2),
			y: monitor.Y + (height / 4),
			width: width,
			height: height
		);

		Activate();
		Win32Helper.SetWindowPos(
			new WindowLocation(_window, windowLocation, WindowState.Normal),
			_window.Handle
		);
	}

	public void Hide()
	{
		_window.Hide();
		_monitor = null;
	}

	public void Toggle()
	{
		if (IsVisible)
		{
			Hide();
		}
		else
		{
			Activate();
		}
	}

	private void TextEntry_KeyDown(object _sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		int selectedIndex = CommandListItems.SelectedIndex;
		switch (e.Key)
		{
			case Windows.System.VirtualKey.Down:
				CommandListItems.SelectedIndex = (selectedIndex + 1) % _matches.Count;
				break;
			case Windows.System.VirtualKey.Up:
				CommandListItems.SelectedIndex = (selectedIndex + _matches.Count - 1) % _matches.Count;
				break;
			case Windows.System.VirtualKey.Enter:
				// TODO: Execute
				break;
			case Windows.System.VirtualKey.Escape:
				Hide();
				break;
			default:
				break;
		}
	}

	private void TextEntry_TextChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs e)
	{
		UpdateMatches();
	}

	private void UpdateMatches()
	{
		string query = TextEntry.Text;
		int idx = 0;

		IEnumerator<CommandPaletteMatch> enumerator = _plugin.Config.Matcher.Match(
			query,
			_allCommands,
			_configContext,
			_plugin
		).GetEnumerator();

		while (enumerator.MoveNext() && idx < _matches.Count)
		{
			if (_matches[idx] != enumerator.Current)
			{
				_matches[idx] = enumerator.Current;
			}
			idx++;
		}

		for (; idx < _matches.Count; idx++)
		{
			_matches.RemoveAt(idx);
		}

		while (enumerator.MoveNext())
		{
			_matches.Add(enumerator.Current);
		}

		CommandListItems.SelectedIndex = _matches.Count > 0 ? 0 : -1;
	}
}
