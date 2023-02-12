using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MenuVariantRowViewModelTests
{
	[Fact]
	public void Update_UpdatesModel()
	{
		// Given
		// Old
		Mock<IVariantRowModel<CommandItem>> modelMock = new();

		MatcherResult<CommandItem> matcherResult = new(modelMock.Object, Array.Empty<FilterTextMatch>(), 0);
		MenuVariantRowViewModel vm = new(matcherResult);

		// New
		Mock<IVariantRowModel<CommandItem>> newModelMock = new();
		newModelMock.Setup(m => m.Title).Returns("Test");

		FilterTextMatch[] matches = new[] { new FilterTextMatch(0, 4) };
		MatcherResult<CommandItem> newMatcherResult = new(newModelMock.Object, matches, 0);

		// When
		Assert.PropertyChanged(vm, string.Empty, () => vm.Update(newMatcherResult));

		// Then
		Assert.Equal(newModelMock.Object, vm.Model);
		Assert.Equal(1, vm.FormattedTitle.Segments.Count);
	}
}
