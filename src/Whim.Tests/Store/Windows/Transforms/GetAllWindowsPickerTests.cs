using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class GetAllWindowsPickerTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given the windows in the slice
		mutableRootSector.Windows.Windows = mutableRootSector.Windows.Windows.Add((HWND)1, window);
		GetAllWindowsPicker sut = new();

		// When
		var result = ctx.Store.Pick(sut);

		// Then
		Assert.Single(result);
	}
}
