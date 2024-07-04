namespace Whim;

internal static class MapUtils
{
	public static HMONITOR GetMonitorByWorkspace(this IMapSector sector, WorkspaceId searchWorkspaceId)
	{
		foreach ((HMONITOR monitor, WorkspaceId workspace) in sector.MonitorWorkspaceMap)
		{
			if (workspace.Equals(searchWorkspaceId))
			{
				return monitor;
			}
		}

		return default;
	}
}
