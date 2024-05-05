using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Bar.Tests;

public class ToggleDirectionCommandCustomization : ICustomization
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		IMonitor monitor = fixture.Freeze<IMonitor>();
		IWorkspace workspace = fixture.Freeze<IWorkspace>();
		ILayoutEngine treeLayoutEngine = fixture.Freeze<ILayoutEngine>();
		ITreeLayoutPlugin plugin = fixture.Freeze<ITreeLayoutPlugin>();

		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);
		fixture.Inject(viewModel);

		workspace.ActiveLayoutEngine.Returns(treeLayoutEngine);

		Store store = new(ctx, internalCtx);
		ctx.Store.Returns(store);

		fixture.Inject(store._root);
		fixture.Inject(store._root.MutableRootSector);

		store._root.MutableRootSector.Maps.MonitorWorkspaceMap =
			store._root.MutableRootSector.Maps.MonitorWorkspaceMap.SetItem(monitor, workspace);
	}
}

public class ToggleDirectionCommandTests
{
	[Theory, AutoSubstituteData<ToggleDirectionCommandCustomization>]
	public void CanExecute_ShouldReturnTrue(TreeLayoutEngineWidgetViewModel viewModel)
	{
		// Given
		ToggleDirectionCommand command = new(viewModel);

		// When
		bool actual = command.CanExecute(null);

		// Then
		Assert.True(actual);
	}

	[Theory, AutoSubstituteData<ToggleDirectionCommandCustomization>]
	public void Execute_ShouldToggleDirection(
		ITreeLayoutPlugin plugin,
		TreeLayoutEngineWidgetViewModel viewModel,
		IMonitor monitor
	)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns(Direction.Up);
		ToggleDirectionCommand command = new(viewModel);

		// When
		command.Execute(null);

		// Then
		plugin.Received(1).SetAddWindowDirection(monitor, Direction.Right);
	}
}
