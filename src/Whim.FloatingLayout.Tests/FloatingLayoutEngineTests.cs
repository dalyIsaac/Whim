using FluentAssertions;
using Moq;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutEngineTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IInternalFloatingLayoutPlugin> Plugin { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();

		public Mock<ILayoutEngine> InnerLayoutEngine { get; } = new();

		public Wrapper()
		{
			Context.SetupGet(x => x.NativeManager).Returns(NativeManager.Object);
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);

			NativeManager
				.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>()))
				.Returns(new Location<int>() { Width = 100, Height = 100 });

			MonitorManager.Setup(mm => mm.GetMonitorAtPoint(It.IsAny<ILocation<int>>())).Returns(Monitor.Object);

			Monitor.SetupGet(m => m.WorkingArea).Returns(new Location<int>() { Width = 1000, Height = 1000 });

			Plugin.SetupGet(x => x.FloatingWindows).Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

			InnerLayoutEngine.Setup(ile => ile.Identity).Returns(new LayoutEngineIdentity());
		}

		public Wrapper MarkAsFloating(IWindow window)
		{
			Plugin
				.SetupGet(x => x.FloatingWindows)
				.Returns(
					new Dictionary<IWindow, ISet<LayoutEngineIdentity>>
					{
						{
							window,
							new HashSet<LayoutEngineIdentity> { InnerLayoutEngine.Object.Identity }
						}
					}
				);
			return this;
		}

		public Wrapper Setup_RemoveWindow(IWindow window, Mock<ILayoutEngine> newInnerLayoutEngine)
		{
			InnerLayoutEngine.Setup(ile => ile.RemoveWindow(window)).Returns(newInnerLayoutEngine.Object);
			newInnerLayoutEngine.Setup(ile => ile.Identity).Returns(InnerLayoutEngine.Object.Identity);
			return this;
		}

		public Wrapper Setup_AddWindow(IWindow window, Mock<ILayoutEngine> newInnerLayoutEngine)
		{
			InnerLayoutEngine.Setup(ile => ile.AddWindow(window)).Returns(newInnerLayoutEngine.Object);
			newInnerLayoutEngine.Setup(ile => ile.Identity).Returns(InnerLayoutEngine.Object.Identity);
			return this;
		}
	}

	#region AddWindow
	[Fact]
	public void AddWindow_UseInner()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
	}

	[Fact]
	public void AddWindow_UseInner_SameInner()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.InnerLayoutEngine.Setup(ile => ile.AddWindow(window.Object)).Returns(wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// Then
		Assert.Same(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
	}

	[Fact]
	public void AddWindow_FloatingInPlugin_Succeed()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new Wrapper().MarkAsFloating(window.Object);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Never);
	}

	[Fact]
	public void AddWindow_FloatingInPlugin_FailOnNoLocation()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new Wrapper().MarkAsFloating(window.Object);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns((Location<int>?)null);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
	}

	[Fact]
	public void AddWindow_FloatingInPlugin_FailOnSameLocation()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);
		ILayoutEngine newEngine2 = newEngine1.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Once);
		newInnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Never);
	}

	[Fact]
	public void AddWindow_FloatingInPlugin_RemoveFromInner()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper().Setup_AddWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window.Object);
		wrapper.MarkAsFloating(window.Object);
		ILayoutEngine newEngine2 = newEngine.AddWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.NotSame(newEngine, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
		newInnerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Once);
	}
	#endregion

	#region RemoveWindow
	[Fact]
	public void RemoveWindow_UseInner()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.RemoveWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Once);
	}

	[Fact]
	public void RemoveWindow_UseInner_SameInner()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.InnerLayoutEngine
			.Setup(ile => ile.RemoveWindow(window.Object))
			.Returns(wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.RemoveWindow(window.Object);

		// Then
		Assert.Same(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Once);
	}

	[Fact]
	public void RemoveWindow_FloatingInPlugin()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		wrapper.MarkAsFloating(window.Object);

		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);

		wrapper.InnerLayoutEngine.Invocations.Clear();
		wrapper.Plugin.Invocations.Clear();

		ILayoutEngine newEngine2 = newEngine1.RemoveWindow(window.Object);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Never);
		wrapper.Plugin.VerifyGet(x => x.FloatingWindows, Times.Once);
	}
	#endregion

	#region MoveWindowToPoint
	[Fact]
	public void MoveWindowToPoint_UseInner()
	{
		// Given
		Mock<IWindow> window = new();
		ILocation<double> location = new Location<double>();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window.Object, location);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, location), Times.Once);
	}

	[Fact]
	public void MoveWindowToPoint_UseInner_SameInner()
	{
		// Given
		Mock<IWindow> window = new();
		ILocation<double> location = new Location<double>();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.InnerLayoutEngine
			.Setup(ile => ile.MoveWindowToPoint(window.Object, location))
			.Returns(wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window.Object, location);

		// Then
		Assert.Same(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, location), Times.Once);
	}

	[Fact]
	public void MoveWindowToPoint_FloatingInPlugin_WindowIsNew()
	{
		// Given
		Mock<IWindow> window = new();
		ILocation<double> location = new Location<double>();

		Wrapper wrapper = new Wrapper().MarkAsFloating(window.Object);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window.Object, location);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, location), Times.Never);
	}

	[Fact]
	public void MoveWindowToPoint_FloatingInPlugin_WindowIsNotNew_SameLocation()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		ILocation<double> location = new Location<double>();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowToPoint(window.Object, location);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, location), Times.Never);
	}

	[Fact]
	public void MoveWindowToPoint_FloatingInPlugin_WindowIsNotNew_DifferentLocation()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		ILocation<double> location = new Location<double>();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);
		wrapper.NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns(new Location<int>());
		ILayoutEngine newEngine2 = newEngine1.MoveWindowToPoint(window.Object, location);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, location), Times.Never);
	}

	[Fact]
	public void MoveWindowToPoint_FloatingInPlugin_CannotGetDwmLocation()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		ILocation<double> location = new Location<double>();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_AddWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns((Location<int>?)null);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowToPoint(window.Object, location);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
		newInnerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, location), Times.Once);
	}
	#endregion

	#region MoveWindowEdgesInDirection
	[Fact]
	public void MoveWindowEdgesInDirection_UseInner()
	{
		// Given
		Mock<IWindow> window = new();
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(
			ile => ile.MoveWindowEdgesInDirection(direction, deltas, window.Object),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_UseInner_SameInner()
	{
		// Given
		Mock<IWindow> window = new();
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.InnerLayoutEngine
			.Setup(ile => ile.MoveWindowEdgesInDirection(direction, deltas, window.Object))
			.Returns(wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window.Object);

		// Then
		Assert.Same(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(
			ile => ile.MoveWindowEdgesInDirection(direction, deltas, window.Object),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_FloatingInPlugin_WindowIsNew()
	{
		// Given
		Mock<IWindow> window = new();
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		Wrapper wrapper = new Wrapper().MarkAsFloating(window.Object);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window.Object);

		// Then
		Assert.NotSame(engine, newEngine);
		wrapper.InnerLayoutEngine.Verify(
			ile => ile.MoveWindowEdgesInDirection(direction, deltas, window.Object),
			Times.Never
		);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_FloatingInPlugin_WindowIsNotNew_SameLocation()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowEdgesInDirection(direction, deltas, window.Object);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.Same(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(
			ile => ile.MoveWindowEdgesInDirection(direction, deltas, window.Object),
			Times.Never
		);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_FloatingInPlugin_WindowIsNotNew_DifferentLocation()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);
		wrapper.NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns(new Location<int>());
		ILayoutEngine newEngine2 = newEngine1.MoveWindowEdgesInDirection(direction, deltas, window.Object);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(
			ile => ile.MoveWindowEdgesInDirection(direction, deltas, window.Object),
			Times.Never
		);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_FloatingInPlugin_CannotGetDwmLocation()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_AddWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.NativeManager.Setup(nm => nm.DwmGetWindowLocation(It.IsAny<HWND>())).Returns((Location<int>?)null);

		// When
		ILayoutEngine newEngine1 = engine.AddWindow(window.Object);
		ILayoutEngine newEngine2 = newEngine1.MoveWindowEdgesInDirection(direction, deltas, window.Object);

		// Then
		Assert.NotSame(engine, newEngine1);
		Assert.NotSame(newEngine1, newEngine2);
		wrapper.InnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
		newInnerLayoutEngine.Verify(
			ile => ile.MoveWindowEdgesInDirection(direction, deltas, window.Object),
			Times.Once
		);
	}
	#endregion

	#region DoLayout
	[Fact]
	public void DoLayout()
	{
		// Given the window has been added and the inner layout engine has a layout
		Mock<IWindow> floatingWindow = new();
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(floatingWindow.Object)
			.Setup_RemoveWindow(floatingWindow.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		newInnerLayoutEngine
			.Setup(ile => ile.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()))
			.Returns(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>(),
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>(),
						WindowSize = WindowSize.Normal
					}
				}
			);
		newInnerLayoutEngine.Setup(ile => ile.Count).Returns(2);

		// When
		ILayoutEngine newEngine = engine.AddWindow(floatingWindow.Object);
		IWindowState[] windowStates = newEngine
			.DoLayout(
				new Location<int>()
				{
					X = 0,
					Y = 0,
					Width = 1000,
					Height = 1000
				},
				wrapper.Monitor.Object
			)
			.ToArray();
		int count = newEngine.Count;

		// Then
		Assert.Equal(3, windowStates.Length);

		IWindowState[] expected = new IWindowState[]
		{
			new WindowState()
			{
				Window = floatingWindow.Object,
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
				Window = window1.Object,
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = window2.Object,
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal
			}
		};

		windowStates.Should().Equal(expected);

		Assert.Equal(3, count);
	}
	#endregion

	#region GetFirstWindow
	[Fact]
	public void GetFirstWindow_NoInnerFirstWindow()
	{
		// Given
		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		IWindow? firstWindow = engine.GetFirstWindow();

		// Then
		Assert.Null(firstWindow);
	}

	[Fact]
	public void GetFirstWindow_InnerFirstWindow()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.InnerLayoutEngine.Setup(ile => ile.GetFirstWindow()).Returns(window.Object);

		// When
		IWindow? firstWindow = engine.GetFirstWindow();

		// Then
		Assert.Same(window.Object, firstWindow);
	}

	[Fact]
	public void GetFirstWindow_FloatingFirstWindow()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		IWindow? firstWindow = engine.AddWindow(window.Object).GetFirstWindow();

		// Then
		Assert.Same(window.Object, firstWindow);
	}
	#endregion

	#region FocusWindowInDirection
	[Fact]
	public void FocusWindowInDirection_UseInner()
	{
		// Given
		Mock<IWindow> window = new();
		Direction direction = Direction.Left;

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.InnerLayoutEngine.Setup(ile => ile.GetFirstWindow()).Returns(window.Object);

		// When
		engine.FocusWindowInDirection(direction, window.Object);

		// Then
		wrapper.InnerLayoutEngine.Verify(ile => ile.FocusWindowInDirection(direction, window.Object), Times.Once);
		wrapper.InnerLayoutEngine.Verify(ile => ile.GetFirstWindow(), Times.Never);
		window.Verify(w => w.Focus(), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_FloatingWindow_NullFirstWindow()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		Direction direction = Direction.Left;

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		engine.AddWindow(window.Object).FocusWindowInDirection(direction, window.Object);

		// Then
		wrapper.InnerLayoutEngine.Verify(ile => ile.FocusWindowInDirection(direction, window.Object), Times.Never);
		wrapper.InnerLayoutEngine.Verify(ile => ile.GetFirstWindow(), Times.Never);

		newInnerLayoutEngine.Verify(ile => ile.FocusWindowInDirection(direction, window.Object), Times.Never);
		newInnerLayoutEngine.Verify(ile => ile.GetFirstWindow(), Times.Once);

		window.Verify(w => w.Focus(), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_FloatingWindow_DefinedFirstWindow()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		Direction direction = Direction.Left;

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		newInnerLayoutEngine.Setup(ile => ile.GetFirstWindow()).Returns(window.Object);

		// When
		engine.AddWindow(window.Object).FocusWindowInDirection(direction, window.Object);

		// Then
		wrapper.InnerLayoutEngine.Verify(ile => ile.FocusWindowInDirection(direction, window.Object), Times.Never);
		wrapper.InnerLayoutEngine.Verify(ile => ile.GetFirstWindow(), Times.Never);

		newInnerLayoutEngine.Verify(ile => ile.FocusWindowInDirection(direction, window.Object), Times.Never);
		newInnerLayoutEngine.Verify(ile => ile.GetFirstWindow(), Times.Once);

		window.Verify(w => w.Focus(), Times.Once);
	}
	#endregion

	#region SwapWindowInDirection
	[Fact]
	public void SwapWindowInDirection_UseInner()
	{
		// Given
		Mock<IWindow> window = new();
		Direction direction = Direction.Left;

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		engine.SwapWindowInDirection(direction, window.Object);

		// Then
		wrapper.InnerLayoutEngine.Verify(ile => ile.SwapWindowInDirection(direction, window.Object), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirection_UseInner_SameInner()
	{
		// Given
		Mock<IWindow> window = new();
		Direction direction = Direction.Left;

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.InnerLayoutEngine
			.Setup(ile => ile.SwapWindowInDirection(direction, window.Object))
			.Returns(wrapper.InnerLayoutEngine.Object);

		// When
		engine.SwapWindowInDirection(direction, window.Object);

		// Then
		wrapper.InnerLayoutEngine.Verify(ile => ile.SwapWindowInDirection(direction, window.Object), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirection_FloatingWindow()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		Direction direction = Direction.Left;

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		engine.AddWindow(window.Object).SwapWindowInDirection(direction, window.Object);

		// Then
		wrapper.InnerLayoutEngine.Verify(ile => ile.SwapWindowInDirection(direction, window.Object), Times.Never);
	}
	#endregion

	#region ContainsWindow
	[Fact]
	public void ContainsWindow_UseInner()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		bool containsWindow = engine.ContainsWindow(window.Object);

		// Then
		Assert.False(containsWindow);
		wrapper.InnerLayoutEngine.Verify(ile => ile.ContainsWindow(window.Object), Times.Once);
	}

	[Fact]
	public void ContainsWindow_UseInner_SameInner()
	{
		// Given
		Mock<IWindow> window = new();

		Wrapper wrapper = new();
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		wrapper.InnerLayoutEngine.Setup(ile => ile.ContainsWindow(window.Object)).Returns(true);

		// When
		bool containsWindow = engine.ContainsWindow(window.Object);

		// Then
		Assert.True(containsWindow);
		wrapper.InnerLayoutEngine.Verify(ile => ile.ContainsWindow(window.Object), Times.Once);
	}

	[Fact]
	public void ContainsWindow_FloatingWindow()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper()
			.MarkAsFloating(window.Object)
			.Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When
		bool containsWindow = engine.AddWindow(window.Object).ContainsWindow(window.Object);

		// Then
		Assert.True(containsWindow);
		wrapper.InnerLayoutEngine.Verify(ile => ile.ContainsWindow(window.Object), Times.Never);
	}
	#endregion

	#region WindowWasFloating_ShouldBeGarbageCollectedByUpdateInner
	[Fact]
	public void WindowWasFloating_AddWindow()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper().Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When the window is floating...
		wrapper.MarkAsFloating(window.Object);
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// ...marked as docked...
		wrapper.Plugin.Setup(p => p.FloatingWindows).Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

		// ... and then added
		ILayoutEngine newEngine2 = newEngine.AddWindow(window.Object);

		// Then AddWindow should be called on the inner layout engine
		newInnerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
	}

	[Fact]
	public void WindowWasFloating_MoveWindowToPoint()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper().Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When the window is floating...
		wrapper.MarkAsFloating(window.Object);
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// ...marked as docked...
		wrapper.Plugin.Setup(p => p.FloatingWindows).Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

		// ... and then moved
		ILayoutEngine newEngine2 = newEngine.MoveWindowToPoint(window.Object, new Location<double>());

		// Then MoveWindowToPoint should be called on the inner layout engine
		newInnerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, new Location<double>()), Times.Once);
	}

	[Fact]
	public void WindowWasFloating_RemoveWindow()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper().Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When the window is floating...
		wrapper.MarkAsFloating(window.Object);
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// ...marked as docked...
		wrapper.Plugin.Setup(p => p.FloatingWindows).Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

		// ... and then removed
		ILayoutEngine newEngine2 = newEngine.RemoveWindow(window.Object);

		// Then RemoveWindow should be called on the inner layout engine
		newInnerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Once);
	}

	[Fact]
	public void WindowWasFloating_MoveWindowEdgesInDirection()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();
		Point<double> deltas = new();

		Wrapper wrapper = new Wrapper().Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When the window is floating...
		wrapper.MarkAsFloating(window.Object);
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// ...marked as docked...
		wrapper.Plugin.Setup(p => p.FloatingWindows).Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

		// ... and then the edges are moved
		ILayoutEngine newEngine2 = newEngine.MoveWindowEdgesInDirection(Direction.Left, deltas, window.Object);

		// Then MoveWindowEdgesInDirection should be called on the inner layout engine
		newInnerLayoutEngine.Verify(
			ile => ile.MoveWindowEdgesInDirection(Direction.Left, deltas, window.Object),
			Times.Once
		);
	}

	[Fact]
	public void WindowWasFloating_SwapWindowInDirection()
	{
		// Given
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> newInnerLayoutEngine = new();

		Wrapper wrapper = new Wrapper().Setup_RemoveWindow(window.Object, newInnerLayoutEngine);
		FloatingLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.InnerLayoutEngine.Object);

		// When the window is floating...
		wrapper.MarkAsFloating(window.Object);
		ILayoutEngine newEngine = engine.AddWindow(window.Object);

		// ...marked as docked...
		wrapper.Plugin.Setup(p => p.FloatingWindows).Returns(new Dictionary<IWindow, ISet<LayoutEngineIdentity>>());

		// ... and then window is swapped in a direction
		ILayoutEngine newEngine2 = newEngine.SwapWindowInDirection(Direction.Left, window.Object);

		// Then SwapWindowInDirection should be called on the inner layout engine
		newInnerLayoutEngine.Verify(ile => ile.SwapWindowInDirection(Direction.Left, window.Object), Times.Once);
	}
	#endregion
}
