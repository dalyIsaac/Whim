using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Whim.LayoutPreview;

/// <summary>
/// Interaction logic for LayoutPreviewWindowItem.xaml
/// </summary>
public sealed partial class LayoutPreviewWindowItem : UserControl
{
	/// <summary>
	/// The <see cref="IWindowState"/> that this <see cref="LayoutPreviewWindowItem"/> represents.
	/// </summary>
	public IWindowState WindowState { get; }

	/// <summary>
	/// The icon for the window.
	/// </summary>
	public ImageSource? ImageSource { get; }

	internal LayoutPreviewWindowItem(IContext context, IWindowState windowState, bool isHovered)
	{
		WindowState = windowState;
		ImageSource = windowState.Window.GetIcon();

		InitializeComponent();

		Color tintColor;
		if (isHovered)
		{
			tintColor = GetHoverTintColor();
			Title.Foreground = new SolidColorBrush(GetTextColor(tintColor));
		}
		else
		{
			tintColor = context.NativeManager.ShouldSystemUseDarkMode()
				? Color.FromArgb(Colors.Black.A, 33, 33, 33)
				: Color.FromArgb(Colors.White.A, 253, 253, 253);
		}

		Panel.Background = new AcrylicBrush()
		{
			Opacity = 0.8,
			TintLuminosityOpacity = 0.8,
			TintColor = tintColor,
		};
	}

	private static Color GetHoverTintColor()
	{
		Color accentColor = new UISettings().GetColorValue(UIColorType.Accent);
		return Color.FromArgb(255, accentColor.R, accentColor.G, accentColor.B);
	}

	// TODO: move to Whim's utils
	/// <summary>
	/// Gets the text color based on the given background color.
	/// </summary>
	/// <param name="backgroundColor"></param>
	/// <returns></returns>
	private static Color GetTextColor(Color backgroundColor)
	{
		double[] uiColors = new double[] { backgroundColor.R / 255, backgroundColor.G / 255, backgroundColor.B / 255 };

		double[] cColors = new double[3];
		for (int idx = 0; idx < 3; idx++)
		{
			double col = uiColors[idx];
			if (col <= 0.03928)
			{
				cColors[idx] = col / 12.92;
			}
			else
			{
				cColors[idx] = Math.Pow((col + 0.055) / 1.055, 2.4);
			}
		}

		double L = (0.2126 * cColors[0]) + (0.7152 * cColors[1]) + (0.0722 * cColors[2]);
		return L > 0.179 ? Colors.White : Colors.Black;
	}
}
