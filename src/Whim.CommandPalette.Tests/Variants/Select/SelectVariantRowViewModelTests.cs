using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.ComponentModel;

namespace Whim.CommandPalette.Tests;

[TestClass]
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

	[TestMethod]
	public void IsSelected_WhenSetToTrue_SetsIsSelectedOnModel()
	{
		// Given
		Mock<IVariantRowModel<SelectOption>> modelMock = CreateModelMock(false, false);
		MatcherResult<SelectOption> matcherResult = new(modelMock.Object, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		using (var monitoredSubject = vm.Monitor())
		{
			vm.IsSelected = true;
			monitoredSubject.Should().RaisePropertyChangeFor(vm => vm.IsSelected);
		}

		// Then
		Assert.IsTrue(modelMock.Object.Data.IsSelected);
	}

	[TestMethod]
	public void IsSelected_WhenSetToFalse_SetsIsSelectedOnModel()
	{
		// Given
		Mock<IVariantRowModel<SelectOption>> modelMock = CreateModelMock(true, false);
		MatcherResult<SelectOption> matcherResult = new(modelMock.Object, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		using (var monitoredSubject = vm.Monitor())
		{
			vm.IsSelected = false;
			monitoredSubject.Should().RaisePropertyChangeFor(vm => vm.IsSelected);
		}

		// Then
		Assert.IsFalse(modelMock.Object.Data.IsSelected);
	}

	[TestMethod]
	public void IsEnabled_WhenSetToTrue_SetsIsEnabledOnModel()
	{
		// Given
		Mock<IVariantRowModel<SelectOption>> modelMock = CreateModelMock(false, false);
		MatcherResult<SelectOption> matcherResult = new(modelMock.Object, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		using (var monitoredSubject = vm.Monitor())
		{
			vm.IsEnabled = true;
			monitoredSubject.Should().RaisePropertyChangeFor(vm => vm.IsEnabled);
		}

		// Then
		Assert.IsTrue(modelMock.Object.Data.IsEnabled);
	}

	[TestMethod]
	public void IsEnabled_WhenSetToFalse_SetsIsEnabledOnModel()
	{
		// Given
		Mock<IVariantRowModel<SelectOption>> modelMock = CreateModelMock(false, true);
		MatcherResult<SelectOption> matcherResult = new(modelMock.Object, Array.Empty<FilterTextMatch>(), 0);

		SelectVariantRowViewModel vm = new(matcherResult);

		// When
		using (var monitoredSubject = vm.Monitor())
		{
			vm.IsEnabled = false;
			monitoredSubject.Should().RaisePropertyChangeFor(vm => vm.IsEnabled);
		}

		// Then
		Assert.IsFalse(modelMock.Object.Data.IsEnabled);
	}

	[TestMethod]
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
		using (var monitoredSubject = vm.Monitor())
		{
			vm.Update(newMatcherResult);
			monitoredSubject
				.Should()
				.Raise("PropertyChanged")
				.WithArgs<PropertyChangedEventArgs>(args => args.PropertyName == string.Empty);
		}

		// Then
		Assert.AreSame(newModelMock.Object, vm.Model);
		Assert.AreEqual(1, vm.FormattedTitle.Segments.Count);
	}
}
