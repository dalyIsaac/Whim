using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class SelectVariantRowViewModelTests
{
	private static Mock<IVariantRowModel<SelectOption>> CreateModelMock(bool isSelected, bool IsEnabled)
	{
		Mock<IVariantRowModel<SelectOption>> modelMock = new();

		SelectOption data =
			new()
			{
				Id = "id",
				Title = "title",
				IsSelected = isSelected,
				IsEnabled = IsEnabled
			};

		modelMock.Setup(m => m.Data).Returns(data);

		return modelMock;
	}

	[Fact]
	public void IsSelected_WhenSetToTrue_SetsIsSelectedOnModel()
	{
		// Given
		Mock<IVariantRowModel<SelectOption>> modelMock = CreateModelMock(false, false);
		MatcherResult<SelectOption> matcherResult = new(modelMock.Object, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		Assert.PropertyChanged(vm, nameof(vm.IsSelected), () => vm.IsSelected = true);

		// Then
		Assert.True(modelMock.Object.Data.IsSelected);
	}

	[Fact]
	public void IsSelected_WhenSetToFalse_SetsIsSelectedOnModel()
	{
		// Given
		Mock<IVariantRowModel<SelectOption>> modelMock = CreateModelMock(true, false);
		MatcherResult<SelectOption> matcherResult = new(modelMock.Object, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		Assert.PropertyChanged(vm, nameof(vm.IsSelected), () => vm.IsSelected = false);

		// Then
		Assert.False(modelMock.Object.Data.IsSelected);
	}

	[Fact]
	public void IsEnabled_WhenSetToTrue_SetsIsEnabledOnModel()
	{
		// Given
		Mock<IVariantRowModel<SelectOption>> modelMock = CreateModelMock(false, false);
		MatcherResult<SelectOption> matcherResult = new(modelMock.Object, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		Assert.PropertyChanged(vm, nameof(vm.IsEnabled), () => vm.IsEnabled = true);

		// Then
		Assert.True(modelMock.Object.Data.IsEnabled);
	}

	[Fact]
	public void IsEnabled_WhenSetToFalse_SetsIsEnabledOnModel()
	{
		// Given
		Mock<IVariantRowModel<SelectOption>> modelMock = CreateModelMock(false, true);
		MatcherResult<SelectOption> matcherResult = new(modelMock.Object, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		Assert.PropertyChanged(vm, nameof(vm.IsEnabled), () => vm.IsEnabled = false);

		// Then
		Assert.False(modelMock.Object.Data.IsEnabled);
	}

	[Fact]
	public void Update_UpdatesModel()
	{
		// Given
		// Old
		Mock<IVariantRowModel<SelectOption>> modelMock = CreateModelMock(false, true);

		MatcherResult<SelectOption> matcherResult = new(modelMock.Object, Array.Empty<FilterTextMatch>(), 0);
		SelectVariantRowViewModel vm = new(matcherResult);

		// New
		Mock<IVariantRowModel<SelectOption>> newModelMock = CreateModelMock(true, false);
		newModelMock.Setup(m => m.Title).Returns("Test");

		FilterTextMatch[] matches = new FilterTextMatch[] { new(0, 4) };
		MatcherResult<SelectOption> newMatcherResult = new(newModelMock.Object, matches, 0);

		// When
		Assert.PropertyChanged(vm, string.Empty, () => vm.Update(newMatcherResult));

		// Then
		Assert.Same(newModelMock.Object, vm.Model);
		Assert.Equal(1, vm.FormattedTitle.Segments.Count);
	}
}
