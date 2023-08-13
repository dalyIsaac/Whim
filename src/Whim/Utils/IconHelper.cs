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
	private const uint U_TIMEOUT = 50;

	public static ImageSource? GetIcon(IWindow window)
	{
		if (window.ProcessFilePath is null)
		{
			return null;
		}

		nuint iconHandle;

		unsafe
		{
			LRESULT result = PInvoke.SendMessageTimeout(
				window.Handle,
				PInvoke.WM_GETICON,
				PInvoke.ICON_BIG,
				0,
				SEND_MESSAGE_TIMEOUT_FLAGS.SMTO_ABORTIFHUNG,
				U_TIMEOUT,
				&iconHandle
			);

			if (result != 0 && iconHandle == 0)
			{
				result = PInvoke.SendMessageTimeout(
					window.Handle,
					PInvoke.WM_GETICON,
					PInvoke.ICON_SMALL,
					0,
					SEND_MESSAGE_TIMEOUT_FLAGS.SMTO_ABORTIFHUNG,
					U_TIMEOUT,
					&iconHandle
				);
			}

			if (result != 0 && iconHandle == 0)
			{
				result = PInvoke.SendMessageTimeout(
					window.Handle,
					PInvoke.WM_GETICON,
					PInvoke.ICON_SMALL2,
					0,
					SEND_MESSAGE_TIMEOUT_FLAGS.SMTO_ABORTIFHUNG,
					U_TIMEOUT,
					&iconHandle
				);
			}

			if (result != 0)
			{
				return ConvertFromHandle(iconHandle);
			}
		}

		return null;
	}

	private static ImageSource? ConvertFromHandle(nuint iconHandle)
	{
		if (iconHandle == 0)
		{
			return null;
		}

		using Icon icon = Icon.FromHandle((nint)iconHandle);
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
