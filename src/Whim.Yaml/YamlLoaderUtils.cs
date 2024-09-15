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
			byte r = Convert.ToByte(brush.Substring(1, 2), 16);
			byte g = Convert.ToByte(brush.Substring(3, 2), 16);
			byte b = Convert.ToByte(brush.Substring(5, 2), 16);
			byte a = Convert.ToByte(brush.Substring(7, 2), 16);
			return new SolidColorBrush(ColorHelper.FromArgb(a, r, g, b));
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
