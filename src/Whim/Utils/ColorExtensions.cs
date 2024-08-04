using Microsoft.UI;
using Windows.UI;

namespace Whim;

/// <summary>
/// Extension methods for <see cref="Color"/>.
/// </summary>
public static class ColorExtensions
{
	/// <summary>
	/// Gets the text color based on the given background color.
	/// From https://stackoverflow.com/questions/3942878/how-to-decide-font-color-in-white-or-black-depending-on-background-color
	/// </summary>
	/// <param name="backgroundColor"></param>
	/// <returns></returns>
	public static Color GetTextColor(this Color backgroundColor)
	{
		double[] uiColors = [backgroundColor.R / 255, backgroundColor.G / 255, backgroundColor.B / 255];

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
