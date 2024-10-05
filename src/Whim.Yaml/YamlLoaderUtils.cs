using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text;
using Corvus.Json;
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

		if (entity.Type.AsOptional() is { } backdropTypeStr)
		{
			backdropType = ((string)backdropTypeStr).ParseBackdropType();
		}

		if (entity.AlwaysShowBackdrop.AsOptional() is { } alwaysShowBackdropValue)
		{
			alwaysShowBackdrop = alwaysShowBackdropValue;
		}

		return new WindowBackdropConfig(backdropType, alwaysShowBackdrop);
	}

	internal static Color ParseColor(this string brush)
	{
		if (brush.StartsWith(HexColorStart))
		{
			byte r = 255;
			byte g = 255;
			byte b = 255;
			byte a = 255;

			ReadOnlySpan<char> span = brush.AsSpan();

			if (brush.Length == 7)
			{
				r = ParseHex(span.Slice(1, 2));
				g = ParseHex(span.Slice(3, 2));
				b = ParseHex(span.Slice(5, 2));
			}
			else if (brush.Length == 9)
			{
				r = ParseHex(span.Slice(1, 2));
				g = ParseHex(span.Slice(3, 2));
				b = ParseHex(span.Slice(5, 2));
				a = ParseHex(span.Slice(7, 2));
			}

			return Color.FromArgb(a, r, g, b);
		}

		string colorStr = brush.SnakeToPascal();
		return Color.FromName(colorStr);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static byte ParseHex(this ReadOnlySpan<char> hex)
	{
		try
		{
			return byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
		}
		catch (Exception)
		{
			return 0;
		}
	}

	/// <summary>
	/// Capitalizes the first letter of the string.
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static string Capitalize(this string str) =>
		str.Length switch
		{
			0 => str,
			1 => char.ToUpperInvariant(str[0]).ToString(),
			_ => char.ToUpperInvariant(str[0]) + str[1..],
		};
}
