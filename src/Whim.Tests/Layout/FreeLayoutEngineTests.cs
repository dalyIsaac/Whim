using System.Linq;
using AutoFixture;

namespace Whim.Tests;

public class FreeLayoutEngineCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext context = fixture.Freeze<IContext>();
		IMonitor monitor = fixture.Freeze<IMonitor>();

		context.MonitorManager.GetMonitorAtPoint(Arg.Any<IRectangle<int>>()).Returns(monitor);
		monitor.WorkingArea.Returns(new Rectangle<int>() { Width = 1000, Height = 1000 });
		context
			.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>())
			.Returns(new Rectangle<int>() { Width = 100, Height = 100 });

		fixture.Inject(context);
		fixture.Inject(monitor);
	}
}

public class FreeLayoutEngineTests
{
	private static readonly LayoutEngineIdentity identity = new();

	[Theory, AutoSubstituteData]
	public void Name(IContext context)
	{
		// Given
		FreeLayoutEngine engine = new(context, identity);

		// When
		string name = engine.Name;

		// Then
		Assert.Equal("Free", name);
	}

	[Theory, AutoSubstituteData<FreeLayoutEngineCustomization>]
	public void AddWindow(IContext context, IWindow window)
	{
		// Given
		FreeLayoutEngine engine = new(context, identity);

		// When
		ILayoutEngine newLayoutEngine = engine.AddWindow(window);

		// Then
		Assert.NotSame(engine, newLayoutEngine);
		Assert.Equal(1, newLayoutEngine.Count);
	}

	[Theory, AutoSubstituteData<FreeLayoutEngineCustomization>]
	public void AddWindow_WindowAlreadyPresent(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window);

		// When
		ILayoutEngine newLayoutEngine = engine.AddWindow(window);

		// Then
		Assert.Same(engine, newLayoutEngine);
		Assert.Equal(1, newLayoutEngine.Count);
	}

	[Theory, AutoSubstituteData<FreeLayoutEngineCustomization>]
	public void Remove(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window);

		// When
		ILayoutEngine newLayoutEngine = engine.RemoveWindow(window);

		// Then
		Assert.NotSame(engine, newLayoutEngine);
		Assert.Equal(0, newLayoutEngine.Count);
	}

	[Theory, AutoSubstituteData<FreeLayoutEngineCustomization>]
	public void Remove_NoChanges(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window);

		// When
		ILayoutEngine newLayoutEngine = engine.RemoveWindow(Substitute.For<IWindow>());

		// Then
		Assert.Same(engine, newLayoutEngine);
		Assert.Equal(1, newLayoutEngine.Count);
	}

	[Theory, AutoSubstituteData<FreeLayoutEngineCustomization>]
	public void Contains(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FreeLayoutEngine(context, identity).AddWindow(window);

		// When
		bool contains = engine.ContainsWindow(window);

		// Then
		Assert.True(contains);
	}

	[Theory, AutoSubstituteData<FreeLayoutEngineCustomization>]
	public void Contains_False(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FreeLayoutEngine(context, identity).AddWindow(window);

		// When
		bool contains = engine.ContainsWindow(Substitute.For<IWindow>());

		// Then
		Assert.False(contains);
	}

	#region DoLayout
	[Fact]
	public void DoLayout_Empty()
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity);

		// When
		IWindowState[] windowStates = engine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		// Then
		Assert.Empty(windowStates);
	}

	[Theory, AutoSubstituteData<FreeLayoutEngineCustomization>]
	public void DoLayout_KeepWindowSize(
		IContext context,
		IWindow windowNormal,
		IWindow windowMinimized,
		IWindow windowMaximized
	)
	{
		// Given
		windowMinimized.IsMinimized.Returns(true);
		windowMaximized.IsMaximized.Returns(true);
		ILayoutEngine engine = new FreeLayoutEngine(context, identity)
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

	[Theory, AutoSubstituteData]
	public void GetFirstWindow_Null(IContext context)
	{
		// Given
		ILayoutEngine engine = new FreeLayoutEngine(context, identity);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<FreeLayoutEngineCustomization>]
	public void GetFirstWindow_SingleWindow(IContext context, IWindow window)
	{
		// Given
		ILayoutEngine engine = new FreeLayoutEngine(context, identity).AddWindow(window);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window, result);
	}
}
