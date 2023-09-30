using NSubstitute;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class SelectVariantRowViewModelTests
{
	private static IVariantRowModel<SelectOption> CreateModelMock(bool isSelected, bool IsEnabled)
	{
		IVariantRowModel<SelectOption> modelMock = Substitute.For<IVariantRowModel<SelectOption>>();
		modelMock.Data.Returns(
			new SelectOption()
			{
				Id = "id",
				Title = "title",
				IsSelected = isSelected,
				IsEnabled = IsEnabled
			}
		);
		return modelMock;
	}

	[Fact]
	public void IsSelected_WhenSetToTrue_SetsIsSelectedOnModel()
	{
		// Given
		IVariantRowModel<SelectOption> modelMock = CreateModelMock(false, false);
		MatcherResult<SelectOption> matcherResult = new(modelMock, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		Assert.PropertyChanged(vm, nameof(vm.IsSelected), () => vm.IsSelected = true);

		// Then
		Assert.True(modelMock.Data.IsSelected);
	}

	[Fact]
	public void IsSelected_WhenSetToFalse_SetsIsSelectedOnModel()
	{
		// Given
		IVariantRowModel<SelectOption> modelMock = CreateModelMock(true, false);
		MatcherResult<SelectOption> matcherResult = new(modelMock, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		Assert.PropertyChanged(vm, nameof(vm.IsSelected), () => vm.IsSelected = false);

		// Then
		Assert.False(modelMock.Data.IsSelected);
	}

	[Fact]
	public void IsEnabled_WhenSetToTrue_SetsIsEnabledOnModel()
	{
		// Given
		IVariantRowModel<SelectOption> modelMock = CreateModelMock(false, false);
		MatcherResult<SelectOption> matcherResult = new(modelMock, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		Assert.PropertyChanged(vm, nameof(vm.IsEnabled), () => vm.IsEnabled = true);

		// Then
		Assert.True(modelMock.Data.IsEnabled);
	}

	[Fact]
	public void IsEnabled_WhenSetToFalse_SetsIsEnabledOnModel()
	{
		// Given
		IVariantRowModel<SelectOption> modelMock = CreateModelMock(false, true);
		MatcherResult<SelectOption> matcherResult = new(modelMock, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		Assert.PropertyChanged(vm, nameof(vm.IsEnabled), () => vm.IsEnabled = false);

		// Then
		Assert.False(modelMock.Data.IsEnabled);
	}

	[Fact]
	public void Update_UpdatesModel()
	{
		// Given
		// Old
		IVariantRowModel<SelectOption> modelMock = CreateModelMock(false, true);

		MatcherResult<SelectOption> matcherResult = new(modelMock, Array.Empty<FilterTextMatch>(), 0);
		SelectVariantRowViewModel vm = new(matcherResult);

		// New
		IVariantRowModel<SelectOption> newModelMock = CreateModelMock(true, false);
		newModelMock.Title.Returns("Test");

		FilterTextMatch[] matches = new FilterTextMatch[] { new(0, 4) };
		MatcherResult<SelectOption> newMatcherResult = new(newModelMock, matches, 0);

		// When
		Assert.PropertyChanged(vm, string.Empty, () => vm.Update(newMatcherResult));

		// Then
		Assert.Same(newModelMock, vm.Model);
		Assert.Single(vm.FormattedTitle.Segments);
	}
}
