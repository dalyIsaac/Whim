using Microsoft.UI.Xaml;
using Moq;
using Windows.System;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class CommandPaletteWindowViewModelTests
{
	private static (
		Mock<IConfigContext>,
		Mock<ICommandManager>,
		CommandPalettePlugin,
		Mock<IVariantControl<MenuVariantConfig>>,
		Mock<IVariantControl<FreeTextVariantConfig>>
	) CreateStubs()
	{
		Mock<ICommandManager> commandManager = new();
		commandManager.Setup(x => x.GetEnumerator()).Returns(new List<CommandItem>().GetEnumerator());

		Mock<IConfigContext> configContext = new();
		configContext.SetupGet(x => x.CommandManager).Returns(commandManager.Object);

		CommandPalettePlugin plugin = new(configContext.Object, new CommandPaletteConfig());

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		Mock<IVariantControl<MenuVariantConfig>> menuVariant = new();
		menuVariant.Setup(m => m.Control).Returns((UIElement)null);

		Mock<IVariantControl<FreeTextVariantConfig>> freeTextVariant = new();
		freeTextVariant.Setup(m => m.Control).Returns((UIElement)null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

		return (configContext, commandManager, plugin, menuVariant, freeTextVariant);
	}

	[Fact]
	public void RequestHide()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			CommandPalettePlugin plugin,
			Mock<IVariantControl<MenuVariantConfig>> menuVariant,
			Mock<IVariantControl<FreeTextVariantConfig>> freeTextVariant
		) = CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, menuVariant.Object, freeTextVariant.Object);

		// When
		// Then
		Assert.Raises<EventArgs>(h => vm.HideRequested += h, h => vm.HideRequested -= h, vm.RequestHide);
	}

	[Fact]
	public void OnKeyDown_Escape()
	{
		// Given
		(
			Mock<IConfigContext> configContext,
			Mock<ICommandManager> commandManager,
			CommandPalettePlugin plugin,
			Mock<IVariantControl<MenuVariantConfig>> menuVariant,
			Mock<IVariantControl<FreeTextVariantConfig>> freeTextVariant
		) = CreateStubs();

		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, menuVariant.Object, freeTextVariant.Object);

		Assert.Raises<EventArgs>(
			h => vm.HideRequested += h,
			h => vm.HideRequested -= h,
			() => vm.OnKeyDown(VirtualKey.Escape)
		);
	}
}
