using Microsoft.UI.Xaml.Media.Imaging;
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
internal static class IconHelper
{
	/// <summary>
	/// Tries to get the icon for a window.
	/// </summary>
	/// <param name="window"></param>
	/// <param name="coreNativeManager"></param>
	/// <param name="nativeManager"></param>
	/// <returns></returns>
	public static BitmapImage? GetIcon(
		this IWindow window,
		ICoreNativeManager coreNativeManager,
		INativeManager nativeManager
	)
	{
		Logger.Debug($"Getting icon for window {window}");
		return window.IsUwp ? GetUwpAppIcon(nativeManager, window) : GetWindowIcon(coreNativeManager, window.Handle);
	}

	/// <summary>
	/// Gets the icon for a UWP app by finding the AppxManifest.xml file and reading the logo path.
	/// </summary>
	/// <remarks>
	/// Based on https://stackoverflow.com/questions/32122679/getting-icon-of-modern-windows-app-from-a-desktop-application
	/// </remarks>
	/// <param name="nativeManager"></param>
	/// <param name="window"></param>
	/// <returns></returns>
	private static BitmapImage? GetUwpAppIcon(INativeManager nativeManager, IWindow window)
	{
		Logger.Debug($"Getting UWP icon for window {window}");
		string? exePath = nativeManager.GetUwpAppProcessPath(window);
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

		// Read the manifest file.
		string? pathToLogo;
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

	private static BitmapImage? GetWindowIcon(ICoreNativeManager coreNativeManager, HWND hwnd)
	{
		Logger.Debug($"Getting window icon for HWND {hwnd}");
		HICON hIcon = new(coreNativeManager.SendMessage(hwnd, PInvoke.WM_GETICON, PInvoke.ICON_BIG, 0));

		if (hIcon == 0)
		{
			hIcon = (HICON)(nint)coreNativeManager.GetClassLongPtr(hwnd, GET_CLASS_LONG_INDEX.GCL_HICON);
		}

		if (hIcon == 0)
		{
			Logger.Error($"Could not load icon for HWND ${hwnd}");
			return null;
		}

		// Get the icon from the handle.
		using Icon icon = Icon.FromHandle((nint)hIcon);
		MemoryStream iconStream = new();

		using (Bitmap bmp = icon.ToBitmap())
		{
			bmp.Save(iconStream, ImageFormat.Png);
		}

		iconStream.Position = 0;
		return ConvertFromStream(iconStream);
	}

	private static BitmapImage ConvertFromStream(Stream stream)
	{
		BitmapImage bitmap = new();
		bitmap.SetSource(stream.AsRandomAccessStream());
		return bitmap;
	}
}
