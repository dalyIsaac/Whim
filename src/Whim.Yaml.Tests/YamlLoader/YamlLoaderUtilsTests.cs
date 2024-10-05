using System.Drawing;
using Xunit;

namespace Whim.Yaml.Tests;

public class YamlLoaderUtilsTests
{
	[Theory]
	[InlineData("route_to_launched_workspace", "RouteToLaunchedWorkspace")]
	[InlineData("route_to_active_workspace", "RouteToActiveWorkspace")]
	[InlineData(" ", " ")]
	public void SnakeToPascal(string snake, string expected)
	{
		// Given a snake case string
		// When converting it to Pascal case
		string result = snake.SnakeToPascal();

		// Then the string is converted to camel case
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("none", BackdropType.None)]
	[InlineData("acrylic", BackdropType.Acrylic)]
	[InlineData("acrylic_thin", BackdropType.AcrylicThin)]
	[InlineData("mica", BackdropType.Mica)]
	[InlineData("mica_alt", BackdropType.MicaAlt)]
	[InlineData(" ", BackdropType.None)]
	[InlineData("", BackdropType.None)]
	public void ParseBackdropType(string backdropType, BackdropType expected)
	{
		// Given a backdrop type string
		// When parsing it
		BackdropType result = backdropType.ParseBackdropType();

		// Then the string is converted to a backdrop type
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("#FF0000", 255, 0, 0, 255)]
	[InlineData("#00FF00", 0, 255, 0, 255)]
	[InlineData("#0000FF", 0, 0, 255, 255)]
	[InlineData("#FFFFFF", 255, 255, 255, 255)]
	[InlineData("#000000", 0, 0, 0, 255)]
	[InlineData("#FFFFFF00", 255, 255, 255, 0)]
	[InlineData("#000000FF", 0, 0, 0, 255)]
	[InlineData("#HHHHHH", 0, 0, 0, 255)]
	[InlineData("bob the builder", 0, 0, 0, 0)]
	public void ParseBrush_Hex(string brush, byte r, byte g, byte b, byte a)
	{
		// Given a hex color string
		// When parsing it
		Color result = brush.ParseColor();

		// Then the string is converted to a brush
		Assert.Equal(r, result.R);
		Assert.Equal(g, result.G);
		Assert.Equal(b, result.B);
		Assert.Equal(a, result.A);
	}

