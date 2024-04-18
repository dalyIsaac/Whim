using System;
using System.Collections.Immutable;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// The slice containing windows.
/// </summary>
public class WindowSlice : ISlice
{
	/// <summary>
	/// All the windows currently tracked by Whim.
	/// </summary>
	public ImmutableDictionary<HWND, IWindow> Windows { get; internal set; } = ImmutableDictionary<HWND, IWindow>.Empty;

	// TODO: Add to StoreTests
	internal override void Initialize() { }

	internal override void DispatchEvents()
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
