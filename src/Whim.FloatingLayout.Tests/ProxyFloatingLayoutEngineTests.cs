using AutoFixture;
using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class ProxyFloatingLayoutEngineCustomization : ICustomization
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

public class ProxyFloatingLayoutEngineTests
{
	private ProxyFloatingLayoutEngineTests MarkWindowAsFloating(
		IInternalProxyFloatingLayoutPlugin plugin,
		IWindow window,
		ILayoutEngine innerLayoutEngine
	)
	{
		IReadOnlyDictionary<IWindow, ISet<LayoutEngineIdentity>> floatingWindows = new Dictionary<
			IWindow,
			ISet<LayoutEngineIdentity>
		>
		{
			{
				window,
				new HashSet<LayoutEngineIdentity> { innerLayoutEngine.Identity }
			}
		};
		plugin.FloatingWindows.Returns(floatingWindows);
		return this;
	}

	private ProxyFloatingLayoutEngineTests Setup_RemoveWindow(
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		innerLayoutEngine.RemoveWindow(window).Returns(newInnerLayoutEngine);
		newInnerLayoutEngine.Identity.Returns(innerLayoutEngine.Identity);
		return this;
	}

	private ProxyFloatingLayoutEngineTests Setup_AddWindow(
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		innerLayoutEngine.AddWindow(window).Returns(newInnerLayoutEngine);
		newInnerLayoutEngine.Identity.Returns(innerLayoutEngine.Identity);
		return this;
	}

	#region AddWindow
	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void AddWindow_UseInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void AddWindow_UseInner_SameInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		innerLayoutEngine.AddWindow(window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window);

		// Then
		Assert.Same(engine, newEngine);
		innerLayoutEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void AddWindow_FloatingInPlugin_Succeed(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void AddWindow_FloatingInPlugin_FailOnNoRectangle(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		context.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>()).Returns((Rectangle<int>?)null);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void AddWindow_FloatingInPlugin_FailOnSameRectangle(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.AddWindow(window);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		innerLayoutEngine.Received(1).RemoveWindow(window);
		newInnerLayoutEngine.DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void AddWindow_FloatingInPlugin_RemoveFromInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Setup_AddWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window);
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ILayoutEngine newEngine2 = newEngine.AddWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.NotSame(newEngine, newEngine2);
		innerLayoutEngine.Received(1).AddWindow(window);
		newInnerLayoutEngine.Received(1).RemoveWindow(window);
	}
	#endregion

