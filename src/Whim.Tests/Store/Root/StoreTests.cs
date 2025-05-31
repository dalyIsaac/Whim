namespace Whim.Tests;

public class StoreTests
{
	private record EmptyTransform : Transform
	{
		internal override Result<Unit> Execute(
			IContext ctx,
			IInternalContext internalCtx,
			MutableRootSector rootSector
		) => Unit.Result;
	}

	private record PopulatedTransform : Transform
	{
		internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
		{
			rootSector.WorkspaceSector.QueueEvent(
				new WorkspaceAddedEventArgs() { Workspace = Substitute.For<IWorkspace>() }
			);
			return Unit.Result;
		}
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Dispatch_IsStaThread_NoQueuedEvents(IContext ctx)
	{
		// Given no queued events

		// When we dispatch
		// Then no events are raised
		CustomAssert.DoesNotRaise<WorkspaceAddedEventArgs>(
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded += h,
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded -= h,
			() => ctx.Store.Dispatch(new EmptyTransform())
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Dispatch_IsStaThread_QueuedEvents(IContext ctx)
	{
		// Given the transform queues an event
		// When we dispatch
		// Then the event is raised
		Assert.Raises<WorkspaceAddedEventArgs>(
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded += h,
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded -= h,
			() => ctx.Store.Dispatch(new PopulatedTransform())
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Dispatch_IsNotStaThread_NoQueuedEvents(IContext ctx, IInternalContext internalCtx)
	{
		// Given no queued events
		internalCtx.CoreNativeManager.IsStaThread().Returns(false);

		// When we dispatch
		// Then no events are raised
		CustomAssert.DoesNotRaise<WorkspaceAddedEventArgs>(
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded += h,
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded -= h,
			() => ctx.Store.Dispatch(new EmptyTransform())
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Dispatch_IsNotStaThread_QueuedEvents(IContext ctx, IInternalContext internalCtx)
	{
		// Given the transform queues an event
		internalCtx.CoreNativeManager.IsStaThread().Returns(false);

		// When we dispatch
		// Then the event is raised
		CustomAssert.DoesNotRaise<WorkspaceAddedEventArgs>(
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded += h,
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded -= h,
			() => ctx.Store.Dispatch(new PopulatedTransform())
		);
	}
}
