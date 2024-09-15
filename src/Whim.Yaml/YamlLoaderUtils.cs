using Corvus.Json;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System.Text;
using static Whim.Yaml.Schema;

namespace Whim.Yaml;

internal static class YamlLoaderUtils
{
	private const string HexColorStart = "#";

	internal static string SnakeToPascal(this string snake)
	{
		string[] parts = snake.Split('_');
		StringBuilder builder = new(snake.Length);

		foreach (string part in parts)
		{
			builder.Append(char.ToUpper(part[0]));
			builder.Append(part.AsSpan(1));
		}

		return builder.ToString();
	}

	internal static BackdropType ParseBackdropType(this string backdropType) =>
		backdropType switch
		{
			"none" => BackdropType.None,
			"acrylic" => BackdropType.Acrylic,
			"acrylic_thin" => BackdropType.AcrylicThin,
			"mica" => BackdropType.Mica,
			"mica_alt" => BackdropType.MicaAlt,
			_ => BackdropType.None,
		};

	internal static WindowBackdropConfig ParseWindowBackdropConfig(WindowBackdropConfigEntity entity)
	{
		BackdropType backdropType = BackdropType.Mica;
		bool alwaysShowBackdrop = true;

		if (entity.BackdropType.AsOptional() is { } backdropTypeStr)
		{
			backdropType = ((string)backdropTypeStr).ParseBackdropType();
		}

		if (entity.AlwaysShowBackdrop.AsOptional() is { } alwaysShowBackdropValue)
		{
			alwaysShowBackdrop = alwaysShowBackdropValue;
		}

		return new WindowBackdropConfig(backdropType, alwaysShowBackdrop);
	}

	internal static Brush ParseBrush(this string brush)
	{
		if (brush.StartsWith(HexColorStart))
		{
			byte r = 255;
			byte g = 255;
			byte b = 255;
			byte a = 255;

			if (brush.Length == 4)
			{
				r = byte.Parse(brush[1].ToString(), System.Globalization.NumberStyles.HexNumber);
				g = byte.Parse(brush[2].ToString(), System.Globalization.NumberStyles.HexNumber);
				b = byte.Parse(brush[3].ToString(), System.Globalization.NumberStyles.HexNumber);
			}
			else if (brush.Length == 7)
			{
				r = byte.Parse(brush[1..3], System.Globalization.NumberStyles.HexNumber);
				g = byte.Parse(brush[3..5], System.Globalization.NumberStyles.HexNumber);
				b = byte.Parse(brush[5..7], System.Globalization.NumberStyles.HexNumber);
			}
			else if (brush.Length == 9)
			{
				r = byte.Parse(brush[1..3], System.Globalization.NumberStyles.HexNumber);
				g = byte.Parse(brush[3..5], System.Globalization.NumberStyles.HexNumber);
				b = byte.Parse(brush[5..7], System.Globalization.NumberStyles.HexNumber);
				a = byte.Parse(brush[7..9], System.Globalization.NumberStyles.HexNumber);
			}
			
			return new SolidColorBrush(
				new Windows.UI.Color()
				{
					R = r,
					G = g,
					B = b,
					A = a,
				}
			);
		}

		string colorStr = brush.SnakeToPascal();
		System.Drawing.Color color = System.Drawing.Color.FromName(colorStr);

		return new SolidColorBrush(
			new Windows.UI.Color()
			{
				R = color.R,
				G = color.G,
				B = color.B,
			}
		);
	}
}
