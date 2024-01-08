using System;

namespace Whim.Bar;

internal static class BatteryWidgetIcons
{
	public const string BatteryCharging9 = "\xe83e";
	public const string Battery10 = "\xe83f";
	public const string Battery0 = "\xe850";
	public const string Battery1 = "\xe851";
	public const string Battery2 = "\xe852";
	public const string Battery3 = "\xe853";
	public const string Battery4 = "\xe854";
	public const string Battery5 = "\xe855";
	public const string Battery6 = "\xe856";
	public const string Battery7 = "\xe857";
	public const string Battery8 = "\xe858";
	public const string Battery9 = "\xe859";
	public const string BatteryCharging0 = "\xe85a";
	public const string BatteryCharging1 = "\xe85b";
	public const string BatteryCharging2 = "\xe85c";
	public const string BatteryCharging3 = "\xe85d";
	public const string BatteryCharging4 = "\xe85e";
	public const string BatteryCharging5 = "\xe85f";
	public const string BatteryCharging6 = "\xe860";
	public const string BatteryCharging7 = "\xe861";
	public const string BatteryCharging8 = "\xe862";
	public const string BatteryCharging10 = "\xe83e";
	public const string BatterySaver0 = "\xe863";
	public const string BatterySaver1 = "\xe864";
	public const string BatterySaver2 = "\xe865";
	public const string BatterySaver3 = "\xe866";
	public const string BatterySaver4 = "\xe867";
	public const string BatterySaver5 = "\xe868";
	public const string BatterySaver6 = "\xe869";
	public const string BatterySaver7 = "\xe86a";
	public const string BatterySaver8 = "\xe86b";
	public const string BatterySaver9 = "\xea94";
	public const string BatterySaver10 = "\xea95";
	public const string BatteryUnknown = "\xe996";

	public static string GetBatteryIcon(int percent, bool isCharging, bool isSaver)
	{
		int iconNumber = (int)Math.Round(percent / 10.0, MidpointRounding.ToZero);

		// Force unknown icon if the icon number is out of range.
		if (percent < 0 || percent > 100)
		{
			iconNumber = 11;
		}

		if (isCharging)
		{
			return iconNumber switch
			{
				0 => BatteryCharging0,
				1 => BatteryCharging1,
				2 => BatteryCharging2,
				3 => BatteryCharging3,
				4 => BatteryCharging4,
				5 => BatteryCharging5,
				6 => BatteryCharging6,
				7 => BatteryCharging7,
				8 => BatteryCharging8,
				9 => BatteryCharging9,
				10 => BatteryCharging10,
				_ => BatteryUnknown,
			};
		}
		else if (isSaver)
		{
			return iconNumber switch
			{
				0 => BatterySaver0,
				1 => BatterySaver1,
				2 => BatterySaver2,
				3 => BatterySaver3,
				4 => BatterySaver4,
				5 => BatterySaver5,
				6 => BatterySaver6,
				7 => BatterySaver7,
				8 => BatterySaver8,
				9 => BatterySaver9,
				10 => BatterySaver10,
				_ => BatteryUnknown,
			};
		}

		return iconNumber switch
		{
			0 => Battery0,
			1 => Battery1,
			2 => Battery2,
			3 => Battery3,
			4 => Battery4,
			5 => Battery5,
			6 => Battery6,
			7 => Battery7,
			8 => Battery8,
			9 => Battery9,
			10 => Battery10,
			_ => BatteryUnknown,
		};
	}
}
