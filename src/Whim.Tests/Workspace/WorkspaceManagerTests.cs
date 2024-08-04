using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WorkspaceManagerTests
{
	[Theory, AutoSubstituteData]
	internal void CreateLayoutEngines_Get(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		var _ = sut.CreateLayoutEngines;

		// Then
		ctx.Store.Received(1).Pick(Pickers.PickCreateLeafLayoutEngines());
	}

	[Theory, AutoSubstituteData]
	internal void CreateLayoutEngines_Set(IContext ctx)
	{
		// Given
#pragma warning disable IDE0017 // Simplify object initialization
		WorkspaceManager sut = new(ctx);

		// When
		sut.CreateLayoutEngines = Array.Empty<CreateLeafLayoutEngine>;
#pragma warning restore IDE0017 // Simplify object initialization

		// Then
		ctx.Store.Received(1).Dispatch(Arg.Any<SetCreateLayoutEnginesTransform>());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void IndexLookup(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		IWorkspace? w = sut["test"];

		// Then
		Assert.Null(w);
	}

	[Theory, AutoSubstituteData]
	internal void ActiveWorkspace(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		var _ = sut.ActiveWorkspace;

		// Then
		ctx.Store.Received(1).Pick(Pickers.PickActiveWorkspace());
	}

	[Theory, AutoSubstituteData]
	internal void Add_UseDefaults(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		var _ = sut.Add();

		// Then
		ctx.Store.Received(1).Dispatch(Arg.Any<AddWorkspaceTransform>());
	}

	[Theory, AutoSubstituteData]
	internal void Add_SpecifyParams(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		var _ = sut.Add("test", []);

		// Then
		ctx.Store.Received(1).Dispatch(Arg.Any<AddWorkspaceTransform>());
	}

	[Theory, AutoSubstituteData]
	internal void AddProxyLayoutEngine(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		sut.AddProxyLayoutEngine(Substitute.For<ProxyLayoutEngineCreator>());

		// Then
		ctx.Store.Received(1).Dispatch(Arg.Any<AddProxyLayoutEngineTransform>());
	}

	[Theory, AutoSubstituteData]
	internal void Contains(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		var _ = sut.Contains(Substitute.For<IWorkspace>());

		// Then
		ctx.Store.Received(1).Pick(Pickers.PickAllWorkspaces());
	}

	[Theory, AutoSubstituteData]
	internal void GetEnumerator(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		sut.GetEnumerator();
		((IEnumerable)sut).GetEnumerator();

		// Then
		ctx.Store.Received(2).Pick(Pickers.PickAllWorkspaces());
	}

	[Theory, AutoSubstituteData]
	internal void Initialize_Dispose(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		sut.Initialize();
		sut.Dispose();

		// Then
#pragma warning disable NS5000 // Received check.
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceAdded += Arg.Any<EventHandler<WorkspaceAddedEventArgs>>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceRemoved += Arg.Any<EventHandler<WorkspaceRemovedEventArgs>>();
		ctx.Store.WorkspaceEvents.Received(1).ActiveLayoutEngineChanged += Arg.Any<
			EventHandler<ActiveLayoutEngineChangedEventArgs>
		>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceRenamed += Arg.Any<EventHandler<WorkspaceRenamedEventArgs>>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceLayoutStarted += Arg.Any<
			EventHandler<WorkspaceLayoutStartedEventArgs>
		>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceLayoutCompleted += Arg.Any<
			EventHandler<WorkspaceLayoutCompletedEventArgs>
		>();

		ctx.Store.WorkspaceEvents.Received(1).WorkspaceAdded -= Arg.Any<EventHandler<WorkspaceAddedEventArgs>>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceRemoved -= Arg.Any<EventHandler<WorkspaceRemovedEventArgs>>();
		ctx.Store.WorkspaceEvents.Received(1).ActiveLayoutEngineChanged -= Arg.Any<
			EventHandler<ActiveLayoutEngineChangedEventArgs>
		>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceRenamed -= Arg.Any<EventHandler<WorkspaceRenamedEventArgs>>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceLayoutStarted -= Arg.Any<
			EventHandler<WorkspaceLayoutStartedEventArgs>
		>();
		ctx.Store.WorkspaceEvents.Received(1).WorkspaceLayoutCompleted -= Arg.Any<
			EventHandler<WorkspaceLayoutCompletedEventArgs>
		>();
#pragma warning restore NS5000 // Received check.
	}

	[Theory, AutoSubstituteData]
	internal void WorkspaceAdded(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		sut.Initialize();
		Assert.Raises<WorkspaceAddedEventArgs>(
			h => sut.WorkspaceAdded += h,
			h => sut.WorkspaceAdded -= h,
			() =>
				ctx.Store.WorkspaceEvents.WorkspaceAdded += Raise.Event<EventHandler<WorkspaceAddedEventArgs>>(
					ctx.Store,
					new WorkspaceAddedEventArgs() { Workspace = Substitute.For<IWorkspace>(), }
				)
		);
	}

	[Theory, AutoSubstituteData]
	internal void WorkspaceRemoved(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		sut.Initialize();
		Assert.Raises<WorkspaceRemovedEventArgs>(
			h => sut.WorkspaceRemoved += h,
			h => sut.WorkspaceRemoved -= h,
			() =>
				ctx.Store.WorkspaceEvents.WorkspaceRemoved += Raise.Event<EventHandler<WorkspaceRemovedEventArgs>>(
					ctx.Store,
					new WorkspaceRemovedEventArgs() { Workspace = Substitute.For<IWorkspace>(), }
				)
		);
	}

	[Theory, AutoSubstituteData]
	internal void ActiveLayoutEngineChanged(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		sut.Initialize();
		Assert.Raises<ActiveLayoutEngineChangedEventArgs>(
			h => sut.ActiveLayoutEngineChanged += h,
			h => sut.ActiveLayoutEngineChanged -= h,
			() =>
				ctx.Store.WorkspaceEvents.ActiveLayoutEngineChanged += Raise.Event<
					EventHandler<ActiveLayoutEngineChangedEventArgs>
				>(
					ctx.Store,
					new ActiveLayoutEngineChangedEventArgs()
					{
						CurrentLayoutEngine = Substitute.For<ILayoutEngine>(),
						PreviousLayoutEngine = Substitute.For<ILayoutEngine>(),
						Workspace = Substitute.For<IWorkspace>(),
					}
				)
		);
	}

	[Theory, AutoSubstituteData]
	internal void WorkspaceRenamed(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		sut.Initialize();
		Assert.Raises<WorkspaceRenamedEventArgs>(
			h => sut.WorkspaceRenamed += h,
			h => sut.WorkspaceRenamed -= h,
			() =>
				ctx.Store.WorkspaceEvents.WorkspaceRenamed += Raise.Event<EventHandler<WorkspaceRenamedEventArgs>>(
					ctx.Store,
					new WorkspaceRenamedEventArgs() { PreviousName = "foo", Workspace = Substitute.For<IWorkspace>() }
				)
		);
	}

	[Theory, AutoSubstituteData]
	internal void WorkspaceLayoutStarted(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		sut.Initialize();
		Assert.Raises<WorkspaceLayoutStartedEventArgs>(
			h => sut.WorkspaceLayoutStarted += h,
			h => sut.WorkspaceLayoutStarted -= h,
			() =>
				ctx.Store.WorkspaceEvents.WorkspaceLayoutStarted += Raise.Event<
					EventHandler<WorkspaceLayoutStartedEventArgs>
				>(ctx.Store, new WorkspaceLayoutStartedEventArgs() { Workspace = Substitute.For<IWorkspace>(), })
		);
	}

	[Theory, AutoSubstituteData]
	internal void WorkspaceLayoutCompleted(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		sut.Initialize();
		Assert.Raises<WorkspaceLayoutCompletedEventArgs>(
			h => sut.WorkspaceLayoutCompleted += h,
			h => sut.WorkspaceLayoutCompleted -= h,
			() =>
				ctx.Store.WorkspaceEvents.WorkspaceLayoutCompleted += Raise.Event<
					EventHandler<WorkspaceLayoutCompletedEventArgs>
				>(ctx.Store, new WorkspaceLayoutCompletedEventArgs() { Workspace = Substitute.For<IWorkspace>(), })
		);
	}

	[Theory, AutoSubstituteData]
	internal void Remove_ByName(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		sut.Remove("foo");

		// Then
		ctx.Store.Received(1).Dispatch(new RemoveWorkspaceByNameTransform("foo"));
	}

	[Theory, AutoSubstituteData]
	internal void Remove_ByWorkspace(IContext ctx, IWorkspace workspace)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		// When
		sut.Remove(workspace);

		// Then
		ctx.Store.Received(1).Dispatch(new RemoveWorkspaceByIdTransform(workspace.Id));
	}

	[Theory, AutoSubstituteData]
	internal void TryGet_Failure(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		ctx.Store.Pick(Arg.Any<PurePicker<Result<IWorkspace>>>())
			.Returns(Result.FromException<IWorkspace>(StoreExceptions.WorkspaceNotFound(Guid.NewGuid())));

		// When
		IWorkspace? result = sut.TryGet("foo");

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData]
	internal void TryGet_Success(IContext ctx)
	{
		// Given
		WorkspaceManager sut = new(ctx);

		ctx.Store.Pick(Arg.Any<PurePicker<Result<IWorkspace>>>())
			.Returns(Result.FromValue(Substitute.For<IWorkspace>()));

		// When
		IWorkspace? result = sut.TryGet("foo");

		// Then
		Assert.NotNull(result);
	}
}
