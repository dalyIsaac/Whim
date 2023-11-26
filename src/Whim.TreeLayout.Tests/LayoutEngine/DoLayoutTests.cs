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

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		IWindowState[] windowStates = engine.DoLayout(rect, monitor).ToArray();

		// Then
		Assert.Empty(windowStates);
	}
}
