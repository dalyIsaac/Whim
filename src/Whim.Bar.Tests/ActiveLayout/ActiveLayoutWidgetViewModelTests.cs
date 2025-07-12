using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.Bar.Tests;

public class ActiveLayoutWidgetViewModelTests
{
	private static ActiveLayoutWidgetViewModel CreateSut(IContext ctx, IMonitor monitor) => new(ctx, monitor);

	private static ILayoutEngine CreateLayoutEngine(string name)
	{
		ILayoutEngine layoutEngine = Substitute.For<ILayoutEngine>();
		layoutEngine.Name.Returns(name);
		return layoutEngine;
	}

	[Theory, AutoSubstituteData]
	internal void Constructor_SetsEventListeners(IContext ctx)
	{
		// GIVEN a monitor and view model
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		ActiveLayoutWidgetViewModel sut = CreateSut(ctx, monitor);

		// THEN all required event listeners are registered with the context store
		Assert.NotNull(sut);
		ctx.Store.WorkspaceEvents.Received().ActiveLayoutEngineChanged += Arg.Any<
			EventHandler<ActiveLayoutEngineChangedEventArgs>
		>();
		ctx.Store.MapEvents.Received().MonitorWorkspaceChanged += Arg.Any<
			EventHandler<MonitorWorkspaceChangedEventArgs>
		>();
		ctx.Store.WindowEvents.Received().WindowFocused += Arg.Any<EventHandler<WindowFocusedEventArgs>>();
	}

	[Theory, AutoSubstituteData]
	internal void Dispose_RemovesEventListeners(IContext ctx)
	{
		// GIVEN a monitor and initialized view model
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		ActiveLayoutWidgetViewModel sut = CreateSut(ctx, monitor);

		// WHEN the view model is disposed
		sut.Dispose();

		// THEN all event listeners are properly unregistered from the context store
		ctx.Store.WorkspaceEvents.Received().ActiveLayoutEngineChanged -= Arg.Any<
			EventHandler<ActiveLayoutEngineChangedEventArgs>
		>();
		ctx.Store.MapEvents.Received().MonitorWorkspaceChanged -= Arg.Any<
			EventHandler<MonitorWorkspaceChangedEventArgs>
		>();
		ctx.Store.WindowEvents.Received().WindowFocused -= Arg.Any<EventHandler<WindowFocusedEventArgs>>();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Store_ActiveLayoutEngineChanged_UpdatesProperty(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine previousLayoutEngine,
		ILayoutEngine currentLayoutEngine
	)
	{
		// GIVEN an initialized view model monitoring a specific workspace
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		Workspace workspace = StoreTestUtils.CreateWorkspace();
		StoreTestUtils.AddWorkspacesToStore(root, workspace);

		ActiveLayoutWidgetViewModel sut = CreateSut(ctx, monitor);
		bool propertyChanged = false;
		sut.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(ActiveLayoutWidgetViewModel.ActiveLayoutEngine))
			{
				propertyChanged = true;
			}
		};

