using Moq;
using Xunit;

namespace Whim.ImmutableTreeLayout.Tests;

public class DoLayoutTests
{
	[Fact]
	public void DoLayout_RootIsNull()
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object);

		Mock<IMonitor> monitor = new();
		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };

		// When
		IWindowState[] windowStates = engine.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.Empty(windowStates);
	}
}
