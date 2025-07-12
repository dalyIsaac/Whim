using System.Text.Json;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class CoreSavedStateManagerTests
{
	[Theory, AutoSubstituteData]
	public void PreInitialize_FileDoesNotExist_DoesNotDeserialize(IContext ctx)
	{
		// Given
		ctx.FileManager.SavedStateDir.Returns("savedStateDir");
		CoreSavedStateManager sut = new(ctx);

		// When
		sut.PreInitialize();

		// Then
		ctx.FileManager.Received(1).EnsureDirExists("savedStateDir");
		ctx.FileManager.DidNotReceive().ReadAllText(Arg.Any<string>());
		ctx.FileManager.Received(1).DeleteFile(Arg.Any<string>());
	}

	[Theory, AutoSubstituteData]
	public void PreInitialize_FileExists_CannotDeserialize(IContext ctx)
	{
		// Given
		ctx.FileManager.SavedStateDir.Returns("savedStateDir");
		ctx.FileManager.FileExists(Arg.Any<string>()).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns("invalid json");
		CoreSavedStateManager sut = new(ctx);

		// When
		sut.PreInitialize();

		// Then
		ctx.FileManager.Received(1).ReadAllText("savedStateDir\\core.json");
		ctx.FileManager.Received(1).DeleteFile(Arg.Any<string>());
	}

	private static CoreSavedState CreateSavedState()
	{
		return new CoreSavedState(
			new List<SavedWorkspace>()
			{
				new(
					"workspace1",
					new() { new SavedWindow(1, new(0, 0, 100, 100)), new SavedWindow(2, new(100, 100, 100, 100)) },
					null
				),
				new(
					"workspace2",
					new() { new SavedWindow(3, new(200, 200, 100, 100)), new SavedWindow(4, new(300, 300, 100, 100)) },
					null
				),
			}
		);
	}

	[Theory, AutoSubstituteData]
	public void PreInitialize_FileExists_CanDeserialize(IContext ctx)
	{
		// Given
		ctx.FileManager.FileExists(Arg.Any<string>()).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(JsonSerializer.Serialize(CreateSavedState()));
		CoreSavedStateManager sut = new(ctx);

		// When
		sut.PreInitialize();

		// Then
		ctx.FileManager.Received(1).ReadAllText(Arg.Any<string>());
		Assert.NotNull(sut.SavedState);
		Assert.Equal(2, sut.SavedState!.Workspaces.Count);
		Assert.Equal(2, sut.SavedState!.Workspaces[0].Windows.Count);
		Assert.Equal(2, sut.SavedState!.Workspaces[1].Windows.Count);
		ctx.FileManager.Received(1).DeleteFile(Arg.Any<string>());
	}

	[Theory, AutoSubstituteData]
	public void PostInitialize(IContext ctx)
	{
		// Given
		ctx.FileManager.SavedStateDir.Returns("savedStateDir");
		ctx.FileManager.FileExists(Arg.Any<string>()).Returns(true);
		ctx.FileManager.ReadAllText(Arg.Any<string>()).Returns(JsonSerializer.Serialize(CreateSavedState()));
		CoreSavedStateManager sut = new(ctx);
		sut.PreInitialize();

		// When
		sut.PostInitialize();

		// Then the saved state should be cleared.
		Assert.Null(sut.SavedState);
	}

	private static string Setup_ContextState(IContext ctx, MutableRootSector root)
	{
		IMonitor monitor = CreateMonitor((HMONITOR)123);
		monitor.WorkingArea.Returns(new Rectangle<int>(0, 0, 1000, 1000));
		AddMonitorsToSector(ctx, root, monitor);
		root.MonitorSector.PrimaryMonitorHandle = monitor.Handle;

		// Create four windows.
		IWindow[] windows = new IWindow[4];
		for (int i = 0; i < 4; i++)
		{
			windows[i] = Substitute.For<IWindow>();
			windows[i].Handle.Returns((HWND)(i + 1));
		}

		// Create two workspaces with two windows each
		ILayoutEngine engine1 = Substitute.For<ILayoutEngine>();
		Workspace workspace1 = CreateWorkspace(ctx) with { Name = "workspace1", LayoutEngines = [engine1] };
		engine1
			.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>())
			.Returns(
				new List<IWindowState>()
				{
					new WindowState()
					{
						Window = windows[0],
						Rectangle = new Rectangle<int>(0, 0, 100, 100),
						WindowSize = WindowSize.Normal,
					},
					new WindowState()
					{
						Window = windows[1],
						Rectangle = new Rectangle<int>(100, 100, 100, 100),
						WindowSize = WindowSize.Normal,
					},
				}
			);

		ILayoutEngine engine2 = Substitute.For<ILayoutEngine>();
		Workspace workspace2 = CreateWorkspace(ctx) with { Name = "workspace2", LayoutEngines = [engine2] };
		engine2
			.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>())
			.Returns(
				new List<IWindowState>()
				{
					new WindowState()
					{
						Window = windows[2],
						Rectangle = new Rectangle<int>(200, 200, 100, 100),
						WindowSize = WindowSize.Normal,
					},
					new WindowState()
					{
						Window = windows[3],
						Rectangle = new Rectangle<int>(300, 300, 100, 100),
						WindowSize = WindowSize.Normal,
					},
				}
			);

		// Load the workspaces into the context.
		AddWorkspacesToStore(ctx, root, workspace1, workspace2);

		// Create the expected JSON.
		return JsonSerializer.Serialize(
			new CoreSavedState(
				new List<SavedWorkspace>()
				{
					new(
						"workspace1",
						new() { new SavedWindow(1, new(0, 0, 0.1, 0.1)), new SavedWindow(2, new(0.1, 0.1, 0.1, 0.1)) },
						null
					),
					new(
						"workspace2",
						new()
						{
							new SavedWindow(3, new(0.2, 0.2, 0.1, 0.1)),
							new SavedWindow(4, new(0.3, 0.3, 0.1, 0.1)),
						},
						null
					),
				}
			)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Dispose_SavesState(IContext ctx, MutableRootSector root)
	{
		// Given
		string expectedJson = Setup_ContextState(ctx, root);
		ctx.FileManager.SavedStateDir.Returns("savedStateDir");

		CoreSavedStateManager sut = new(ctx);
		sut.PreInitialize();

		// When
		sut.Dispose();

		// Then
		ctx.FileManager.Received(1).WriteAllText("savedStateDir\\core.json", expectedJson);
	}
}
