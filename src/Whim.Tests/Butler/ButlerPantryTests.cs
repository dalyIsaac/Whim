using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class ButlerPantryTests
{
	#region RemoveMonitor
	[Theory, AutoSubstituteData]
	public void RemoveMonitor_MonitorNotFound(IContext ctx)
	{
		// Given the monitor to remove is not in the pantry
		ButlerPantry sut = new(ctx);

		// When we attempt to remove the monitor
		bool result = sut.RemoveMonitor(Substitute.For<IMonitor>());

		// Then the monitor is not removed
		Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void RemoveMonitor_MonitorFound(IContext ctx, IMonitor monitor, IWorkspace workspace)
	{
		// Given the monitor to remove is in the pantry
		ButlerPantry sut = new(ctx);

		// When we attempt to remove the monitor
		sut.SetMonitorWorkspace(monitor, workspace);
		bool result = sut.RemoveMonitor(monitor);

		// Then the monitor is removed
		Assert.True(result);
	}
	#endregion

	#region RemoveWindow
	[Theory, AutoSubstituteData]
	public void RemoveWindow_WindowNotFound(IContext ctx)
	{
		// Given the window to remove is not in the pantry
		ButlerPantry sut = new(ctx);

		// When we attempt to remove the window
		bool result = sut.RemoveWindow(Substitute.For<IWindow>());

		// Then the window is not removed
		Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void RemoveWindow_WindowFound(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given the window to remove is in the pantry
		ButlerPantry sut = new(ctx);

		// When we attempt to remove the window
		sut.SetWindowWorkspace(window, workspace);
		bool result = sut.RemoveWindow(window);

		// Then the window is removed
		Assert.True(result);
	}
	#endregion

	#region MergeWorkspaceWindows
	[Theory, AutoSubstituteData]
	public void MergeWorkspaceWindows_NoMonitorForWorkspaceToDelete(
		IContext ctx,
		IWorkspace workspaceToDelete,
		IWorkspace workspaceMergeTarget
	)
	{
		// Given the workspace to delete is not mapped to a monitor
		ButlerPantry sut = new(ctx);

		// When we add windows for the workspace to delete, and then merge the workspaces
		sut.SetWindowWorkspace(Substitute.For<IWindow>(), workspaceToDelete);
		sut.SetWindowWorkspace(Substitute.For<IWindow>(), workspaceToDelete);
		sut.MergeWorkspaceWindows(workspaceToDelete, workspaceMergeTarget);

		// Then the monitor is not remapped
		Assert.Null(sut.GetMonitorForWorkspace(workspaceMergeTarget));
	}
	#endregion
}
