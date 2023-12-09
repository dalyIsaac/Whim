namespace Whim.Updater;

public enum ReleaseChannel
{
	Alpha,
	Beta,
	Stable,
}

public enum UpdateFrequency
{
	Daily,
	Weekly,
	Monthly,
	Never,
}

public class UpdaterConfig
{
	public ReleaseChannel ReleaseChannel { get; set; } = ReleaseChannel.Stable;

	public UpdateFrequency UpdateFrequency { get; set; } = UpdateFrequency.Weekly;
}
