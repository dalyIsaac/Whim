using System.ComponentModel;

namespace Whim.CommandPalette;

internal class MenuVariantRowViewModel(MatcherResult<MenuVariantRowModelData> matcherResult) : IVariantRowViewModel<MenuVariantRowModelData>
{
	public IVariantRowModel<MenuVariantRowModelData> Model { get; private set; } = matcherResult.Model;

	public PaletteText FormattedTitle { get; private set; } = matcherResult.FormattedTitle;

	public event PropertyChangedEventHandler? PropertyChanged;

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
