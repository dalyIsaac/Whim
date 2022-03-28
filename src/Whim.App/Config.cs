using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whim.NewBar;
using Whim.FloatingLayout;
using Whim.Gaps;
//using Whim.TreeLayout;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.App;

// The temporary home of the Config, until #57 is resolved.
internal static class Config
{
	internal static ConfigContext CreateConfigContext()
	{
		ConfigContext configContext = new(new LoggerConfig(LogLevel.Verbose));

		// Add workspaces.
		//List<TreeLayoutEngine> treeLayoutEngines = new();
		for (int i = 0; i < 4; i++)
		{
			//TreeLayoutEngine treeLayoutEngine = new(configContext);
			//treeLayoutEngines.Add(treeLayoutEngine);

			Workspace workspace = new(configContext,
							 i.ToString(),
							 new ColumnLayoutEngine(),
							 new ColumnLayoutEngine("Right to left", false)
							 );
							 //treeLayoutEngine);

			configContext.WorkspaceManager.Add(workspace);
		}

		// Add bar
		List<BarComponent> leftComponents = new() { /* WorkspaceWidget.CreateComponent() */ };
		List<BarComponent> centerComponents = new() { /* FocusedWindowWidget.CreateComponent() */ };
		List<BarComponent> rightComponents = new() { /* ActiveLayoutWidget.CreateComponent(), DateTimeWidget.CreateComponent() */ };

		BarConfig barConfig = new(leftComponents, centerComponents, rightComponents);
		BarPlugin barPlugin = new(configContext, barConfig);

		configContext.PluginManager.RegisterPlugin(barPlugin);

		// Add gap
		GapsConfig gapsConfig = new(outerGap: 0, innerGap: 10);
		GapsPlugin gapsPlugin = new(configContext, gapsConfig);
		configContext.PluginManager.RegisterPlugin(gapsPlugin);

		// Add floating layout.
		FloatingLayoutPlugin floatingLayoutPlugin = new(configContext);
		configContext.PluginManager.RegisterPlugin(floatingLayoutPlugin);

		// Keyboard shortcuts
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift, VIRTUAL_KEY.VK_LEFT), (args) => configContext.WorkspaceManager.MoveWindowToPreviousMonitor());
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift, VIRTUAL_KEY.VK_RIGHT), (args) => configContext.WorkspaceManager.MoveWindowToNextMonitor());

		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_LEFT), (args) =>
		{
			IWorkspace workspace = configContext.WorkspaceManager.ActiveWorkspace;
			if (workspace.LastFocusedWindow == null)
			{
				return;
			}

			workspace.ActiveLayoutEngine.FocusWindowInDirection(Direction.Left, workspace.LastFocusedWindow);
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_RIGHT), (args) =>
		{
			IWorkspace workspace = configContext.WorkspaceManager.ActiveWorkspace;
			if (workspace.LastFocusedWindow == null)
			{
				return;
			}

			workspace.ActiveLayoutEngine.FocusWindowInDirection(Direction.Right, workspace.LastFocusedWindow);
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_UP), (args) =>
		{
			IWorkspace workspace = configContext.WorkspaceManager.ActiveWorkspace;
			if (workspace.LastFocusedWindow == null)
			{
				return;
			}

			workspace.ActiveLayoutEngine.FocusWindowInDirection(Direction.Up, workspace.LastFocusedWindow);
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_DOWN), (args) =>
		{
			IWorkspace workspace = configContext.WorkspaceManager.ActiveWorkspace;
			if (workspace.LastFocusedWindow == null)
			{
				return;
			}

			workspace.ActiveLayoutEngine.FocusWindowInDirection(Direction.Down, workspace.LastFocusedWindow);
		});

		// Swap windows in direction.
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_LEFT), (args) => configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Left));
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_RIGHT), (args) => configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Right));
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_UP), (args) => configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Up));
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_DOWN), (args) => configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(Direction.Down));

		//#region TreeLayoutEngine
		//// Set direction.
		//void setNodeDirection(KeybindEventArgs args)
		//{
		//	// Get direction.
		//	Direction? direction = args.Keybind.Key switch
		//	{
		//		VIRTUAL_KEY.VK_LEFT => Direction.Left,
		//		VIRTUAL_KEY.VK_RIGHT => Direction.Right,
		//		VIRTUAL_KEY.VK_UP => Direction.Up,
		//		VIRTUAL_KEY.VK_DOWN => Direction.Down,
		//		_ => null
		//	};

		//	if (direction == null)
		//	{
		//		return;
		//	}

		//	foreach (TreeLayoutEngine treeLayoutEngine in treeLayoutEngines)
		//	{
		//		treeLayoutEngine.AddNodeDirection = direction.Value;
		//	}
		//}
		//configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_RIGHT), setNodeDirection);
		//configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_DOWN), setNodeDirection);
		//configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_LEFT), setNodeDirection);
		//configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_UP), setNodeDirection);

		//configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_S), (args) =>
		//{
		//	TreeLayoutEngine? layoutEngine = ILayoutEngine.GetLayoutEngine<TreeLayoutEngine>(configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine);
		//	if (layoutEngine != null)
		//	{
		//		layoutEngine.SplitFocusedWindow();
		//	}
		//});

		//configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift | KeyModifiers.LAlt | KeyModifiers.LControl, VIRTUAL_KEY.VK_F), (args) =>
		//{
		//	TreeLayoutEngine? layoutEngine = ILayoutEngine.GetLayoutEngine<TreeLayoutEngine>(configContext.WorkspaceManager.ActiveWorkspace.ActiveLayoutEngine);
		//	if (layoutEngine != null)
		//	{
		//		layoutEngine.FlipAndMerge();
		//	}
		//});
		//#endregion

		// configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_G), (args) => gapsPlugin.UpdateInnerGap(10));
		// configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_G), (args) => gapsPlugin.UpdateOuterGap(10));

		Direction edge = Direction.Left;

		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift, VIRTUAL_KEY.VK_J), (args) =>
		{
			edge = Direction.Left;
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift, VIRTUAL_KEY.VK_K), (args) =>
		{
			edge = Direction.Right;
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift, VIRTUAL_KEY.VK_I), (args) =>
		{
			edge = Direction.Up;
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift, VIRTUAL_KEY.VK_M), (args) =>
		{
			edge = Direction.Down;
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift, VIRTUAL_KEY.VK_H), (args) =>
		{
			configContext.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(edge, edge == Direction.Left || edge == Direction.Up ? 0.1 : -0.1);
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift, VIRTUAL_KEY.VK_L), (args) =>
		{
			configContext.WorkspaceManager.ActiveWorkspace.MoveWindowEdgeInDirection(edge, edge == Direction.Left || edge == Direction.Up ? -0.1 : 0.1);
		});

		// Floating layout
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LShift, VIRTUAL_KEY.VK_F), (args) =>
		{
			floatingLayoutPlugin.ToggleWindowFloating();
		});

		return configContext;
	}
}
