using System;
using System.Timers;

namespace Whim.Updater;

public enum ReleaseChannel
{
	Alpha = 0,
	Beta = 1,
	Stable = 2,
}

public enum UpdateFrequency
{
	Daily,
	Weekly,
	Monthly,
	Never,
}

public static class UpdateFrequencyExtensions
{
	public static Timer GetTimer(this UpdateFrequency frequency)
	{
		Timer timer = new();
		switch (frequency)
		{
			case UpdateFrequency.Daily:
				timer.Interval = TimeSpan.FromDays(1).TotalMilliseconds;
				break;
			case UpdateFrequency.Weekly:
				timer.Interval = TimeSpan.FromDays(7).TotalMilliseconds;
				break;
			case UpdateFrequency.Monthly:
				timer.Interval = TimeSpan.FromDays(30).TotalMilliseconds;
				break;
			case UpdateFrequency.Never:
				timer.Interval = -1;
				break;
		}
		return timer;
	}
}

// TODO: bind
public class UpdaterConfig
{
	public ReleaseChannel ReleaseChannel { get; set; } = ReleaseChannel.Stable;

	public UpdateFrequency UpdateFrequency { get; set; } = UpdateFrequency.Weekly;
}
