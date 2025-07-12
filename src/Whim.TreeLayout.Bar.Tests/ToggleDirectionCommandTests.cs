using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.TreeLayout.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ToggleDirectionCommandTests
{
	private class Customization : StoreCustomization
	{
		protected override void PostCustomize(IFixture fixture)
		{
			IMonitor monitor = CreateMonitor((HMONITOR)1234);
			fixture.Inject(monitor);

			var root = _store._root.MutableRootSector;

			ILayoutEngine layoutEngine = fixture.Freeze<ILayoutEngine>();
			Workspace workspace = CreateWorkspace(_ctx) with { LayoutEngines = [layoutEngine] };
			fixture.Inject(workspace);

			PopulateMonitorWorkspaceMap(_ctx, root, monitor, workspace);

			ITreeLayoutPlugin plugin = fixture.Freeze<ITreeLayoutPlugin>();

			TreeLayoutEngineWidgetViewModel viewModel = new(_ctx, plugin, monitor);
			fixture.Inject(viewModel);
		}
	}

	[Theory, AutoSubstituteData<Customization>]
	public void CanExecute_ShouldReturnTrue(TreeLayoutEngineWidgetViewModel viewModel)
	{
		// Given
		ToggleDirectionCommand command = new(viewModel);

		// When
		bool actual = command.CanExecute(null);

		// Then
		Assert.True(actual);
	}

	[Theory, AutoSubstituteData<Customization>]
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
