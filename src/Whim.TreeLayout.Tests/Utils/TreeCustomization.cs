using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim.TreeLayout.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
internal class TreeCustomization : StoreCustomization
{
	private int _windowIdx = 1;

	protected override void PostCustomize(IFixture fixture)
	{
		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1234);
		monitor.WorkingArea.Returns(new Rectangle<int>() { Width = 100, Height = 100 });
		fixture.Inject(monitor);

		Workspace workspace = StoreTestUtils.CreateWorkspace(_ctx);
		fixture.Inject(workspace);

		MutableRootSector root = _store._root.MutableRootSector;
		StoreTestUtils.PopulateMonitorWorkspaceMap(_ctx, root, monitor, workspace);

		ITreeLayoutPlugin plugin = Substitute.For<ITreeLayoutPlugin>();
		plugin.GetAddWindowDirection(Arg.Any<TreeLayoutEngine>()).Returns(Direction.Right);
		fixture.Inject(plugin);

		fixture.Customize<IWindow>(c =>
		{
			return c.FromFactory(() =>
			{
				HWND handle = (HWND)_windowIdx;
				_windowIdx++;
				return StoreTestUtils.CreateWindow(handle);
			});
		});

		fixture.Customize<LayoutEngineIdentity>(c =>
		{
			return c.FromFactory(() => new LayoutEngineIdentity());
		});
	}

	public static void SetAddWindowDirection(ITreeLayoutPlugin plugin, Direction direction)
	{
		plugin.GetAddWindowDirection(Arg.Any<TreeLayoutEngine>()).Returns(direction);
	}

	public static void SetAsLastFocusedWindow(
		IContext ctx,
		MutableRootSector root,
		Workspace workspace,
		IWindow? window
	)
	{
		workspace = workspace with { LastFocusedWindowHandle = window?.Handle ?? default };

		if (window != null)
		{
			StoreTestUtils.PopulateWindowWorkspaceMap(ctx, root, window, workspace);
		}
	}
}
