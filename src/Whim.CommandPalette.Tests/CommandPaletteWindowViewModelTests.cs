using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class CommandPaletteWindowViewModelTests
{
	[Fact]
	public void Constructor()
	{
		// Given
		Mock<ICommandManager> commandManager = new();
		commandManager
			.Setup(cm => cm.GetEnumerator())
			.Returns(
				new List<CommandItem>()
				{
					new CommandItem() { Command = new Command("id", "title", () => { }) }
				}.GetEnumerator()
			);

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.CommandManager).Returns(commandManager.Object);

		CommandPalettePlugin plugin = new(configContext.Object, new());

		// When
		CommandPaletteWindowViewModel vm =
			new(configContext.Object, plugin, (rowItem) => new Mock<IPaletteRow>().Object);

		// Then
		Assert.Single(vm.PaletteRows);
	}

	[Fact]
	public void Activate()
	{
		// TODO
	}

	[Fact]
	public void RequestHide()
	{
		// TODO
	}

	[Fact]
	public void OnKeyDown()
	{
		// TODO
	}

	[Fact]
	public void UpdateMatches()
	{
		// TODO
	}

	[Fact]
	public void ExecuteCommand()
	{
		// TODO
	}
}
