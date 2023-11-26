using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class DoLayoutTests
{
	[Theory, AutoSubstituteData]
	public void DoLayout_RootIsNull(IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine = new(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		IRectangle<int> location = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		IWindowState[] windowStates = engine.DoLayout(location, monitor).ToArray();

		// Then
		Assert.Empty(windowStates);
	}
}
