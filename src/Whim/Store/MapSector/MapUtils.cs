using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal static class MapUtils
{
	public static HMONITOR GetMonitorForWorkspace(this IMapSector sector, WorkspaceId workspaceId)
	{
		foreach ((HMONITOR monitor, WorkspaceId workspace) in sector.MonitorWorkspaceMap)
		{
			if (workspace.Equals(workspaceId))
			{
				return monitor;
			}
		}

		return default;
	}
}
