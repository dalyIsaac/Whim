using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.LayoutPreview.Tests;

public class LayoutPreviewWindowCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		fixture.Freeze<IContext>();
		fixture.Freeze<IInternalContext>();
	}
}

public class LayoutPreviewWindowTests
{
	#region ShouldContinue
	[Fact]
	public void ShouldContinue_DifferentLength()
	{
		// Given
		IWindowState[] prevWindowStates = [];
		int prevHoveredIndex = -1;
		IWindowState[] windowStates = new IWindowState[1];
		IPoint<int> cursorPoint = new Rectangle<int>();

		// When
		bool shouldContinue = LayoutPreviewWindow.ShouldContinue(
			prevWindowStates,
			prevHoveredIndex,
			windowStates,
			cursorPoint
		);

		// Then
		Assert.True(shouldContinue);
	}

	[Fact]
	public void ShouldContinue_DifferentWindowState()
	{
		// Given
		IWindowState[] prevWindowStates = new IWindowState[]
		{
			new WindowState()
			{
				Window = Substitute.For<IWindow>(),
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = Substitute.For<IWindow>(),
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal
			},
		};
		int prevHoveredIndex = -1;
		IWindowState[] windowStates = new IWindowState[]
		{
			prevWindowStates[0],
			new WindowState()
			{
				Window = Substitute.For<IWindow>(),
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Maximized
			},
		};
		IPoint<int> cursorPoint = new Rectangle<int>();

		// When
		bool shouldContinue = LayoutPreviewWindow.ShouldContinue(
			prevWindowStates,
			prevHoveredIndex,
			windowStates,
			cursorPoint
		);

		// Then
		Assert.True(shouldContinue);
	}

	[Fact]
	public void ShouldContinue_HoveredIndexChanged()
	{
		// Given
		Rectangle<int> rect = new() { Height = 100, Width = 100 };
		IWindowState[] prevWindowStates = new IWindowState[]
		{
			new WindowState()
			{
				Window = Substitute.For<IWindow>(),
				Rectangle = rect,
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = Substitute.For<IWindow>(),
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal
			},
		};
		int prevHoveredIndex = 0;
		IWindowState[] windowStates = new IWindowState[]
		{
			new WindowState()
			{
				Window = Substitute.For<IWindow>(),
				Rectangle = rect,
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = Substitute.For<IWindow>(),
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal
			},
		};
		IPoint<int> cursorPoint = new Rectangle<int>() { X = 100, Y = 101 };

		// When
		bool shouldContinue = LayoutPreviewWindow.ShouldContinue(
			prevWindowStates,
			prevHoveredIndex,
			windowStates,
			cursorPoint
		);

		// Then
		Assert.True(shouldContinue);
	}

	[Fact]
	public void ShouldContinue_HoveredIndexNotChanged()
	{
		// Given
		Rectangle<int> rect = new() { Height = 100, Width = 100 };
		IWindowState[] prevWindowStates = new IWindowState[]
		{
			new WindowState()
			{
				Window = Substitute.For<IWindow>(),
				Rectangle = rect,
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = Substitute.For<IWindow>(),
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal
			},
		};
		int prevHoveredIndex = 0;
		IPoint<int> cursorPoint = new Rectangle<int>() { X = 50, Y = 50 };

		// When
		bool shouldContinue = LayoutPreviewWindow.ShouldContinue(
			prevWindowStates,
			prevHoveredIndex,
			prevWindowStates,
			cursorPoint
		);

		// Then
		Assert.False(shouldContinue);
	}
	#endregion

	[Theory, AutoSubstituteData<LayoutPreviewWindowCustomization>]
	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	internal void Activate(
		IContext ctx,
		IInternalContext internalCtx,
		IWindow layoutWindow,
		IWindow movingWindow,
		IMonitor monitor
	)
	{
		// Given
		ctx.NativeManager.DeferWindowPos().Returns(new DeferWindowPosHandle(ctx, internalCtx));

		// When
		LayoutPreviewWindow.Activate(ctx, layoutWindow, movingWindow, monitor);

		// Then
		ctx.NativeManager.Received(1).DeferWindowPos();
	}
}
