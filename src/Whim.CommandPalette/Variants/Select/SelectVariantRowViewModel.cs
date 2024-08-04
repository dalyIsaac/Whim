using System.ComponentModel;

namespace Whim.CommandPalette;

internal class SelectVariantRowViewModel(MatcherResult<SelectOption> matcherResult) : IVariantRowViewModel<SelectOption>
{
	public IVariantRowModel<SelectOption> Model { get; private set; } = matcherResult.Model;

	public PaletteText FormattedTitle { get; private set; } = matcherResult.FormattedTitle;

	public bool IsSelected
	{
		get => Model.Data.IsSelected;
		set
		{
			if (Model.Data.IsSelected != value)
			{
				Model.Data.IsSelected = value;
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsSelected)));
			}
		}
	}

	public bool IsEnabled
	{
		get => Model.Data.IsEnabled;
		set
		{
			if (Model.Data.IsEnabled != value)
			{
				Model.Data.IsEnabled = value;
				OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsEnabled)));
			}
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public void Update(MatcherResult<SelectOption> matcherResult)
	{
		Model = matcherResult.Model;
		FormattedTitle = matcherResult.FormattedTitle;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		PropertyChanged?.Invoke(this, e);
	}
}
