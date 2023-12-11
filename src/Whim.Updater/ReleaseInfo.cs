using Octokit;

namespace Whim.Updater;

public class ReleaseInfo
{
	public Release Release { get; }
	public Version Version { get; }

	public bool IsOnlyAvailableRelease { get; }
	public string ButtonContent => $"Install {Version}";

	public ReleaseInfo(Release release, Version version, bool isOnlyAvailableRelease)
	{
		Release = release;
		Version = version;
		IsOnlyAvailableRelease = isOnlyAvailableRelease;
	}
}
