using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.FloatingWindow.Tests;

public class FloatingLayoutEngineTests
{
	private static readonly LayoutEngineIdentity identity = new();

	[Theory, AutoSubstituteData]
	public void Name(IContext context)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity);

		// When
		string name = engine.Name;

		// Then
		Assert.Equal("Floating", name);
	}

	#region AddWindow
	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void AddWindow(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity);

		// When
		ILayoutEngine newLayoutEngine = engine.AddWindow(window);

		// Then
		Assert.Equal(0, engine.Count);
		Assert.Equal(1, newLayoutEngine.Count);
	}

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void AddWindow_UntrackedWindow(IContext context)
	{
		// Given a window which isn't tracked by the store
		IWindow window = Substitute.For<IWindow>();
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity);

		// When
		ILayoutEngine newLayoutEngine = engine.AddWindow(window);

		// Then
		Assert.Equal(0, engine.Count);
		Assert.Same(engine, newLayoutEngine);
	}

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void AddWindow_WindowAlreadyPresent(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);

		// When
		ILayoutEngine newLayoutEngine = engine.AddWindow(window);

		// Then
		Assert.Equal(1, engine.Count);
		Assert.Equal(1, newLayoutEngine.Count);
	}
	#endregion

	#region RemoveWindow
	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void Remove(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);

		// When
		ILayoutEngine newLayoutEngine = engine.RemoveWindow(window);

		// Then
		Assert.Equal(1, engine.Count);
		Assert.Equal(0, newLayoutEngine.Count);
	}

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void Remove_NoChanges(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);

		// When
		ILayoutEngine newLayoutEngine = engine.RemoveWindow(Substitute.For<IWindow>());

		// Then
		Assert.Equal(1, engine.Count);
		Assert.Equal(1, newLayoutEngine.Count);
	}
	#endregion RemoveWindow

	#region Contains
	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void Contains(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);

		// When
		bool contains = engine.ContainsWindow(window);

		// Then
		Assert.True(contains);
	}

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void Contains_False(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);

		// When
		bool contains = engine.ContainsWindow(Substitute.For<IWindow>());

		// Then
		Assert.False(contains);
	}
	#endregion

	#region DoLayout
	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void DoLayout_Empty(IContext context)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity);

		// When
		IWindowState[] windowStates = engine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		// Then
		Assert.Empty(windowStates);
	}

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	internal void DoLayout_KeepWindowSize(IContext context, MutableRootSector root)
	{
		// Given
		IWindow[] allWindows = root.WindowSector.Windows.Values.ToArray();
		IWindow windowNormal = allWindows[0];
		IWindow windowMinimized = allWindows[1];
		IWindow windowMaximized = allWindows[2];

		windowMinimized.IsMinimized.Returns(true);
		windowMaximized.IsMaximized.Returns(true);
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity)
			.AddWindow(windowNormal)
			.AddWindow(windowMinimized)
			.AddWindow(windowMaximized);

		// When
		WindowSize[] windowSizes = engine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.Select(window => window.WindowSize)
			.ToArray();

		// Then
		Assert.Equal(3, windowSizes.Length);
		Assert.Contains(WindowSize.Normal, windowSizes);
		Assert.Contains(WindowSize.Minimized, windowSizes);
		Assert.Contains(WindowSize.Maximized, windowSizes);
	}
	#endregion

	#region GetFirstWindow
	[Theory, AutoSubstituteData]
	public void GetFirstWindow_Null(IContext context)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void GetFirstWindow_SingleWindow(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window, result);
	}
	#endregion

	#region WindowRelated
	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void MoveWindowToPoint(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);
		IRectangle<double> rect = new Rectangle<double>();

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, rect);

		// Then
		Assert.Equal(engine, newEngine);
	}

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void MoveWindowEdgesInDirection(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);
		IRectangle<double> rect = new Rectangle<double>();

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(Direction.Up, rect, window);

		// Then
		Assert.Equal(engine, newEngine);
	}

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void MinimizeWindowStart(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);

		// When
		ILayoutEngine newEngine = engine.MinimizeWindowStart(window);

		// Then
		Assert.Equal(engine, newEngine);
	}

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void MinimizeWindowEnd(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);

		// When
		ILayoutEngine newEngine = engine.MinimizeWindowEnd(window);

		// Then
		Assert.Equal(engine, newEngine);
	}

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void FocusWindowInDirection(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);

		// When
		ILayoutEngine newEngine = engine.FocusWindowInDirection(Direction.Up, window);

		// Then
		Assert.Equal(engine, newEngine);
	}

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void SwapWindowInDirection(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Up, window);

		// Then
		Assert.Equal(engine, newEngine);
	}
	#endregion

	[Theory, AutoSubstituteData<FloatingWindowCustomization>]
	public void PerformCustomAction(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FloatingLayoutEngine(context, identity).AddWindow(window);
		LayoutEngineCustomAction<string> action = new()
		{
			Name = "Action",
			Payload = "payload",
			Window = null,
		};

		// When
		ILayoutEngine newEngine = engine.PerformCustomAction(action);

		// Then
		Assert.Equal(engine, newEngine);
	}
}