		// WHEN the active layout engine changes in the workspace
		root.WorkspaceSector.QueueEvent(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = workspace,
				PreviousLayoutEngine = previousLayoutEngine,
				CurrentLayoutEngine = currentLayoutEngine,
			}
		);
		root.WorkspaceSector.DispatchEvents();

		// THEN the view model property is updated and notifies of the change
		Assert.True(propertyChanged);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Store_MonitorWorkspaceChanged_SameMonitor_UpdatesProperties(IContext ctx, MutableRootSector root)
	{
		// GIVEN
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		ILayoutEngine layoutEngine1 = CreateLayoutEngine("Layout1");
		ILayoutEngine layoutEngine2 = CreateLayoutEngine("Layout2");
		Workspace workspace = StoreTestUtils.CreateWorkspace() with { LayoutEngines = [layoutEngine1, layoutEngine2] };
		StoreTestUtils.PopulateMonitorWorkspaceMap(root, monitor, workspace);

		ActiveLayoutWidgetViewModel sut = CreateSut(ctx, monitor);
		List<string> propertyChangedNames = [];
		sut.PropertyChanged += (s, e) => propertyChangedNames.Add(e.PropertyName ?? string.Empty);

		// WHEN
		root.MapSector.QueueEvent(
			new MonitorWorkspaceChangedEventArgs
			{
				Monitor = monitor,
				CurrentWorkspace = workspace,
				PreviousWorkspace = null,
			}
		);
		root.MapSector.DispatchEvents();

		// THEN
		Assert.Contains(nameof(ActiveLayoutWidgetViewModel.LayoutEngines), propertyChangedNames);
		Assert.Contains(nameof(ActiveLayoutWidgetViewModel.ActiveLayoutEngine), propertyChangedNames);
		Assert.Collection(
			sut.LayoutEngines,
			item => Assert.Equal("Layout1", item),
			item => Assert.Equal("Layout2", item)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Store_MonitorWorkspaceChanged_DifferentMonitor_DoesNotUpdateProperties(
		IContext ctx,
		MutableRootSector root
	)
	{
		// GIVEN
		IMonitor widgetMonitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		IMonitor differentMonitor = StoreTestUtils.CreateMonitor((HMONITOR)2);
		ILayoutEngine layoutEngine1 = CreateLayoutEngine("Layout1");
		ILayoutEngine layoutEngine2 = CreateLayoutEngine("Layout2");
		Workspace workspace = StoreTestUtils.CreateWorkspace() with { LayoutEngines = [layoutEngine1, layoutEngine2] };
		StoreTestUtils.PopulateMonitorWorkspaceMap(root, differentMonitor, workspace);

		ActiveLayoutWidgetViewModel sut = CreateSut(ctx, widgetMonitor);
		List<string> propertyChangedNames = [];
		sut.PropertyChanged += (s, e) => propertyChangedNames.Add(e.PropertyName ?? string.Empty);

		// WHEN
		root.MapSector.QueueEvent(
			new MonitorWorkspaceChangedEventArgs
			{
				Monitor = differentMonitor,
				CurrentWorkspace = workspace,
				PreviousWorkspace = null,
			}
		);
		root.MapSector.DispatchEvents();

		// THEN
		Assert.DoesNotContain(nameof(ActiveLayoutWidgetViewModel.LayoutEngines), propertyChangedNames);
		Assert.DoesNotContain(nameof(ActiveLayoutWidgetViewModel.ActiveLayoutEngine), propertyChangedNames);
		Assert.Empty(sut.LayoutEngines);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowEvents_WindowFocused_ClosesDropDown(IContext ctx, MutableRootSector root, IWindow window)
	{
		// GIVEN a view model with an open dropdown
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		ActiveLayoutWidgetViewModel sut = CreateSut(ctx, monitor);
		sut.IsDropDownOpen = true;

		// WHEN a window focus event occurs
		root.WindowSector.QueueEvent(new WindowFocusedEventArgs { Window = window });
		root.WindowSector.DispatchEvents();

		// THEN the dropdown is automatically closed
		Assert.False(sut.IsDropDownOpen);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ActiveLayoutEngine_Get_ReturnsEmptyString_WhenNoLayoutEngine(IContext ctx)
	{
		// GIVEN a view model with no associated workspace or layout engine
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		ActiveLayoutWidgetViewModel sut = CreateSut(ctx, monitor);
		// No layout engine or workspace is populated in the store

		// WHEN requesting the active layout engine name
		string result = sut.ActiveLayoutEngine;

		// THEN an empty string is returned
		Assert.Equal("", result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ActiveLayoutEngine_Get_ReturnsLayoutEngineName(IContext ctx, MutableRootSector root)
	{
		// GIVEN a view model with a workspace containing a specific layout engine
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		ILayoutEngine layoutEngine = CreateLayoutEngine("TestLayout");
		Workspace workspace = StoreTestUtils.CreateWorkspace() with { LayoutEngines = [layoutEngine] };
		StoreTestUtils.PopulateMonitorWorkspaceMap(root, monitor, workspace);

		ActiveLayoutWidgetViewModel sut = CreateSut(ctx, monitor);

		// WHEN requesting the active layout engine name
		string result = sut.ActiveLayoutEngine;

		// THEN the correct layout engine name is returned
		Assert.Equal("TestLayout", result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ActiveLayoutEngine_Set_DoesNothing_WhenNoWorkspace(IContext ctx)
	{
		// GIVEN a view model with no associated workspace
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		ActiveLayoutWidgetViewModel sut = CreateSut(ctx, monitor);
		// No workspace is populated in the store
		bool propertyChanged = false;
		sut.PropertyChanged += (s, e) => propertyChanged = true;

		// WHEN attempting to set a new layout engine
		sut.ActiveLayoutEngine = "NewLayout";

		// THEN no property change occurs since there is no workspace to modify
		Assert.False(propertyChanged);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ActiveLayoutEngine_Set_DoesNotDispatch_WhenSameLayoutEngine(IContext ctx, List<object> transforms)
	{
		// GIVEN a view model with a workspace and active layout engine
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		ILayoutEngine layoutEngine = CreateLayoutEngine("TestLayout");
		Workspace workspace = StoreTestUtils.CreateWorkspace() with { LayoutEngines = [layoutEngine] };

		ActiveLayoutWidgetViewModel sut = CreateSut(ctx, monitor);
		bool propertyChanged = false;
		sut.PropertyChanged += (s, e) => propertyChanged = true;

		// WHEN setting the same layout engine name
		sut.ActiveLayoutEngine = "TestLayout";

		// THEN no changes are dispatched and no property change is triggered
		Assert.Empty(transforms);
		Assert.False(propertyChanged);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ActiveLayoutEngine_Set_Dispatches_WhenDifferentLayoutEngine(
		IContext ctx,
		MutableRootSector root,
		List<object> transforms
	)
	{
		// GIVEN a view model with a workspace and specific layout engine
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		ILayoutEngine layoutEngine = CreateLayoutEngine("TestLayout");
		Workspace workspace = StoreTestUtils.CreateWorkspace() with { LayoutEngines = [layoutEngine] };
		StoreTestUtils.PopulateMonitorWorkspaceMap(root, monitor, workspace);

		ActiveLayoutWidgetViewModel sut = CreateSut(ctx, monitor);
		bool propertyChanged = false;
		sut.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(ActiveLayoutWidgetViewModel.ActiveLayoutEngine))
			{
				propertyChanged = true;
			}
		};

		// WHEN setting a different layout engine name
		sut.ActiveLayoutEngine = "NewLayout";

		// THEN a transform is dispatched and the property change is notified
		Assert.Contains(transforms, t => t.Equals(new SetLayoutEngineFromNameTransform(workspace.Id, "NewLayout")));
		Assert.True(propertyChanged);
	}
}
