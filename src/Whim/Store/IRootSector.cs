namespace Whim;

public interface IRootSector
{
	/// <inheritdoc cref="IMonitorSector"/>
	public IMonitorSector Monitors { get; }

	/// <inheritdoc cref="WorkspaceSector" />
	public WorkspaceSector Workspaces { get; }

	/// <inheritdoc cref="MapSector" />
	public MapSector Maps { get; }

	/// <inheritdoc cref="MapSector" />
	public WindowSector Windows { get; }
}
