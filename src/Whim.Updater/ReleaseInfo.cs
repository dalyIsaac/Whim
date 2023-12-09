using System.Text.RegularExpressions;
using Octokit;

namespace Whim.Updater;

public partial record ReleaseInfo(
	int Major,
	int Minor,
	int Patch,
	ReleaseChannel Channel,
	string Commit,
	Release? Release = null
)
{
	[GeneratedRegex(@"^v(\d+).(\d+).(\d+)-(alpha|beta|stable)\+[a-z0-9]+$")]
	private static partial Regex ReleaseTagRegex();

	public static ReleaseInfo? Create(string tagName, Release? release = null)
	{
		Match match = ReleaseTagRegex().Match(tagName);
		if (!match.Success)
		{
			Logger.Error($"Invalid release tag: {tagName}");
			return null;
		}

		if (match.Groups.Count < 5)
		{
			Logger.Error("Not enough capture groups");
			return null;
		}

		int major = int.Parse(match.Groups[1].Value);
		int minor = int.Parse(match.Groups[2].Value);
		int patch = int.Parse(match.Groups[3].Value);
		ReleaseChannel channel = match.Groups[4].Value switch
		{
			"alpha" => ReleaseChannel.Alpha,
			"beta" => ReleaseChannel.Beta,
			"stable" => ReleaseChannel.Stable,
			_ => ReleaseChannel.Stable
		};
		string commit = match.Groups[5].Value;

		return new ReleaseInfo(major, minor, patch, channel, commit, release);
	}

	/// <summary>
	/// Returns true if this is a newer release than the <paramref name="other"/> release.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public bool IsNewerRelease(ReleaseInfo other)
	{
		if (Major > other.Major)
		{
			return true;
		}
		else if (Major < other.Major)
		{
			return false;
		}

		if (Minor > other.Minor)
		{
			return true;
		}
		else if (Minor < other.Minor)
		{
			return false;
		}

		if (Patch > other.Patch)
		{
			return true;
		}
		else if (Patch < other.Patch)
		{
			return false;
		}

		// The releases are the same version, so compare the channels
		return Channel > other.Channel;
	}

	public override string ToString() => $"{Major}.{Minor}.{Patch}-{Channel}+{Commit}";
};
