using System;
using System.Collections.Generic;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Whim.Bar;
using Whim.Dashboard;
using Whim.Gaps;

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
		ConfigContext configContext = new();

		// Add workspaces
		for (int i = 0; i < 4; i++)
		{
			Workspace workspace = new(configContext, i.ToString(), new ColumnLayoutEngine(), new ColumnLayoutEngine("Right to left", false));
			configContext.WorkspaceManager.Add(workspace);
		}

		// Add dashboard
		DashboardPlugin dashboardPlugin = new(configContext);

		configContext.PluginManager.RegisterPlugin(dashboardPlugin);
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_F12), (args) => dashboardPlugin.Toggle());

		// Add bar
		List<BarComponent> leftComponents = new()
		{
			WorkspaceWidget.CreateComponent(),
			ActiveLayoutWidget.CreateComponent(),
			FocusedWindowWidget.CreateComponent(),
		};
		List<BarComponent> centerComponents = new() { TextWidget.CreateComponent("Hello World!") };
		List<BarComponent> rightComponents = new() { DateTimeWidget.CreateComponent() };

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
			if (workspace.FocusedWindow == null)
			{
				return;
			}

			workspace.ActiveLayoutEngine.FocusWindowInDirection(WindowDirection.Left, workspace.FocusedWindow);
		});
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_RIGHT), (args) =>
		{
			IWorkspace workspace = configContext.WorkspaceManager.ActiveWorkspace;
			if (workspace.FocusedWindow == null)
			{
				return;
			}

			workspace.ActiveLayoutEngine.FocusWindowInDirection(WindowDirection.Right, workspace.FocusedWindow);
		});

		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_LEFT), (args) => configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(WindowDirection.Left));
		configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_RIGHT), (args) => configContext.WorkspaceManager.ActiveWorkspace.SwapWindowInDirection(WindowDirection.Right));

		// configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin, VIRTUAL_KEY.VK_G), (args) => gapsPlugin.UpdateInnerGap(10));
		// configContext.KeybindManager.Add(new Keybind(KeyModifiers.LWin | KeyModifiers.LAlt, VIRTUAL_KEY.VK_G), (args) => gapsPlugin.UpdateOuterGap(10));

		return configContext;
	}
}
