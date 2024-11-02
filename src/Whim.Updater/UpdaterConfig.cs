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
	Daily = 1,

	/// <summary>
	/// Check for updates weekly.
	/// </summary>
	Weekly = 7,

	/// <summary>
	/// Check for updates monthly.
	/// </summary>
	Monthly = 28,

	/// <summary>
	/// Never check for updates.
	/// </summary>
	Never = 0,
}

/// <summary>
/// Configuration for <see cref="UpdaterPlugin"/>.
/// </summary>
public class UpdaterConfig
{
	/// <summary>
	/// The release channel to install. Defaults to <see cref="ReleaseChannel.Alpha"/>.
	/// </summary>
	public ReleaseChannel ReleaseChannel { get; set; } = ReleaseChannel.Alpha;

	/// <summary>
	/// The frequency at which the updater should check for updates. Defaults to <see cref="UpdateFrequency.Weekly"/>.
	/// </summary>
	public UpdateFrequency UpdateFrequency { get; set; } = UpdateFrequency.Weekly;
}
