using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WindowUtilsTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	public void Map_DoesNotContainWorkspaceForWindow(IContext ctx, IWindow window)
	{
		// Given the map sector doesn't contain the window

		// When we get the moved edges
		var result = WindowUtils.GetMovedEdges(ctx, window);

		// Then we get a null response
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Workspace_DoesNotContainWindow(IContext ctx, IWindow window)
	{
		// Given the workspace doesn't contain the window

		// When we get the moved edges
		var result = WindowUtils.GetMovedEdges(ctx, window);

		// Then we get a null response
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NativeManager_DoesNotHaveWindowRectangle(IContext ctx, MutableRootSector rootSector, IWindow window)
	{
		// Given we can't get the window position from the native manager
		window.Handle.Returns((HWND)12);
		Workspace workspace = CreateWorkspace(ctx) with
		{
			WindowPositions = ImmutableDictionary<HWND, WindowPosition>.Empty.Add(window.Handle, new())
		};
		PopulateWindowWorkspaceMap(ctx, rootSector, window, workspace);

		ctx.NativeManager.DwmGetWindowRectangle(window.Handle).ReturnsNull();

		// When we get the moved edges
		var result = WindowUtils.GetMovedEdges(ctx, window);

		// Then we get a null response
		Assert.Null(result);
	}

	[Theory]
	[InlineAutoSubstituteData<StoreCustomization>(1, 0, 1, 0)]
	[InlineAutoSubstituteData<StoreCustomization>(0, 1, 1, 0)]
	[InlineAutoSubstituteData<StoreCustomization>(1, 1, 1, 1)]
	internal void MoveTooManyEdges(
		int newX,
		int newY,
		int newWidth,
		int newHeight,
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window
	)
	{
		// Given the new window position
		window.Handle.Returns((HWND)12);
		Workspace workspace = CreateWorkspace(ctx) with
		{
			WindowPositions = ImmutableDictionary<HWND, WindowPosition>.Empty.Add(window.Handle, new())
		};

		PopulateWindowWorkspaceMap(ctx, rootSector, window, workspace);

		ctx.NativeManager.DwmGetWindowRectangle(window.Handle)
			.Returns(new Rectangle<int>(newX, newY, newWidth, newHeight));

		// When we get the moved edges
		var result = WindowUtils.GetMovedEdges(ctx, window);

		// Then we get a null response
		Assert.Null(result);
	}

	public static TheoryData<IRectangle<int>, IRectangle<int>, Direction, IPoint<int>> MoveEdgesSuccessData() =>
		new()
		{
			// Move left edge to the left
			{
				new Rectangle<int>() { X = 4, Width = 4 },
				new Rectangle<int>() { X = 3, Width = 5 },
				Direction.Left,
				new Point<int>() { X = -1, Y = 0 }
			},
			// Move left edge to the right
			{
				new Rectangle<int>() { X = 4, Width = 4 },
				new Rectangle<int>() { X = 5, Width = 3 },
				Direction.Left,
				new Point<int>() { X = 1, Y = 0 }
			},
			// Move right edge to the right
			{
				new Rectangle<int>() { X = 4, Width = 4 },
				new Rectangle<int>() { X = 4, Width = 5 },
				Direction.Right,
				new Point<int>() { X = 1, Y = 0 }
			},
			// Move right edge to the left
			{
				new Rectangle<int>() { X = 4, Width = 4 },
				new Rectangle<int>() { X = 4, Width = 3 },
				Direction.Right,
				new Point<int>() { X = -1, Y = 0 }
			},
			// Move top edge up
			{
				new Rectangle<int>() { Y = 4, Height = 4 },
				new Rectangle<int>() { Y = 3, Height = 5 },
				Direction.Up,
				new Point<int>() { X = 0, Y = -1 }
			},
			// Move top edge down
			{
				new Rectangle<int>() { Y = 4, Height = 4 },
				new Rectangle<int>() { Y = 5, Height = 3 },
				Direction.Up,
				new Point<int>() { X = 0, Y = 1 }
			},
			// Move bottom edge down
			{
				new Rectangle<int>() { Y = 4, Height = 4 },
				new Rectangle<int>() { Y = 4, Height = 5 },
				Direction.Down,
				new Point<int>() { X = 0, Y = 1 }
			},
			// Move bottom edge up
			{
				new Rectangle<int>() { Y = 4, Height = 4 },
				new Rectangle<int>() { Y = 4, Height = 3 },
				Direction.Down,
				new Point<int>() { X = 0, Y = -1 }
			}
		};

	[Theory]
	[MemberAutoSubstituteData<StoreCustomization>(nameof(MoveEdgesSuccessData))]
	internal void Move(
		IRectangle<int> originalRect,
		IRectangle<int> newRect,
		Direction expectedDirection,
		IPoint<int> expectedMovedPoint,
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window
	)
	{
		// Given the new window position
		window.Handle.Returns((HWND)12);
		Workspace workspace = CreateWorkspace(ctx) with
		{
			WindowPositions = ImmutableDictionary<HWND, WindowPosition>.Empty.Add(
				window.Handle,
				new() { LastWindowRectangle = originalRect, WindowSize = WindowSize.Normal }
			)
		};

		PopulateWindowWorkspaceMap(ctx, rootSector, window, workspace);
		ctx.NativeManager.DwmGetWindowRectangle(window.Handle).Returns(newRect);

		// When we get the moved edges
		var result = WindowUtils.GetMovedEdges(ctx, window);

		// Then we get the moved edges and point
		Assert.Equal(expectedDirection, result!.Value.MovedEdges);
		Assert.Equal(expectedMovedPoint, result!.Value.MovedPoint);
	}
}