	public static TheoryData<string, Color> ParseBrushData =>
		new()
		{
			{ "alice_blue", Color.AliceBlue },
			{ "antique_white", Color.AntiqueWhite },
			{ "aqua", Color.Aqua },
			{ "aquamarine", Color.Aquamarine },
			{ "azure", Color.Azure },
			{ "beige", Color.Beige },
			{ "bisque", Color.Bisque },
			{ "black", Color.Black },
			{ "blanched_almond", Color.BlanchedAlmond },
			{ "blue", Color.Blue },
			{ "blue_violet", Color.BlueViolet },
			{ "brown", Color.Brown },
			{ "burly_wood", Color.BurlyWood },
			{ "cadet_blue", Color.CadetBlue },
			{ "chartreuse", Color.Chartreuse },
			{ "chocolate", Color.Chocolate },
			{ "coral", Color.Coral },
			{ "cornflower_blue", Color.CornflowerBlue },
			{ "cornsilk", Color.Cornsilk },
			{ "crimson", Color.Crimson },
			{ "cyan", Color.Cyan },
			{ "dark_blue", Color.DarkBlue },
			{ "dark_cyan", Color.DarkCyan },
			{ "dark_goldenrod", Color.DarkGoldenrod },
			{ "dark_gray", Color.DarkGray },
			{ "dark_green", Color.DarkGreen },
			{ "dark_khaki", Color.DarkKhaki },
			{ "dark_magenta", Color.DarkMagenta },
			{ "dark_olive_green", Color.DarkOliveGreen },
			{ "dark_orange", Color.DarkOrange },
			{ "dark_orchid", Color.DarkOrchid },
			{ "dark_red", Color.DarkRed },
			{ "dark_salmon", Color.DarkSalmon },
			{ "dark_sea_green", Color.DarkSeaGreen },
			{ "dark_slate_blue", Color.DarkSlateBlue },
			{ "dark_slate_gray", Color.DarkSlateGray },
			{ "dark_turquoise", Color.DarkTurquoise },
			{ "dark_violet", Color.DarkViolet },
			{ "deep_pink", Color.DeepPink },
			{ "deep_sky_blue", Color.DeepSkyBlue },
			{ "dim_gray", Color.DimGray },
			{ "dodger_blue", Color.DodgerBlue },
			{ "firebrick", Color.Firebrick },
			{ "floral_white", Color.FloralWhite },
			{ "forest_green", Color.ForestGreen },
			{ "fuchsia", Color.Fuchsia },
			{ "gainsboro", Color.Gainsboro },
			{ "ghost_white", Color.GhostWhite },
			{ "gold", Color.Gold },
			{ "goldenrod", Color.Goldenrod },
			{ "gray", Color.Gray },
			{ "green", Color.Green },
			{ "green_yellow", Color.GreenYellow },
			{ "honeydew", Color.Honeydew },
			{ "hot_pink", Color.HotPink },
			{ "indian_red", Color.IndianRed },
			{ "indigo", Color.Indigo },
			{ "ivory", Color.Ivory },
			{ "khaki", Color.Khaki },
			{ "lavender", Color.Lavender },
			{ "lavender_blush", Color.LavenderBlush },
			{ "lawn_green", Color.LawnGreen },
			{ "lemon_chiffon", Color.LemonChiffon },
			{ "light_blue", Color.LightBlue },
			{ "light_coral", Color.LightCoral },
			{ "light_cyan", Color.LightCyan },
			{ "light_goldenrod_yellow", Color.LightGoldenrodYellow },
			{ "light_gray", Color.LightGray },
			{ "light_green", Color.LightGreen },
			{ "light_pink", Color.LightPink },
			{ "light_salmon", Color.LightSalmon },
			{ "light_sea_green", Color.LightSeaGreen },
			{ "light_sky_blue", Color.LightSkyBlue },
			{ "light_slate_gray", Color.LightSlateGray },
			{ "light_steel_blue", Color.LightSteelBlue },
			{ "light_yellow", Color.LightYellow },
			{ "lime", Color.Lime },
			{ "lime_green", Color.LimeGreen },
			{ "linen", Color.Linen },
			{ "magenta", Color.Magenta },
			{ "maroon", Color.Maroon },
			{ "medium_aquamarine", Color.MediumAquamarine },
			{ "medium_blue", Color.MediumBlue },
			{ "medium_orchid", Color.MediumOrchid },
			{ "medium_purple", Color.MediumPurple },
			{ "medium_sea_green", Color.MediumSeaGreen },
			{ "medium_slate_blue", Color.MediumSlateBlue },
			{ "medium_spring_green", Color.MediumSpringGreen },
			{ "medium_turquoise", Color.MediumTurquoise },
			{ "medium_violet_red", Color.MediumVioletRed },
			{ "midnight_blue", Color.MidnightBlue },
			{ "mint_cream", Color.MintCream },
			{ "misty_rose", Color.MistyRose },
			{ "moccasin", Color.Moccasin },
			{ "navajo_white", Color.NavajoWhite },
			{ "navy", Color.Navy },
			{ "old_lace", Color.OldLace },
			{ "olive", Color.Olive },
			{ "olive_drab", Color.OliveDrab },
			{ "orange", Color.Orange },
			{ "orange_red", Color.OrangeRed },
			{ "orchid", Color.Orchid },
			{ "pale_goldenrod", Color.PaleGoldenrod },
			{ "pale_green", Color.PaleGreen },
			{ "pale_turquoise", Color.PaleTurquoise },
			{ "pale_violet_red", Color.PaleVioletRed },
			{ "papaya_whip", Color.PapayaWhip },
			{ "peach_puff", Color.PeachPuff },
			{ "peru", Color.Peru },
			{ "pink", Color.Pink },
			{ "plum", Color.Plum },
			{ "powder_blue", Color.PowderBlue },
			{ "purple", Color.Purple },
			{ "red", Color.Red },
			{ "rosy_brown", Color.RosyBrown },
			{ "royal_blue", Color.RoyalBlue },
			{ "saddle_brown", Color.SaddleBrown },
			{ "salmon", Color.Salmon },
			{ "sandy_brown", Color.SandyBrown },
			{ "sea_green", Color.SeaGreen },
			{ "sea_shell", Color.SeaShell },
			{ "sienna", Color.Sienna },
			{ "silver", Color.Silver },
			{ "sky_blue", Color.SkyBlue },
			{ "slate_blue", Color.SlateBlue },
			{ "slate_gray", Color.SlateGray },
			{ "snow", Color.Snow },
			{ "spring_green", Color.SpringGreen },
			{ "steel_blue", Color.SteelBlue },
			{ "tan", Color.Tan },
			{ "teal", Color.Teal },
			{ "thistle", Color.Thistle },
			{ "tomato", Color.Tomato },
			{ "transparent", Color.Transparent },
			{ "turquoise", Color.Turquoise },
			{ "violet", Color.Violet },
			{ "wheat", Color.Wheat },
			{ "white", Color.White },
			{ "white_smoke", Color.WhiteSmoke },
			{ "yellow", Color.Yellow },
			{ "yellow_green", Color.YellowGreen },
		};

	[Theory]
	[MemberData(nameof(ParseBrushData))]
	public void ParseBrush_String(string brush, Color expected)
	{
		// Given a named color string
		// When parsing it
		Color result = brush.ParseColor();

		// Then the string is converted to a brush
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(" ", " ")]
	[InlineData("", "")]
	[InlineData("a", "A")]
	[InlineData("ab", "Ab")]
	[InlineData("abc", "Abc")]
	public void Capitalize(string str, string expected)
	{
		// Given a string
		// When capitalizing it
		string result = str.Capitalize();

		// Then the first letter is capitalized
		Assert.Equal(expected, result);
	}
}
