using AutoFixture;
using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutEngineCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext context = fixture.Freeze<IContext>();
		IMonitor monitor = fixture.Freeze<IMonitor>();

		context.MonitorManager.GetMonitorAtPoint(Arg.Any<ILocation<int>>()).Returns(monitor);
		monitor.WorkingArea.Returns(new Location<int>() { Width = 1000, Height = 1000 });
		context.NativeManager
			.DwmGetWindowLocation(Arg.Any<HWND>())
			.Returns(new Location<int>() { Width = 100, Height = 100 });

		fixture.Inject(context);
		fixture.Inject(monitor);
	}
}

public class FloatingLayoutEngineTests
{
	private FloatingLayoutEngineTests MarkWindowAsFloating(
		IInternalFloatingLayoutPlugin plugin,
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

	private FloatingLayoutEngineTests Setup_RemoveWindow(
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		innerLayoutEngine.RemoveWindow(window).Returns(newInnerLayoutEngine);
		newInnerLayoutEngine.Identity.Returns(innerLayoutEngine.Identity);
		return this;
	}

	private FloatingLayoutEngineTests Setup_AddWindow(
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
	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void AddWindow_UseInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void AddWindow_UseInner_SameInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		innerLayoutEngine.AddWindow(window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window);

		// Then
		Assert.Same(engine, newEngine);
		innerLayoutEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void AddWindow_FloatingInPlugin_Succeed(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(0).AddWindow(window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void AddWindow_FloatingInPlugin_FailOnNoLocation(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		context.NativeManager.DwmGetWindowLocation(Arg.Any<HWND>()).Returns((Location<int>?)null);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void AddWindow_FloatingInPlugin_FailOnSameLocation(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.AddWindow(window);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		innerLayoutEngine.Received(1).RemoveWindow(window);
		newInnerLayoutEngine.Received(0).AddWindow(window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void AddWindow_FloatingInPlugin_RemoveFromInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Setup_AddWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

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
	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void RemoveWindow_UseInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.RemoveWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(1).RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void RemoveWindow_UseInner_SameInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.RemoveWindow(window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.RemoveWindow(window);

		// Then
		Assert.Same(engine, newEngine);
		innerLayoutEngine.Received(1).RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void RemoveWindow_FloatingInPlugin(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		innerLayoutEngine.ClearReceivedCalls();
		plugin.ClearReceivedCalls();

		ILayoutEngine newEngine2 = newEngine1.RemoveWindow(window);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		innerLayoutEngine.Received(0).RemoveWindow(window);
		_ = plugin.Received(1).FloatingWindows;
	}
	#endregion

	#region MoveWindowToPoint
	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_UseInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ILocation<double> location = new Location<double>();
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, location);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(1).MoveWindowToPoint(window, location);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_UseInner_SameInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ILocation<double> location = new Location<double>();

		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.MoveWindowToPoint(window, location).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, location);

		// Then
		Assert.Same(engine, newEngine);
		innerLayoutEngine.Received(1).MoveWindowToPoint(window, location);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_FloatingInPlugin_WindowIsNew(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		ILocation<double> location = new Location<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, location);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(0).MoveWindowToPoint(window, location);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_FloatingInPlugin_WindowIsNotNew_SameLocation(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		ILocation<double> location = new Location<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowToPoint(window, location);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		innerLayoutEngine.Received(0).MoveWindowToPoint(window, location);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_FloatingInPlugin_WindowIsNotNew_DifferentLocation(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		ILocation<double> location = new Location<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowToPoint(window, location);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		innerLayoutEngine.Received(0).MoveWindowToPoint(window, location);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowToPoint_FloatingInPlugin_CannotGetDwmLocation(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		ILocation<double> location = new Location<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_AddWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		context.NativeManager.DwmGetWindowLocation(Arg.Any<HWND>()).Returns((Location<int>?)null);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowToPoint(window, location);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		innerLayoutEngine.Received(1).AddWindow(window);
		newInnerLayoutEngine.Received(1).MoveWindowToPoint(window, location);
	}
	#endregion

	#region MoveWindowEdgesInDirection
	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_UseInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(1).MoveWindowEdgesInDirection(direction, deltas, window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_UseInner_SameInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		innerLayoutEngine.MoveWindowEdgesInDirection(direction, deltas, window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.Same(engine, newEngine);
		innerLayoutEngine.Received(1).MoveWindowEdgesInDirection(direction, deltas, window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_FloatingInPlugin_WindowIsNew(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(engine, newEngine);
		innerLayoutEngine.Received(0).MoveWindowEdgesInDirection(direction, deltas, window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_FloatingInPlugin_WindowIsNotNew_SameLocation(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
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
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		innerLayoutEngine.Received(0).MoveWindowEdgesInDirection(direction, deltas, window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_FloatingInPlugin_WindowIsNotNew_DifferentLocation(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
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
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		innerLayoutEngine.Received(0).MoveWindowEdgesInDirection(direction, deltas, window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void MoveWindowEdgesInDirection_FloatingInPlugin_CannotGetDwmLocation(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
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
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		context.NativeManager.DwmGetWindowLocation(Arg.Any<HWND>()).Returns((Location<int>?)null);

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
	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void DoLayout(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
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

		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		newInnerLayoutEngine
			.DoLayout(Arg.Any<ILocation<int>>(), Arg.Any<IMonitor>())
			.Returns(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1,
						Location = new Location<int>(),
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2,
						Location = new Location<int>(),
						WindowSize = WindowSize.Normal
					}
				}
			);
		newInnerLayoutEngine.Count.Returns(2);

		// When
		ILayoutEngine newEngine = engine.AddWindow(floatingWindow);
		IWindowState[] windowStates = newEngine
			.DoLayout(
				new Location<int>()
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
				Location = new Location<int>()
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
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = window2,
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal
			}
		};

		windowStates.Should().Equal(expected);

		Assert.Equal(3, count);
	}
	#endregion

	#region GetFirstWindow
	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void GetFirstWindow_NoInnerFirstWindow(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine
	)
	{
		// Given
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.GetFirstWindow().Returns((IWindow?)null);

		// When
		IWindow? firstWindow = engine.GetFirstWindow();

		// Then
		Assert.Null(firstWindow);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void GetFirstWindow_InnerFirstWindow(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.GetFirstWindow().Returns(window);

		// When
		IWindow? firstWindow = engine.GetFirstWindow();

		// Then
		Assert.Same(window, firstWindow);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void GetFirstWindow_FloatingFirstWindow(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

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
	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void FocusWindowInDirection_UseInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.GetFirstWindow().Returns(window);

		// When
		engine.FocusWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.Received(1).FocusWindowInDirection(direction, window);
		innerLayoutEngine.Received(0).GetFirstWindow();
		window.Received(0).Focus();
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void FocusWindowInDirection_FloatingWindow_NullFirstWindow(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		engine.AddWindow(window).FocusWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.Received(0).FocusWindowInDirection(direction, window);
		innerLayoutEngine.Received(0).GetFirstWindow();

		newInnerLayoutEngine.Received(0).FocusWindowInDirection(direction, window);
		newInnerLayoutEngine.Received(1).GetFirstWindow();

		window.Received(0).Focus();
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void FocusWindowInDirection_FloatingWindow_DefinedFirstWindow(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		newInnerLayoutEngine.GetFirstWindow().Returns(window);

		// When
		engine.AddWindow(window).FocusWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.Received(0).FocusWindowInDirection(direction, window);
		innerLayoutEngine.Received(0).GetFirstWindow();

		newInnerLayoutEngine.Received(0).FocusWindowInDirection(direction, window);
		newInnerLayoutEngine.Received(1).GetFirstWindow();

		window.Received(1).Focus();
	}
	#endregion

	#region SwapWindowInDirection
	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void SwapWindowInDirection_UseInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		engine.SwapWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.Received(1).SwapWindowInDirection(direction, window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void SwapWindowInDirection_UseInner_SameInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		Direction direction = Direction.Left;
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		innerLayoutEngine.SwapWindowInDirection(direction, window).Returns(innerLayoutEngine);

		// When
		engine.SwapWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.Received(1).SwapWindowInDirection(direction, window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void SwapWindowInDirection_FloatingWindow(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;

		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		engine.AddWindow(window).SwapWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.Received(0).SwapWindowInDirection(direction, window);
	}
	#endregion

	#region ContainsWindow
	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void ContainsWindow_UseInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		bool containsWindow = engine.ContainsWindow(window);

		// Then
		Assert.False(containsWindow);
		innerLayoutEngine.Received(1).ContainsWindow(window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void ContainsWindow_UseInner_SameInner(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window
	)
	{
		// Given
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);
		innerLayoutEngine.ContainsWindow(window).Returns(true);

		// When
		bool containsWindow = engine.ContainsWindow(window);

		// Then
		Assert.True(containsWindow);
		innerLayoutEngine.Received(1).ContainsWindow(window);
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void ContainsWindow_FloatingWindow(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		MarkWindowAsFloating(plugin, window, innerLayoutEngine)
			.Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When
		bool containsWindow = engine.AddWindow(window).ContainsWindow(window);

		// Then
		Assert.True(containsWindow);
		innerLayoutEngine.Received(0).ContainsWindow(window);
	}
	#endregion

	#region WindowWasFloating_ShouldBeGarbageCollectedByUpdateInner
	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void WindowWasFloating_AddWindow(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

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

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void WindowWasFloating_MoveWindowToPoint(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

		// When the window is floating...
		MarkWindowAsFloating(plugin, window, innerLayoutEngine);
		ILayoutEngine newEngine = engine.AddWindow(window);

		// ...marked as docked...
		plugin.FloatingWindows.Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

		// ... and then moved
		ILayoutEngine newEngine2 = newEngine.MoveWindowToPoint(window, new Location<double>());

		// Then MoveWindowToPoint should be called on the inner layout engine
		Assert.NotSame(engine, newEngine);
		Assert.NotSame(newEngine, newEngine2);
		newInnerLayoutEngine.Received(1).MoveWindowToPoint(window, new Location<double>());
	}

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void WindowWasFloating_RemoveWindow(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

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

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void WindowWasFloating_MoveWindowEdgesInDirection(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Point<double> deltas = new();

		Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

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

	[Theory, AutoSubstituteData<FloatingLayoutEngineCustomization>]
	internal void WindowWasFloating_SwapWindowInDirection(
		IContext context,
		IInternalFloatingLayoutPlugin plugin,
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Setup_RemoveWindow(innerLayoutEngine, window, newInnerLayoutEngine);
		FloatingLayoutEngine engine = new(context, plugin, innerLayoutEngine);

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
}
