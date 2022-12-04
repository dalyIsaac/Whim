using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MatchTests
{
	[Fact]
	public void Match_NoKeybind()
	{
		CommandItem match = new() { Command = new Mock<ICommand>().Object };

		Assert.Null(match.Keybind);
	}
}
