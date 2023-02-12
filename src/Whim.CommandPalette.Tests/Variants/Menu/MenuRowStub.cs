namespace Whim.CommandPalette.Tests;

internal class MenuRowStub : IVariantRowControl<CommandItem, MenuVariantRowViewModel>
{
	public bool IsUpdated { get; private set; }

	public required MenuVariantRowViewModel ViewModel { get; init; }

	public void Initialize() { }

	public void Update(MatcherResult<CommandItem> matcherResult)
	{
		ViewModel.Update(matcherResult);
		IsUpdated = true;
	}
}
