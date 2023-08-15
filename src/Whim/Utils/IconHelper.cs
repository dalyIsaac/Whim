using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Linq;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// Helper class for getting icons for windows.
/// </summary>
public static class IconHelper
{
	/// <summary>
	/// Tries to get the icon for a window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static ImageSource? GetIcon(IWindow window)
	{
		Logger.Debug($"Getting icon for window {window}");

		return window.ProcessFileName == "ApplicationFrameHost.exe"
		? GetUwpAppIcon(window.Handle)
		: GetWindowIcon(window.Handle);
	}

	/// <summary>
	/// Gets the icon for a UWP app by finding the AppxManifest.xml file and reading the logo path.
	/// </summary>
	/// <remarks>
	/// Based on https://stackoverflow.com/questions/32122679/getting-icon-of-modern-windows-app-from-a-desktop-application
	/// </remarks>
	/// <param name="hwnd"></param>
	/// <returns></returns>
	private static ImageSource? GetUwpAppIcon(HWND hwnd)
	{
		Logger.Debug($"Getting UWP icon for HWND {hwnd}");
		string? exePath = GetUwpAppProcessPath(hwnd);
		if (exePath is null)
		{
			Logger.Error("Could not get UWP app process path");
			return null;
		}

		string? exeDir = Path.GetDirectoryName(exePath);
		if (exeDir is null)
		{
			Logger.Error("Could not get directory name for UWP app process path");
			return null;
		}

		string manifestPath = Path.Combine(exeDir, "AppxManifest.xml");
		if (!File.Exists(manifestPath))
		{
			Logger.Error($"Could not find AppxManifest.xml at {manifestPath}");
			return null;
		}

		string? pathToLogo;

		// Read the manifest file.
		using (FileStream fs = File.OpenRead(manifestPath))
		{
			XDocument manifest = XDocument.Load(fs);
			const string ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
			pathToLogo = manifest.Root?.Element(XName.Get("Properties", ns))?.Element(XName.Get("Logo", ns))?.Value;
		}

		if (pathToLogo is null)
		{
			Logger.Error("Could not find logo path in AppxManifest.xml");
			return null;
		}

		string finalLogo = Path.Combine(exeDir, pathToLogo);
		if (!File.Exists(finalLogo))
		{
			Logger.Error($"Could not find logo at {finalLogo}");
			return null;
		}

		using FileStream fileStream = File.OpenRead(finalLogo);
		return ConvertFromStream(fileStream);
	}

	// TODO: Consider pulling into IWindow (with a flag for IsModernApp)
	private static string? GetUwpAppProcessPath(HWND hwnd)
	{
		uint pid = 0;
		unsafe
		{
			PInvoke.GetWindowThreadProcessId(hwnd, &pid);
		}

		// now this is a bit tricky. Modern apps are hosted inside ApplicationFrameHost process, so we need to find
		// child window which does NOT belong to this process. This should be the process we need
		List<HWND> children = GetChildWindows(hwnd);
		foreach (HWND childHwnd in children)
		{
			uint childPid;
			unsafe
			{
				PInvoke.GetWindowThreadProcessId(childHwnd, &childPid);
			}
			if (childPid != pid)
			{
				// here we are
				Process childProc = Process.GetProcessById((int)childPid);
				return childProc.MainModule?.FileName;
			}
		}

		Logger.Error("Cannot find a path to Uwp App executable file for HWND ${hwnd}");
		return null;
	}

	private static List<HWND> GetChildWindows(HWND parent)
	{
		List<HWND> windows = new();

		PInvoke.EnumChildWindows(
			parent,
			(handle, param) =>
			{
				windows.Add(handle);
				return true;
			},
			0
		);

		return windows;
	}

	private static ImageSource? GetWindowIcon(HWND hwnd)
	{
		Logger.Debug($"Getting window icon for HWND {hwnd}");
		// TODO: Consider using SendMessageTimeout
		HICON hIcon = new(PInvoke.SendMessage(hwnd, PInvoke.WM_GETICON, PInvoke.ICON_BIG, 0));

		if (hIcon == 0)
		{
			hIcon = (HICON)(nint)PInvoke.GetClassLongPtr(hwnd, GET_CLASS_LONG_INDEX.GCL_HICON);
		}

		if (hIcon != 0)
		{
			return ConvertFromHandle(hIcon);
		}
		else
		{
			Logger.Error($"Could not load icon for HWND ${hwnd}");
			return null;
		}
	}

	// TODO: Are the following methods used by GetUwpAppIcon?
	private static ImageSource? ConvertFromHandle(HICON hIcon)
	{
		using Icon icon = Icon.FromHandle((nint)hIcon);
		MemoryStream iconStream = new();

		using (Bitmap bmp = icon.ToBitmap())
		{
			bmp.Save(iconStream, ImageFormat.Png);
		}

		iconStream.Position = 0;
		return ConvertFromStream(iconStream);
	}

	private static ImageSource ConvertFromStream(Stream stream)
	{
		BitmapImage bitmap = new();
		bitmap.SetSource(stream.AsRandomAccessStream());
		return bitmap;
	}
}
