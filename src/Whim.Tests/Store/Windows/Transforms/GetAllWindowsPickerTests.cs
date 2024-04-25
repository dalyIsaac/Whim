using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class GetAllWindowsPickerTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	public void Success(IContext ctx, IWindow window)
	{
		// Given the windows in the slice
		ctx.Store.WindowSlice.Windows = ctx.Store.WindowSlice.Windows.Add((HWND)1, window);
		GetAllWindowsPicker sut = new();

		// When
		var result = ctx.Store.Pick(sut);

		// Then
		Assert.Single(result);
	}
}
