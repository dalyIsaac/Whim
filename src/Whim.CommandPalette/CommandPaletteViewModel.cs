using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;

namespace Whim.CommandPalette;

public class CommandPaletteViewModel : INotifyPropertyChanged
{
	private readonly IConfigContext _configContext;
	private readonly CommandPaletteConfig _config;

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
			}
		}
	}

	public ObservableCollection<CommandPaletteMatch> Matches { get; } = new();

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public CommandPaletteViewModel(IConfigContext configContext, CommandPaletteConfig config)
	{
		_configContext = configContext;
		_config = config;
	}

	public void Activate(IEnumerable<(ICommand, IKeybind?)>? items)
	{
		items ??= _configContext.CommandManager;
		Query = "";
		Matches.Clear();

		foreach (CommandPaletteMatch match in _config.Matcher(Query, items, _configContext))
		{
			Matches.Add(match);
		}
	}
}
