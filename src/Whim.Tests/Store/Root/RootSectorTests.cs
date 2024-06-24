using System;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class RootSectorTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Initialize_Dispose(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		RootSector sut = new(ctx, internalCtx);
		CaptureWinEventProc.Create(internalCtx);

		// When
		sut.Initialize();
		sut.Dispose();

		// Then
		internalCtx.MouseHook.MouseLeftButtonUp += Arg.Any<EventHandler<MouseEventArgs>>();
		internalCtx.MouseHook.MouseLeftButtonDown += Arg.Any<EventHandler<MouseEventArgs>>();
		internalCtx.MouseHook.MouseLeftButtonUp -= Arg.Any<EventHandler<MouseEventArgs>>();
		internalCtx.MouseHook.MouseLeftButtonDown -= Arg.Any<EventHandler<MouseEventArgs>>();
	}
}
