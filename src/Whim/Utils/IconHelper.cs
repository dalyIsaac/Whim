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

// TODO: Reference the stackoverflow answer
// https://stackoverflow.com/questions/32122679/getting-icon-of-modern-windows-app-from-a-desktop-application
public static class IconHelper
{
	public static ImageSource? GetIcon(IWindow window)
	{
		if (window.ProcessFilePath is null)
		{
			return null;
		}

		if (window.ProcessFileName == "ApplicationFrameHost.exe")
		{
			// TODO
			// Handles applications like Settings and Calculator.
			return GetUwpAppIcon(window.Handle);
		}

		return GetWindowIcon(window.Handle);
	}

	private static ImageSource? GetUwpAppIcon(HWND hwnd)
	{
		string? actualExePath = GetUwpAppProcessPath(hwnd);
		if (actualExePath is null)
		{
			return null;
		}

		string? dir = Path.GetDirectoryName(actualExePath);
		if (dir is null)
		{
			return null;
		}

		string manifestPath = Path.Combine(dir, "AppxManifest.xml");
		if (!File.Exists(manifestPath))
		{
			return null;
		}

		string pathToLogo;

		// Read the manifest file.
		using (FileStream fs = File.OpenRead(manifestPath))
		{
			XDocument manifest = XDocument.Load(fs);
			const string ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

			// rude parsing - take more care here
			// TODO: fix the parsing here to not use !
			pathToLogo = manifest.Root!.Element(XName.Get("Properties", ns))!.Element(XName.Get("Logo", ns))!.Value;
		}

		// now here it is tricky again - there are several files that match logo, for example
		// black, white, contrast white. Here we choose first, but you might do differently
		string finalLogo = "";

		// search for all files that match file name in Logo element but with any suffix (like "Logo.black.png, Logo.white.png etc)
		string? logoParent = Path.GetDirectoryName(pathToLogo);
		if (logoParent is null)
		{
			return null;
		}

		foreach (string logoFile in Directory.GetFiles(Path.Combine(dir, logoParent),
			Path.GetFileNameWithoutExtension(pathToLogo) + "*" + Path.GetExtension(pathToLogo)))
		{
			finalLogo = logoFile;
			break;
		}

		if (!File.Exists(finalLogo))
		{
			return null;
		}

		using FileStream fileStream = File.OpenRead(finalLogo);

		BitmapImage bitmap = new();
		bitmap.SetSource(fileStream.AsRandomAccessStream());
		return bitmap;
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
	// TODO: Combine with ConvertFromIcon
	private static ImageSource? ConvertFromHandle(HICON hIcon)
	{
		if (hIcon == 0)
		{
			return null;
		}

		using Icon icon = Icon.FromHandle((nint)hIcon);
		return ConvertFromIcon(icon);
	}

	private static ImageSource ConvertFromIcon(Icon icon)
	{
		MemoryStream iconStream = new();

		using (Bitmap bmp = icon.ToBitmap())
		{
			bmp.Save(iconStream, ImageFormat.Png);
		}

		iconStream.Position = 0;

		BitmapImage bitmap = new();
		bitmap.SetSource(iconStream.AsRandomAccessStream());
		return bitmap;
	}
}
