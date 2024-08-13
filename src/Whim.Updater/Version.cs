using System.Text.RegularExpressions;

namespace Whim.Updater;

/// <summary>
/// A semantic version.
/// </summary>
public partial record Version
{
	/// <summary>
	/// The major version number.
	/// </summary>
	public int Major { get; }

	/// <summary>
	/// The minor version number.
	/// </summary>
	public int Minor { get; }

	/// <summary>
	/// The patch version number.
	/// </summary>
	public int Patch { get; }

	/// <summary>
	/// The release channel.
	/// </summary>
	public ReleaseChannel ReleaseChannel { get; }

	/// <summary>
	/// The commit hash.
	/// </summary>
	public string Commit { get; }

	private Version(int major, int minor, int patch, ReleaseChannel releaseChannel, string commit)
	{
		Major = major;
		Minor = minor;
		Patch = patch;
		ReleaseChannel = releaseChannel;
		Commit = commit;
	}

	/// <summary>
	/// Creates a new <see cref="Version"/> by parsing the given <paramref name="tagName"/>.
	/// </summary>
	/// <param name="tagName"></param>
	/// <returns>
	/// A new <see cref="Version"/> if the tag name is valid, otherwise null.
	/// </returns>
	public static Version? ParseSemver(string tagName)
	{
		Match match = SemverRegex().Match(tagName);
		if (!match.Success || match.Groups.Count != 6)
		{
			return null;
		}

		return GetVersionFromMatch(match);
	}

	[GeneratedRegex(@"^v(\d+).(\d+).(\d+)-(alpha|beta|stable)\+([a-z0-9]{8})$")]
	private static partial Regex SemverRegex();

	/// <summary>
	/// Parses a version string in the form of "v{major}.{minor}.{patch}.{build}".
	/// This is the format returned by <see cref="INativeManager.GetWhimVersion"/>
	/// </summary>
	/// <param name="version"></param>
	/// <returns></returns>
	public static Version? ParseProductVersion(string version)
	{
		Match match = ProductVersionRegex().Match(version);
		if (!match.Success || match.Groups.Count != 6)
		{
			return null;
		}

		return GetVersionFromMatch(match);
	}

	[GeneratedRegex(@"^(\d+).(\d+).(\d+)-(alpha|beta|stable)\+([a-z\d]{8}).[a-z\d]*$")]
	private static partial Regex ProductVersionRegex();

	private static Version GetVersionFromMatch(Match match)
	{
		int major = int.Parse(match.Groups[1].Value);
		int minor = int.Parse(match.Groups[2].Value);
		int patch = int.Parse(match.Groups[3].Value);
		ReleaseChannel releaseChannel = match.Groups[4].Value switch
		{
			"alpha" => ReleaseChannel.Alpha,
			"beta" => ReleaseChannel.Beta,
			"stable" => ReleaseChannel.Stable,
			_ => ReleaseChannel.Stable
		};
		string commit = match.Groups[5].Value;

		return new Version(major, minor, patch, releaseChannel, commit);
	}

	/// <summary>
	/// Returns true if this is a newer release than the <paramref name="other"/> release.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public bool IsNewerVersion(Version other)
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

		if (ReleaseChannel == ReleaseChannel.Unknown || other.ReleaseChannel == ReleaseChannel.Unknown)
		{
			return false;
		}

		// The releases are the same version, so compare the channels
		return ReleaseChannel > other.ReleaseChannel;
	}

	/// <inheritdoc/>
	public override string ToString() => $"v{Major}.{Minor}.{Patch}-{ReleaseChannel.ToString().ToLower()}+{Commit}";
};
