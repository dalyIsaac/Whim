using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal partial class Screen
{
	// This identifier is just for us, so that we don't try to call the multimon
	// functions if we just need the primary monitor... this is safer for
	// non-multimon OSes.
	private static readonly HMONITOR PRIMARY_MONITOR = unchecked((HMONITOR)(int)0xBAADF00D);

	/// <summary>
	///  Static counter of desktop size changes.
	/// </summary>
	private static int s_desktopChangedCount = -1;

	/// <summary>
	///  Used to lock this class before syncing to SystemEvents.
	/// </summary>
	private static readonly object s_syncLock = new();

	private class MonitorEnumCallback
	{
		public required ICoreNativeManager CoreNativeManager { get; init; }

		public List<Screen> Screens { get; } = new();

		public virtual unsafe BOOL Callback(HMONITOR monitor, HDC hdc, RECT* rect, LPARAM param)
		{
			Screens.Add(new Screen(CoreNativeManager, monitor));
			return (BOOL)true;
		}
	}

	/// <summary>
	/// Used to store the screens until <see cref="OnDisplaySettingsChanging" /> invalidates it.
	/// </summary>
	private static Screen[]? s_screens;

	internal static unsafe Screen[] GetAllScreens(ICoreNativeManager coreNativeManager)
	{
		if (s_screens is null)
		{
			if (coreNativeManager.HasMultipleMonitors())
			{
				MonitorEnumCallback closure = new() { CoreNativeManager = coreNativeManager };
				MONITORENUMPROC proc = new(closure.Callback);
				coreNativeManager.EnumDisplayMonitors(null, null, proc, (LPARAM)0);

				if (closure.Screens.Count > 0)
				{
					Screen[] temp = new Screen[closure.Screens.Count];
					closure.Screens.CopyTo(temp, 0);
					s_screens = temp;
				}
				else
				{
					s_screens = new Screen[] { new Screen(coreNativeManager, PRIMARY_MONITOR) };
				}
			}
			else
			{
				Screen? primaryScreen = GetPrimaryScreen(coreNativeManager);

				s_screens = primaryScreen != null ? new Screen[] { primaryScreen } : Array.Empty<Screen>();
			}
		}

		return s_screens;
	}
}
