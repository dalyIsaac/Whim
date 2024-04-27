namespace Whim;

public interface IRootSector
{
	/// <inheritdoc cref="IMonitorSector"/>
	public IMonitorSector Monitors { get; }

	/// <inheritdoc cref="WorkspaceSector" />
	public IWorkspaceSector Workspaces { get; }

	/// <inheritdoc cref="MapSector" />
	public IMapSector Maps { get; }

	/// <inheritdoc cref="MapSector" />
	public IWindowSector Windows { get; }
}
