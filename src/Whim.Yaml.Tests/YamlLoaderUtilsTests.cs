using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
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

	// TODO: Try again with .NET 8.0.402 or later
	// See https://learn.microsoft.com/en-us/windows/apps/winui/winui3/testing/
	//[Theory]
	//[InlineData("#FF0000", 255, 0, 0, 255)]
	//[InlineData("#00FF00", 0, 255, 0, 255)]
	//[InlineData("#0000FF", 0, 0, 255, 255)]
	//[InlineData("#FFFFFF", 255, 255, 255, 255)]
	//[InlineData("#000000", 0, 0, 0, 255)]
	//[InlineData("#FFFFFF00", 255, 255, 255, 0)]
	//[InlineData("#000000FF", 0, 0, 0, 255)]
	//[InlineData("#FFF", 255, 255, 255, 255)]
	//[InlineData("#000", 0, 0, 0, 255)]
	//[InlineData("#HHHHHH", 0, 0, 0, 0)]
	//[InlineData("bob the builder", 0, 0, 0, 0)]
	//public void ParseBrush_Hex(string brush, byte r, byte g, byte b, byte a)
	//{
	//	// Given a hex color string
	//	// When parsing it
	//	Brush result = brush.ParseBrush();

	//	// Then the string is converted to a brush
	//	SolidColorBrush solidColor = (SolidColorBrush)result;
	//	Assert.Equal(r, solidColor.Color.R);
	//	Assert.Equal(g, solidColor.Color.G);
	//	Assert.Equal(b, solidColor.Color.B);
	//	Assert.Equal(a, solidColor.Color.A);
	//}

	//public static TheoryData<string, Color> ParseBrushData =>
	//	new()
	//	{
	//		{ "alice_blue", Colors.AliceBlue },
	//		{ "antique_white", Colors.AntiqueWhite },
	//		{ "aqua", Colors.Aqua },
	//		{ "aquamarine", Colors.Aquamarine },
	//		{ "azure", Colors.Azure },
	//		{ "beige", Colors.Beige },
	//		{ "bisque", Colors.Bisque },
	//		{ "black", Colors.Black },
	//		{ "blanched_almond", Colors.BlanchedAlmond },
	//		{ "blue", Colors.Blue },
	//		{ "blue_violet", Colors.BlueViolet },
	//		{ "brown", Colors.Brown },
	//		{ "burly_wood", Colors.BurlyWood },
	//		{ "cadet_blue", Colors.CadetBlue },
	//		{ "chartreuse", Colors.Chartreuse },
	//		{ "chocolate", Colors.Chocolate },
	//		{ "coral", Colors.Coral },
	//		{ "cornflower_blue", Colors.CornflowerBlue },
	//		{ "cornsilk", Colors.Cornsilk },
	//		{ "crimson", Colors.Crimson },
	//		{ "cyan", Colors.Cyan },
	//		{ "dark_blue", Colors.DarkBlue },
	//		{ "dark_cyan", Colors.DarkCyan },
	//		{ "dark_goldenrod", Colors.DarkGoldenrod },
	//		{ "dark_gray", Colors.DarkGray },
	//		{ "dark_green", Colors.DarkGreen },
	//		{ "dark_khaki", Colors.DarkKhaki },
	//		{ "dark_magenta", Colors.DarkMagenta },
	//		{ "dark_olive_green", Colors.DarkOliveGreen },
	//		{ "dark_orange", Colors.DarkOrange },
	//		{ "dark_orchid", Colors.DarkOrchid },
	//		{ "dark_red", Colors.DarkRed },
	//		{ "dark_salmon", Colors.DarkSalmon },
	//		{ "dark_sea_green", Colors.DarkSeaGreen },
	//		{ "dark_slate_blue", Colors.DarkSlateBlue },
	//		{ "dark_slate_gray", Colors.DarkSlateGray },
	//		{ "dark_turquoise", Colors.DarkTurquoise },
	//		{ "dark_violet", Colors.DarkViolet },
	//		{ "deep_pink", Colors.DeepPink },
	//		{ "deep_sky_blue", Colors.DeepSkyBlue },
	//		{ "dim_gray", Colors.DimGray },
	//		{ "dodger_blue", Colors.DodgerBlue },
	//		{ "firebrick", Colors.Firebrick },
	//		{ "floral_white", Colors.FloralWhite },
	//		{ "forest_green", Colors.ForestGreen },
	//		{ "fuchsia", Colors.Fuchsia },
	//		{ "gainsboro", Colors.Gainsboro },
	//		{ "ghost_white", Colors.GhostWhite },
	//		{ "gold", Colors.Gold },
	//		{ "goldenrod", Colors.Goldenrod },
	//		{ "gray", Colors.Gray },
	//		{ "green", Colors.Green },
	//		{ "green_yellow", Colors.GreenYellow },
	//		{ "honeydew", Colors.Honeydew },
	//		{ "hot_pink", Colors.HotPink },
	//		{ "indian_red", Colors.IndianRed },
	//		{ "indigo", Colors.Indigo },
	//		{ "ivory", Colors.Ivory },
	//		{ "khaki", Colors.Khaki },
	//		{ "lavender", Colors.Lavender },
	//		{ "lavender_blush", Colors.LavenderBlush },
	//		{ "lawn_green", Colors.LawnGreen },
	//		{ "lemon_chiffon", Colors.LemonChiffon },
	//		{ "light_blue", Colors.LightBlue },
	//		{ "light_coral", Colors.LightCoral },
	//		{ "light_cyan", Colors.LightCyan },
	//		{ "light_goldenrod_yellow", Colors.LightGoldenrodYellow },
	//		{ "light_gray", Colors.LightGray },
	//		{ "light_green", Colors.LightGreen },
	//		{ "light_pink", Colors.LightPink },
	//		{ "light_salmon", Colors.LightSalmon },
	//		{ "light_sea_green", Colors.LightSeaGreen },
	//		{ "light_sky_blue", Colors.LightSkyBlue },
	//		{ "light_slate_gray", Colors.LightSlateGray },
	//		{ "light_steel_blue", Colors.LightSteelBlue },
	//		{ "light_yellow", Colors.LightYellow },
	//		{ "lime", Colors.Lime },
	//		{ "lime_green", Colors.LimeGreen },
	//		{ "linen", Colors.Linen },
	//		{ "magenta", Colors.Magenta },
	//		{ "maroon", Colors.Maroon },
	//		{ "medium_aquamarine", Colors.MediumAquamarine },
	//		{ "medium_blue", Colors.MediumBlue },
	//		{ "medium_orchid", Colors.MediumOrchid },
	//		{ "medium_purple", Colors.MediumPurple },
	//		{ "medium_sea_green", Colors.MediumSeaGreen },
	//		{ "medium_slate_blue", Colors.MediumSlateBlue },
	//		{ "medium_spring_green", Colors.MediumSpringGreen },
	//		{ "medium_turquoise", Colors.MediumTurquoise },
	//		{ "medium_violet_red", Colors.MediumVioletRed },
	//		{ "midnight_blue", Colors.MidnightBlue },
	//		{ "mint_cream", Colors.MintCream },
	//		{ "misty_rose", Colors.MistyRose },
	//		{ "moccasin", Colors.Moccasin },
	//		{ "navajo_white", Colors.NavajoWhite },
	//		{ "navy", Colors.Navy },
	//		{ "old_lace", Colors.OldLace },
	//		{ "olive", Colors.Olive },
	//		{ "olive_drab", Colors.OliveDrab },
	//		{ "orange", Colors.Orange },
	//		{ "orange_red", Colors.OrangeRed },
	//		{ "orchid", Colors.Orchid },
	//		{ "pale_goldenrod", Colors.PaleGoldenrod },
	//		{ "pale_green", Colors.PaleGreen },
	//		{ "pale_turquoise", Colors.PaleTurquoise },
	//		{ "pale_violet_red", Colors.PaleVioletRed },
	//		{ "papaya_whip", Colors.PapayaWhip },
	//		{ "peach_puff", Colors.PeachPuff },
	//		{ "peru", Colors.Peru },
	//		{ "pink", Colors.Pink },
	//		{ "plum", Colors.Plum },
	//		{ "powder_blue", Colors.PowderBlue },
	//		{ "purple", Colors.Purple },
	//		{ "red", Colors.Red },
	//		{ "rosy_brown", Colors.RosyBrown },
	//		{ "royal_blue", Colors.RoyalBlue },
	//		{ "saddle_brown", Colors.SaddleBrown },
	//		{ "salmon", Colors.Salmon },
	//		{ "sandy_brown", Colors.SandyBrown },
	//		{ "sea_green", Colors.SeaGreen },
	//		{ "sea_shell", Colors.SeaShell },
	//		{ "sienna", Colors.Sienna },
	//		{ "silver", Colors.Silver },
	//		{ "sky_blue", Colors.SkyBlue },
	//		{ "slate_blue", Colors.SlateBlue },
	//		{ "slate_gray", Colors.SlateGray },
	//		{ "snow", Colors.Snow },
	//		{ "spring_green", Colors.SpringGreen },
	//		{ "steel_blue", Colors.SteelBlue },
	//		{ "tan", Colors.Tan },
	//		{ "teal", Colors.Teal },
	//		{ "thistle", Colors.Thistle },
	//		{ "tomato", Colors.Tomato },
	//		{ "transparent", Colors.Transparent },
	//		{ "turquoise", Colors.Turquoise },
	//		{ "violet", Colors.Violet },
	//		{ "wheat", Colors.Wheat },
	//		{ "white", Colors.White },
	//		{ "white_smoke", Colors.WhiteSmoke },
	//		{ "yellow", Colors.Yellow },
	//		{ "yellow_green", Colors.YellowGreen },
	//	};

	//[Theory]
	//[MemberData(nameof(ParseBrushData))]
	//public void ParseBrush_String(string brush, Color expected)
	//{
	//	// Given a named color string
	//	// When parsing it
	//	Brush result = brush.ParseBrush();

	//	// Then the string is converted to a brush
	//	SolidColorBrush solidColor = (SolidColorBrush)result;
	//	Assert.Equal(expected, solidColor.Color);
	//}
}
