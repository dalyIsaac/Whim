using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

public static class IconHelper
{
	public static ImageSource? GetIcon(IWindow window)
	{
		if (window.ProcessFilePath is null)
		{
			return null;
		}

		return GetWindowIcon(window.Handle);
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
