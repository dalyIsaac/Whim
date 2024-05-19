using System;
using System.Collections.Generic;
using NSubstitute;
using Windows.Win32.Foundation;

namespace Whim.TestUtils;

internal static class MapTestUtils
{
	public static void SetupWindowWorkspaceMapping(
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window,
		IWorkspace workspace
	)
	{
		if (workspace.Id == default)
		{
			workspace.Id.Returns(Guid.NewGuid());
		}

		if (window.Handle == default)
		{
			window.Handle.Returns((HWND)2);
		}

		rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.SetItem(
			window.Handle,
			workspace.Id
		);
		ctx.WorkspaceManager.GetEnumerator().Returns(_ => new List<IWorkspace>() { workspace }.GetEnumerator());
	}
}
