using System.Linq;

namespace Whim.Tests;

public class WindowPickersTests
{
	[Theory, AutoSubstituteData]
	public void PickAllWindows(IRootSector rootSector, IWindow window1, IWindow window2, IWindow window3)
	{
		// Given there are three windows
		rootSector.WindowSector.Windows.Returns(
			new Dictionary<HWND, IWindow>
			{
				[(HWND)1] = window1,
				[(HWND)2] = window2,
				[(HWND)3] = window3,
			}.ToImmutableDictionary()
		);

		// When we get the windows
		var result = Pickers.PickAllWindows()(rootSector);

		// Then we get the windows
		Assert.Equal(3, result.Count());
	}

	[Theory, AutoSubstituteData]
	public void PickWindowByHandle_Failure(IRootSector rootSector, IWindow window1, IWindow window2, IWindow window3)
	{
		// Given there are three windows
		rootSector.WindowSector.Windows.Returns(
			new Dictionary<HWND, IWindow>
			{
				[(HWND)1] = window1,
				[(HWND)2] = window2,
				[(HWND)3] = window3,
			}.ToImmutableDictionary()
		);

		HWND handle = (HWND)4;

		// When we get the window
		var result = Pickers.PickWindowByHandle(handle)(rootSector);

		// Then we get an exception
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData]
	public void PickWindowByHandle_Success(IRootSector rootSector, IWindow window1, IWindow window2, IWindow window3)
	{
		// Given there are three windows
		rootSector.WindowSector.Windows.Returns(
			new Dictionary<HWND, IWindow>
			{
				[(HWND)1] = window1,
				[(HWND)2] = window2,
				[(HWND)3] = window3,
			}.ToImmutableDictionary()
		);

		// When we get the window
		var result = Pickers.PickWindowByHandle((HWND)1)(rootSector);

		// Then we get the window
		Assert.True(result.IsSuccessful);
		Assert.Same(window1, result.Value);
	}

	[Theory, AutoSubstituteData]
	public void PickIsStartupWindow_Failure(IRootSector rootSector)
	{
		// Given there are three startup windows
		rootSector.WindowSector.StartupWindows.Returns(
			new HashSet<HWND> { (HWND)1, (HWND)2, (HWND)3 }.ToImmutableHashSet()
		);

		HWND handle = (HWND)4;

		// When we get whether the window is a startup window
		var result = Pickers.PickIsStartupWindow(handle)(rootSector);

		// Then we get false
		Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void PickIsStartupWindow_Success(IRootSector rootSector)
	{
		// Given there are three startup windows
		rootSector.WindowSector.StartupWindows.Returns(
			new HashSet<HWND> { (HWND)1, (HWND)2, (HWND)3 }.ToImmutableHashSet()
		);

		// When we get whether the window is a startup window
		var result = Pickers.PickIsStartupWindow((HWND)1)(rootSector);

		// Then we get true
		Assert.True(result);
	}
}
