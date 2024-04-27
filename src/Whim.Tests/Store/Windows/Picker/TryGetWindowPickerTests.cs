using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim;

public class TryGetWindowPickerTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void CannotFindWindow(IContext ctx)
	{
		// Given the window cannot be found
		HWND hwnd = (HWND)1;
		TryGetWindowPicker sut = new(hwnd);

		// When we execute the picker
		var result = ctx.Store.Pick(sut);

		// Then we get an unsuccessful response
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void FoundWindow(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given the window can be found
		mutableRootSector.Windows.Windows = mutableRootSector.Windows.Windows.Add(window.Handle, window);
		TryGetWindowPicker sut = new(window.Handle);

		// When
		var result = ctx.Store.Pick(sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(window, result.Value);
	}
}
