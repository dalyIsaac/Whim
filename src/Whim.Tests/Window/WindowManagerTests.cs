using System.Collections;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WindowManagerTests
{
	private static void DispatchEvent(MutableRootSector mutableRootSector, EventArgs ev)
	{
		mutableRootSector.WindowSector.QueueEvent(ev);
		mutableRootSector.WindowSector.DispatchEvents();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSector_WindowAdded(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowManager sut = new(ctx);

		// When the WindowSector receives a WindowAddedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowAddedEventArgs>(
			h => sut.WindowAdded += h,
			h => sut.WindowAdded -= h,
			() => DispatchEvent(mutableRootSector, new WindowAddedEventArgs() { Window = window })
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSector_WindowFocused(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowManager sut = new(ctx);

		// When the WindowSector receives a WindowFocusedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowFocusedEventArgs>(
			h => sut.WindowFocused += h,
			h => sut.WindowFocused -= h,
			() => DispatchEvent(mutableRootSector, new WindowFocusedEventArgs() { Window = window })
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSector_WindowRemoved(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowManager sut = new(ctx);

		// When the WindowSector receives a WindowRemovedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowRemovedEventArgs>(
			h => sut.WindowRemoved += h,
			h => sut.WindowRemoved -= h,
			() => DispatchEvent(mutableRootSector, new WindowRemovedEventArgs() { Window = window })
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSector_WindowMoveStarted(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowManager sut = new(ctx);

		// When the WindowSector receives a WindowMoveStartedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowMoveStartedEventArgs>(
			h => sut.WindowMoveStart += h,
			h => sut.WindowMoveStart -= h,
			() =>
				DispatchEvent(
					mutableRootSector,
					new WindowMoveStartedEventArgs()
					{
						Window = window,
						MovedEdges = Direction.None,
						CursorDraggedPoint = new Point<int>(),
					}
				)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSector_WindowMoved(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowManager sut = new(ctx);

		// When the WindowSector receives a WindowMovedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowMovedEventArgs>(
			h => sut.WindowMoved += h,
			h => sut.WindowMoved -= h,
			() =>
				DispatchEvent(
					mutableRootSector,
					new WindowMovedEventArgs()
					{
						Window = window,
						MovedEdges = Direction.None,
						CursorDraggedPoint = new Point<int>(),
					}
				)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSector_WindowMoveEnd(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowManager sut = new(ctx);

		// When the WindowSector receives a WindowMoveEndTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowMoveEndedEventArgs>(
			h => sut.WindowMoveEnd += h,
			h => sut.WindowMoveEnd -= h,
			() =>
				DispatchEvent(
					mutableRootSector,
					new WindowMoveEndedEventArgs()
					{
						Window = window,
						MovedEdges = Direction.None,
						CursorDraggedPoint = new Point<int>(),
					}
				)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSector_WindowMinimizeStarted(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowManager sut = new(ctx);

		// When the WindowSector receives a WindowMinimizeStartedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowMinimizeStartedEventArgs>(
			h => sut.WindowMinimizeStart += h,
			h => sut.WindowMinimizeStart -= h,
			() => DispatchEvent(mutableRootSector, new WindowMinimizeStartedEventArgs() { Window = window })
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSector_WindowMinimizeEnded(IContext ctx, MutableRootSector mutableRootSector, IWindow window)
	{
		// Given
		WindowManager sut = new(ctx);

		// When the WindowSector receives a WindowMinimizeEndedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowMinimizeEndedEventArgs>(
			h => sut.WindowMinimizeEnd += h,
			h => sut.WindowMinimizeEnd -= h,
			() => DispatchEvent(mutableRootSector, new WindowMinimizeEndedEventArgs() { Window = window })
		);
	}

	[Theory, AutoSubstituteData]
	internal void GetEnumerator(IContext ctx)
	{
		// Given
		WindowManager sut = new(ctx);

		// When
		sut.GetEnumerator();
		((IEnumerable)sut).GetEnumerator();

		// Then
		ctx.Store.Received(2).Pick(Pickers.PickAllWindows());
	}

	[Theory, AutoSubstituteData]
	internal void AddWindow(IContext ctx)
	{
		// Given
		HWND hwnd = (HWND)2;
		WindowManager sut = new(ctx);

		// When
		sut.AddWindow(hwnd);

		// Then
		ctx.Store.Received(1).Dispatch(new WindowAddedTransform(hwnd));
	}

	[Theory, AutoSubstituteData]
	internal void OnWindowFocused(IContext ctx, IWindow window)
	{
		// Given
		WindowManager sut = new(ctx);

		// When
		sut.OnWindowFocused(window);

		// Then
		ctx.Store.Received(1).Dispatch(new WindowFocusedTransform(window));
	}

	[Theory, AutoSubstituteData]
	internal void OnWindowRemoved(IContext ctx, IWindow window)
	{
		// Given
		WindowManager sut = new(ctx);

		// When
		sut.OnWindowRemoved(window);

		// Then
		ctx.Store.Received(1).Dispatch(new WindowRemovedTransform(window));
	}

	[Theory, AutoSubstituteData]
	internal void Initialize_Dispose(IContext ctx)
	{
		// Given
		WindowManager sut = new(ctx);

		// When
		sut.Initialize();
		sut.Dispose();

		// Then
#pragma warning disable NS5000 // Received check.
		ctx.Store.WindowEvents.Received(1).WindowAdded += Arg.Any<EventHandler<WindowAddedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowFocused += Arg.Any<EventHandler<WindowFocusedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowRemoved += Arg.Any<EventHandler<WindowRemovedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowMoveStarted += Arg.Any<EventHandler<WindowMoveStartedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowMoved += Arg.Any<EventHandler<WindowMovedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowMoveEnded += Arg.Any<EventHandler<WindowMoveEndedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowMinimizeStarted += Arg.Any<
			EventHandler<WindowMinimizeStartedEventArgs>
		>();
		ctx.Store.WindowEvents.Received(1).WindowMinimizeEnded += Arg.Any<EventHandler<WindowMinimizeEndedEventArgs>>();

		ctx.Store.WindowEvents.Received(1).WindowAdded -= Arg.Any<EventHandler<WindowAddedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowFocused -= Arg.Any<EventHandler<WindowFocusedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowRemoved -= Arg.Any<EventHandler<WindowRemovedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowMoveStarted -= Arg.Any<EventHandler<WindowMoveStartedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowMoved -= Arg.Any<EventHandler<WindowMovedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowMoveEnded -= Arg.Any<EventHandler<WindowMoveEndedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowMinimizeStarted -= Arg.Any<
			EventHandler<WindowMinimizeStartedEventArgs>
		>();
#pragma warning restore NS5000 // Received check.
	}
}
