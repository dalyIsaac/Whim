using Xunit;

namespace Whim.Updater.Tests;

public class UpdaterConfigTests
{
	public static TheoryData<UpdateFrequency, double?> GetTimerData =>
		new()
		{
			{ UpdateFrequency.Daily, TimeSpan.FromDays(1).TotalMilliseconds },
			{ UpdateFrequency.Weekly, TimeSpan.FromDays(7).TotalMilliseconds },
			{ UpdateFrequency.Monthly, TimeSpan.FromDays(30).TotalMilliseconds },
			{ UpdateFrequency.Never, null },
			{ (UpdateFrequency)int.MaxValue, null },
		};

	[Theory]
	[MemberData(nameof(GetTimerData))]
	public void GetTimer(UpdateFrequency frequency, double? expectedInterval)
	{
		// Given
		double? interval = frequency.GetInterval();

		// Then
		Assert.Equal(expectedInterval, interval);
	}
}
