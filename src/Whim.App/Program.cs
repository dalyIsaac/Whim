using System;
using System.Collections.Generic;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Whim.Bar;
using Whim.Dashboard;
using Whim.Gaps;
using Whim.TreeLayout;

namespace Whim.App;

public class Program
{
	[STAThread]
	public static void Main()
	{
		_ = new App(CreateConfigContext());
	}

	private static ConfigContext CreateConfigContext()
	{
		ConfigContext configContext = new(new LoggerConfig(LogLevel.Verbose));
		List<TreeLayoutEngine> treeLayoutEngines = new();

		// Add workspaces
		for (int i = 0; i < 4; i++)
		{
			// Workspace workspace = new(configContext, i.ToString(), new ColumnLayoutEngine(), new ColumnLayoutEngine("Right to left", false));
			TreeLayoutEngine treeLayoutEngine = new(configContext);
			treeLayoutEngines.Add(treeLayoutEngine);

			Workspace workspace = new(configContext, i.ToString(), treeLayoutEngine);
			configContext.WorkspaceManager.Add(workspace);
		}

		// Add dashboard
		DashboardPlugin dashboardPlugin = new(configContext);

		configContext.PluginManager.RegisterPlugin(dashboardPlugin);
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_F12), (args) => dashboardPlugin.Toggle());

		// Add bar
		List<BarComponent> leftComponents = new() { WorkspaceWidget.CreateComponent() };
		List<BarComponent> centerComponents = new() { FocusedWindowWidget.CreateComponent() };
		List<BarComponent> rightComponents = new() { ActiveLayoutWidget.CreateComponent(), DateTimeWidget.CreateComponent() };

		BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
		BarPlugin barPlugin = new(configContext, barConfig);

		configContext.PluginManager.RegisterPlugin(barPlugin);

		// Add gap
		GapsConfig gapsConfig = new(outerGap: 0, innerGap: 10);
		GapsPlugin gapsPlugin = new(configContext, gapsConfig);

		configContext.PluginManager.RegisterPlugin(gapsPlugin);

		// Keyboard shortcuts
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift, VIRTUAL_KEY.VK_LEFT), (args) => configContext.WorkspaceManager.MoveWindowToPreviousMonitor());
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift, VIRTUAL_KEY.VK_RIGHT), (args) => configContext.WorkspaceManager.MoveWindowToNextMonitor());

		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_LEFT), (args) =>
		{
			IWorkspace workspace = configContext.WorkspaceManager.ActiveWorkspace;
			if (workspace.LastFocusedWindow == null)
				return;

			workspace.ActiveLayoutEngine.FocusWindowInDirection(Direction.Left, workspace.LastFocusedWindow);
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_RIGHT), (args) =>
		{
			IWorkspace workspace = configContext.WorkspaceManager.ActiveWorkspace;
			if (workspace.LastFocusedWindow == null)
				return;

			workspace.ActiveLayoutEngine.FocusWindowInDirection(Direction.Right, workspace.LastFocusedWindow);
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_UP), (args) =>
		{
			IWorkspace workspace = configContext.WorkspaceManager.ActiveWorkspace;
			if (workspace.LastFocusedWindow == null)
				return;

			workspace.ActiveLayoutEngine.FocusWindowInDirection(Direction.Up, workspace.LastFocusedWindow);
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_DOWN), (args) =>
		{
			IWorkspace workspace = configContext.WorkspaceManager.ActiveWorkspace;
			if (workspace.LastFocusedWindow == null)
				return;

			workspace.ActiveLayoutEngine.FocusWindowInDirection(Direction.Down, workspace.LastFocusedWindow);
		});

		// Swap windows in direction.
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_LEFT), (args) => configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Left));
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_RIGHT), (args) => configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Right));
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_UP), (args) => configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Up));
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_DOWN), (args) => configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Down));

		#region TreeLayoutEngine
		// Set direction.
		KeybindEventHandler setNodeDirection = (args) =>
		{
			// Get direction.
			Direction? direction = args.Keybind.Key switch
			{
				VIRTUAL_KEY.VK_LEFT => Direction.Left,
				VIRTUAL_KEY.VK_RIGHT => Direction.Right,
				VIRTUAL_KEY.VK_UP => Direction.Up,
				VIRTUAL_KEY.VK_DOWN => Direction.Down,
				_ => null
			};

			if (direction == null)
				return;

			foreach (TreeLayoutEngine treeLayoutEngine in treeLayoutEngines)
				treeLayoutEngine.AddNodeDirection = direction.Value;
		};
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_RIGHT), setNodeDirection);
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_DOWN), setNodeDirection);
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_LEFT), setNodeDirection);
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_UP), setNodeDirection);

		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_S), (args) =>
		{
			TreeLayoutEngine? layoutEngine = ILayoutEngine.GetLayoutEngine<TreeLayoutEngine>(configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine);
			if (layoutEngine != null)
			{
				layoutEngine.SplitFocusedWindow();
			}
		});

		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_F), (args) =>
		{
			TreeLayoutEngine? layoutEngine = ILayoutEngine.GetLayoutEngine<TreeLayoutEngine>(configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine);
			if (layoutEngine != null)
			{
				layoutEngine.FlipAndMerge();
			}
		});
		#endregion

		// configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_G), (args) => gapsPlugin.UpdateInnerGap(10));
		// configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_G), (args) => gapsPlugin.UpdateOuterGap(10));

		return configContext;
	}
}
