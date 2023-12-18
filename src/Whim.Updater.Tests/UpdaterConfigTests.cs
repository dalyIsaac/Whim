using Xunit;

namespace Whim.Updater.Tests;

public class UpdaterConfigTests
{
	public static IEnumerable<object[]> GetTimerData()
	{
		yield return new object[] { UpdateFrequency.Daily, TimeSpan.FromDays(1).TotalMilliseconds };
		yield return new object[] { UpdateFrequency.Weekly, TimeSpan.FromDays(7).TotalMilliseconds };
		yield return new object[] { UpdateFrequency.Monthly, TimeSpan.FromDays(30).TotalMilliseconds };
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		yield return new object[] { UpdateFrequency.Never, null };
		yield return new object[] { (UpdateFrequency)int.MaxValue, null };
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	}

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
