using System;
using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

internal class WindowSector : SectorBase, IWindowSector
{
	/// <summary>
	/// All the windows currently tracked by Whim.
	/// </summary>
	public ImmutableDictionary<HWND, IWindow> Windows { get; set; } = ImmutableDictionary<HWND, IWindow>.Empty;

	// TODO: Add to StoreTests
	public override void Initialize() { }

	public override void DispatchEvents()
	{
		foreach (EventArgs eventArgs in _events)
		{
			switch (eventArgs)
			{
				// TODO
				default:
					break;
			}
		}

		_events.Clear();
	}
}
