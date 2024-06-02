using System.Collections.Generic;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class WindowUtilsTests
{
	private static void Setup_TryGetWindowState(IWindow window, Workspace workspace, IRectangle<int>? rect = null)
	{
		workspace
			.TryGetWindowState(window)
			.Returns(
				new WindowState()
				{
					Rectangle = rect ?? new Rectangle<int>(),
					Window = window,
					WindowSize = WindowSize.Normal
				}
			);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	public void Butler_DoesNotContainWorkspaceForWindow(IContext ctx, IWindow window)
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
	internal void NativeManager_DoesNotHaveWindowRectangle(
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window,
		Workspace workspace
	)
	{
		// Given we can't get the window position from the native manager
		StoreTestUtils.SetupWindowWorkspaceMapping(ctx, rootSector, window, workspace);
		Setup_TryGetWindowState(window, workspace);
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
		IWindow window,
		Workspace workspace
	)
	{
		// Given the new window position
		window.Handle.Returns((HWND)1);
		StoreTestUtils.SetupWindowWorkspaceMapping(ctx, rootSector, window, workspace);
		Setup_TryGetWindowState(window, workspace);
		ctx.NativeManager.DwmGetWindowRectangle(window.Handle)
			.Returns(new Rectangle<int>(newX, newY, newWidth, newHeight));

		// When we get the moved edges
		var result = WindowUtils.GetMovedEdges(ctx, window);

		// Then we get a null response
		Assert.Null(result);
	}

	public static IEnumerable<object[]> MoveEdgesSuccessData()
	{
		// Move left edge to the left
		yield return new object[]
		{
			new Rectangle<int>() { X = 4, Width = 4 },
			new Rectangle<int>() { X = 3, Width = 5 },
			Direction.Left,
			new Point<int>() { X = -1, Y = 0 }
		};

		// Move left edge to the right
		yield return new object[]
		{
			new Rectangle<int>() { X = 4, Width = 4 },
			new Rectangle<int>() { X = 5, Width = 3 },
			Direction.Left,
			new Point<int>() { X = 1, Y = 0 }
		};

		// Move right edge to the right
		yield return new object[]
		{
			new Rectangle<int>() { X = 4, Width = 4 },
			new Rectangle<int>() { X = 4, Width = 5 },
			Direction.Right,
			new Point<int>() { X = 1, Y = 0 }
		};

		// Move right edge to the left
		yield return new object[]
		{
			new Rectangle<int>() { X = 4, Width = 4 },
			new Rectangle<int>() { X = 4, Width = 3 },
			Direction.Right,
			new Point<int>() { X = -1, Y = 0 }
		};

		// Move top edge up
		yield return new object[]
		{
			new Rectangle<int>() { Y = 4, Height = 4 },
			new Rectangle<int>() { Y = 3, Height = 5 },
			Direction.Up,
			new Point<int>() { X = 0, Y = -1 }
		};

		// Move top edge down
		yield return new object[]
		{
			new Rectangle<int>() { Y = 4, Height = 4 },
			new Rectangle<int>() { Y = 5, Height = 3 },
			Direction.Up,
			new Point<int>() { X = 0, Y = 1 }
		};

		// Move bottom edge down
		yield return new object[]
		{
			new Rectangle<int>() { Y = 4, Height = 4 },
			new Rectangle<int>() { Y = 4, Height = 5 },
			Direction.Down,
			new Point<int>() { X = 0, Y = 1 }
		};

		// Move bottom edge up
		yield return new object[]
		{
			new Rectangle<int>() { Y = 4, Height = 4 },
			new Rectangle<int>() { Y = 4, Height = 3 },
			Direction.Down,
			new Point<int>() { X = 0, Y = -1 }
		};
	}

	[Theory]
	[MemberAutoSubstituteData<StoreCustomization>(nameof(MoveEdgesSuccessData))]
	internal void Move(
		IRectangle<int> originalRect,
		IRectangle<int> newRect,
		Direction expectedDirection,
		IPoint<int> expectedMovedPoint,
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window,
		Workspace workspace
	)
	{
		// Given the new window position
		StoreTestUtils.SetupWindowWorkspaceMapping(ctx, rootSector, window, workspace);
		Setup_TryGetWindowState(window, workspace, originalRect);
		ctx.NativeManager.DwmGetWindowRectangle(window.Handle).Returns(newRect);

		// When we get the moved edges
		var result = WindowUtils.GetMovedEdges(ctx, window);

		// Then we get the moved edges and point
		Assert.Equal(expectedDirection, result!.Value.MovedEdges);
		Assert.Equal(expectedMovedPoint, result!.Value.MovedPoint);
	}
}
