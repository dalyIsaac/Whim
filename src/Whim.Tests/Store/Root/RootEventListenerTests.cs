using System;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class RootEventListenerTests
{
	[Theory, AutoSubstituteData]
	internal void Initialize(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		RootEventListener sut = new(ctx, internalCtx);

		// When
		sut.Initialize();

		// Then
		internalCtx.MouseHook.MouseLeftButtonUp += Arg.Any<EventHandler<MouseEventArgs>>();
		internalCtx.MouseHook.MouseLeftButtonDown += Arg.Any<EventHandler<MouseEventArgs>>();
	}

	[Theory, AutoSubstituteData]
	internal void MouseLeftButtonUp(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		RootEventListener sut = new(ctx, internalCtx);

		// When
		sut.Initialize();
		internalCtx.MouseHook.MouseLeftButtonUp += Raise.Event<EventHandler<MouseEventArgs>>(
			ctx.Store.MonitorEvents,
			new MouseEventArgs(new Point<int>(1, 2))
		);

		// Then
		ctx.Store.Received(1).Dispatch(new MouseLeftButtonUpTransform(new Point<int>(1, 2)));
	}

	[Theory, AutoSubstituteData]
	internal void MouseLeftButtonDown(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		RootEventListener sut = new(ctx, internalCtx);

		// When
		sut.Initialize();
		internalCtx.MouseHook.MouseLeftButtonDown += Raise.Event<EventHandler<MouseEventArgs>>(
			ctx.Store.MonitorEvents,
			new MouseEventArgs(new Point<int>(1, 2))
		);

		// Then
		ctx.Store.Received(1).Dispatch(new MouseLeftButtonDownTransform());
	}

	[Theory, AutoSubstituteData]
	internal void Dispose(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		RootEventListener sut = new(ctx, internalCtx);

		// When
		sut.Initialize();
		sut.Dispose();

		// Then
		internalCtx.MouseHook.MouseLeftButtonUp -= Arg.Any<EventHandler<MouseEventArgs>>();
		internalCtx.MouseHook.MouseLeftButtonDown -= Arg.Any<EventHandler<MouseEventArgs>>();
	}
}
