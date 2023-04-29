using System.ComponentModel;

namespace Whim.CommandPalette;

internal class MenuVariantRowViewModel : IVariantRowViewModel<MenuVariantRowModelData>
{
	public IVariantRowModel<MenuVariantRowModelData> Model { get; private set; }

	public PaletteText FormattedTitle { get; private set; }

	public event PropertyChangedEventHandler? PropertyChanged;

	public MenuVariantRowViewModel(MatcherResult<MenuVariantRowModelData> matcherResult)
	{
		Model = matcherResult.Model;
		FormattedTitle = matcherResult.FormattedTitle;
	}

	public void Update(MatcherResult<MenuVariantRowModelData> matcherResult)
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