	#region RemoveWindow
	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void RemoveWindow_UseInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.RemoveWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(1).RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void RemoveWindow_UseInner_SameInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.RemoveWindow(window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.RemoveWindow(window);

		// Then
		Assert.Same(engine, newEngine);
		innerLayoutEngine.Received(1).RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void RemoveWindow_FloatingInPlugin(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		innerLayoutEngine.ClearReceivedCalls();
		plugin.ClearReceivedCalls();

		ILayoutEngine newEngine2 = newEngine1.RemoveWindow(window);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		innerLayoutEngine.DidNotReceive().RemoveWindow(window);
		_ = plugin.Received(1).FloatingWindows;
	}
	#endregion

	#region MoveWindowToPoint
	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_UseInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		IRectangle<double> rect = new Rectangle<double>();
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, rect);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(1).MoveWindowToPoint(window, rect);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_UseInner_SameInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		IRectangle<double> rect = new Rectangle<double>();

		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.MoveWindowToPoint(window, rect).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, rect);

		// Then
		Assert.Same(engine, newEngine);
		innerLayoutEngine.Received(1).MoveWindowToPoint(window, rect);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_FloatingInPlugin_WindowIsNew(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		IRectangle<double> rect = new Rectangle<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, rect);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.DidNotReceive().MoveWindowToPoint(window, rect);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_FloatingInPlugin_WindowIsNotNew_SameRectangle(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		IRectangle<double> rect = new Rectangle<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowToPoint(window, rect);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		innerLayoutEngine.DidNotReceive().MoveWindowToPoint(window, rect);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_FloatingInPlugin_WindowIsNotNew_DifferentRectangle(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		IRectangle<double> rect = new Rectangle<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowToPoint(window, rect);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		innerLayoutEngine.DidNotReceive().MoveWindowToPoint(window, rect);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_FloatingInPlugin_CannotGetDwmRectangle(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		IRectangle<double> rect = new Rectangle<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_AddWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		context.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>()).Returns((Rectangle<int>?)null);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowToPoint(window, rect);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		innerLayoutEngine.Received(1).AddWindow(window);
		newInnerLayoutEngine.Received(1).MoveWindowToPoint(window, rect);
	}
	#endregion

	#region MoveWindowEdgesInDirection
	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_UseInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(1).MoveWindowEdgesInDirection(direction, deltas, window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_UseInner_SameInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		innerLayoutEngine.MoveWindowEdgesInDirection(direction, deltas, window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.Same(engine, newEngine);
		innerLayoutEngine.Received(1).MoveWindowEdgesInDirection(direction, deltas, window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_FloatingInPlugin_WindowIsNew(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.DidNotReceive().MoveWindowEdgesInDirection(direction, deltas, window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_FloatingInPlugin_WindowIsNotNew_SameRectangle(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		innerLayoutEngine.DidNotReceive().MoveWindowEdgesInDirection(direction, deltas, window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_FloatingInPlugin_WindowIsNotNew_DifferentRectangle(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		innerLayoutEngine.DidNotReceive().MoveWindowEdgesInDirection(direction, deltas, window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_FloatingInPlugin_CannotGetDwmRectangle(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_AddWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		context.NativeManager.DwmGetWindowRectangle(Arg.Any<HWND>()).Returns((Rectangle<int>?)null);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		innerLayoutEngine.Received(1).AddWindow(window);
		newInnerLayoutEngine.Received(1).MoveWindowEdgesInDirection(direction, deltas, window);
	}
	#endregion

	#region DoLayout
	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void DoLayout(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		ILayoutEngine newInnerLayoutEngine,
		IWindow window1,
		IWindow window2,
		IWindow floatingWindow,
		IMonitor monitor
	)
	{
		// Given the window has been added and the inner layout engine has a layout
		MarkWindowAsFloating(plugin, floatingWindow, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, floatingWindow, newInnerLayoutEngine);

		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		newInnerLayoutEngine
			.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>())
			.Returns(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>(),
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2,
						Rectangle = new Rectangle<int>(),
						WindowSize = WindowSize.Normal
					}
				}
			);
		newInnerLayoutEngine.Count.Returns(2);

		// When
		ILayoutEngine newEngine = engine.AddWindow(floatingWindow);
		IWindowState[] windowStates = newEngine
			.DoLayout(
				new Rectangle<int>()
				{
					X = 0,
					Y = 0,
					Width = 1000,
					Height = 1000
				},
				monitor
			)
			.ToArray();
		int count = newEngine.Count;

		// Then
		Assert.Equal(3, windowStates.Length);

		IWindowState[] expected = new IWindowState[]
		{
			new WindowState()
			{
				Window = floatingWindow,
				Rectangle = new Rectangle<int>()
				{
					X = 0,
					Y = 0,
					Width = 100,
					Height = 100
				},
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = window1,
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = window2,
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal
			}
		};

		windowStates.Should().Equal(expected);

		Assert.Equal(3, count);
	}
	#endregion

	#region GetFirstWindow
	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void GetFirstWindow_NoInnerFirstWindow(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.GetFirstWindow().Returns((IWindow?)null);

		// When
		IWindow? firstWindow = engine.GetFirstWindow();

		// Then
		Assert.Null(firstWindow);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void GetFirstWindow_InnerFirstWindow(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.GetFirstWindow().Returns(window);

		// When
		IWindow? firstWindow = engine.GetFirstWindow();

		// Then
		Assert.Same(window, firstWindow);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void GetFirstWindow_FloatingFirstWindow(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);

		newInnerLayoutEngine.GetFirstWindow().Returns((IWindow?)null);

		// When
		IWindow? firstWindow = engine.AddWindow(window).GetFirstWindow();

		// Then
		Assert.Same(window, firstWindow);
	}
	#endregion

	#region FocusWindowInDirection
	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void FocusWindowInDirection_UseInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.GetFirstWindow().Returns(window);

		// When
		ILayoutEngine newEngine = engine.FocusWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.Received(1).FocusWindowInDirection(direction, window);
		innerLayoutEngine.DidNotReceive().GetFirstWindow();
		window.DidNotReceive().Focus();
		Assert.NotSame(engine, newEngine);
		Assert.IsType<ProxyFloatingLayoutEngine>(newEngine);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void FocusWindowInDirection_FloatingWindow_NullFirstWindow(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window).FocusWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.DidNotReceive().FocusWindowInDirection(direction, window);
		innerLayoutEngine.DidNotReceive().GetFirstWindow();

		newInnerLayoutEngine.DidNotReceive().FocusWindowInDirection(direction, window);
		newInnerLayoutEngine.Received(1).GetFirstWindow();

		window.DidNotReceive().Focus();
		Assert.NotSame(engine, newEngine);
		Assert.IsType<ProxyFloatingLayoutEngine>(newEngine);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void FocusWindowInDirection_FloatingWindow_DefinedFirstWindow(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		newInnerLayoutEngine.GetFirstWindow().Returns(window);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window).FocusWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.DidNotReceive().FocusWindowInDirection(direction, window);
		innerLayoutEngine.DidNotReceive().GetFirstWindow();

		newInnerLayoutEngine.DidNotReceive().FocusWindowInDirection(direction, window);
		newInnerLayoutEngine.Received(1).GetFirstWindow();

		window.Received(1).Focus();
		Assert.NotSame(engine, newEngine);
		Assert.IsType<ProxyFloatingLayoutEngine>(newEngine);
	}
	#endregion

	#region SwapWindowInDirection
	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void SwapWindowInDirection_UseInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.Received(1).SwapWindowInDirection(direction, window);
		Assert.NotSame(engine, newEngine);
		Assert.IsType<ProxyFloatingLayoutEngine>(newEngine);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void SwapWindowInDirection_UseInner_SameInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		innerLayoutEngine.SwapWindowInDirection(direction, window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.Received(1).SwapWindowInDirection(direction, window);
		Assert.Same(engine, newEngine);
		Assert.IsType<ProxyFloatingLayoutEngine>(newEngine);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void SwapWindowInDirection_FloatingWindow(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window).SwapWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.DidNotReceive().SwapWindowInDirection(direction, window);
		Assert.NotSame(engine, newEngine);
		Assert.IsType<ProxyFloatingLayoutEngine>(newEngine);
	}
	#endregion

	#region ContainsWindow
	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void ContainsWindow_UseInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		bool containsWindow = engine.ContainsWindow(window);

		// Then
		Assert.False(containsWindow);
		innerLayoutEngine.Received(1).ContainsWindow(window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void ContainsWindow_UseInner_SameInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.ContainsWindow(window).Returns(true);

		// When
		bool containsWindow = engine.ContainsWindow(window);

		// Then
		Assert.True(containsWindow);
		innerLayoutEngine.Received(1).ContainsWindow(window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void ContainsWindow_FloatingWindow(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		bool containsWindow = engine.AddWindow(window).ContainsWindow(window);

		// Then
		Assert.True(containsWindow);
		innerLayoutEngine.DidNotReceive().ContainsWindow(window);
	}
	#endregion

	#region WindowWasFloating_ShouldBeGarbageCollectedByUpdateInner
	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void WindowWasFloating_AddWindow(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When the window is floating...
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ILayoutEngine newEngine = engine.AddWindow(window);

		// ...marked as docked...
		plugin.FloatingWindows.Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

		// ... and then added
		ILayoutEngine newEngine2 = newEngine.AddWindow(window);

		// Then AddWindow should be called on the inner layout engine
		Assert.NotSame(engine, newEngine);
		Assert.NotSame(newEngine, newEngine2);
		newInnerLayoutEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void WindowWasFloating_MoveWindowToPoint(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When the window is floating...
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ILayoutEngine newEngine = engine.AddWindow(window);

		// ...marked as docked...
		plugin.FloatingWindows.Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

		// ... and then moved
		ILayoutEngine newEngine2 = newEngine.MoveWindowToPoint(window, new Rectangle<double>());

		// Then MoveWindowToPoint should be called on the inner layout engine
		Assert.NotSame(engine, newEngine);
		Assert.NotSame(newEngine, newEngine2);
		newInnerLayoutEngine.Received(1).MoveWindowToPoint(window, new Rectangle<double>());
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void WindowWasFloating_RemoveWindow(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When the window is floating...
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ILayoutEngine newEngine = engine.AddWindow(window);

		// ...marked as docked...
		plugin.FloatingWindows.Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

		// ... and then removed
		ILayoutEngine newEngine2 = newEngine.RemoveWindow(window);

		// Then RemoveWindow should be called on the inner layout engine
		Assert.NotSame(engine, newEngine);
		Assert.NotSame(newEngine, newEngine2);
		newInnerLayoutEngine.Received(1).RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void WindowWasFloating_MoveWindowEdgesInDirection(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Point<double> deltas = new();

		Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When the window is floating...
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ILayoutEngine newEngine = engine.AddWindow(window);

		// ...marked as docked...
		plugin.FloatingWindows.Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

		// ... and then the edges are moved
		ILayoutEngine newEngine2 = newEngine.MoveWindowEdgesInDirection(Direction.Left, deltas, window);

		// Then MoveWindowEdgesInDirection should be called on the inner layout engine
		Assert.NotSame(engine, newEngine);
		Assert.NotSame(newEngine, newEngine2);
		newInnerLayoutEngine.Received(1).MoveWindowEdgesInDirection(Direction.Left, deltas, window);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void WindowWasFloating_SwapWindowInDirection(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When the window is floating...
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ILayoutEngine newEngine = engine.AddWindow(window);

		// ...marked as docked...
		plugin.FloatingWindows.Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

		// ... and then window is swapped in a direction
		ILayoutEngine newEngine2 = newEngine.SwapWindowInDirection(Direction.Left, window);

		// Then SwapWindowInDirection should be called on the inner layout engine
		Assert.NotSame(engine, newEngine);
		Assert.NotSame(newEngine, newEngine2);
		newInnerLayoutEngine.Received(1).SwapWindowInDirection(Direction.Left, window);
	}
	#endregion

	#region PerformCustomAction
	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void PerformCustomAction_UseInner(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		LayoutEngineCustomAction<string> action =
			new()
			{
				Name = "Action",
				Payload = "payload",
				Window = null
			};

		// When
		ILayoutEngine newEngine = engine.PerformCustomAction(action);

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.IsType<ProxyFloatingLayoutEngine>(newEngine);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void PerformCustomAction_UseInner_WindowIsDefined(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		LayoutEngineCustomAction<string> action =
			new()
			{
				Name = "Action",
				Payload = "payload",
				Window = Substitute.For<IWindow>()
			};
		innerLayoutEngine.PerformCustomAction(action).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.PerformCustomAction(action);

		// Then
		Assert.Same(engine, newEngine);
		innerLayoutEngine.Received(1).PerformCustomAction(action);
		Assert.IsType<ProxyFloatingLayoutEngine>(newEngine);
	}

	[Theory, AutoSubstituteData<ProxyFloatingLayoutEngineCustomization>]
	internal void PerformCustomAction_FloatingWindow(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		LayoutEngineCustomAction<string> action =
			new()
			{
				Name = "Action",
				Payload = "payload",
				Window = window
			};
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ILayoutEngine newEngine = engine.AddWindow(window);

		// When
		ILayoutEngine newEngine2 = newEngine.PerformCustomAction(action);

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.Same(newEngine, newEngine2);
		innerLayoutEngine.DidNotReceive().PerformCustomAction(action);
		Assert.IsType<ProxyFloatingLayoutEngine>(newEngine);
	}
	#endregion


	[Theory, AutoSubstituteData]
	internal void MinimizeWindowStart_NotSame(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MinimizeWindowStart(window);

		// Then
		Assert.NotSame(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	internal void MinimizeWindowStart_Same(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.MinimizeWindowStart(window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MinimizeWindowStart(window);

		// Then
		Assert.Same(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	internal void MinimizeWindowEnd_NotSame(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MinimizeWindowEnd(window);

		// Then
		Assert.NotSame(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	internal void MinimizeWindowEnd_Same(
		IContext context,
		IInternalProxyFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ProxyFloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.MinimizeWindowEnd(window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MinimizeWindowEnd(window);

		// Then
		Assert.Same(engine, newEngine);
	}
}
