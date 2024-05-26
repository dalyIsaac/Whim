namespace Whim;

/// <summary>
/// The root sector of the state. This is read-only.
/// </summary>
public interface IRootSector
{
	/// <inheritdoc cref="IMonitorSector"/>
	IMonitorSector MonitorSector { get; }

	/// <inheritdoc cref="IWindowSector"/>
	IWindowSector WindowSector { get; }

	/// <inheritdoc cref="IMapSector"/>
	IMapSector MapSector { get; }

	/// <inheritdoc cref="IWorkspaceSector"/>
	IWorkspaceSector WorkspaceSector { get; }
}
