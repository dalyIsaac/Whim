namespace Whim.CommandPalette.Tests;

internal class MenuRowStub : IVariantRowView<MenuVariantRowModelData, MenuVariantRowViewModel>
{
	public bool IsUpdated { get; private set; }

	public required MenuVariantRowViewModel ViewModel { get; init; }

	public void Initialize() { }

	public void Update(MatcherResult<MenuVariantRowModelData> matcherResult)
	{
		ViewModel.Update(matcherResult);
		IsUpdated = true;
	}
}
