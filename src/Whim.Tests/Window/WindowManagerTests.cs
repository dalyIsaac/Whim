using System;
using System.Collections;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WindowManagerTests
{
	private static void DispatchEvent(MutableRootSector mutableRootSector, EventArgs ev)
	{
		mutableRootSector.Windows.QueueEvent(ev);
		mutableRootSector.Windows.DispatchEvents();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSlice_WindowAdded(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given
		WindowManager sut = new(ctx, internalCtx);

		// When the WindowSlice receives a WindowAddedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowAddedEventArgs>(
			h => sut.WindowAdded += h,
			h => sut.WindowAdded -= h,
			() => DispatchEvent(mutableRootSector, new WindowAddedEventArgs() { Window = window })
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSlice_WindowFocused(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given
		WindowManager sut = new(ctx, internalCtx);

		// When the WindowSlice receives a WindowFocusedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowFocusedEventArgs>(
			h => sut.WindowFocused += h,
			h => sut.WindowFocused -= h,
			() => DispatchEvent(mutableRootSector, new WindowFocusedEventArgs() { Window = window })
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSlice_WindowRemoved(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given
		WindowManager sut = new(ctx, internalCtx);

		// When the WindowSlice receives a WindowRemovedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowRemovedEventArgs>(
			h => sut.WindowRemoved += h,
			h => sut.WindowRemoved -= h,
			() => DispatchEvent(mutableRootSector, new WindowRemovedEventArgs() { Window = window })
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSlice_WindowMoveStarted(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given
		WindowManager sut = new(ctx, internalCtx);

		// When the WindowSlice receives a WindowMoveStartedTransform
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
						CursorDraggedPoint = new Point<int>()
					}
				)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSlice_WindowMoved(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given
		WindowManager sut = new(ctx, internalCtx);

		// When the WindowSlice receives a WindowMovedTransform
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
						CursorDraggedPoint = new Point<int>()
					}
				)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSlice_WindowMoveEnd(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given
		WindowManager sut = new(ctx, internalCtx);

		// When the WindowSlice receives a WindowMoveEndTransform
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
						CursorDraggedPoint = new Point<int>()
					}
				)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSlice_WindowMinimizeStarted(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given
		WindowManager sut = new(ctx, internalCtx);

		// When the WindowSlice receives a WindowMinimizeStartedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowMinimizeStartedEventArgs>(
			h => sut.WindowMinimizeStart += h,
			h => sut.WindowMinimizeStart -= h,
			() => DispatchEvent(mutableRootSector, new WindowMinimizeStartedEventArgs() { Window = window })
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowSlice_WindowMinimizeEnded(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given
		WindowManager sut = new(ctx, internalCtx);

		// When the WindowSlice receives a WindowMinimizeEndedTransform
		sut.Initialize();

		// Then the WindowManager receives an event
		Assert.Raises<WindowMinimizeEndedEventArgs>(
			h => sut.WindowMinimizeEnd += h,
			h => sut.WindowMinimizeEnd -= h,
			() => DispatchEvent(mutableRootSector, new WindowMinimizeEndedEventArgs() { Window = window })
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void CreateWindow_Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given window creation succeeds
		HWND hwnd = (HWND)1;
		WindowManager sut = new(ctx, internalCtx);

		// When we try create a window
		var result = sut.CreateWindow(hwnd);

		// Then it succeeds
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void CreateWindow_RetrieveExisting(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector,
		IWindow window
	)
	{
		// Given the window already exists
		HWND hwnd = (HWND)1;
		window.Handle.Returns(hwnd);
		mutableRootSector.Windows.Windows = mutableRootSector.Windows.Windows.Add(hwnd, window);

		WindowManager sut = new(ctx, internalCtx);

		// When we try create a window
		var result = sut.CreateWindow(hwnd);

		// Then it succeeds
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData]
	internal void GetEnumerator(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WindowManager sut = new(ctx, internalCtx);

		// When
		sut.GetEnumerator();
		((IEnumerable)sut).GetEnumerator();

		// Then
		ctx.Store.Received(2).Pick(new GetAllWindowsPicker());
	}
}
