# Monitors

Whim supports multiple monitors via the <xref:Whim.IMonitorManager>, which stores the current monitor configuration. `IMonitorManager` provides various methods including the ability get adjacent monitors, and the monitor which contains a given point.

Each <xref:Whim.IMonitor> contains properties like its scale factor.

> [!NOTE]
> Whim does not support Windows' native "virtual" desktops, as they lack the ability to activate "desktops" independently of monitors.
