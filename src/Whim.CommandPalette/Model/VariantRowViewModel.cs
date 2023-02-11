using System.ComponentModel;

namespace Whim.CommandPalette;

internal class VariantRowViewModel<T> : IVariantRowViewModel<T>
{
	private IVariantRowModel<T> _model;

	public string Id => _model.Id;

	public string Title => _model.Title;

	public PaletteText FormattedTitle { get; private set; }

	public T Data => _model.Data;

	public event PropertyChangedEventHandler? PropertyChanged;

	public VariantRowViewModel(MatcherResult<T> matcherResult)
	{
		_model = matcherResult.Model;
		FormattedTitle = matcherResult.FormattedTitle;
	}

	public void Update(MatcherResult<T> matcherResult)
	{
		_model = matcherResult.Model;
		FormattedTitle = matcherResult.FormattedTitle;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		PropertyChanged?.Invoke(this, e);
	}
}
