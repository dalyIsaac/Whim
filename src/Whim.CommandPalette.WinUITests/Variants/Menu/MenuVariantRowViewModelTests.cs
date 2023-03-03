using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.ComponentModel;

namespace Whim.CommandPalette.WinUITests;

[TestClass]
public class MenuVariantRowViewModelTests
{
	[TestMethod]
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
		using (var monitoredSubject = vm.Monitor())
		{
			vm.Update(newMatcherResult);
			monitoredSubject
				.Should()
				.Raise("PropertyChanged")
				.WithArgs<PropertyChangedEventArgs>(args => args.PropertyName == string.Empty);
		}

		// Then
		Assert.AreEqual(newModelMock.Object, vm.Model);
		Assert.AreEqual(1, vm.FormattedTitle.Segments.Count);
	}
}
