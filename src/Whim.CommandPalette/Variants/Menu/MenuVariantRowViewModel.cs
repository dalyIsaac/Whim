using System.ComponentModel;

namespace Whim.CommandPalette;

internal class MenuVariantRowViewModel : IVariantRowViewModel<CommandItem>
{
	public IVariantRowModel<CommandItem> Model { get; private set;}

	public PaletteText FormattedTitle { get; private set; }

	public event PropertyChangedEventHandler? PropertyChanged;

	public MenuVariantRowViewModel(MatcherResult<CommandItem> matcherResult)
	{
		Model = matcherResult.Model;
		FormattedTitle = matcherResult.FormattedTitle;
	}

	public void Update(MatcherResult<CommandItem> matcherResult)
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
