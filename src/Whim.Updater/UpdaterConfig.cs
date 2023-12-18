using System;
using System.Timers;

namespace Whim.Updater;

/// <summary>
/// Release channels are used to determine which stream of release to install.
/// </summary>
public enum ReleaseChannel
{
	/// <summary>
	/// Alpha releases are the latest changes in the <c>main</c> branch.
	/// </summary>
	Alpha = 0,

	/// <summary>
	/// Beta releases are the latest changes in a release branch <c>release/v*</c>.
	/// </summary>
	Beta = 1,

	/// <summary>
	/// Stable releases are specific releases in a release branch <c>release/v*</c>.
	/// </summary>
	Stable = 2,
}

/// <summary>
/// The frequency at which the updater should check for updates.
/// </summary>
public enum UpdateFrequency
{
	/// <summary>
	/// Check for updates daily.
	/// </summary>
	Daily,

	/// <summary>
	/// Check for updates weekly.
	/// </summary>
	Weekly,

	/// <summary>
	/// Check for updates monthly.
	/// </summary>
	Monthly,

	/// <summary>
	/// Never check for updates.
	/// </summary>
	Never,
}

/// <summary>
/// Extensions for <see cref="UpdateFrequency"/>.
/// </summary>
public static class UpdateFrequencyExtensions
{
	/// <summary>
	/// Gets a <see cref="double"/> value <see cref="Timer.Interval"/> for the given
	/// <see cref="UpdateFrequency"/>.
	/// </summary>
	/// <param name="frequency"></param>
	/// <returns></returns>
	public static double? GetInterval(this UpdateFrequency frequency) =>
		frequency switch
		{
			UpdateFrequency.Daily => TimeSpan.FromDays(1).TotalMilliseconds,
			UpdateFrequency.Weekly => TimeSpan.FromDays(7).TotalMilliseconds,
			UpdateFrequency.Monthly => TimeSpan.FromDays(30).TotalMilliseconds,
			UpdateFrequency.Never => null,
			_ => null
		};
}

/// <summary>
/// Configuration for <see cref="UpdaterPlugin"/>.
/// </summary>
public class UpdaterConfig
{
	/// <summary>
	/// The release channel to install. Defaults to <see cref="ReleaseChannel.Stable"/>.
	/// </summary>
	public ReleaseChannel ReleaseChannel { get; set; } = ReleaseChannel.Stable;

	/// <summary>
	/// The frequency at which the updater should check for updates. Defaults to <see cref="UpdateFrequency.Weekly"/>.
	/// </summary>
	public UpdateFrequency UpdateFrequency { get; set; } = UpdateFrequency.Weekly;
}
