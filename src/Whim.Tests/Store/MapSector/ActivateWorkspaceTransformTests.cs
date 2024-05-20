using System;
using System.Collections.Generic;
using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class ActivateWorkspaceTransformTests
{
	private static Result<Unit> AssertDoesNotRaise(
		IContext ctx,
		MutableRootSector rootSector,
		ActivateWorkspaceTransform sut
	)
	{
		Result<Unit>? result = null;
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => rootSector.MapSector.MonitorWorkspaceChanged += h,
			h => rootSector.MapSector.MonitorWorkspaceChanged -= h,
			() => result = ctx.Store.Dispatch(sut)
		);
		return result!.Value;
	}

	private static IWorkspace SetupWorkspace(IContext ctx, MutableRootSector rootSector)
	{
		IWorkspace workspace = Substitute.For<IWorkspace>();
		ctx.WorkspaceManager.GetEnumerator().Returns(_ => new List<IWorkspace>() { workspace }.GetEnumerator());
		return workspace;
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceNotFound(IContext ctx, MutableRootSector rootSector)
	{
		// Given the workspace doesn't exist
		ActivateWorkspaceTransform sut = new(Guid.NewGuid());

		// When we activate the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void MonitorNotFound(IContext ctx, MutableRootSector rootSector)
	{
		// Given the monitor doesn't exist
		IWorkspace workspace = SetupWorkspace(ctx, rootSector);
		ActivateWorkspaceTransform sut = new(workspace.Id);

		// When we activate the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}
}
