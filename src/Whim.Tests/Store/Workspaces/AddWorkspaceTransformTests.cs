using System;
using System.Collections.Generic;
using DotNext;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class AddWorkspaceTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoEngineCreators(IContext ctx, MutableRootSector mutableRootSector)
	{
		// Given no engine creators were provided
		mutableRootSector.Workspaces.CreateLayoutEngines = Array.Empty<CreateLeafLayoutEngine>;
		AddWorkspaceTransform sut = new();

		// When the transform is dispatched
		Result<ImmutableWorkspace>? result = null;
		CustomAssert.DoesNotRaise<WorkspaceAddedEventArgs>(
			h => mutableRootSector.Workspaces.WorkspaceAdded += h,
			h => mutableRootSector.Workspaces.WorkspaceAdded -= h,
			() =>
			{
				result = ctx.Store.Dispatch(sut);
			}
		);

		// Then nothing happens
		Assert.False(result!.Value.IsSuccessful);
	}

	public static IEnumerable<object[]> Success_Data()
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		yield return new object[]
		{
			null,
			new CreateLeafLayoutEngine[] { (id) => new TestLayoutEngine() },
			null,
			"Workspace 1"
		};

		yield return new object[]
		{
			"Bob",
			null,
			new Func<CreateLeafLayoutEngine[]>(() => new CreateLeafLayoutEngine[] { (id) => new TestLayoutEngine() }),
			"Bob"
		};
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

	[MemberAutoSubstituteData<StoreCustomization>(nameof(Success_Data))]
	[Theory]
	internal void Success(
		string? name,
		IEnumerable<CreateLeafLayoutEngine>? transformCreators,
		Func<CreateLeafLayoutEngine[]>? sliceCreators,
		string expectedName,
		IContext ctx,
		MutableRootSector mutableRootSector
	)
	{
		// Given
		if (sliceCreators is not null)
		{
			mutableRootSector.Workspaces.CreateLayoutEngines = sliceCreators;
		}

		AddWorkspaceTransform sut = new(name, transformCreators);

		// When the transform is dispatched
		Result<ImmutableWorkspace>? result = null;
		Assert.Raises<WorkspaceAddedEventArgs>(
			h => mutableRootSector.Workspaces.WorkspaceAdded += h,
			h => mutableRootSector.Workspaces.WorkspaceAdded -= h,
			() =>
			{
				result = ctx.Store.Dispatch(sut);
			}
		);

		// Then the worksapce is created
		Assert.True(result!.Value.IsSuccessful);
		Assert.Equal(expectedName, result!.Value.Value.Name);
		Assert.Single(mutableRootSector.Workspaces.Workspaces);
	}
}
