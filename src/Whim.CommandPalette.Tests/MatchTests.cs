using Moq;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class MatchTests
{
	[Fact]
	public void Match_NoKeybind()
	{
		Match match = new(new Mock<ICommand>().Object);

		Assert.Null(match.Keys);
	}

	[Fact]
	public void Match_Keybind()
	{
		Match match = new(new Mock<ICommand>().Object, new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_A));
		Assert.Equal("LWin + A", match.Keys);
	}
}
