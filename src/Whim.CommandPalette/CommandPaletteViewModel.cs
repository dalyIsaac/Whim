using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Whim.CommandPalette;

public class CommandPaletteViewModel : INotifyPropertyChanged
{
	private readonly IConfigContext _configContext;
	private readonly CommandPalettePlugin _plugin;

	private string _query = "";
	public string Query
	{
		get => _query;
		set
		{
			if (_query != value)
			{
				_query = value;
				OnPropertyChanged(nameof(Query));
				UpdateMatches();
			}
		}
	}

	/// <summary>
	/// The current commands from which <see cref="Matches"/> is derived.
	/// </summary>
	private readonly List<CommandPaletteMatch> _currentCommands = new();

	/// <summary>
	/// The matching commands which are shown to the user.
	/// </summary>

	public ObservableCollection<CommandPaletteMatch> Matches { get; } = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public CommandPaletteViewModel(IConfigContext configContext, CommandPalettePlugin plugin)
	{
		_configContext = configContext;
		_plugin = plugin;
	}

	public void Activate(IEnumerable<(ICommand, IKeybind?)>? items)
	{
		_currentCommands.Clear();

		foreach ((ICommand command, IKeybind? keybind) in items ?? _configContext.CommandManager)
		{
			if (command.CanExecute())
			{
				_currentCommands.Add(new CommandPaletteMatch(command, keybind));
			}
		}

		Query = "";
		UpdateMatches();
	}

	private void UpdateMatches()
	{
		int idx = 0;
		IEnumerator<CommandPaletteMatch> enumerator = _plugin.Config.Matcher.Match(Query, _currentCommands, _configContext, _plugin).GetEnumerator();

		while (enumerator.MoveNext() && idx < Matches.Count)
		{
			Matches[idx] = enumerator.Current;
			idx++;
		}

		while (idx < Matches.Count)
		{
			Matches.RemoveAt(idx);
		}

		while (enumerator.MoveNext())
		{
			Matches.Add(enumerator.Current);
		}
	}
}
