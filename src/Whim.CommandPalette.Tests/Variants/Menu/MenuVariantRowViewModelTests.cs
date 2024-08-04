using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MenuVariantRowViewModelTests
{
	[Theory, AutoSubstituteData]
	public void Update_UpdatesModel(
		IVariantRowModel<MenuVariantRowModelData> modelMock,
		IVariantRowModel<MenuVariantRowModelData> newModelMock
	)
	{
		// Given
		// Old
		MatcherResult<MenuVariantRowModelData> matcherResult = new(modelMock, [], 0);
		MenuVariantRowViewModel vm = new(matcherResult);

		// New
		newModelMock.Title.Returns("Test");

		FilterTextMatch[] matches = new[] { new FilterTextMatch(0, 4) };
		MatcherResult<MenuVariantRowModelData> newMatcherResult = new(newModelMock, matches, 0);

		// When
		Assert.PropertyChanged(vm, string.Empty, () => vm.Update(newMatcherResult));

		// Then
		Assert.Equal(newModelMock, vm.Model);
		Assert.Single(vm.FormattedTitle.Segments);
	}
}
