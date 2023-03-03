using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Whim.CommandPalette.Tests;

[TestClass]
public class MatchTests
{
	[TestMethod]
	public void Match_NoKeybind()
	{
		CommandItem match = new() { Command = new Mock<ICommand>().Object };

		Assert.IsNull(match.Keybind);
	}
}
